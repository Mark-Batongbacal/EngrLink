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
using Supabase;                 // Keep this
using Supabase.Postgrest;       // Keep this
using Supabase.Storage;         // Keep this
using Windows.Storage;          // Keep this
using Windows.Storage.Pickers;  // Keep this
using Microsoft.UI.Xaml.Media.Imaging; // Keep this
using System.Threading.Tasks;  // Keep this

namespace EngrLink.Main_Window.Enrollee
{
    public sealed partial class EnrolleePage : Page
    {
        private StorageFile _selectedImageFile;
        private string _uploadedImageUrl;

        public string program;
        public string year;

        public EnrolleePage()
        {
            this.InitializeComponent();
            CheckValid();
        }

        private void CheckValid()
        {
            {
                bool isValid = !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
                               !string.IsNullOrWhiteSpace(AddressTextBox.Text) &&
                               !string.IsNullOrWhiteSpace(ContactTextBox.Text) &&
                               ProgramComboBox.SelectedItem is ComboBoxItem programItem &&
                               YearLevelComboBox.SelectedItem is ComboBoxItem yearItem &&
                               BirthdayDatePicker.SelectedDate.HasValue;
                SubmitButton.IsEnabled = isValid;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

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

            _selectedImageFile = await openPicker.PickSingleFileAsync();

            if (_selectedImageFile != null)
            {
                using (var stream = await _selectedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    ProfileImagePreview.Source = bitmapImage;
                }
                ImageStatusTextBlock.Text = $"Selected: {_selectedImageFile.Name}";
            }
            else
            {
                ImageStatusTextBlock.Text = "Image selection cancelled.";
                ProfileImagePreview.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                _selectedImageFile = null;
            }
            CheckValid();
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

                    // The Upload method in Supabase.Storage 2.x expects a Stream and then the file name
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
                catch (Exception ex) // This should now be recognized
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

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckValid();
        }

        private void NumberOnly_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            int caretIndex = textBox.SelectionStart;

            string filteredText = new string(textBox.Text.Where(char.IsDigit).ToArray());

            if (textBox.Text != filteredText)
            {
                textBox.Text = filteredText;
                textBox.SelectionStart = Math.Min(caretIndex, textBox.Text.Length);
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
    }
}