using EngrLink.Models; // Make sure to include your models namespace
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq; // For Any()
using static Supabase.Postgrest.Constants; // Add this import
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace EngrLink.Main_Window.Students.SubPages
{
    public sealed partial class Dashboard : Page
    {
        public string Program { get; set; }
        public ObservableCollection<Announcement> StudentAnnouncements { get; set; } = new ObservableCollection<Announcement>();

        public Dashboard()
        {
            this.InitializeComponent();
            this.DataContext = this; 
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string program)
            {
                this.Program = program;
                Debug.Write(program);
            }
            LoadStudentAnnouncements();
        }

        private async void LoadStudentAnnouncements()
        {
            Debug.Write(this.Program);
            var client = App.SupabaseClient;

            try
            {
                
                // Fetch all announcements
                var response = await client
                    .From<Announcement>()
                    .Filter("program", Operator.Equals, this.Program)
                    .Get();

                    if (response.Models != null && response.Models.Any())
                {
                    StudentAnnouncements.Clear();

                    // Filter announcements for students
                    foreach (var announcement in response.Models)
                    {
                        if (announcement.ForStud)
                        {
                            StudentAnnouncements.Add(announcement);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Loaded {StudentAnnouncements.Count} announcements for students.");
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