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

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Main_Window.Students.LoginStudent)); 
            StudentButton.IsEnabled = false; 
        }

        private void FacultyButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Main_Window.Instructor.FacultyLogin));
            FacultyButton.IsEnabled = false;
        }

        private void DepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Main_Window.Department_Chairman.LoginDepartment));
            DepartmentButton.IsEnabled = false;
        }

        private void AccountingButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Main_Window.Accounting.LoginAccounting));
            AccountingButton.IsEnabled = false;
        }

        private void EnrolleeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Main_Window.Enrollee.EnrolleePage));
            EnrolleeButton.IsEnabled = false;
        }
    }
}
