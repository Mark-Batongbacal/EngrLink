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
    public sealed partial class Enrollees : Page
    {

        public string Program;
        public Enrollees()
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
           
            try
            {
                var response = await client
                    .From<Student>()
                    .Filter("enrolled", Supabase.Postgrest.Constants.Operator.Equals, "false")
                    .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, this.Program)
                    .Filter("id", Supabase.Postgrest.Constants.Operator.GreaterThan, 17)
                    .Order("year", ordering: Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                var studentViewModels = response.Models
                    .Select(s => new StudentViewModel { Student2 = s })
                    .ToList();

                StudentsListView.ItemsSource = studentViewModels;
            }
            catch
            {
                Frame.Navigate(typeof(ErrorPage), (typeof(Dashboard), this.Program, ""));
            }
        }
        private async void StudentButton_Click(object sender, RoutedEventArgs e)
        {

            var button = sender as Button;
            var viewModel = button?.DataContext as StudentViewModel;
            button.IsEnabled = false;

            if (viewModel != null)
            {
                var student = viewModel.Student2;


                student.Enrolled = true;


                var client = App.SupabaseClient;
                var result = await client
                    .From<Student>()
                    .Where(s => s.Id == student.Id) 
                    .Update(student);


                var response = await client
                .From<Subjects>()
                .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, student.Program)
                .Filter("year", Supabase.Postgrest.Constants.Operator.Equals, student.Year)
                .Get();



                if (result.Models.Count > 0)
                {
                    Debug.WriteLine($"Successfully updated enrollment for {student.Name}.");
                    foreach (var subject in response.Models)
                    {

                        var indivSubject = new IndivSubject
                        {
                            Id = student.Id,
                            Code = subject.Code,
                            Subject = subject.Subject,
                            Program = subject.Program,
                            Remarks = subject.Remarks,
                            Year = subject.Year,
                            Schedule = subject.Schedule,
                            ProfCode = subject.ProfCode,
                            Units = subject.Units,
                            Grade = 0
                        };

                        await client
                            .From<IndivSubject>()
                            .Insert(indivSubject);
                    }
                }
                else
                {
                    Debug.WriteLine($"Failed to update enrollment for {student.Name}.");
                }
            }
            LoadStudents();
        }


    }
}
