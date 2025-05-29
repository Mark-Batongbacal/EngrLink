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
            private string _period;
            private string _profileImageUrl;

            public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
            public string Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
            public string Program { get => _program; set { _program = value; OnPropertyChanged(nameof(Program)); } }
            public string Year { get => _year; set { _year = value; OnPropertyChanged(nameof(Year)); } }
            public double GWA { get => _gwa; set { _gwa = value; OnPropertyChanged(nameof(GWA)); } }
            public string Period { get => _period; set { _period = value; OnPropertyChanged(nameof(Period)); } }
            public string ProfileImageUrl { get => _profileImageUrl; set { _profileImageUrl = value; OnPropertyChanged(nameof(ProfileImageUrl)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<IndivSubjectView> SubjectViews { get; set; } = new ObservableCollection<IndivSubjectView>();
        public StudentProfileModel StudentProfile { get; set; } = new StudentProfileModel();
        public string StudentId { get; set; }
        public string Program { get; set; }

        public AcadPerformancePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            SubjectsListView.ItemsSource = SubjectViews;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is (string studentId, string program))
            {
                this.StudentId = studentId;
                this.Program = program;
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
                    StudentProfile.ProfileImageUrl = student.ProfileImageUrl;

                    if (!string.IsNullOrEmpty(StudentProfile.ProfileImageUrl))
                    {
                        try
                        {
                            var bitmapImage = new BitmapImage(new Uri(StudentProfile.ProfileImageUrl));
                            StudentProfileImage.Source = bitmapImage;
                        }
                        catch
                        {
                            StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                        }
                    }
                    else
                    {
                        StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                    }
                }
            }
            catch (Exception ex)
            {
                Frame.Navigate(typeof(ErrorPage), (typeof(Dashboard), this.Program, this.StudentId));
            }

            try
            {
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

                UpdateDisplayedGrades(StudentProfile.Period);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading subjects or calculating GWA: {ex.Message}");
            }
        }

        private void GradePeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GradePeriodComboBox.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is string periodTag)
            {
                StudentProfile.Period = periodTag; // Update the StudentProfile's period
                UpdateDisplayedGrades(StudentProfile.Period); // Recalculate GWA and trigger subject view updates
                LoadStudentData();
            }
        }

        private void UpdateDisplayedGrades(string period)
        {
            
            double totalUnits = 0;
            double totalWeightedGrades = 0;

            foreach (var item in SubjectViews)
            {
                item.Period = period;
                Debug.Write(item.DisplayedGrade);
                double grade = period == "final" ? item.Sub.Grade_F : item.Sub.Grade;
                item.DisplayedGrade = grade;

                if (grade > 0)
                {
                    totalUnits += item.Sub.Units;
                    totalWeightedGrades += grade * item.Sub.Units;
                }
            }

            StudentProfile.GWA = totalUnits > 0 ? Math.Round(totalWeightedGrades / totalUnits, 2) : 0;
        }
    }
}