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
using System.Diagnostics;

namespace EngrLink.Main_Window.Students.SubPages
{
    public sealed partial class PersonalInfoPage : Page, INotifyPropertyChanged
    {
        public string StudentId { get; set; }

        private Student _personalInfo;
        public Student PersonalInfo
        {
            get => _personalInfo;
            set
            {
                if (_personalInfo != value)
                {
                    _personalInfo = value;
                    OnPropertyChanged(nameof(PersonalInfo));
                }
            }
        }

        public PersonalInfoPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string studentId)
            {
                this.StudentId = studentId;
                Debug.WriteLine($"Navigated to PersonalInfoPage with Student ID: {this.StudentId}");
                await LoadPersonalInfo();
            }
        }

        private async System.Threading.Tasks.Task LoadPersonalInfo()
        {
            var client = App.SupabaseClient;

            try
            {
                var response = await client
                    .From<Student>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, this.StudentId)
                    .Single();

                if (response != null)
                {
                    PersonalInfo = response;
                    Debug.WriteLine($"Personal Info Loaded for {PersonalInfo.Name}");
                }
                else
                {
                    Debug.WriteLine($"No personal info found for student ID: {this.StudentId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading personal info: {ex.Message}");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}