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

namespace EngrLink.Main_Window.Department_Chairman.SubPages
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
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Program { get; set; }
        public string Trash { get; set; }

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
            Debug.WriteLine($"Chairman Dashboard: Loaded {ImageSources.Count} images for FlipView.");

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
                this.Trash = parameterTuple.Item2; 
                Debug.WriteLine($"Chairman Dashboard: Navigated with Program: {this.Program}");

                try
                {
                    var client = App.SupabaseClient;
                    var response = await client
                        .From<DeptChair>()
                        .Filter("program", Operator.Equals, this.Program)
                        .Get();

                    var chair = response.Models.FirstOrDefault();

                    if (chair != null)
                    {
                        if (this.Program == "ARCHI")
                        {
                            this.Name = $"Welcome, Architect {chair.Name}!";
                            Debug.WriteLine($"Chairman Dashboard: Fetched Chairman Name: {chair.Name}");
                        }
                        else if (this.Program == "CPE")
                        {
                            this.Name = $"Welcome, Doctor {chair.Name}!";
                            Debug.WriteLine($"Chairman Dashboard: Fetched Chairman Name: {chair.Name}");
                        }
                        else
                        {
                            this.Name = $"Welcome, Engineer {chair.Name}!";
                            Debug.WriteLine($"Chairman Dashboard: Fetched Chairman Name: {chair.Name}");
                        }
                    }
                    else
                    {
                        this.Name = $"Welcome, Chairman of {this.Program}!";
                        Debug.WriteLine($"Chairman Dashboard: DeptChair not found for Program: {this.Program}");
                    }
                }
                catch (Exception ex)
                {
                    this.Name = $"Welcome, Chairman of {this.Program}!";
                    Debug.WriteLine($"Chairman Dashboard: Error fetching DeptChair name: {ex.Message}");
                }
            }
            else
            {
                this.Name = "Welcome, Department Chairman!";
                this.Program = "N/A";
                Debug.WriteLine("Chairman Dashboard: Navigated without specific program info.");
            }

            if (ImageSources.Any())
            {
                _autoFlipTimer.Start();
                Debug.WriteLine("Chairman Dashboard: FlipView timer started.");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _autoFlipTimer.Stop();
            Debug.WriteLine("Chairman Dashboard: FlipView timer stopped.");
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

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}