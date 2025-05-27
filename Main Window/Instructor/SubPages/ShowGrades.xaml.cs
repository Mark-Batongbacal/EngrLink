using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace EngrLink.Main_Window.Instructor.SubPages;

public sealed partial class ShowGrades : Page
{
    public class StudentProfileModel : INotifyPropertyChanged
    {
        private string _name;
        private string _program;
        private string _id;
        private string _year;
        private double _gwa;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public string Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }
        public string Program { get => _program; set { _program = value; OnPropertyChanged(nameof(Program)); } }
        public string Year { get => _year; set { _year = value; OnPropertyChanged(nameof(Year)); } }
        public double GWA { get => _gwa; set { _gwa = value; OnPropertyChanged(nameof(GWA)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public StudentProfileModel StudentProfile { get; set; } = new();
    public List<IndivSubjectView> SubjectViews { get; set; } = new();

    public bool CanEditGrades { get; set; }


    public ShowGrades()
    {
        InitializeComponent();
        this.DataContext = this;
    }

    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is (int studentId, string profCode))
        {

            CanEditGrades = SubjectViews.Any() && SubjectViews.First().Sub.ProfCode == profCode;
           
            Debug.WriteLine($"Navigated with Student ID: {studentId}");
            var client = App.SupabaseClient;

            // Get grades
            var gradesResponse = await client
                .From<IndivSubject>()
                .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, studentId)
                .Get();

            SubjectViews = gradesResponse.Models
            .Select(sub => new IndivSubjectView
            {
                Sub = sub,
                IsEditable = sub.ProfCode == profCode   
            })
            .ToList();

            StudentsListView.ItemsSource = SubjectViews;

            // Get student info
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
                StudentProfile.Id = student.Id.ToString();

                Debug.WriteLine(StudentProfile.Name);
                Debug.WriteLine(StudentProfile.Program);
                Debug.WriteLine(StudentProfile.Year);
            }

            RecalculateGWA();
        }
    }

    private void RecalculateGWA()
    {
        double totalUnits = 0;
        double totalWeightedGrades = 0;

        foreach (var view in SubjectViews)
        {
            var grade = view.Sub.Grade;
            if (grade > 0)
            {
                totalUnits += view.Sub.Units;
                totalWeightedGrades += grade * view.Sub.Units;
            }
        }

        StudentProfile.GWA = totalUnits > 0
            ? Math.Round(totalWeightedGrades / totalUnits, 2)
            : 0;
    }

    private async void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        var client = App.SupabaseClient;
        bool hasError = false;
        var button = sender as Button;

        button.IsEnabled = false;
        foreach (var view in SubjectViews)
        {
            try
            {
                await client
                    .From<IndivSubject>()
                    .Where(x => x.Eme == view.Sub.Eme)
                    .Set(x => x.Grade, view.Sub.Grade)
                    .Update();

                Debug.WriteLine($"Updated subject {view.Sub.Subject} with grade {view.Sub.Grade}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating subject {view.Sub.Subject}: {ex.Message}");
                hasError = true;
            }
        }

        RecalculateGWA();

        var dialog = new ContentDialog
        {
            Title = hasError ? "Some Errors Occurred" : "Grades Saved Successfully",
            Content = hasError
                ? "Some grades may not have been saved. Please check the debug output or try again."
                : "All grades have been successfully saved to the database.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
        button.IsEnabled = true;
    }
}
