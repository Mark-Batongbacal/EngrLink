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
using EngrLink.Models;
using System.ComponentModel;


namespace EngrLink.Main_Window.Students.SubPages
{
    public sealed partial class Schedules : Page
    {
        public string Id { get; set; }
        public Schedules()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string Id)
            {
                this.Id = Id;
            }
            LoadSchedule();
        }
        private async void LoadSchedule()
        {
            var client = App.SupabaseClient;

            var response = await client
              .From<IndivSubject>()
              .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, this.Id)
              .Get();

            ScheduleListView.ItemsSource = response.Models;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
