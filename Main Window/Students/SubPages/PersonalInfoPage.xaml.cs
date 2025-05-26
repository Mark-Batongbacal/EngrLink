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
using EngrLink.Models; // Make sure to include your models namespace
using System.ComponentModel; // For INotifyPropertyChanged
using System.Diagnostics; // For Debug.WriteLine

namespace EngrLink.Main_Window.Students.SubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PersonalInfoPage : Page, INotifyPropertyChanged
    {
        // Property to hold the student's ID passed from the previous page
        public string StudentId { get; set; }

        // Model to bind personal information to the UI
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
            this.DataContext = this; // Set DataContext so XAML can bind to properties on this page
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get the student ID passed as a parameter
            if (e.Parameter is string studentId)
            {
                this.StudentId = studentId;
                Debug.WriteLine($"Navigated to PersonalInfoPage with Student ID: {this.StudentId}");
                await LoadPersonalInfo();
            }
        }

        private async System.Threading.Tasks.Task LoadPersonalInfo()
        {
            var client = App.SupabaseClient;

            try
            {
                var response = await client
                    .From<Student>() // Use your new StudentPersonalInfo model
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, this.StudentId)
                    .Single(); // Use Single() as you expect only one student profile

                if (response != null)
                {
                    PersonalInfo = response; // Assign the fetched data to the bound property
                    Debug.WriteLine($"Personal Info Loaded for {PersonalInfo.Name}");
                }
                else
                {
                    Debug.WriteLine($"No personal info found for student ID: {this.StudentId}");
                    // Optionally, you might want to display a message to the user
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading personal info: {ex.Message}");
                // Handle exceptions (e.g., show an error message)
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}