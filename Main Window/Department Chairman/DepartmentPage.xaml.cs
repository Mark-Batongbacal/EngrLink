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


namespace EngrLink.Main_Window.Department_Chairman
{
    public sealed partial class DepartmentPage : Page
    {
        public string Program { get; set; }

        public DepartmentPage()
        {
            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string Program)
            {
                Debug.WriteLine($"Navigated with Program: {Program}");
                this.Program = Program;
            }
            DepartmentChairFrame.Navigate(typeof(Dashboard), (this.Program, ""));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
            var button = sender as Button;
            button.IsEnabled = false;

        }
  

        private void Schedules_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentChairFrame.Content?.GetType() != typeof(Schedules))
            {
                DepartmentChairFrame.Content = null;
                DepartmentChairFrame.Navigate(typeof(Schedules), this.Program);

            }
        }


        private void ListStudents_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentChairFrame.Content?.GetType() != typeof(ListOfStudents))
            { 
                DepartmentChairFrame.Content = null;
                DepartmentChairFrame.Navigate(typeof(ListOfStudents), this.Program);

            }
        }

        private void Announcements_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentChairFrame.Content?.GetType() != typeof(AnnouncementPage))
            {
                DepartmentChairFrame.Content = null;
                DepartmentChairFrame.Navigate(typeof(AnnouncementPage), this.Program);
            }
        }

        private void Enrollees_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentChairFrame.Content?.GetType() != typeof(Enrollees))
            {
                DepartmentChairFrame.Content = null;
                DepartmentChairFrame.Navigate(typeof(Enrollees), this.Program);
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            if (DepartmentChairFrame.Content?.GetType() != typeof(Dashboard))
            {
                DepartmentChairFrame.Content = null;
                DepartmentChairFrame.Navigate(typeof(Dashboard), (this.Program, ""));
            }
        }
    }
}
