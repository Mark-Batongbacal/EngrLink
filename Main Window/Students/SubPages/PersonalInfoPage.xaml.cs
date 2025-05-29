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
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.UI.Xaml.Media.Imaging; // ADDED: For BitmapImage

namespace EngrLink.Main_Window.Students.SubPages
{
    public sealed partial class PersonalInfoPage : Page, INotifyPropertyChanged
    {
        public string StudentId { get; set; }
        public string Program { get; set; }

        private Student _personalInfo;
        public Student PersonalInfo
        {
            get => _personalInfo;
            set
            {
                if (_personalInfo != value)
                {
                    _personalInfo = value;
                    OnPropertyChanged(nameof(PersonalInfo));
                }
            }
        }

        public PersonalInfoPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is (string studentId, string program))
            {
                this.StudentId = studentId;
                this.Program = program;
                Debug.WriteLine($"Navigated to PersonalInfoPage with Student ID: {this.StudentId}");
                await LoadPersonalInfo();
            }
            else
            {
                Debug.WriteLine("PersonalInfoPage navigated to without a Student ID parameter.");
                // Optionally, display an error or navigate back if no ID is provided
            }
        }

        private async System.Threading.Tasks.Task LoadPersonalInfo()
        {

            var client = App.SupabaseClient;

            try
            {
                var response = await client
                    .From<Student>()
                    // Assuming 'id' column in your Supabase table is compatible with string,
                    // or your Supabase client handles int.Parse implicitly if 'id' is int.
                    // If 'id' is an integer, ensure StudentId is parsed: .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, int.Parse(this.StudentId))
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, this.StudentId)
                    .Single();

                if (response != null)
                {
                    PersonalInfo = response; // This will update the bound TextBlocks

                    // NEW: Load and display the profile image
                    if (!string.IsNullOrEmpty(PersonalInfo.ProfileImageUrl))
                    {
                        try
                        {
                            Uri imageUri = new Uri(PersonalInfo.ProfileImageUrl);
                            BitmapImage bitmapImage = new BitmapImage(imageUri);
                            StudentProfileImage.Source = bitmapImage;
                            Debug.WriteLine($"Successfully loaded image for {PersonalInfo.Name} from: {PersonalInfo.ProfileImageUrl}");
                        }
                        catch (UriFormatException ex)
                        {
                            Debug.WriteLine($"Invalid image URL for student {PersonalInfo.Id}: {PersonalInfo.ProfileImageUrl} - {ex.Message}");
                            StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png")); // Fallback
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error loading image for student {PersonalInfo.Id}: {ex.Message}");
                            StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png")); // Fallback
                        }
                    }
                    else
                    {
                        // No ProfileImageUrl, show default placeholder
                        StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                        Debug.WriteLine($"No profile image URL found for student {PersonalInfo.Id}. Displaying placeholder.");
                    }
                }
                else
                {
                    Debug.WriteLine($"No personal info found for student ID: {this.StudentId}");
                    // Optionally, clear existing info or show a "not found" message
                    PersonalInfo = null; // Clear any previously displayed info
                    StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png")); // Reset image
                }
            }
            catch (Exception ex)
            {
                Frame.Navigate(typeof(ErrorPage), (typeof(Dashboard), this.Program, this.StudentId));
                Debug.WriteLine($"Error loading personal info from Supabase: {ex.Message}");
                // Handle the error (e.g., show a message to the user)
                PersonalInfo = null; // Clear info on error
                StudentProfileImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/error_placeholder.png")); // Show error image

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}