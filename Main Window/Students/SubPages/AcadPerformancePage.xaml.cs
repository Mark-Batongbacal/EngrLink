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
using Microsoft.UI.Xaml.Media.Imaging;

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

        public StudentProfileModel StudentProfile { get; set; } = new StudentProfileModel();

        public string Id { get; set; }

        public AcadPerformancePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string Id)
            {
                this.Id = Id;
                Debug.WriteLine($"Navigated with Student ID: {this.Id}");
            }
            else
            {
                Debug.WriteLine("AcadPerformancePage navigated to without a Student ID parameter.");
                // Optionally handle error or navigate back
                return; // Stop if no ID is provided
            }
            await LoadStudentData();
        }

        private async System.Threading.Tasks.Task LoadStudentData()
        {
            var client = App.SupabaseClient;

            try
            {
                // First, load subjects/grades
                var gradesResponse = await client
                    .From<IndivSubject>()
                    .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, this.Id)
                    .Order("code", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                var subjects = gradesResponse.Models;
                var subjectViews = subjects
                .Select(sub => new IndivSubjectView { Sub = sub })
                .ToList();

                SubjectsListView.ItemsSource = subjectViews;
                double totalUnits = 0;
                double totalWeightedGrades = 0;

                foreach (var subject in subjects)
                {

                    if (subject.Grade > 0)
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
                    StudentProfile.GWA = 0;
                }
                Debug.WriteLine($"Calculated GWA: {StudentProfile.GWA}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading subjects or calculating GWA: {ex.Message}");
                // Handle the error appropriately, e.g., show a message to the user
            }

            try
            {
                // Second, load student profile details (including image URL)
                var studentResponse = await client
                    .From<Student>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, this.Id)
                    .Get();

                var student = studentResponse.Models.FirstOrDefault();

                if (student != null)
                {
                    StudentProfile.Name = student.Name;
                    StudentProfile.Program = student.Program;
                    StudentProfile.Id = student.Id.ToString();
                    StudentProfile.Year = student.Year.ToString();
                    StudentProfile.ProfileImageUrl = student.ProfileImageUrl;

                    Debug.WriteLine($"Student Profile Loaded - Name: {StudentProfile.Name}, Program: {StudentProfile.Program}, Year: {StudentProfile.Year}, Image URL: {StudentProfile.ProfileImageUrl}");

                    if (!string.IsNullOrEmpty(StudentProfile.ProfileImageUrl))
                    {
                        try
                        {
                            Uri imageUri = new Uri(StudentProfile.ProfileImageUrl);
                            BitmapImage bitmapImage = new BitmapImage(imageUri);
                            StudentProfileImage.Source = bitmapImage;
                            Debug.WriteLine($"Successfully loaded image for {StudentProfile.Name} from: {StudentProfile.ProfileImageUrl}");
                        }
                        catch (UriFormatException ex)
                        {
                            Debug.WriteLine($"Invalid image URL for student {StudentProfile.Id}: {StudentProfile.ProfileImageUrl} - {ex.Message}");
                            StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png")); // Fallback
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error loading image for student {StudentProfile.Id}: {ex.Message}");
                            StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png")); // Fallback
                        }
                    }
                    else
                    {
                        StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                        Debug.WriteLine($"No profile image URL found for student {StudentProfile.Id}. Displaying placeholder.");
                    }
                }
                else
                {
                    Debug.WriteLine($"Student with ID {this.Id} not found in the 'Student' table.");
                    StudentProfile.Name = "N/A";
                    StudentProfile.Program = "N/A";
                    StudentProfile.Id = this.Id;
                    StudentProfile.Year = "N/A";
                    StudentProfile.ProfileImageUrl = null;
                    StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png")); // Reset image
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading student profile: {ex.Message}");
            }
        }
    }
}