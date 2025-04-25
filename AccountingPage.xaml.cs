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
    public sealed partial class AccountingPage : Page
    {
        private Supabase.Client supabaseClient;
        private Student currentStudent;

        public AccountingPage()
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
            if (!int.TryParse(StudentIdInput.Text, out var studentId))
            {
                await ShowDialogAsync("Please enter a valid student ID.");
                return;
            }

            try
            {
                var response = await supabaseClient
                    .From<Student>()
                    .Filter("id", Operator.Equals, studentId)
                    .Get();

                if (response != null && response.Models != null && response.Models.Any())
                {
                    var student = response.Models.FirstOrDefault();
                    if (student != null)
                    {
                        currentStudent = student;
                        NameText.Text = student.Name;
                        ProgramText.Text = student.Program;
                        YearText.Text = student.Year;
                        TotalFeesText.Text = student.Total?.ToString("N0") ?? "?0";

                        InfoPanel.Visibility = Visibility.Visible;
                        S_TotalFeesText.Text = student.Total?.ToString("N0") ?? "?0";
                        RemainingBalanceText.Text = student.Fees?.ToString("N0");
                    }
                    else
                    {
                        currentStudent = null;
                        NameText.Text = "--";
                        ProgramText.Text = "--";
                        YearText.Text = "--";
                        TotalFeesText.Text = "--";
                        InfoPanel.Visibility = Visibility.Collapsed;
                        await ShowDialogAsync("Student not found.");
                    }
                }
                else
                {
                    await ShowDialogAsync($"Search error: Student not found.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Search error: " + ex.Message);
            }
        }

        private async void UpdateStudentInfo_Click(object sender, RoutedEventArgs e)
        {
            if (currentStudent == null)
            {
                await ShowDialogAsync("No student loaded to update.");
                return;
            }

            try
            {
                var updatedStudent = new Student
                {
                    Id = currentStudent.Id,
                    Name = NameText.Text,
                    Program = ProgramText.Text,
                    Year = YearText.Text
                };

                var updateResponse = await supabaseClient
                    .From<Student>()
                    .Where(x => x.Id == updatedStudent.Id)
                    .Update(updatedStudent);

                if (updateResponse != null && updateResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    await ShowDialogAsync("Student information updated successfully!");

                    var getResponse = await supabaseClient
                        .From<Student>()
                        .Where(x => x.Id == currentStudent.Id)
                        .Get();

                    if (getResponse != null && getResponse.Models != null && getResponse.Models.Any())
                    {
                        var refreshedStudent = getResponse.Models.FirstOrDefault();
                        if (refreshedStudent != null)
                        {
                            currentStudent = refreshedStudent;
                            NameText.Text = currentStudent.Name;
                            ProgramText.Text = currentStudent.Program;
                            YearText.Text = currentStudent.Year;
                        }
                    }
                }
                else
                {
                    string errorContent = await updateResponse?.ResponseMessage?.Content?.ReadAsStringAsync();
                    await ShowDialogAsync($"Failed to update student information: {updateResponse?.ResponseMessage?.ReasonPhrase} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Update error: " + ex.Message);
            }
        }

        private async void SubmitPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStudent == null)
            {
                await ShowDialogAsync("Search for a student first.");
                return;
            }

            if (!int.TryParse(AmountPaidInput.Text, out int amountPaid) || amountPaid <= 0)
            {
                await ShowDialogAsync("Please enter a valid amount.");
                return;
            }

            if (amountPaid > currentStudent.Fees)
            {
                await ShowDialogAsync("Payment exceeds the remaining balance.");
                return;
            }

            try
            {
                currentStudent.Fees -= amountPaid;

                var updateResponse = await supabaseClient
                    .From<Student>()
                    .Where(x => x.Id == currentStudent.Id)
                    .Update(currentStudent); // Pass the entire updated currentStudent object

                if (updateResponse != null && updateResponse.ResponseMessage.IsSuccessStatusCode)
                {
                    RemainingBalanceText.Text = currentStudent.Fees?.ToString("N0");
                    AmountPaidInput.Text = string.Empty;
                    await ShowDialogAsync($"Payment successful. Remaining balance: ?{currentStudent.Fees:N0}");

                    var getResponse = await supabaseClient
                        .From<Student>()
                        .Where(x => x.Id == currentStudent.Id)
                        .Get();

                    if (getResponse != null && getResponse.Models != null && getResponse.Models.Any())
                    {
                        currentStudent = getResponse.Models.FirstOrDefault();
                        // Optionally update other displayed fields if needed
                    }
                }
                else
                {
                    string errorContent = await updateResponse?.ResponseMessage?.Content?.ReadAsStringAsync();
                    await ShowDialogAsync($"Failed to update payment: {updateResponse?.ResponseMessage?.ReasonPhrase} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Failed to update payment: " + ex.Message);
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