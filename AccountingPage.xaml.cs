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
        private int remainingBalance;

        public AccountingPage()
        {
            this.InitializeComponent();
            _ = InitializeSupabaseAsync();
        }

        private async Task InitializeSupabaseAsync()
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            supabaseClient = new Supabase.Client(
                "https://dpouedmzpftnpodbopbi.supabase.co", // Replace with your Supabase URL
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRwb3VlZG16cGZ0bnBvZGJvcGJpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDUwNDI2ODMsImV4cCI6MjA2MDYxODY4M30.XeZs98NROksWaNqE_q1HrgdxTLZ-Wmogwz4bWi4d_6s",                    // Replace with your Supabase anon key
                options);

            await supabaseClient.InitializeAsync();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private async void LoadLatestStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                

var studentResponse = await supabaseClient
    .From<Student>()
    .Filter("id", Postgrest.Constants.Operator.Equals, "9")
    .Get();





                var student = lastStudentResponse.Models.FirstOrDefault();
                if (student != null)
                {
                    NameText.Text = student.Name;
                    ProgramText.Text = student.Program;
                    YearText.Text = student.Year;
                    TotalFeesText.Text = student.Total?.ToString("N0") ?? "0";
                }
                else
                {
                    await ShowDialogAsync("No student data found.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync(ex.Message);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(StudentIdInput.Text, out var id))
                return;

            try
            {
                var resp = await supabaseClient
                    .From<Student>()
                    .Filter("id", Operator.Equals, id)
                    .Get();

                currentStudent = resp.Models.FirstOrDefault();
                if (currentStudent != null)
                {
                    InfoPanel.Visibility = Visibility.Visible;
                    S_TotalFeesText.Text = currentStudent.Total?.ToString("N0") ?? "0";
                    remainingBalance = currentStudent.Total ?? 0;
                    RemainingBalanceText.Text = remainingBalance.ToString("N0");
                }
                else
                {
                    InfoPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync(ex.Message);
            }
        }

        private void SubmitPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentStudent == null ||
                !int.TryParse(AmountPaidInput.Text, out var paid))
                return;

            remainingBalance -= paid;
            if (remainingBalance < 0) remainingBalance = 0;

            RemainingBalanceText.Text = remainingBalance.ToString("N0");

            // Optionally update Supabase:
            // currentStudent.Total = remainingBalance;
            // await supabaseClient.From<Student>().Update(currentStudent);
        }

        private async Task ShowDialogAsync(string message)
        {
            var dlg = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dlg.ShowAsync();
        }
    }
}
