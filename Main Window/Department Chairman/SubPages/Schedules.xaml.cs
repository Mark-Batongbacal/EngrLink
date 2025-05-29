using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Supabase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EngrLink.Main_Window.Department_Chairman.SubPages
{
    public sealed partial class Schedules : Page
    {
        public string Program { get; set; }
        public ObservableCollection<Faculty> FacultySchedules { get; set; } = new ObservableCollection<Faculty>();

        public Schedules()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string program)
            {
                Debug.WriteLine($"Navigated with Program: {program}");
                this.Program = program;
            }
            _ = LoadFacultySchedules();
        }

        private async Task LoadFacultySchedules()
        {
            var client = App.SupabaseClient;

            if (client == null)
            {
                Debug.WriteLine("Error: SupabaseClient is not initialized. Please ensure App.SupabaseClient is set up correctly in your App.xaml.cs or startup code.");
                return;
            }

            try
            {
                Debug.WriteLine("Attempting to fetch faculty members for the current program...");

                var facultyResponse = await client
                    .From<Faculty>()
                    .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, this.Program)
                    .Order("profcode", ordering: Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                if (facultyResponse.Models != null && facultyResponse.Models.Any())
                {
                    FacultySchedules.Clear();

                    foreach (var faculty in facultyResponse.Models)
                    {
                        if (string.IsNullOrEmpty(faculty.ProfCode))
                        {
                            Debug.WriteLine($"Warning: Skipping schedule fetch for faculty '{faculty.Name}' due to empty or null ProfCode.");
                            continue;
                        }

                        try
                        {
                            Debug.WriteLine($"Fetching schedules for ProfCode: '{faculty.ProfCode}'...");
                            var scheduleResponse = await client
                                .From<Subjects>()
                                .Filter("profcode", Supabase.Postgrest.Constants.Operator.Equals, faculty.ProfCode)
                                .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, this.Program)
                                .Order("year", ordering: Supabase.Postgrest.Constants.Ordering.Ascending)
                                .Get();

                            if (scheduleResponse.Models != null && scheduleResponse.Models.Any())
                            {
                                Debug.WriteLine($"Found {scheduleResponse.Models.Count} schedules for ProfCode: '{faculty.ProfCode}'.");

                                faculty.ScheduleDetails = scheduleResponse.Models
                                    .Where(subject => !string.IsNullOrEmpty(subject.Schedule))
                                    .Select(subject => new ScheduleDetail
                                    {
                                        Day = ExtractDayFromSchedule(subject.Schedule),
                                        Time = ExtractTimeFromSchedule(subject.Schedule),
                                        Subject = subject.Subject
                                    })
                                    .ToList();
                            }
                            else
                            {
                                Debug.WriteLine($"No schedules found for ProfCode: '{faculty.ProfCode}' in program '{this.Program}'.");
                                faculty.ScheduleDetails = new List<ScheduleDetail>();
                            }
                        }
                        catch (Exception scheduleEx)
                        {
                            Debug.WriteLine($"Error fetching schedules for ProfCode '{faculty.ProfCode}': {scheduleEx.Message}");
                            Debug.WriteLine(scheduleEx.StackTrace);
                            faculty.ScheduleDetails = new List<ScheduleDetail>();
                        }

                        FacultySchedules.Add(faculty);
                    }

                    Debug.WriteLine($"Finished loading faculty schedules. Total faculty with schedules/details processed: {FacultySchedules.Count}.");
                }
                else
                {
                    Debug.WriteLine($"No faculty members found in the '{this.Program}' program. Please check your database.");
                }
            }
            catch (Exception ex)
            {
                Frame.Navigate(typeof(ErrorPage), (typeof(Dashboard), this.Program, ""));
            }
        }

        private string ExtractDayFromSchedule(string schedule)
        {
            if (!string.IsNullOrEmpty(schedule))
            {
                var parts = schedule.Split(' ', 1);
                return parts.Length > 0 ? parts[0] : "Unknown_Day";
            }
            return "Unknown_Day";
        }

        private string ExtractTimeFromSchedule(string schedule)
        {
            if (!string.IsNullOrEmpty(schedule))
            {
                var parts = schedule.Split(' ', 2);
                if (parts.Length > 1)
                {
                    return $"{parts[0]} {parts[1]}"; 
                }
                return parts[0];
            }
            return "Unknown_Time";
        }


        private void ViewScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Faculty faculty)
            {
                Debug.WriteLine($"Viewing schedule for: {faculty.Name} (Code: {faculty.ProfCode})");

                Frame.Navigate(typeof(ViewSchedulePage), faculty);

                button.IsEnabled = false;
            }
        }
    }
}
