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
                    var faculty = response.Models.FirstOrDefault();
                    if (faculty != null)
                    {
                        currentFaculty = faculty;
                        NameText.Text = faculty.Name;
                        DepartmentText.Text = faculty.Department;
                        PositionText.Text = faculty.Position;
                        SalaryText.Text = faculty.Salary?.ToString("N0") ?? "?0";

                        InfoPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ResetFields();
                        await ShowDialogAsync("Faculty not found.");
                    }
                }
                else
                {
                    await ShowDialogAsync("Search error: Faculty not found.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Search error: " + ex.Message);
            }
        }

        private async void UpdateSalaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFaculty == null)
            {
                await ShowDialogAsync("No faculty loaded to update.");
                return;
            }

            if (!decimal.TryParse(NewSalaryInput.Text, out var newSalary) || newSalary <= 0)
            {
                await ShowDialogAsync("Please enter a valid salary amount.");
                return;
            }

            try
            {
                currentFaculty.Salary = newSalary;

                var updateResponse = await supabaseClient
                    .From<Faculty>()
                    .Where(x => x.Id == currentFaculty.Id)
                    .Update(currentFaculty);

                if (updateResponse != null && updateResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    SalaryText.Text = currentFaculty.Salary?.ToString("N0");
                    NewSalaryInput.Text = string.Empty;
                    await ShowDialogAsync("Salary updated successfully!");
                }
                else
                {
                    string errorContent = await updateResponse?.ResponseMessage?.Content?.ReadAsStringAsync();
                    await ShowDialogAsync($"Failed to update salary: {updateResponse?.ResponseMessage?.ReasonPhrase} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Update error: " + ex.Message);
            }
        }

        private void ResetFields()
        {
            currentFaculty = null;
            NameText.Text = "--";
            DepartmentText.Text = "--";
            PositionText.Text = "--";
            SalaryText.Text = "--";
            InfoPanel.Visibility = Visibility.Collapsed;
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
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
