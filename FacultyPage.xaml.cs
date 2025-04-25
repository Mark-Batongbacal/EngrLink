using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase;
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
                await ShowDialogAsync("Please enter a valid Faculty ID.");
                return;
            }

            try
            {
                var response = await supabaseClient
                    .From<Faculty>()
                    .Filter("id", Operator.Equals, facultyId)
                    .Get();

                if (response != null && response.Models.Any())
                {
                    currentFaculty = response.Models.First();
                    NameText.Text = currentFaculty.Name;
                    DepartmentText.Text = currentFaculty.Department;
                    PositionText.Text = currentFaculty.Position;
                    SalaryText.Text = $"?{currentFaculty.Salary:N0}";
                    InfoCard.Visibility = Visibility.Visible;
                }
                else
                {
                    await ShowDialogAsync("Faculty not found.");
                    InfoCard.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Search error: " + ex.Message);
            }
        }

        private async Task ShowDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Notification",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
