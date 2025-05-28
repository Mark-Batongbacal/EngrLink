using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams; // For InMemoryRandomAccessStream
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices.WindowsRuntime; // For ToArray()
using Windows.Graphics.Imaging; // For BitmapEncoder and BitmapFileFormat

namespace EngrLink.Main_Window.Enrollee
{
    public sealed partial class ImageCropperDialog : ContentDialog
    {
        private IRandomAccessStreamReference _imageStreamRef;
        public WriteableBitmap CroppedBitmap { get; private set; } // This will hold the final cropped image

        public ImageCropperDialog(IRandomAccessStreamReference imageStreamRef)
        {
            this.InitializeComponent();
            _imageStreamRef = imageStreamRef;
            this.Loaded += ImageCropperDialog_Loaded;
            this.IsPrimaryButtonEnabled = true;
            this.IsSecondaryButtonEnabled = true;
        }

        private async void ImageCropperDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_imageStreamRef != null)
            {
                try
                {
                    using (var stream = await _imageStreamRef.OpenReadAsync())
                    {
                        // First, load into a BitmapImage to get dimensions, then transfer to WriteableBitmap.
                        // This handles the 'BitmapImage' to 'WriteableBitmap' conversion for the Source.
                        var tempBitmapImage = new BitmapImage();
                        await tempBitmapImage.SetSourceAsync(stream);

                        // Now, create a WriteableBitmap to be the actual source for the ImageCropperControl
                        // You need to rewind the stream for the second SetSourceAsync call.
                        stream.Seek(0);
                        var writableBitmapSource = new WriteableBitmap(tempBitmapImage.PixelWidth, tempBitmapImage.PixelHeight);
                        await writableBitmapSource.SetSourceAsync(stream);

                        ImageCropperControl.Source = writableBitmapSource; // Assign the WriteableBitmap as the source
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image into cropper: {ex.Message}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "Image Load Error",
                        Content = $"Could not load image for cropping: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot // Ensure error dialog also has XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    this.Hide(); // Close the dialog on error
                }
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                // Use InMemoryRandomAccessStream to capture the cropped image data
                using (var stream = new InMemoryRandomAccessStream())
                {
                    // This is the correct method to get the cropped image from the ImageCropper control
                    await ImageCropperControl.SaveAsync(stream, BitmapFileFormat.Png); // Save as PNG

                    stream.Seek(0); // Important: Rewind the stream to read from it

                    // Create a new WriteableBitmap and load the cropped image from the stream
                    var croppedWritableBitmap = new WriteableBitmap(
                        (int)ImageCropperControl.CroppedRegion.Width,
                        (int)ImageCropperControl.CroppedRegion.Height);
                    await croppedWritableBitmap.SetSourceAsync(stream);

                    CroppedBitmap = croppedWritableBitmap; // Assign the result to the public property
                }

                if (CroppedBitmap == null)
                {
                    Console.WriteLine("Cropped image is null after cropping attempt.");
                    // If for some reason cropping produced no image, prevent dialog from closing
                    args.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cropped image: {ex.Message}");
                CroppedBitmap = null; // Clear cropped bitmap on error
                var errorDialog = new ContentDialog
                {
                    Title = "Cropping Error",
                    Content = $"An error occurred during cropping: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                args.Cancel = true; // Prevent the dialog from closing if an error occurs
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}