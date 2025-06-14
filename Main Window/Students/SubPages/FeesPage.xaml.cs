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
using System.Globalization; 

namespace EngrLink.Main_Window.Students.SubPages
{
    public sealed partial class FeesPage : Page, INotifyPropertyChanged
    {
        public int StudentId { get; set; }

        private Student _currentStudent;
        public Student CurrentStudent
        {
            get => _currentStudent;
            set
            {
                if (_currentStudent != value)
                {
                    _currentStudent = value;
                    OnPropertyChanged(nameof(CurrentStudent));
                    UpdateDisplayValues();
                }
            }
        }

        private string _formattedTotalFees;
        public string FormattedTotalFees
        {
            get => _formattedTotalFees;
            set
            {
                if (_formattedTotalFees != value)
                {
                    _formattedTotalFees = value;
                    OnPropertyChanged(nameof(FormattedTotalFees));
                }
            }
        }

        private string _formattedPaidAmount;
        public string FormattedPaidAmount
        {
            get => _formattedPaidAmount;
            set
            {
                if (_formattedPaidAmount != value)
                {
                    _formattedPaidAmount = value;
                    OnPropertyChanged(nameof(FormattedPaidAmount));
                }
            }
        }

        private string _formattedRemainingFees;
        public string FormattedRemainingFees
        {
            get => _formattedRemainingFees;
            set
            {
                if (_formattedRemainingFees != value)
                {
                    _formattedRemainingFees = value;
                    OnPropertyChanged(nameof(FormattedRemainingFees));
                }
            }
        }

        private SolidColorBrush _remainingFeesColor;
        public SolidColorBrush RemainingFeesColor
        {
            get => _remainingFeesColor;
            set
            {
                if (_remainingFeesColor != value)
                {
                    _remainingFeesColor = value;
                    OnPropertyChanged(nameof(RemainingFeesColor));
                }
            }
        }

        public string Program { get; set; }
        public string Id { get; set; }
        public FeesPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            FormattedTotalFees = 0.ToString("C2", new CultureInfo("en-PH")); 
            FormattedPaidAmount = 0.ToString("C2", new CultureInfo("en-PH")); 
            FormattedRemainingFees = 0.ToString("C2", new CultureInfo("en-PH")); 
            RemainingFeesColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 165, 0));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is (string studentIdString, string program))
            {
                this.Id = studentIdString;
                this.StudentId = int.Parse(studentIdString);
                this.Program = program;
                Debug.WriteLine($"Navigated to FeesPage with Student ID (parsed string): {this.StudentId}");
                try
                {
                    await LoadFeesInfo();
                }
                catch
                {
                }
            }
            else
            {
                Debug.WriteLine("Error: Student ID not passed or not a valid integer to FeesPage.");
                CurrentStudent = new Student { Name = "Invalid ID", Total = 0, Fees = 0 };
            }
        }

        private async System.Threading.Tasks.Task LoadFeesInfo()
        {
            try
            {
                var client = App.SupabaseClient;

            
                var response = await client
                    .From<Student>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, this.StudentId)
                    .Single();

                if (response != null)
                {
                    CurrentStudent = response;
                    Debug.WriteLine($"Fees Info Loaded for {CurrentStudent.Name}");
                    Debug.WriteLine($"Raw DB Total (total_fees): {CurrentStudent.Total ?? 0}");
                    Debug.WriteLine($"Raw DB Fees (fees): {CurrentStudent.Fees ?? 0} (This is your Remaining Balance)");
                }
                else
                {
                    Debug.WriteLine($"No fees information found for student ID: {this.StudentId}");
                    CurrentStudent = new Student { Name = "N/A", Total = 0, Fees = 0 };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading fees information: {ex.Message}");
                Frame.Navigate(typeof(ErrorPage), (typeof(Dashboard), this.Program, this.Id));
            }
        }

        private void UpdateDisplayValues()
        {
            if (CurrentStudent == null)
            {
                FormattedTotalFees = 0.ToString("C2", new CultureInfo("en-PH"));
                FormattedPaidAmount = 0.ToString("C2", new CultureInfo("en-PH"));
                FormattedRemainingFees = 0.ToString("C2", new CultureInfo("en-PH"));
                RemainingFeesColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 165, 0));
                return;
            }

            int totalFees = CurrentStudent.Total ?? 0;
            FormattedTotalFees = totalFees.ToString("C2", new CultureInfo("en-PH")); 

            int remainingBalance = CurrentStudent.Fees ?? 0;
            FormattedRemainingFees = remainingBalance.ToString("C2", new CultureInfo("en-PH")); 

            int paidAmount = totalFees - remainingBalance;
            FormattedPaidAmount = paidAmount.ToString("C2", new CultureInfo("en-PH")); 

            if (remainingBalance == 0)
            {
                RemainingFeesColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 128, 0));
            }
            else if (remainingBalance < 0)
            {
                RemainingFeesColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 100, 0));
            }
            else
            {
                RemainingFeesColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 165, 0));
            }
            Debug.WriteLine($"Remaining Fees Color Updated to: {RemainingFeesColor}");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}