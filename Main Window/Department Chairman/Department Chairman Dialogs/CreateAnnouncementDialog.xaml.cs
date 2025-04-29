using Microsoft.UI.Xaml.Controls;
using System;

namespace EngrLink.Dialogs
{
    public sealed partial class CreateAnnouncementDialog : ContentDialog
    {
        public string AnnouncementTitle { get; private set; }
        public string AnnouncementContent { get; private set; }
        public DateTimeOffset? AnnouncementDate { get; private set; }

        public CreateAnnouncementDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            AnnouncementTitle = TitleTextBox.Text;
            AnnouncementContent = ContentTextBox.Text;
            AnnouncementDate = DatePicker.Date;
        }
    }
}
