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
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace EngrLink.Main_Window.Students.SubPages
{
    public sealed partial class AcadPerformancePage : Page
    {
        public class StudentProfileModel : INotifyPropertyChanged
        {
            private string _name;
            private string _id;
            private string _program;
            private string _year;
            private double _gwa;
            private string _profileImageUrl;

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

            // NEW PROPERTY for the Profile Image URL
            public string ProfileImageUrl
            {
                get => _profileImageUrl;
                set { _profileImageUrl = value; OnPropertyChanged(nameof(ProfileImageUrl)); }
            }


            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<IndivSubjectView> SubjectViews { get; set; } = new ObservableCollection<IndivSubjectView>();

        public StudentProfileModel StudentProfile { get; set; } = new StudentProfileModel();

        public string StudentId { get; set; }

        public AcadPerformancePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            SubjectsListView.ItemsSource = SubjectViews;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string studentId)
            {
                this.StudentId = studentId;
                Debug.WriteLine($"Navigated with Student ID: {this.StudentId}");
            }
            else
            {
                Debug.WriteLine("Navigation parameter is not a string or is null.");
            }
            await LoadStudentData();
        }

        private async System.Threading.Tasks.Task LoadStudentData()
        {
            var client = App.SupabaseClient;

            try
            {
                if (string.IsNullOrEmpty(StudentId))
                {
                    Debug.WriteLine("StudentId is null or empty. Cannot load student profile.");
                    return;
                }

                var studentResponse = await client
                    .From<Student>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, StudentId)
                    .Get();

                var student = studentResponse.Models.FirstOrDefault();

                if (student != null)
                {
                    StudentProfile.Name = student.Name;
                    StudentProfile.Program = student.Program;
                    StudentProfile.Id = student.Id.ToString();
                    StudentProfile.Year = student.Year.ToString();
                    Debug.WriteLine($"Student Profile Loaded - Name: {StudentProfile.Name}, Program: {StudentProfile.Program}, Year: {StudentProfile.Year}");
                }
                else
                {
                    Debug.WriteLine($"Student with ID {StudentId} not found in the 'Student' table.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading student profile: {ex.Message}");
            }

            try
            {
                if (string.IsNullOrEmpty(StudentId))
                {
                    Debug.WriteLine("StudentId is null or empty. Cannot load grades.");
                    return;
                }

                var gradesResponse = await client
                    .From<IndivSubject>()
                    .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, StudentId)
                    .Order("code", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                SubjectViews.Clear();
                foreach (var sub in gradesResponse.Models)
                {
                    SubjectViews.Add(new IndivSubjectView { Sub = sub });
                }

                UpdateDisplayedGrades("midterm");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading subjects or calculating GWA: {ex.Message}");
            }
        }

        private void GradePeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GradePeriodComboBox.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is string period)
            {
                UpdateDisplayedGrades(period);
            }
        }

        private void UpdateDisplayedGrades(string period)
        {
            if (SubjectViews == null || !SubjectViews.Any())
            {
                Debug.WriteLine("No subjects to update grades for.");
                StudentProfile.GWA = 0;
                return;
            }

            double totalUnits = 0;
            double totalWeightedGrades = 0;

            foreach (var item in SubjectViews)
            {
                double grade = period == "final" ? item.Sub.Grade_F : item.Sub.Grade;
                item.DisplayedGrade = grade;

                if (grade > 0)
                {
                    totalUnits += item.Sub.Units;
                    totalWeightedGrades += grade * item.Sub.Units;
                }
            }

            StudentProfile.GWA = totalUnits > 0 ? Math.Round(totalWeightedGrades / totalUnits, 2) : 0;

            SubjectsListView.ItemsSource = null;
            SubjectsListView.ItemsSource = SubjectViews;
        }
    }
}