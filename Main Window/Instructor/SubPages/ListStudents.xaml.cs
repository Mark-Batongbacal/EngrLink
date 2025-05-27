using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace EngrLink.Main_Window.Instructor.SubPages
{
    public sealed partial class ListStudents : Page
    {
        public string Program;

        public ListStudents()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string program)
            {
                Debug.WriteLine($"Navigated with Department ID: {program}");
                this.Program = program;

            }
            LoadStudents();
        }

        private async void LoadStudents()
        {
            var client = App.SupabaseClient;

            // Only get students where Enrolled is false
            var response = await client
                .From<Subjects>()
                .Filter("profcode", Supabase.Postgrest.Constants.Operator.Equals, this.Program)
                .Get();
            if (response.Models is not null)
            {
                // Get distinct (Program, Year) pairs
                var distinctPairs = response.Models
                    .Select(s => new { Program = s.Program, Year = s.Year })
                    .Where(x => !string.IsNullOrEmpty(x.Program) && !string.IsNullOrEmpty(x.Year))
                    .Distinct()
                    .OrderBy(x => x.Program)
                    .ThenBy(x => x.Year)
                    .ToList();

                foreach (var pair in distinctPairs)
                {
                    var studResponse = await client
                        .From<Student>()
                        .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, pair.Program)
                        .Filter("year", Supabase.Postgrest.Constants.Operator.Equals, pair.Year)
                        .Filter("id", Supabase.Postgrest.Constants.Operator.GreaterThan, 17)
                        .Get();

                    // Get student IDs from the response
                    var studentIds = studResponse.Models
                        .Select(s => s.Id) // Assuming your Student class has an Id property
                        .ToList();

                    Debug.WriteLine($"{pair.Program} - {pair.Year}: {string.Join(", ", studentIds)}");
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button?.DataContext as StudentViewModel;

            if (viewModel?.Student2 != null)
            {
                int studentId = viewModel.Student2.Id;

                Debug.WriteLine($"Student ID: {studentId}"); // Output the integer

                Frame.Content = null;
                //Frame.Navigate(typeof(ShowGrades), studentId); // Pass the ID to the next page
            }
        }

    }
}
