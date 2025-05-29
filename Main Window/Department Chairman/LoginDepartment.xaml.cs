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

namespace EngrLink.Main_Window.Department_Chairman
{
    public sealed partial class LoginDepartment : Page
    {
        public LoginDepartment()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void CheckValid()
        {
            bool isValid = !string.IsNullOrWhiteSpace(DepID.Text) &&
                           !string.IsNullOrWhiteSpace(Password.Password);
            SubmitButton.IsEnabled = isValid;
        }

        private void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string currentText = textBox.Text;
                string newText = string.Empty;

                foreach (char c in currentText)
                {
                    if (char.IsDigit(c))
                    {
                        newText += c;
                    }
                }

                if (newText.Length > 5)
                {
                    newText = newText.Substring(0, 5);
                }

                if (textBox.Text != newText)
                {
                    textBox.TextChanged -= Input_TextChanged;
                    textBox.Text = newText;
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.TextChanged += Input_TextChanged;
                }
            }
            CheckValid();
        }


        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

            string depid = DepID.Text.Trim();
            string password = Password.Password.Trim();

            try
            {
                // this is our main database call.
                var supabaseCallTask = App.SupabaseClient
                    .From<DeptChair>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, depid)
                    .Get();

                // this is our 5-second timeout.
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

                // waits for either the database call to finish or the timeout to occur.
                var completedTask = await Task.WhenAny(supabaseCallTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    // if the timeout task finished first, it means the connection is slow.
                    await ShowDialog("Connection Slow", "Connection is slow, please try again");
                    button.IsEnabled = true;
                    return; // exit the method.
                }

                // if we reached here, the supabase call completed within the timeout.
                var response = await supabaseCallTask; // actually await the original task to get its result.


                var chair = response.Models.FirstOrDefault();
                Debug.WriteLine($"Returned rows: {response.Models.Count}");


                if (chair != null && chair.Password == password)
                {
                    await ShowDialog("Login Successful", "Welcome back!");
                    Frame.Navigate(typeof(DepartmentPage), chair.Program);
                }

                else
                {
                    ContentDialog failedDialog = new ContentDialog
                    {
                        Title = "Login Failed",
                        Content = "Invalid Department Chair ID or Password.",
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
            button.IsEnabled = true;
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