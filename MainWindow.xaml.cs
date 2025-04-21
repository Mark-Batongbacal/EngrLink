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
using Supabase.Postgrest;


namespace EngrLink
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
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
            MainFrame.Navigate(typeof(StudentPage)); // Navigate to StudentPage
        }

        private void FacultyButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            MainFrame.Navigate(typeof(FacultyPage)); // Navigate to FacultyPage
        }

        private void DepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            MainFrame.Navigate(typeof(DepartmentPage)); // Navigate to DepartmentPage
        }

        private void AccountingButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            MainFrame.Navigate(typeof(AccountingPage));
        }

        private void EnrolleeButton_Click(object sender, RoutedEventArgs e)
        {
            DisableAndHideButtons();
            MainFrame.Navigate(typeof(EnrolleePage));
        }


    }
}
