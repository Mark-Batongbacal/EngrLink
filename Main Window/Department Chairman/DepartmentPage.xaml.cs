using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using EngrLink.Main_Window.Department_Chairman.SubPages;
using EngrLink.Main_Window.Students;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink.Main_Window.Department_Chairman
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DepartmentPage : Page
    {
        public DepartmentPage()
        {
            this.InitializeComponent();
            DepartmentChairFrame.Navigate(typeof(Dashboard));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
  

        private void Schedules_Click(object sender, RoutedEventArgs e)
        {
            //if (DepartmentChairFrame.Content?.GetType() != typeof(Schedules))
            //{
            //    DepartmentChairFrame.Content = null;
            //    DepartmentChairFrame.Navigate(typeof(Schedules));
            //}
        }
       

        private void ListStudents_Click(object sender, RoutedEventArgs e)
        {

            if (DepartmentChairFrame.Content?.GetType() != typeof(ListOfStudents))
            {
                DepartmentChairFrame.Content = null;
                DepartmentChairFrame.Navigate(typeof(ListOfStudents));
            }
        }

        private void ListFaculty_Click(object sender, RoutedEventArgs e)
        {
            //if (DepartmentChairFrame.Content?.GetType() != typeof(ListOfFaculty))
            //{
            //    DepartmentChairFrame.Content = null;
            //    DepartmentChairFrame.Navigate(typeof(ListOfFaculty));
            //}
        }

        private void Announcements_Click(object sender, RoutedEventArgs e)
        {
            //if (DepartmentChairFrame.Content?.GetType() != typeof(AnnouncementPage))
            //{
            //    if (DepartmentChairFrame.CanGoBack)
            //    {
            //        DepartmentChairFrame.GoBack();
            //    }
            //    DepartmentChairFrame.Navigate(typeof(AnnouncementPage));
            //}
        }

        private void Enrollees_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentChairFrame.Content?.GetType() != typeof(Enrollees))
            {
                DepartmentChairFrame.Content = null;
                DepartmentChairFrame.Navigate(typeof(Enrollees));
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentChairFrame.Content?.GetType() != typeof(Dashboard))
            {
                DepartmentChairFrame.Content = null;
                DepartmentChairFrame.Navigate(typeof(Dashboard));
            }
        }
    }
}
