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
        public string ProfCode;
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
                this.ProfCode = program;

            }
            LoadStudents();
        }

        private async void LoadStudents()
        {
            var client = App.SupabaseClient;

            var response = await client
                .From<Subjects>()
                .Filter("profcode", Supabase.Postgrest.Constants.Operator.Equals, this.ProfCode)
                .Get();
            if (response.Models is not null)
            {

                var distinctPairs = response.Models
                    .Select(s => new { Program = s.Program, Year = s.Year })
                    .Where(x => !string.IsNullOrEmpty(x.Program) && !string.IsNullOrEmpty(x.Year))
                    .Distinct()
                    .OrderBy(x => x.Program)
                    .ThenBy(x => x.Year)
                    .ToList();
                var allStudents = new List<StudentViewModel>();
                foreach (var pair in distinctPairs)
                {
                    var studResponse = await client
                        .From<Student>()
                        .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, pair.Program)
                        .Filter("year", Supabase.Postgrest.Constants.Operator.Equals, pair.Year)
                        .Filter("id", Supabase.Postgrest.Constants.Operator.GreaterThan, 17)
                        .Get();

                    if (studResponse.Models != null)
                    {
                        var studentViewModels = studResponse.Models
                            .Select(s => new StudentViewModel
                            {
                                Student2 = s
                            });

                        allStudents.AddRange(studentViewModels);

                        Debug.WriteLine($"{pair.Program} - {pair.Year}: {string.Join(", ", studResponse.Models.Select(s => s.Id))}");
                    }
                }
                StudentsListView.ItemsSource = allStudents;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            var viewModel = button?.DataContext as StudentViewModel;

            if (viewModel?.Student2 != null)
            {
                int studentId = viewModel.Student2.Id;

                Frame.Content = null;
                Frame.Navigate(typeof(ShowGrades), (studentId,this.ProfCode));
            }
        }

    }
}
