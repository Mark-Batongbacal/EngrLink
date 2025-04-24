using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using EngrLink.Models;
using Supabase;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace EngrLink
{
    public sealed partial class LoginStudent : Page
    {
        private Supabase.Client supabaseClient;

        public LoginStudent()
        {
            this.InitializeComponent();
            InitializeSupabaseClient();
        }

        private async void InitializeSupabaseClient()
        {
            supabaseClient = new Supabase.Client(
                "https://dpouedmzpftnpodbopbi.supabase.co",  // Replace with your Supabase URL
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRwb3VlZG16cGZ0bnBvZGJvcGJpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDUwNDI2ODMsImV4cCI6MjA2MDYxODY4M30.XeZs98NROksWaNqE_q1HrgdxTLZ-Wmogwz4bWi4d_6s"                      // Replace with your Supabase anon/public key
            );
            await supabaseClient.InitializeAsync();
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
    }
}