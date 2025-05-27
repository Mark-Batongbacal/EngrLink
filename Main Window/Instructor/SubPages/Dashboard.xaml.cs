using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using EngrLink.Models; 
using Supabase;
using Supabase.Postgrest.Models;
using System.Linq; 
using Supabase.Postgrest;
using static Supabase.Postgrest.Constants;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.ComponentModel;

namespace EngrLink.Main_Window.Instructor.SubPages
{
    public sealed partial class Dashboard : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Announcement> FacultyAnnouncements { get; set; } = new ObservableCollection<Announcement>();

        private ObservableCollection<string> _imageSources;
        public ObservableCollection<string> ImageSources
        {
            get => _imageSources;
            set
            {
                if (_imageSources != value)
                {
                    _imageSources = value;
                    OnPropertyChanged(nameof(ImageSources));
                }
            }
        }

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
            Debug.WriteLine($"Instructor Dashboard: Loaded {ImageSources.Count} images for FlipView.");

            _autoFlipTimer = new DispatcherTimer();
            _autoFlipTimer.Interval = TimeSpan.FromSeconds(5);
            _autoFlipTimer.Tick += AutoFlipTimer_Tick;

            LoadFacultyAnnouncements();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ImageSources.Any())
            {
                _autoFlipTimer.Start();
                Debug.WriteLine("Instructor Dashboard: FlipView timer started.");
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _autoFlipTimer.Stop();
            Debug.WriteLine("Instructor Dashboard: FlipView timer stopped.");
        }

        private void AutoFlipTimer_Tick(object sender, object e)
        {
            if (DashboardFlipView != null && ImageSources.Any())
            {
                int currentIndex = DashboardFlipView.SelectedIndex;
                int nextIndex = (currentIndex + 1) % ImageSources.Count;

                DashboardFlipView.SelectedIndex = nextIndex;
            }
        }

        private async void LoadFacultyAnnouncements()
        {
            var client = App.SupabaseClient;
            Debug.WriteLine("Instructor Dashboard: Attempting to load faculty announcements...");

            try
            {
                var response = await client
                    .From<Announcement>()
                    .Get();

                if (response?.Models != null && response.Models.Any())
                {
                    FacultyAnnouncements.Clear();

                    foreach (var announcement in response.Models)
                    {
                        if (announcement.ForFac)
                        {
                            FacultyAnnouncements.Add(announcement);
                        }
                    }
                    Debug.WriteLine($"Instructor Dashboard: Loaded {FacultyAnnouncements.Count} announcements for faculty.");
                }
                else
                {
                    Debug.WriteLine("Instructor Dashboard: No announcements found for faculty.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Instructor Dashboard: Error loading faculty announcements: {ex.Message}");
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}