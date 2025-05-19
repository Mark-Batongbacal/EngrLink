using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.ComponentModel;

namespace EngrLink.Main_Window.Department_Chairman.SubPages;

public sealed partial class ShowGrades : Page
{
    public class StudentProfileModel : INotifyPropertyChanged
    {
        private string _name;
        private string _program;
        private string _year;
        private double _gwa;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
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
    public StudentProfileModel StudentProfile { get; set; } = new StudentProfileModel();

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is int studentId)
        {
            Debug.WriteLine($"Navigated with Student ID: {studentId}");

            var client = App.SupabaseClient;

            // Get grades for the student
            var gradesResponse = await client
                .From<IndivSubjects>()
                .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, 231632846)
                .Get();

            StudentsListView.ItemsSource = gradesResponse.Models;

            // Get student profile
            var studentResponse = await client
                .From<Student>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
                .Get();

            var student = studentResponse.Models.FirstOrDefault();

            if (student != null)
            {
                StudentProfile.Name = student.Name;
                StudentProfile.Program = student.Program;
                StudentProfile.Year = student.Year;
                Debug.WriteLine(StudentProfile.Name);
                Debug.WriteLine(StudentProfile.Program);
                Debug.WriteLine(StudentProfile.Year);
            }
            var subjects = gradesResponse.Models;

            double totalUnits = 0;
            double totalWeightedGrades = 0;

            foreach (var subject in subjects)
            {
                // Assuming grades below 50 or 0 mean 'Not Yet Graded'
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
            this.DataContext = this;
        }
    }

    public ShowGrades()
    {
        InitializeComponent();
        this.DataContext = this;
    }
    
    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        var client = App.SupabaseClient;
        bool hasError = false;

        if (StudentsListView.ItemsSource is IEnumerable<Subjects> subjects)
        {
            foreach (var subject in subjects)
            {
                try
                {
                    await client
                        .From<Subjects>()
                        .Where(x => x.Id == subject.Id)
                        .Set(x => x.Grade, subject.Grade)
                        .Update();

                    Debug.WriteLine($"Updated subject {subject.Subject} with grade {subject.Grade}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating subject {subject.Subject}: {ex.Message}");
                    hasError = true;
                }
            }

            var dialog = new ContentDialog
            {
                Title = hasError ? "Some Errors Occurred" : "Grades Saved Successfully",
                Content = hasError
                    ? "Some grades may not have been saved. Please check the debug output or try again."
                    : "All grades have been successfully saved to the database.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot // Required for WinUI 3
            };

            await dialog.ShowAsync();
        }
    }


}
