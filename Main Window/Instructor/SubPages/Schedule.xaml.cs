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
using System.ComponentModel;
using global::EngrLink.Models;


namespace EngrLink.Main_Window.Instructor.SubPages
{
    public sealed partial class Schedule : Page
    {
        public string Profcode { get; set; }
        public string Program { get; set; }
        public string Name { get; set; }
        public Schedule()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is (string code, string name, string program))
            {
                this.Profcode = code;
                this.Name = name;
                this.Program = program;
            }
            LoadSchedule();
        }
        private async void LoadSchedule()
        {
            try
            {
                var client = App.SupabaseClient;

                var response = await client
                    .From<Subjects>()
                    .Filter("profcode", Supabase.Postgrest.Constants.Operator.Equals, this.Profcode)
                    .Order("program", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Order("year", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                ScheduleListView.ItemsSource = response.Models;
            }
            catch
            {
                Frame.Navigate(typeof(ErrorPage), (typeof(Dashboard), this.Program, this.Name));
            }
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

  
