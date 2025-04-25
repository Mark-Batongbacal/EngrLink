using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase;
using Supabase.Postgrest;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace EngrLink
{
    public sealed partial class FacultyPage : Page
    {
        private Supabase.Client supabaseClient;
        private Faculty currentFaculty;

        public FacultyPage()
        {
            this.InitializeComponent();
            _ = InitializeSupabaseAsync();
        }

        private async Task InitializeSupabaseAsync()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            supabaseClient = new Supabase.Client(
                "https://dpouedmzpftnpodbopbi.supabase.co",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRwb3VlZG16cGZ0bnBvZGJvcGJpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDUwNDI2ODMsImV4cCI6MjA2MDYxODY4M30.XeZs98NROksWaNqE_q1HrgdxTLZ-Wmogwz4bWi4d_6s",
                options);

            await supabaseClient.InitializeAsync();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(FacultyIdInput.Text, out var facultyId))
            {
                await ShowDialogAsync("Please enter a valid faculty ID.");
                return;
            }

            try
            {
                var response = await supabaseClient
                    .From<Faculty>()
                    .Filter("id", Operator.Equals, facultyId)
                    .Get();

                if (response != null && response.Models != null && response.Models.Any())
                {
                    currentFaculty = response.Models.First();
                    NameText.Text = currentFaculty.Name;
                    DepartmentText.Text = currentFaculty.Department;
                    FacultyInfoPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    FacultyInfoPanel.Visibility = Visibility.Collapsed;
                    await ShowDialogAsync("Faculty not found.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Error fetching faculty: " + ex.Message);
            }
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for navigation
            // this.Frame.Navigate(typeof(SchedulePage));
            _ = ShowDialogAsync("Schedule page not yet available.");
        }

        private void ListOfStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for navigation
            // this.Frame.Navigate(typeof(ListOfStudentsPage));
            _ = ShowDialogAsync("List of Students page not yet available.");
        }

        private async Task ShowDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Info",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
