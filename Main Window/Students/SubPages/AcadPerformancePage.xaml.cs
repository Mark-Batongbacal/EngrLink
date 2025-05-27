using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using EngrLink.Models;
using System.ComponentModel; // Needed for INotifyPropertyChanged
using System.Diagnostics; // For Debug.WriteLine

namespace EngrLink.Main_Window.Students.SubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AcadPerformancePage : Page
    {
        // Inner class for StudentProfileModel, mirroring the ShowGrades page
        public class StudentProfileModel : INotifyPropertyChanged
        {
            private string _name;
            private string _id;
            private string _program;
            private string _year;
            private double _gwa;

            public string Name
            {
                get => _name;
                set { _name = value; OnPropertyChanged(nameof(Name)); }
            }

            public string Id
            {
                get => _id;
                set { _id = value; OnPropertyChanged(nameof(Id)); }
            }

            public string Program
            {
                get => _program;
                set { _program = value; OnPropertyChanged(nameof(Program)); }
            }

            public string Year
            {
                get => _year;
                set { _year = value; OnPropertyChanged(nameof(Year)); }
            }
            public double GWA
            {
                get => _gwa;
                set { _gwa = value; OnPropertyChanged(nameof(GWA)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Public property to expose the StudentProfileModel to XAML
        public StudentProfileModel StudentProfile { get; set; } = new StudentProfileModel();

        public string Id { get; set; } // Renamed from StudentId to Id as per your original code

        public AcadPerformancePage()
        {
            this.InitializeComponent();
            // Set the DataContext of the page to itself so bindings can resolve properties
            this.DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string Id) // Assuming Id is a string as in your original code
            {
                this.Id = Id;
                Debug.WriteLine($"Navigated with Student ID: {this.Id}");
            }
            // Load all necessary data when navigating to the page
            await LoadStudentData();
        }

        private async System.Threading.Tasks.Task LoadStudentData()
        {
            var client = App.SupabaseClient;

            // Get grades for the student
            try
            {
                var gradesResponse = await client
                    .From<IndivSubject>()
                    .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, this.Id) // Use this.Id
                    .Get();

                SubjectsListView.ItemsSource = gradesResponse.Models;

                // Calculate GWA using the loaded subjects
                var subjects = gradesResponse.Models;
                double totalUnits = 0;
                double totalWeightedGrades = 0;

                foreach (var subject in subjects)
                {
                    // Assuming grades below 50 or 0 mean 'Not Yet Graded'
                    if (subject.Grade > 0) // Only include graded subjects in GWA calculation
                    {
                        totalUnits += subject.Units;
                        totalWeightedGrades += subject.Grade * subject.Units;
                    }
                }

                if (totalUnits > 0)
                {
                    StudentProfile.GWA = Math.Round(totalWeightedGrades / totalUnits, 2);
                }
                else
                {
                    StudentProfile.GWA = 0; // No graded subjects, GWA is 0
                }
                Debug.WriteLine($"Calculated GWA: {StudentProfile.GWA}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading subjects or calculating GWA: {ex.Message}");
                // Optionally show a message to the user
            }


            // Get student profile
            try
            {
                var studentResponse = await client
                    .From<Student>() // Assuming 'Student' is your model for student profiles
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, this.Id) // Use this.Id
                    .Get();

                var student = studentResponse.Models.FirstOrDefault();

                if (student != null)
                {
                    StudentProfile.Name = student.Name;
                    StudentProfile.Program = student.Program;
                    StudentProfile.Id = student.Id.ToString();
                    // Ensure 'Year' in your Student model is a string or convert it
                    StudentProfile.Year = student.Year.ToString(); // Assuming student.Year is int/double, convert to string
                    Debug.WriteLine($"Student Profile Loaded - Name: {StudentProfile.Name}, Program: {StudentProfile.Program}, Year: {StudentProfile.Year}");
                }
                else
                {
                    Debug.WriteLine($"Student with ID {this.Id} not found in the 'Student' table.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading student profile: {ex.Message}");
                // Optionally show a message to the user
            }
        }
    }
}