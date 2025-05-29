using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ErrorPage : Page
    {
        public ErrorPage()
        {
            this.InitializeComponent();
        }

        public Type TargetPageType;
        public string Program;
        public string Id;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.GoBack();
            if (e.Parameter is (Type pageType, string program, string id))
            {
                TargetPageType = pageType;
                Program = program;
                Id = id;
            }
        }

        private async void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            RetryButton.IsEnabled = false;
            LoadingRing.IsActive = true;

            await Task.Delay(3000);
            bool isReady = await App.TryReinitializeSupabase();

            LoadingRing.IsActive = false;

            if (isReady)
            {

                Frame.Navigate(TargetPageType, (Program, Id));
            }
            else
            {
                RetryButton.IsEnabled = true;
                LoadingRing.IsActive = false;
            }
        }
    }
}
