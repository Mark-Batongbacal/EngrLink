using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using EngrLink.Models; // Make sure to include your models namespace
using Supabase;
using Supabase.Postgrest.Models;
using System.Linq; // For Any()
using Supabase.Postgrest;

namespace EngrLink.Main_Window.Students.SubPages
{
    public sealed partial class Dashboard : Page
    {
        public ObservableCollection<Announcement> StudentAnnouncements { get; set; } = new ObservableCollection<Announcement>();

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
                var response = await client
                    .From<Announcement>()
                    .Filter("student_announcements", Constants.Operator.Equals, true)
                    .Get();

                if (response.Models != null && response.Models.Any())
                {
                    StudentAnnouncements.Clear(); 
                    foreach (var announcement in response.Models)
                    {
                        StudentAnnouncements.Add(announcement);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No announcements");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading student announcements: {ex.Message}");
            }
        }
    }
}