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


namespace EngrLink.Main_Window.Department_Chairman.SubPages
{

    public sealed partial class ListOfStudents : Page
    {
        public string Program;

        public ListOfStudents()
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

            var response = await client
                .From<Student>()
                .Filter("enrolled", Supabase.Postgrest.Constants.Operator.Equals, "true")
                .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, this.Program)
                .Filter("id", Supabase.Postgrest.Constants.Operator.GreaterThan, 17)
                .Get();

            var studentViewModels = response.Models
                .Select(s => new StudentViewModel { Student2 = s })
                .ToList();

            StudentsListView.ItemsSource = studentViewModels;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();

            var button = sender as Button;
            button.IsEnabled = false;
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = button?.DataContext as StudentViewModel;

            if (viewModel?.Student2 != null)
            {
                int studentId = viewModel.Student2.Id;

                Debug.WriteLine($"Student ID: {studentId}");

                Frame.Content = null;
                Frame.Navigate(typeof(ShowGrades), studentId); 

                button.IsEnabled = false;
            }
        }

    }
}
