using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using EngrLink.Models; // Make sure to include your models namespace
using Supabase;
using Supabase.Postgrest.Models;
using System.Linq; // For Any()
using Supabase.Postgrest;
using static Supabase.Postgrest.Constants; // Add this import

namespace EngrLink.Main_Window.Instructor.SubPages
{
    public sealed partial class Dashboard : Page
    {
        public ObservableCollection<Announcement> FacultyAnnouncements { get; set; } = new ObservableCollection<Announcement>();

        public Dashboard()
        {
            this.InitializeComponent();
            this.DataContext = this;
            LoadStudentAnnouncements();
        }

        private async void LoadStudentAnnouncements()
        {
            var client = App.SupabaseClient;

            try
            {
                // Fetch all announcements
                var response = await client
                    .From<Announcement>()
                    .Get();

                if (response.Models != null && response.Models.Any())
                {
                    FacultyAnnouncements.Clear();

                    // Filter announcements for students
                    foreach (var announcement in response.Models)
                    {
                        if (announcement.ForFac)
                        {
                            FacultyAnnouncements.Add(announcement);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Loaded {FacultyAnnouncements.Count} announcements for students.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No announcements found.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading student announcements: {ex.Message}");
            }
        }
    }
}