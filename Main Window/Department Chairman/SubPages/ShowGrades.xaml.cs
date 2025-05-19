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

namespace EngrLink.Main_Window.Department_Chairman.SubPages;

public sealed partial class ShowGrades : Page
{
   
    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        int Id = 0;
        base.OnNavigatedTo(e);
        
        if (e.Parameter is int studentId)
        {
            // Now you have the ID and can load student grades
            Debug.WriteLine($"Navigated with Student ID: {studentId}");
            Id = studentId;

            var client = App.SupabaseClient;
            //tite
            // Only get students where Enrolled is false
            var response = await client
                .From<Subjects>()
                .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, Id)
                .Get();

            StudentsListView.ItemsSource = response.Models;
        }
    }


    public ShowGrades()
    {
        InitializeComponent();
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
