using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Supabase;
using Supabase.Postgrest;
using Supabase.Storage;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace EngrLink.Main_Window.Enrollee
{
    public sealed partial class EnrolleePage : Page
    {
        private StorageFile _selectedImageFile;
        private WriteableBitmap _croppedBitmapImage;
        private string _uploadedImageUrl;

        public string program;
        public string year;

        public EnrolleePage()
        {
            this.InitializeComponent();
            CheckValid();
            BirthdayDatePicker.MaxYear = new DateTimeOffset(new DateTime(2008, 12, 31));
            if (string.IsNullOrEmpty(ContactTextBox.Text))
            {
                ContactTextBox.Text = "+63";
                ContactTextBox.SelectionStart = ContactTextBox.Text.Length;
            }
        }

        private void CheckValid()
        {
            bool isValid = !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
                            !string.IsNullOrWhiteSpace(AddressTextBox.Text) &&
                            !string.IsNullOrWhiteSpace(ContactTextBox.Text) &&
                            ProgramComboBox.SelectedItem is ComboBoxItem programItem &&
                            YearLevelComboBox.SelectedItem is ComboBoxItem yearItem &&
                            BirthdayDatePicker.SelectedDate.HasValue &&
                            _selectedImageFile != null;
            SubmitButton.IsEnabled = isValid;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private async void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            _selectedImageFile = await openPicker.PickSingleFileAsync(); // Get the original file

            if (_selectedImageFile != null)
            {
                // Display a temporary preview while the cropper dialog opens
                using (var stream = await _selectedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var tempBitmap = new BitmapImage();
                    await tempBitmap.SetSourceAsync(stream);
                    ProfileImagePreview.Source = tempBitmap; // Show original image before cropping
                }
                ImageStatusTextBlock.Text = $"Opening cropper for: {_selectedImageFile.Name}";

                // Create the dialog, passing the original image file
                var imageCropperDialog = new ImageCropperDialog(_selectedImageFile);
                imageCropperDialog.XamlRoot = this.Content.XamlRoot; // Set XamlRoot for the dialog

                var result = await imageCropperDialog.ShowAsync();

                if (result == ContentDialogResult.Primary) // If user clicked "Crop"
                {
                    // *** CHANGE THIS LINE: Use the CroppedBitmap property ***
                    _croppedBitmapImage = imageCropperDialog.CroppedBitmap;

                    if (_croppedBitmapImage != null)
                    {
                        ProfileImagePreview.Source = _croppedBitmapImage; // Display the cropped image
                        ImageStatusTextBlock.Text = $"Image cropped successfully.";

                        // Convert the WriteableBitmap from the dialog to a StorageFile for upload

                        // Update the problematic line to use the new method:
                        _selectedImageFile = await ConvertWriteableBitmapToStorageFileAsync(_croppedBitmapImage, "cropped_image.png");
                    }
                    else
                    {
                        // This case should be caught by the dialog now, but as a fallback
                        ImageStatusTextBlock.Text = "Image cropping failed or cancelled by dialog.";
                        ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                        _selectedImageFile = null;
                        _croppedBitmapImage = null;
                    }
                }
                else // If user clicked "Cancel" or dismissed the dialog
                {
                    ImageStatusTextBlock.Text = "Image selection or cropping cancelled.";
                    ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                    _selectedImageFile = null; // Clear the original selected file
                    _croppedBitmapImage = null; // Clear cropped image reference
                }
            }
            else
            {
                ImageStatusTextBlock.Text = "Image selection cancelled.";
                ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                _selectedImageFile = null;
                _croppedBitmapImage = null;
            }
            CheckValid();
        }

        private async Task<StorageFile> ConvertBitmapImageToStorageFileAsync(BitmapImage bitmapImage, string desiredFileName)
        {
            if (bitmapImage == null) return null;

            // Get a temporary folder
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile tempFile = await tempFolder.CreateFileAsync(desiredFileName, CreationCollisionOption.ReplaceExisting);

            // Get the BitmapImage's actual pixel data (this can be tricky, as BitmapImage often doesn't expose raw pixels directly)
            // A more robust approach might be to capture the cropped content directly from the ImageCropperControl
            // For now, let's assume we can get the render target bitmap from the preview itself if needed,
            // or better yet, get the byte array directly from the ImageCropperDialog.

            // Given your ImageCropperDialog returns a BitmapImage,
            // you'll need to render it to a RenderTargetBitmap and then encode it.
            var renderTargetBitmap = new RenderTargetBitmap();
            // Assuming ProfileImagePreview is the Image element where the cropped image is displayed
            await renderTargetBitmap.RenderAsync(ProfileImagePreview); // Render the displayed cropped image

            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            var pixels = pixelBuffer.ToArray();

            using (var stream = await tempFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream); // You can choose PngEncoderId or JpegEncoderId
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    96.0, 96.0,
                    pixels);
                await encoder.FlushAsync();
            }

            return tempFile;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ImageStatusTextBlock.Text = "";

            string name = NameTextBox.Text;
            string address = AddressTextBox.Text;
            string contact = ContactTextBox.Text;
            string birthday = BirthdayDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");

            _uploadedImageUrl = null;

            if (_selectedImageFile != null)
            {
                try
                {
                    string bucketName = "profile";
                    string fileName = $"{Guid.NewGuid()}_{_selectedImageFile.Name}";

                    using (var stream = await _selectedImageFile.OpenStreamForReadAsync())
                    {
                        byte[] fileBytes;
                        using (var memoryStream = new MemoryStream())
                        {
                            await stream.CopyToAsync(memoryStream);
                            fileBytes = memoryStream.ToArray();
                        }

                        var response = await App.SupabaseClient.Storage
                            .From(bucketName)
                            .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
                            {
                                Upsert = false,
                                ContentType = _selectedImageFile.ContentType
                            });

                        if (!string.IsNullOrEmpty(response))
                        {
                            _uploadedImageUrl = App.SupabaseClient.Storage
                                .From(bucketName)
                                .GetPublicUrl(fileName);

                            ImageStatusTextBlock.Text = $"Image uploaded successfully.";
                        }
                        else
                        {
                            ImageStatusTextBlock.Text = "Image upload failed. Please try again.";
                            return;
                        }
                    }

                }
                catch (Exception ex)
                {
                    ImageStatusTextBlock.Text = $"Supabase error during image upload: {ex.Message}";
                    return;
                }
            }
            else
            {
                ImageStatusTextBlock.Text = "No profile image selected. Proceeding without image.";
            }

            int newId = 1;
            try
            {
                var lastStudentResponse = await App.SupabaseClient
                    .From<Student>()
                    .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                if (lastStudentResponse.Models.Count > 0)
                {
                    var lastStudent = lastStudentResponse.Models[0];
                    newId = lastStudent.Id + 1;
                }

                var fee = FeeCalculators.GetFee(year, program);

                var newStudent = new Student()
                {
                    Id = newId,
                    Name = name,
                    Address = address,
                    Contact = contact,
                    Year = year,
                    Fees = fee,
                    Total = fee,
                    Program = program,
                    Password = newId.ToString(),
                    Birthday = birthday,
                    Enrolled = false,
                    Paid = false,
                    ProfileImageUrl = _uploadedImageUrl
                };

                var response = await App.SupabaseClient
                    .From<Student>()
                    .Insert(newStudent);

                if (response.Models.Count > 0)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Submission Successful",
                        Content = $"Your Student ID is {newId}. Remember this for your Login.\n" +
                                    $"Your Total Balance is ₱{fee}\n" +
                                    $"Please pay a minimum amount of ₱5000\n",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dialog.ShowAsync();

                    NameTextBox.Text = "";
                    AddressTextBox.Text = "";
                    ContactTextBox.Text = "";
                    ProgramComboBox.SelectedItem = null;
                    YearLevelComboBox.SelectedItem = null;
                    ProgramComboBox.Header = "Enter Program";
                    YearLevelComboBox.Header = "Enter Year Level";
                    BirthdayDatePicker.SelectedDate = null;
                    ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                    ImageStatusTextBlock.Text = "";
                    _selectedImageFile = null;
                    _uploadedImageUrl = null;

                    program = null;
                    year = null;

                    SubmitButton.IsEnabled = false;
                    Frame.GoBack();
                }
                else
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Submission Failed",
                        Content = "Failed to enroll student. Please check your inputs and try again.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void Input_NameChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            int caretIndex = textBox.SelectionStart;
            string originalText = textBox.Text;
            string filteredText = "";
            int periodCount = 0;

            foreach (char c in originalText)
            {
                if (char.IsLetter(c) || c == ' ' || c == '-')
                {
                    filteredText += c;
                }
                else if (c == '.')
                {
                    if (periodCount == 0)
                    {
                        filteredText += c;
                        periodCount++;
                    }
                }
            }

            if (originalText != filteredText)
            {
                textBox.Text = filteredText;
                textBox.SelectionStart = Math.Min(caretIndex, textBox.Text.Length);
            }
            CheckValid();
        }
        private void Input_AddressChanged(object sender, TextChangedEventArgs e)
        {
            CheckValid();
        }

        private void NumberOnly_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            string currentText = textBox.Text;
            string newText = currentText;
            int caretIndex = textBox.SelectionStart;

            if (!currentText.StartsWith("+63"))
            {
                newText = "+63";
                if (currentText.Length > 0 && currentText[0] == '+')
                {
                    string digitsAfterPlus = new string(currentText.SkipWhile(c => c != '+').Skip(1).Where(char.IsDigit).ToArray());
                    newText += digitsAfterPlus;
                }
                else
                {
                    newText = "+63" + new string(currentText.Where(char.IsDigit).ToArray());
                }

                if (caretIndex < newText.Length)
                {
                    caretIndex = Math.Max(3, caretIndex);
                }
            }

            string prefix = "+63";
            string digitsOnly = "";
            if (newText.Length > prefix.Length)
            {
                digitsOnly = new string(newText.Substring(prefix.Length).Where(char.IsDigit).ToArray());
                if (digitsOnly.Length > 10)
                {
                    digitsOnly = digitsOnly.Substring(0, 10);
                }
            }

            newText = prefix + digitsOnly;

            if (textBox.Text != newText)
            {
                textBox.Text = newText;

                if (caretIndex < 3)
                {
                    textBox.SelectionStart = 3;
                }
                else
                {
                    textBox.SelectionStart = Math.Min(caretIndex, textBox.Text.Length);
                }
            }

            CheckValid();
        }

        private void Input_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            CheckValid();
        }

        private void YearLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YearLevelComboBox.SelectedItem is ComboBoxItem yearitem)
            {
                YearLevelComboBox.Header = yearitem.Content;
                year = yearitem.Content.ToString();
            }
            CheckValid();
        }

        private void Program_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProgramComboBox.SelectedItem is ComboBoxItem programitem)
            {
                ProgramComboBox.Header = programitem.Content;
                program = programitem.Content.ToString();
            }

            if (program == "ARCHI")
                FifthYearItem.Visibility = Visibility.Visible;
            else
            {
                if (YearLevelComboBox.SelectedItem == FifthYearItem)
                {
                    YearLevelComboBox.SelectedIndex = -1;
                    YearLevelComboBox.Header = "Enter Year Level";
                    year = null;
                }
                FifthYearItem.Visibility = Visibility.Collapsed;
            }
            CheckValid();
        }
        private async Task<StorageFile> ConvertWriteableBitmapToStorageFileAsync(WriteableBitmap writeableBitmap, string desiredFileName)
        {
            if (writeableBitmap == null) return null;

            // Get a temporary folder
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile tempFile = await tempFolder.CreateFileAsync(desiredFileName, CreationCollisionOption.ReplaceExisting);

            using (var stream = await tempFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)writeableBitmap.PixelWidth,
                    (uint)writeableBitmap.PixelHeight,
                    96.0, 96.0,
                    writeableBitmap.PixelBuffer.ToArray());
                await encoder.FlushAsync();
            }

            return tempFile;
        }
    }
}