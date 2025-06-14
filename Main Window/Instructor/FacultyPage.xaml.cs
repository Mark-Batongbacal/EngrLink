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
using EngrLink.Main_Window.Instructor.SubPages;
using EngrLink.Main_Window.Department_Chairman.SubPages;
using System.Diagnostics;


namespace EngrLink.Main_Window.Instructor
{
    public sealed partial class FacultyPage : Page
    {
        public string Profcode { get; set; }
        public string Id { get; set; }

        public string Program { get; set; }
        
        public string Name { get; set; }
        public FacultyPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is (string code, string id, string prog, string name))
            {
                this.Profcode = code;
                this.Id = id;
                this.Program = prog;
                this.Name = name;
            }
            FacultyFrame.Navigate(typeof(Instructor.SubPages.Dashboard), (this.Program, this.Name));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (FacultyFrame.Content?.GetType() != typeof(Instructor.SubPages.Dashboard))
            {
                FacultyFrame.Navigate(typeof(Instructor.SubPages.Dashboard), (this.Program,this.Name));
            }
        }

        private void ListStudents_Click(object sender, RoutedEventArgs e)
        {

            if (FacultyFrame.Content?.GetType() != typeof(ListStudents))
            {
                FacultyFrame.Content = null;
                FacultyFrame.Navigate(typeof(ListStudents), (this.Profcode,this.Program, this.Name));
            }
        }

        private void Schedules_Click(object sender, RoutedEventArgs e)
        {
            if (FacultyFrame.Content?.GetType() != typeof(Schedule))
            {
                FacultyFrame.Content = null;
                FacultyFrame.Navigate(typeof(Schedule), (this.Profcode,this.Name, this.Program));
            }
        }
    }
}
