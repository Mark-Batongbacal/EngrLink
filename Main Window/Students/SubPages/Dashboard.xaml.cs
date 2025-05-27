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
                string studentId = parameterTuple.Item2;

                var response = await App.SupabaseClient
                    .From<Student>()
                    .Filter("id", Operator.Equals, studentId)
                    .Get();

                this.Name = $"Welcome {response.Models.FirstOrDefault()?.Name ?? "Unknown Student"}!";
                Debug.WriteLine($"Dashboard loaded for student: {this.Name}, Program: {this.Program}, ID: {studentId}");
            }
            else
            {
                this.Name = "Welcome";
                this.Program = "N/A";
                Debug.WriteLine("Dashboard loaded without specific student info.");
            }

            LoadStudentAnnouncements();

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

        private async void LoadStudentAnnouncements()
        {
            var client = App.SupabaseClient;
            Debug.WriteLine($"Loading announcements for Program: {this.Program}");

            try
            {
                var response = await client
                    .From<Announcement>()
                    .Filter("program", Operator.Equals, this.Program)
                    .Get();

                if (response?.Models != null && response.Models.Any())
                {
                    StudentAnnouncements.Clear();

                    foreach (var announcement in response.Models)
                    {
                        if (announcement.ForStud)
                        {
                            StudentAnnouncements.Add(announcement);
                        }
                    }
                    Debug.WriteLine($"Loaded {StudentAnnouncements.Count} announcements for students in program {this.Program}.");
                }
                else
                {
                    Debug.WriteLine($"No announcements found for program {this.Program}.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading student announcements: {ex.Message}");
            }
        }
    }
}