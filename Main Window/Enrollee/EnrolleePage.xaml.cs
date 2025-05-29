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
        private StorageFile _selectedImageFile; // this is the image file chosen by the user.
        private WriteableBitmap _croppedBitmapImage; // this holds the cropped image.
        private string _uploadedImageUrl; // where the uploaded image url goes.

        public string program; // stores the selected program.
        public string year; // stores the selected year level.

        private const long max_image_size_bytes = 10 * 1024 * 1024; // 10 mb in bytes.

        public EnrolleePage()
        {
            this.InitializeComponent();
            CheckValid(); // checks if inputs are valid for submission.
            BirthdayDatePicker.MinYear = new DateTimeOffset(new DateTime(1950, 01, 01)); // sets the max birth year.
            BirthdayDatePicker.MaxYear = new DateTimeOffset(new DateTime(2008, 12, 31)); // sets the max birth year.
            if (string.IsNullOrEmpty(ContactTextBox.Text))
            {
                ContactTextBox.Text = "+63"; // default contact prefix.
                ContactTextBox.SelectionStart = ContactTextBox.Text.Length;
            }
        }

        private void CheckValid() // checks if all necessary fields are filled.
        {
            bool isValid = !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
                            !string.IsNullOrWhiteSpace(AddressTextBox.Text) &&
                            !string.IsNullOrWhiteSpace(ContactTextBox.Text) &&
                            ProgramComboBox.SelectedItem is ComboBoxItem programItem &&
                            YearLevelComboBox.SelectedItem is ComboBoxItem yearItem &&
                            BirthdayDatePicker.SelectedDate.HasValue &&
                            _selectedImageFile != null;
            SubmitButton.IsEnabled = isValid; // enables/disables submit button.
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) // handles back button click.
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false;
            }
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private async void SelectImageButton_Click(object sender, RoutedEventArgs e) // handles image selection and cropping.
        {
            FileOpenPicker openPicker = new();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            StorageFile file = await openPicker.PickSingleFileAsync(); // gets the chosen file.

            if (file != null)
            {
                Windows.Storage.FileProperties.BasicProperties basicProperties = await file.GetBasicPropertiesAsync();
                if (basicProperties.Size > max_image_size_bytes) // checks file size.
                {
                    ImageStatusTextBlock.Text = $"Image size exceeds 10 mb. Please choose a smaller image.";
                    ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///assets/placeholder.png"));
                    _selectedImageFile = null;
                    _croppedBitmapImage = null;
                    CheckValid();
                    return; // exits the method if too large.
                }

                _selectedImageFile = file; // assigns the valid file.

                using (var stream = await _selectedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var tempBitmap = new BitmapImage();
                    await tempBitmap.SetSourceAsync(stream);
                    ProfileImagePreview.Source = tempBitmap; // shows original image.
                }
                ImageStatusTextBlock.Text = $"Opening cropper for: {_selectedImageFile.Name}";

                var imageCropperDialog = new ImageCropperDialog(_selectedImageFile);
                imageCropperDialog.XamlRoot = this.Content.XamlRoot;

                var result = await imageCropperDialog.ShowAsync();

                if (result == ContentDialogResult.Primary) // if user clicked "crop".
                {
                    _croppedBitmapImage = imageCropperDialog.CroppedBitmap;

                    if (_croppedBitmapImage != null)
                    {
                        ProfileImagePreview.Source = _croppedBitmapImage; // displays cropped image.
                        ImageStatusTextBlock.Text = $"Image cropped successfully.";

                        _selectedImageFile = await ConvertWriteableBitmapToStorageFileAsync(_croppedBitmapImage, "cropped_image.png"); // converts cropped bitmap for upload.
                    }
                    else
                    {
                        ImageStatusTextBlock.Text = "Image cropping failed or cancelled by dialog.";
                        ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///assets/placeholder.png"));
                        _selectedImageFile = null;
                        _croppedBitmapImage = null;
                    }
                }
                else // if user clicked "cancel".
                {
                    ImageStatusTextBlock.Text = "Image selection or cropping cancelled.";
                    ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///assets/placeholder.png"));
                    _selectedImageFile = null;
                    _croppedBitmapImage = null;
                }
            }
            else // if no file was selected at all.
            {
                ImageStatusTextBlock.Text = "Image selection cancelled.";
                ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///assets/placeholder.png"));
                _selectedImageFile = null;
                _croppedBitmapImage = null;
            }
            CheckValid();
        }

        private async Task<StorageFile> ConvertWriteableBitmapToStorageFileAsync(WriteableBitmap writeableBitmap, string desiredFileName) // converts writeablebitmap to storagefile.
        {
            if (writeableBitmap == null) return null;

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

        private async void SubmitButton_Click(object sender, RoutedEventArgs e) // handles form submission.
        {
            ImageStatusTextBlock.Text = "";

            string name = NameTextBox.Text;
            string address = AddressTextBox.Text;
            string contact = ContactTextBox.Text;
            string birthday = BirthdayDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");

            _uploadedImageUrl = null;

            if (_selectedImageFile != null) // checks if an image is available for upload.
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
                var lastStudentResponse = await App.SupabaseClient // gets the last student's id.
                    .From<Student>()
                    .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                if (lastStudentResponse.Models.Count > 0)
                {
                    var lastStudent = lastStudentResponse.Models[0];
                    newId = lastStudent.Id + 1;
                }

                var fee = FeeCalculators.GetFee(year, program); // calculates the fee.

                var newStudent = new Student() // creates new student object.
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
                    .Insert(newStudent); // inserts student data into db.

                if (response.Models.Count > 0)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Submission successful",
                        Content = $"Your student id is {newId}. Remember this for your login.\n" +
                                    $"Your total balance is ₱{fee}\n" +
                                    $"Please pay a minimum amount of ₱5000\n",
                        CloseButtonText = "Ok",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dialog.ShowAsync();

                    // clears all input fields after successful submission.
                    NameTextBox.Text = "";
                    AddressTextBox.Text = "";
                    ContactTextBox.Text = "";
                    ProgramComboBox.SelectedItem = null;
                    YearLevelComboBox.SelectedItem = null;
                    ProgramComboBox.Header = "Enter program";
                    YearLevelComboBox.Header = "Enter year level";
                    BirthdayDatePicker.SelectedDate = null;
                    ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///assets/placeholder.png"));
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
                        Title = "Submission failed",
                        Content = "Failed to enroll student. Please check your inputs and try again.",
                        CloseButtonText = "Ok",
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
                    CloseButtonText = "Ok",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void Input_NameChanged(object sender, TextChangedEventArgs e) // validates name input.
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
        private void Input_AddressChanged(object sender, TextChangedEventArgs e) // triggers validation for address.
        {
            CheckValid();
        }

        private void NumberOnly_TextChanged(object sender, TextChangedEventArgs e) // validates contact number input.
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

        private void Input_DateChanged(object sender, DatePickerValueChangedEventArgs e) // triggers validation for date.
        {
            CheckValid();
        }

        private void YearLevel_SelectionChanged(object sender, SelectionChangedEventArgs e) // handles year level selection.
        {
            if (YearLevelComboBox.SelectedItem is ComboBoxItem yearitem)
            {
                YearLevelComboBox.Header = yearitem.Content;
                year = yearitem.Content.ToString();
            }
            CheckValid();
        }

        private void Program_SelectionChanged(object sender, SelectionChangedEventArgs e) // handles program selection.
        {
            if (ProgramComboBox.SelectedItem is ComboBoxItem programitem)
            {
                ProgramComboBox.Header = programitem.Content;
                program = programitem.Content.ToString();
            }

            if (program == "ARCHI") // shows 5th year for architecture.
                FifthYearItem.Visibility = Visibility.Visible;
            else
            {
                if (YearLevelComboBox.SelectedItem == FifthYearItem)
                {
                    YearLevelComboBox.SelectedIndex = -1;
                    YearLevelComboBox.Header = "Enter year level";
                    year = null;
                }
                FifthYearItem.Visibility = Visibility.Collapsed; // hides 5th year for other programs.
            }
            CheckValid();
        }
    }
}