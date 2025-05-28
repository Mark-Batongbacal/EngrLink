using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Devices.AllJoyn;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConnectionErrorWindow : Window
    {
        public ConnectionErrorWindow()
        {
            this.InitializeComponent();
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
                var main = new MainWindow();
                App.MainWindow = main;
                main.Activate();
                this.Close(); 
            }
            else
            {
                RetryButton.IsEnabled = true;
                LoadingRing.IsActive = false;
            }
        }
    }
}
