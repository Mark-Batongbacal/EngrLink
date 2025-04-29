using EngrLink.Models;
using Microsoft.UI.Xaml.Controls;
using System;

namespace EngrLink.Dialogs
{
    public sealed partial class EditAnnouncementDialog : ContentDialog
    {
        public string UpdatedTitle { get; private set; }
        public string UpdatedContent { get; private set; }
        public DateTimeOffset? UpdatedDate { get; private set; }

        public EditAnnouncementDialog(Announcement announcement)
        {
            this.InitializeComponent();

            // Pre-fill values
            TitleTextBox.Text = announcement.Title;
            ContentTextBox.Text = announcement.Content;
            DatePicker.Date = new DateTimeOffset(announcement.CreatedAt);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            UpdatedTitle = TitleTextBox.Text;
            UpdatedContent = ContentTextBox.Text;
            UpdatedDate = DatePicker.Date;
        }
    }
}
