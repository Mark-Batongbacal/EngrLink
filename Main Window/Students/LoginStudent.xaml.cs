using EngrLink.Main_Window.Department_Chairman;
using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EngrLink.Main_Window.Students
{
    public sealed partial class LoginStudent : Page
    {
        public LoginStudent()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }
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

                if (newText.Length > 9)
                {
                    newText = newText.Substring(0, 9);
                }

                if (textBox.Text != newText ) 
                {
                    textBox.TextChanged -= Input_TextChanged;
                    textBox.Text = newText;
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.TextChanged += Input_TextChanged;
                }
            }
            CheckValid();
        }

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordPanel.Visibility = Visibility.Visible;
        }

        private async void SubmitNewPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

            string studentIdInput = StudentID.Text.Trim();
            string newPassword = NewPassword.Password.Trim();
            string confirmPassword = ConfirmNewPassword.Password.Trim();

            if (string.IsNullOrEmpty(studentIdInput) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                await ShowDialog("Missing Fields", "Please fill in all fields.");
                if (button != null) button.IsEnabled = true;
                return;
            }

            if (newPassword != confirmPassword)
            {
                await ShowDialog("Password Mismatch", "New passwords do not match.");
                if (button != null) button.IsEnabled = true;
                return;
            }

            try
            {
                // this is our main database call for password change.
                var supabaseCallTask = App.SupabaseClient
                    .From<Student>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, int.Parse(studentIdInput))
                    .Get();

                // this is our 5-second timeout.
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

                // waits for either the database call to finish or the timeout to occur.
                var completedTask = await Task.WhenAny(supabaseCallTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    // if the timeout task finished first, it means the connection is slow.
                    await ShowDialog("Connection Slow", "Connection is slow, please try again");
                    if (button != null) button.IsEnabled = true;
                    return; // exit the method.
                }

                // if we reached here, the supabase call completed within the timeout.
                var studentResponse = await supabaseCallTask; // actually await the original task to get its result.


                var student = studentResponse.Models.FirstOrDefault();

                if (student != null)
                {
                    student.Password = newPassword;

                    var result = await App.SupabaseClient.From<Student>().Update(student);
                    await ShowDialog("Success", "Password successfully changed!");

                    ChangePasswordPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    await ShowDialog("User Not Found", "No student found with the given ID.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error changing password: {ex.Message}");
                await ShowDialog("Error", $"Something went wrong: {ex.Message}");
            }
            button.IsEnabled = true;
        }


        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

            string studentid = StudentID.Text.Trim();
            string password = Password.Password.Trim();
            string studid = "";
            string studpassword = "";

            try
            {
                // this is our main database call for login.
                var supabaseCallTask = App.SupabaseClient
                    .From<Student>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, int.Parse(StudentID.Text))
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
                var studentResponse = await supabaseCallTask; // actually await the original task to get its result.

                var student = studentResponse.Models.FirstOrDefault();
                if (student != null)
                {
                    studid = student.Id.ToString();
                    studpassword = student.Password;
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
                        Frame.Navigate(typeof(StudentPage), (student.Id.ToString(), student.Program));
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
                Debug.WriteLine($"Error during login: {ex.Message}");
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