using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void DisableAndHideButtons()
        {
            StudentButton.IsEnabled = false;
            FacultyButton.IsEnabled = false;
            DepartmentButton.IsEnabled = false;
            AccountingButton.IsEnabled = false;
            EnrolleeButton.IsEnabled = false;

            StudentButton.Visibility = Visibility.Collapsed;
            FacultyButton.Visibility = Visibility.Collapsed;
            DepartmentButton.Visibility = Visibility.Collapsed;
            AccountingButton.Visibility = Visibility.Collapsed;
            EnrolleeButton.Visibility = Visibility.Collapsed;
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            Frame.Navigate(typeof(LoginStudent)); // Navigate to StudentPage
        }

        private void FacultyButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            Frame.Navigate(typeof(FacultyPage)); // Navigate to FacultyPage
        }

        private void DepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            Frame.Navigate(typeof(DepartmentPage)); // Navigate to DepartmentPage
        }

        private void AccountingButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            Frame.Navigate(typeof(AccountingPage));
        }

        private void EnrolleeButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            Frame.Navigate(typeof(EnrolleePage));
        }
    }
}
