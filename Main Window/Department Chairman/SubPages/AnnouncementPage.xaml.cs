using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Supabase;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using EngrLink.Dialogs;
using System.Collections.Generic;

namespace EngrLink.Main_Window.Department_Chairman.SubPages
{
    public sealed partial class AnnouncementPage : Page
    {
        private Supabase.Client? _supabaseClient;
        private ObservableCollection<Announcement> _announcements = new ObservableCollection<Announcement>();

        public AnnouncementPage()
        {
            this.InitializeComponent();
            var announcements = new List<string>
            {
                "Midterm exams will start on May 5.",
                "Deadline for enrollment is May 10.",
                "Engineering General Assembly this Friday at 2PM.",
                "Faculty meeting scheduled on May 8, 1:00PM."
            };

            AnnouncementsListView.ItemsSource = announcements;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _supabaseClient = (Supabase.Client?)e.Parameter;
            base.OnNavigatedTo(e);
        }

        private async void AnnouncementPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAnnouncements();
        }

        private async Task LoadAnnouncements()
        {
            if (_supabaseClient == null)
            {
                await ShowErrorDialog("Supabase client not initialized.");
                return;
            }

            try
            {
                var response = await _supabaseClient
                    .From<Announcement>()
                    .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                    .Get();

                _announcements.Clear();
                foreach (var announcement in response.Models)
                    _announcements.Add(announcement);

                AnnouncementsListView.ItemsSource = _announcements;
                EmptyAnnouncementsTextBlock.Visibility = _announcements.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error loading announcements: {ex.Message}");
            }
        }

        private async void NewAnnouncementButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateAnnouncementDialog();
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var newAnnouncement = new Announcement
                {
                    Title = dialog.AnnouncementTitle,
                    Content = dialog.AnnouncementContent,
                    CreatedAt = (dialog.AnnouncementDate ?? DateTimeOffset.Now).DateTime
                };

                await _supabaseClient?.From<Announcement>().Insert(newAnnouncement);
                await LoadAnnouncements();
            }
        }

        private async void AnnouncementsListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (AnnouncementsListView.SelectedItem is Announcement selectedAnnouncement)
            {
                var dialog = new EditAnnouncementDialog(selectedAnnouncement);
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    selectedAnnouncement.Title = dialog.UpdatedTitle;
                    selectedAnnouncement.Content = dialog.UpdatedContent;
                    selectedAnnouncement.CreatedAt = (dialog.UpdatedDate ?? DateTimeOffset.Now).DateTime;

                    await _supabaseClient?.From<Announcement>().Update(selectedAnnouncement);
                    await LoadAnnouncements();
                }
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        // 🆕 ADDED THIS METHOD TO FIX THE ERROR
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
