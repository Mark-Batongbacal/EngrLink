using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EngrLink.Models;
using Supabase;
using System;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;

namespace EngrLink.Main_Window.Department_Chairman.SubPages
{
    public sealed partial class Schedules : Page
    {
        public ObservableCollection<Faculty> FacultySchedules { get; set; } = new ObservableCollection<Faculty>();

        public Schedules()
        {
            this.InitializeComponent();
            this.DataContext = this;
            LoadFacultySchedules();
        }

        private async void LoadFacultySchedules()
        {
            var client = App.SupabaseClient;

            try
            {
                // Fetch faculty info from Supabase
                var response = await client
                    .From<Faculty>()
                    .Get();

                if (response.Models != null && response.Models.Any())
                {
                    FacultySchedules.Clear();

                    foreach (var faculty in response.Models)
                    {
                        FacultySchedules.Add(faculty);
                    }

                    Debug.WriteLine($"Loaded {FacultySchedules.Count} faculty schedules.");
                }
                else
                {
                    Debug.WriteLine("No faculty schedules found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading faculty schedules: {ex.Message}");
            }
        }

        private void ViewScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Faculty faculty)
            {
                Debug.WriteLine($"Viewing schedule for: {faculty.Name} (Code: {faculty.ProfCode})");
                // Implement logic to display the schedule details
            }
        }

        private void Schedules_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
