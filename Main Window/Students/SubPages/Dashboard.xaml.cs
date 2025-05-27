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
using Windows.Devices.AllJoyn;
using System.ComponentModel;

namespace EngrLink.Main_Window.Students.SubPages
{
    public sealed partial class Dashboard : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
        private string _program;
        public string Program
        {
            get => _program;
            set
            {
                if (_program != value)
                {
                    _program = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Program)));
                }
            }
        }

        public ObservableCollection<Announcement> StudentAnnouncements { get; set; } = new ObservableCollection<Announcement>();

        public Dashboard()
        {
            this.InitializeComponent();
            this.DataContext = this; 
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is (string program, string id))
            {
                this.Program = program;

                var response = await App.SupabaseClient
                    .From<Student>()
                    .Filter("id", Operator.Equals, id)
                    .Get();

                this.Name = $"Welcome {response.Models.FirstOrDefault()?.Name ?? "Unknown Student"}!";
                
            }
            LoadStudentAnnouncements();
        }

        private async void LoadStudentAnnouncements()
        {
            var client = App.SupabaseClient;
            Debug.Write($"This is your name and Program{this.Name}, {this.Program}");
            try
            {
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