using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using static Supabase.Postgrest.Constants;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.ComponentModel;
using Supabase.Postgrest.Interfaces;

namespace EngrLink.Main_Window.Instructor.SubPages
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

        public ObservableCollection<string> ImageSources { get; set; }

        public ObservableCollection<Announcement> StudentAnnouncements { get; set; } = new ObservableCollection<Announcement>();

        private DispatcherTimer _autoFlipTimer;

        public Dashboard()
        {
            this.InitializeComponent();
            this.DataContext = this;

            ImageSources = new ObservableCollection<string>
            {
                "ms-appx:///Assets/carousel_image1.png",
                "ms-appx:///Assets/carousel_image2.png",
                "ms-appx:///Assets/carousel_image3.png"
            };

            _autoFlipTimer = new DispatcherTimer();
            _autoFlipTimer.Interval = TimeSpan.FromSeconds(5);
            _autoFlipTimer.Tick += AutoFlipTimer_Tick;

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<string, string> parameterTuple)
            {
                this.Program = parameterTuple.Item1;
                string name = parameterTuple.Item2;

                this.Name = $"Welcome {name ?? "Unknown Student"}!";
            }
            else
            {
                this.Name = "Welcome";
                this.Program = "N/A";
                Debug.WriteLine("Dashboard loaded without specific student info.");
            }

            LoadFacultyAnnouncements();

            if (ImageSources.Any())
            {
                _autoFlipTimer.Start();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _autoFlipTimer.Stop();
        }

        private void AutoFlipTimer_Tick(object sender, object e)
        {
            if (ImageSources.Any())
            {
                int currentIndex = DashboardFlipView.SelectedIndex;
                int nextIndex = (currentIndex + 1) % ImageSources.Count;

                DashboardFlipView.SelectedIndex = nextIndex;
            }
        }

        private async void LoadFacultyAnnouncements()
        {
            var client = App.SupabaseClient;
            Debug.WriteLine($"Loading announcements for Program: {this.Program}");

            try
            {
                // Fix: Initialize query as IPostgrestTable<Announcement>
                // This ensures the type remains consistent throughout the query building.
                var query = client.From<Announcement>() as IPostgrestTable<Announcement>;

                if (this.Program != "N/A")
                {
                    query = query.Filter("program", Operator.Equals, this.Program);
                }

                // Now call .Get() on the IPostgrestTable type
                var response = await query.Get();

                if (response?.Models != null && response.Models.Any())
                {
                    StudentAnnouncements.Clear();

                    foreach (var announcement in response.Models)
                    {
                        if (announcement.ForFac) // Ensure we only add announcements for faculty
                        {
                            StudentAnnouncements.Add(announcement);
                        }
                    }

                    Debug.WriteLine($"Loaded {StudentAnnouncements.Count} announcements for faculty in program {this.Program}.");
                }
                else
                {
                    Debug.WriteLine($"No announcements found for program {this.Program}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading announcements: {ex.Message}");
            }
        }
    }
}