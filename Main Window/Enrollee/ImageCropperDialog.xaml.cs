using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace EngrLink.Main_Window.Enrollee
{
    public sealed partial class ImageCropperDialog : ContentDialog
    {
        private IRandomAccessStreamReference _imageStreamRef;
        public WriteableBitmap CroppedBitmap { get; private set; } // holds the final cropped image.

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
                        var tempBitmapImage = new BitmapImage();
                        await tempBitmapImage.SetSourceAsync(stream);

                        stream.Seek(0); // rewind the stream for the next load.
                        var writableBitmapSource = new WriteableBitmap(tempBitmapImage.PixelWidth, tempBitmapImage.PixelHeight);
                        await writableBitmapSource.SetSourceAsync(stream);

                        ImageCropperControl.Source = writableBitmapSource; // sets the image source for the cropper.
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error loading image into cropper: {ex.Message}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "image load error",
                        Content = $"could not load image for cropping: {ex.Message}",
                        CloseButtonText = "ok",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    this.Hide(); // closes the dialog on error.
                }
            }
        }
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await ImageCropperControl.SaveAsync(stream, BitmapFileFormat.Png); // saves the cropped image to stream.

                    stream.Seek(0); // rewind the stream to read.

                    var croppedWritableBitmap = new WriteableBitmap(
                        (int)ImageCropperControl.CroppedRegion.Width,
                        (int)ImageCropperControl.CroppedRegion.Height);
                    await croppedWritableBitmap.SetSourceAsync(stream);

                    CroppedBitmap = croppedWritableBitmap; // assigns the cropped image.
                }

                if (CroppedBitmap == null)
                {
                    Console.WriteLine("cropped image is null after cropping attempt.");
                    args.Cancel = true; // prevents dialog from closing if no image.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error getting cropped image: {ex.Message}");
                CroppedBitmap = null; // clears cropped bitmap on error.
                var errorDialog = new ContentDialog
                {
                    Title = "cropping error",
                    Content = $"an error occurred during cropping: {ex.Message}",
                    CloseButtonText = "ok",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                args.Cancel = true; // prevents dialog from closing on error.
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}