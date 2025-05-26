using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using EngrLink.Models;
using Supabase;
using Supabase.Postgrest.Models;

namespace EngrLink.Main_Window.Department_Chairman.SubPages
{
    public sealed partial class AnnouncementPage : Page
    {
        // ObservableCollection to hold announcements
        public ObservableCollection<Announcement> Announcements { get; set; } = new ObservableCollection<Announcement>();

        public AnnouncementPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            LoadAnnouncements();
        }

        private async void LoadAnnouncements()
        {
            var client = App.SupabaseClient;

            try
            {
                var response = await client
                    .From<Announcement>()
                    .Get();

                // Clear existing announcements and add the fetched ones
                Announcements.Clear();
                foreach (var announcement in response.Models)
                {
                    Announcements.Add(announcement);
                }
            }
            catch (Exception ex)
            {
                // Handle errors (optional)
                MessageTextBlock.Text = $"Error loading announcements: {ex.Message}";
                MessageTextBlock.Visibility = Visibility.Visible;
            }
        }

        private async void PostAnnouncementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Collapsed;

            string content = AnnouncementContentTextBox.Text.Trim();
            bool forStudents = ShowToStudentsCheckBox.IsChecked == true;
            bool forTeachers = ShowToTeachersCheckBox.IsChecked == true;

            if (string.IsNullOrEmpty(content))
            {
                MessageTextBlock.Text = "Announcement content cannot be empty.";
                MessageTextBlock.Visibility = Visibility.Visible;
                return;
            }

            var newAnnouncement = new Announcement
            {
                Announcements = content,
                ForStud = forStudents,
                ForFac = forTeachers,
            };

            var client = App.SupabaseClient;

            try
            {
                var response = await client
                    .From<Announcement>()
                    .Insert(newAnnouncement);

                if (response.Models.Count > 0)
                {
                    // Add the new announcement to the ObservableCollection
                    Announcements.Add(response.Models[0]);

                    // Clear input fields
                    AnnouncementContentTextBox.Text = "";
                    ShowToStudentsCheckBox.IsChecked = false;
                    ShowToTeachersCheckBox.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"Error posting announcement: {ex.Message}";
                MessageTextBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
