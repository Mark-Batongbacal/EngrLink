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
        public ObservableCollection<Faculty> FacultySchedules { get; set; } = new ObservableCollection<Faculty>();

        public Schedules()
        {
            this.InitializeComponent();
            this.DataContext = this;
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
                Debug.WriteLine("Attempting to fetch all faculty members from Supabase...");
                var facultyResponse = await client
                    .From<Faculty>()
                    .Get();

                if (facultyResponse.Models != null && facultyResponse.Models.Any())
                {
                    Debug.WriteLine($"Successfully fetched {facultyResponse.Models.Count} faculty members.");

                    FacultySchedules.Clear();

                    foreach (var faculty in facultyResponse.Models)
                    {
                        Debug.WriteLine($"Processing faculty: Name='{faculty.Name}', ProfCode='{faculty.ProfCode}' (ID: {faculty.Id})");

                        if (string.IsNullOrEmpty(faculty.ProfCode))
                        {
                            Debug.WriteLine($"Warning: Skipping schedule fetch for faculty '{faculty.Name}' due to empty or null ProfCode.");
                            continue;
                        }

                        try
                        {
                            Debug.WriteLine($"Fetching schedules for ProfCode: '{faculty.ProfCode}'...");
                            var scheduleResponse = await client
                                .From<IndivSubject>()
                                .Filter("profcode", Supabase.Postgrest.Constants.Operator.Equals, faculty.ProfCode)
                                .Get();

                            if (scheduleResponse.Models != null && scheduleResponse.Models.Any())
                            {
                                Debug.WriteLine($"Found {scheduleResponse.Models.Count} schedules for ProfCode: '{faculty.ProfCode}'.");

                                // Ensure all schedules are processed and added
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
                                Debug.WriteLine($"No schedules found for ProfCode: '{faculty.ProfCode}'.");
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
                    Debug.WriteLine("No faculty members found in the 'Faculty' table. Please check your database.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Critical Error loading faculty schedules: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private string ExtractDayFromSchedule(string schedule)
        {
            if (!string.IsNullOrEmpty(schedule))
            {
                var parts = schedule.Split(' ');
                return parts.Length > 0 ? parts[0] : "Unknown_Day";
            }
            return "Unknown_Day";
        }

        private string ExtractTimeFromSchedule(string schedule)
        {
            if (!string.IsNullOrEmpty(schedule))
            {
                var parts = schedule.Split(' ', 2);
                return parts.Length > 1 ? parts[1] : "Unknown_Time";
            }
            return "Unknown_Time";
        }

        private void ViewScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Faculty faculty)
            {
                Debug.WriteLine($"Viewing schedule for: {faculty.Name} (Code: {faculty.ProfCode})");

                // Navigate to the ViewSchedulePage and pass the selected faculty
                Frame.Navigate(typeof(ViewSchedulePage), faculty);
            }
        }

        private void ScrollToTopButton_Click(object sender, RoutedEventArgs e)
        {
            SchedulesScrollViewer.ChangeView(null, 0, null);
        }

        private void ScrollToBottomButton_Click(object sender, RoutedEventArgs e)
        {
            SchedulesScrollViewer.ChangeView(null, SchedulesScrollViewer.ScrollableHeight, null);
        }
    }
}
