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
using Microsoft.UI.Xaml.Media; // Needed for SolidColorBrush
using Microsoft.UI.Xaml.Navigation;
using EngrLink.Models; // Ensure your Models namespace is included
using System.ComponentModel; // For INotifyPropertyChanged
using System.Diagnostics; // For Debug.WriteLine
using System.Globalization; // Needed for CultureInfo for currency formatting

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

        public FeesPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            // Initialize all display properties with Peso sign
            FormattedTotalFees = 0.ToString("C2", new CultureInfo("en-PH")); // Explicitly use Philippine Peso culture
            FormattedPaidAmount = 0.ToString("C2", new CultureInfo("en-PH")); // Explicitly use Philippine Peso culture
            FormattedRemainingFees = 0.ToString("C2", new CultureInfo("en-PH")); // Explicitly use Philippine Peso culture
            RemainingFeesColor = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 165, 0)); // Default Orange
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int studentIdInt)
            {
                this.StudentId = studentIdInt;
                Debug.WriteLine($"Navigated to FeesPage with Student ID (int): {this.StudentId}");
                await LoadFeesInfo();
            }
            else if (e.Parameter is string studentIdString && int.TryParse(studentIdString, out int parsedId))
            {
                this.StudentId = parsedId;
                Debug.WriteLine($"Navigated to FeesPage with Student ID (parsed string): {this.StudentId}");
                await LoadFeesInfo();
            }
            else
            {
                Debug.WriteLine("Error: Student ID not passed or not a valid integer to FeesPage.");
                CurrentStudent = new Student { Name = "Invalid ID", Total = 0, Fees = 0 };
            }
        }

        private async System.Threading.Tasks.Task LoadFeesInfo()
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
                CurrentStudent = new Student { Name = "Error Loading", Total = 0, Fees = 0 };
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
            FormattedTotalFees = totalFees.ToString("C2", new CultureInfo("en-PH")); // Use en-PH

            int remainingBalance = CurrentStudent.Fees ?? 0;
            FormattedRemainingFees = remainingBalance.ToString("C2", new CultureInfo("en-PH")); // Use en-PH

            int paidAmount = totalFees - remainingBalance;
            FormattedPaidAmount = paidAmount.ToString("C2", new CultureInfo("en-PH")); // Use en-PH

            // Update the color based on the remaining balance
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