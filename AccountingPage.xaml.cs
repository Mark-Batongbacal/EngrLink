using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase.Postgrest;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace EngrLink
{
    public sealed partial class AccountingPage : Page
    {
        private Student currentStudent;
        private int remainingBalance;

        public AccountingPage()
        {
            this.InitializeComponent();
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
                var lastStudentResponse = await App.SupabaseClient
                    .From<Student>()
                    .Order("id", Ordering.Descending)
                    .Limit(1)
                    .Get();

                var student = lastStudentResponse.Models.FirstOrDefault();
                if (student != null)
                {
                    NameText.Text = student.Name;
                    ProgramText.Text = student.Program;
                    YearText.Text = student.Year;
                    TotalFeesText.Text = student.Total.ToString("N0");
                }
                else
                {
                    NameText.Text = "No record found";
                    ProgramText.Text = "--";
                    YearText.Text = "--";
                    TotalFeesText.Text = "--";
                }
            }
            catch (Exception ex)
            {
                var dlg = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dlg.ShowAsync();
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(StudentIdInput.Text, out var id))
                return;

            try
            {
                var resp = await App.SupabaseClient
                    .From<Student>()
                    .Filter("id", Operator.Equals, id)
                    .Get();

                currentStudent = resp.Models.FirstOrDefault();
                if (currentStudent != null)
                {
                    InfoPanel.Visibility = Visibility.Visible;
                    S_TotalFeesText.Text = currentStudent.Total.ToString("N0");
                    remainingBalance = currentStudent.Total;
                    RemainingBalanceText.Text = remainingBalance.ToString("N0");
                }
                else
                {
                    InfoPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                var dlg = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dlg.ShowAsync();
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
            // await App.SupabaseClient.From<Student>().Update(currentStudent);
        }
    }
}
