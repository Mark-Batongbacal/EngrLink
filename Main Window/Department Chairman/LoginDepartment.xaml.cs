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

namespace EngrLink.Main_Window.Department_Chairman
{
    public sealed partial class LoginDepartment : Page
    {
        public LoginDepartment()
        {
            this.InitializeComponent();
            InitializeSupabaseClient();
        }

        private async void InitializeSupabaseClient()
        {
            var client = App.SupabaseClient;

            // Only get students where Enrolled is false
            var response = await client
                .From<DeptChair>()
                .Get();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void CheckValid()
        {
            bool isValid = !string.IsNullOrWhiteSpace(StudentID.Text) &&
                           !string.IsNullOrWhiteSpace(Password.Password);
            SubmitButton.IsEnabled = isValid;
        }

        private void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            CheckValid();
        }

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordPanel.Visibility = Visibility.Visible;
        }

        private async void SubmitNewPassword_Click(object sender, RoutedEventArgs e)
        {
            string studentIdInput = StudentID.Text.Trim();
            string newPassword = NewPassword.Password.Trim();
            string confirmPassword = ConfirmNewPassword.Password.Trim();

            if (string.IsNullOrEmpty(studentIdInput) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                await ShowDialog("Missing Fields", "Please fill in all fields.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                await ShowDialog("Password Mismatch", "New passwords do not match.");
                return;
            }

            try
            {
                var studentResponse = await supabaseClient
                    .From<Student>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, int.Parse(studentIdInput))
                    .Get();

                var student = studentResponse.Models.FirstOrDefault();

                if (student != null)
                {
                    student.Password = newPassword;
                    student.Id = int.Parse(studentIdInput);

                    var result = await supabaseClient.From<Student>().Update(student);
                    await ShowDialog("Success", "Password successfully changed!");
                }
                else
                {
                    await ShowDialog("User Not Found", "No student found with the given ID.");
                }
            }
            catch (Exception ex)
            {
                await ShowDialog("Error", $"Something went wrong: {ex.Message}");
            }
        }


        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string studentid = StudentID.Text.Trim();
            string password = Password.Password.Trim();
            string studid = "";
            string studpassword = "";
            try
            {
                var studentResponse = await supabaseClient
                    .From<Student>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, int.Parse(StudentID.Text))
                    .Get();

                var student = studentResponse.Models.FirstOrDefault();
                if (student != null)
                {
                    studid = student.Id.ToString();
                    studpassword = student.Password;
                }

                if (Password.Password == studpassword)
                {
                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Login Successful",
                        Content = "Welcome back!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                    // Navigate to next page if needed
                    Frame.Navigate(typeof(StudentPage));
                }
                else
                {
                    ContentDialog failedDialog = new ContentDialog
                    {
                        Title = "Login Failed",
                        Content = "Invalid Student ID or Password.",
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
