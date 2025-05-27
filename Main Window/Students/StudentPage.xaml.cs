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
using EngrLink.Main_Window.Students.SubPages;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink.Main_Window.Students
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    
    public sealed partial class StudentPage : Page
    {
        public string Id { get; set; }
        public string Program { get; set; }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is (string Id, string Program))
            {
                Debug.WriteLine($"Navigated with Student ID: {Id} Program: {Program}");
                this.Id = Id;
                this.Program = Program;
            }
            StudentsFrame.Navigate(typeof(Dashboard), (this.Program,this.Id));
        }
        public StudentPage()
        {
            this.InitializeComponent();
           
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (StudentsFrame.Content?.GetType() != typeof(Dashboard))
            {
                StudentsFrame.Navigate(typeof(Dashboard), (this.Program, this.Id));
            }
        }

        private void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (StudentsFrame.Content?.GetType() != typeof(Schedules))
            {
                StudentsFrame.Navigate(typeof(Schedules), this.Id);
            }
        }

        private void AcadPerformanceButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (StudentsFrame.Content?.GetType() != typeof(AcadPerformancePage))
            {
                StudentsFrame.Navigate(typeof(AcadPerformancePage), this.Id);
            }
        }

        private void PerInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (StudentsFrame.Content?.GetType() != typeof(PersonalInfoPage))
            {
                StudentsFrame.Navigate(typeof(PersonalInfoPage), this.Id);
            }
        }

        private void FeesButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (StudentsFrame.Content?.GetType() != typeof(FeesPage))
            {
                StudentsFrame.Navigate(typeof(FeesPage), this.Id);
            }
        }
    }
}
