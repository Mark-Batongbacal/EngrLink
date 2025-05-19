using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase.Gotrue;
using Supabase.Realtime;
using Supabase.Realtime.Models;
using Windows.UI.Popups;

namespace EngrLink.Main_Window.Department_Chairman.SubPages
{
    public class AnnouncementViewModel
    {
        public string Content { get; set; }
        public bool ForStudents { get; set; }
        public bool ForTeachers { get; set; }

        public Visibility ForStudentsVisibility { get; set; }
        public Visibility ForTeachersVisibility { get; set; }

        public AnnouncementViewModel(Models.Announcement announcement)
        {
            this.Content = announcement.Content;
            this.ForStudents = announcement.ForStudents;
            this.ForTeachers = announcement.ForTeachers;
            this.ForStudentsVisibility = announcement.ForStudents ? Visibility.Visible : Visibility.Collapsed;
            this.ForTeachersVisibility = announcement.ForTeachers ? Visibility.Visible : Visibility.Collapsed;
        }
    }
    public sealed partial class AnnouncementPage : Page
    {
        private readonly Supabase.Client _supabaseClient;
        public List<AnnouncementViewModel> Announcements { get; set; } = new List<AnnouncementViewModel>();

        public AnnouncementPage()
        {
            this.InitializeComponent();
            _supabaseClient = App.SupabaseClient;
            LoadAnnouncements();
        }

        private async void PostAnnouncementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Collapsed; // Reset message

            string content = AnnouncementContentTextBox.Text.Trim();
            bool forStudents = ShowToStudentsCheckBox.IsChecked ?? false; // ?? handles null
            bool forTeachers = ShowToTeachersCheckBox.IsChecked ?? false;

            if (string.IsNullOrEmpty(content))
            {
                MessageTextBlock.Text = "Announcement content cannot be empty.";
                MessageTextBlock.Visibility = Visibility.Visible;
                return;
            }

            var newAnnouncement = new Models.Announcement
            {
                Content = content,
                ForStudents = forStudents,
                ForTeachers = forTeachers
            };

            try
            {
                var response = await _supabaseClient.From<Models.Announcement>().Insert(newAnnouncement);
                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Announcements.Add(new AnnouncementViewModel(response.Model)); //add to the list
                    AnnouncementsListView.ItemsSource = null;
                    AnnouncementsListView.ItemsSource = Announcements;
                    AnnouncementContentTextBox.Text = ""; // Clear input
                    ShowToStudentsCheckBox.IsChecked = false;
                    ShowToTeachersCheckBox.IsChecked = false;

                    // Show a success message
                    var dialog = new MessageDialog("Announcement posted successfully!", "Success");
                    await dialog.ShowAsync();
                }
                else
                {
                    MessageTextBlock.Text = $"Error posting announcement: {response.ResponseMessage.ReasonPhrase}";
                    MessageTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = $"An error occurred: {ex.Message}";
                MessageTextBlock.Visibility = Visibility.Visible;
            }
        }

        private async void LoadAnnouncements()
        {
            try
            {
                var response = await _supabaseClient.From<Models.Announcement>().Get();
                if (response.ResponseMessage.IsSuccessStatusCode)
                {
                    Announcements = response.Models.Select(a => new AnnouncementViewModel(a)).ToList();
                    AnnouncementsListView.ItemsSource = Announcements;
                }
                else
                {
                    var dialog = new MessageDialog($"Failed to load announcements: {response.ResponseMessage.ReasonPhrase}", "Error");
                    await dialog.ShowAsync();
                }

            }
            catch (Exception e)
            {
                var dialog = new MessageDialog($"Error loading announcements: {e.Message}", "Error");
                await dialog.ShowAsync();
            }
        }
    }
}
