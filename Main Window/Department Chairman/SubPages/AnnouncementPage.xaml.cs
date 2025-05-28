using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase;
using Supabase.Postgrest.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq; // Keep for potential future use or if other LINQ is needed elsewhere
using System.Collections.Generic; // Keep for potential future use or if other collections are needed
using Microsoft.UI.Xaml.Navigation;

namespace EngrLink.Main_Window.Department_Chairman.SubPages
{
    public sealed partial class AnnouncementPage : Page
    {
        public string Program { get; set; }

        public ObservableCollection<Announcement> Announcements { get; set; } = new ObservableCollection<Announcement>();

        public AnnouncementPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string program)
            {
                Debug.WriteLine($"Navigated with Department ID: {program}");
                this.Program = program;
            }
            LoadAnnouncements();
        }

        private async void LoadAnnouncements()
        {
            var client = App.SupabaseClient;

            try
            {
                var response = await client
                    .From<Announcement>()
                    .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, this.Program)
                    .Get();

                Announcements.Clear();
                foreach (var announcement in response.Models)
                {
                    Announcements.Add(announcement);
                }
            }
            catch (Exception ex)
            {
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

            if (!forStudents && !forTeachers)
            {
                MessageTextBlock.Text = "Please select at least one audience (Students or Teachers).";
                MessageTextBlock.Visibility = Visibility.Visible;
                return;
            }

            var client = App.SupabaseClient;
            string chairmanName = "Unknown Author";

            try
            {
                var chairmanResponse = await client
                    .From<DeptChair>()
                    .Filter("program", Supabase.Postgrest.Constants.Operator.Equals, this.Program)
                    .Limit(1)
                    .Get();

                if (chairmanResponse.Models.Any())
                {
                    chairmanName = chairmanResponse.Models.First().Name;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching chairman name: {ex.Message}");
            }

            string combinedMessage = $"{content} - {chairmanName}";

            var newAnnouncement = new Announcement
            {
                Announcements = combinedMessage,
                ForStud = forStudents,
                ForFac = forTeachers,
                Program = this.Program
            };

            try
            {
                var response = await client
                    .From<Announcement>()
                    .Insert(newAnnouncement);

                if (response.Models.Count > 0)
                {
                    Announcements.Add(response.Models[0]);

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

        private async void RemoveAnnouncementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Visibility = Visibility.Collapsed;

            var button = sender as Button;
            if (button == null) return;

            var announcementToRemove = button.DataContext as Announcement;

            if (announcementToRemove == null) return;

            ContentDialog deleteConfirmDialog = new ContentDialog
            {
                Title = "Delete announcement?",
                Content = $"Are you sure you want to delete this announcement:\n\n\"{announcementToRemove.Announcements}\"?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            if (this.XamlRoot != null)
            {
                deleteConfirmDialog.XamlRoot = this.XamlRoot;
            }
            else
            {
                Debug.WriteLine("Error: Page's XamlRoot is null. Cannot show dialog.");
                MessageTextBlock.Text = "Could not display confirmation dialog. Page not fully loaded.";
                MessageTextBlock.Visibility = Visibility.Visible;
                return;
            }

            ContentDialogResult result = await deleteConfirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var client = App.SupabaseClient;
                try
                {
                    await client
                        .From<Announcement>()
                        .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, announcementToRemove.Id)
                        .Delete();

                    Announcements.Remove(announcementToRemove);
                }
                catch (Exception ex)
                {
                    MessageTextBlock.Text = $"Error deleting announcement: {ex.Message}";
                    MessageTextBlock.Visibility = Visibility.Visible;
                }
            }
        }
    }
}