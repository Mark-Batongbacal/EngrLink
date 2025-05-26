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
        public AnnouncementPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            LoadAnnouncements();
        }

        private async void LoadAnnouncements()
        {
            var client = App.SupabaseClient;

            var response = await client
                .From<Announcement>()
                .Get();

         
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
