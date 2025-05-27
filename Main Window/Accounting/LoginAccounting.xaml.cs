using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using EngrLink.Models;
using EngrLink.Main_Window.Students;
using Supabase.Interfaces;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using EngrLink.Main_Window.Accounting;

namespace EngrLink.Main_Window.Accounting
{
    public sealed partial class LoginAccounting : Page
    {
        public LoginAccounting()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void CheckValid()
        {
            bool isValid = !string.IsNullOrWhiteSpace(AccID.Text) &&
                           !string.IsNullOrWhiteSpace(Password.Password);
            SubmitButton.IsEnabled = isValid;
        }

        private void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            CheckValid();
        }


        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string accid = AccID.Text.Trim();
            string password = Password.Password.Trim();

            try
            {
                var response = await App.SupabaseClient
                    .From<Faculty>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, accid)
                    .Get();

                var ac = response.Models.FirstOrDefault();
                Debug.WriteLine($"Returned rows: {response.Models.Count}");


                if (ac != null && ac.Password == password && ac.Id == 100)
                {
                    await ShowDialog("Login Successful", "Welcome back!");
                    Frame.Navigate(typeof(AccountingPage), ac.Program);
                }

                else
                {
                    ContentDialog failedDialog = new ContentDialog
                    {
                        Title = "Login Failed",
                        Content = "Invalid Faculty ID or Password.",
                        CloseButtonText = "Try Again",
                        XamlRoot = this.XamlRoot
                    };
                    await failedDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Something went wrong: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async Task ShowDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
