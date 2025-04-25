using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using EngrLink.Models;
using System.Windows.Forms.VisualStyles;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Supabase;
using Supabase.Postgrest;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink.Main_Window.Enrollee
{
    public sealed partial class EnrolleePage : Page
    {
        public EnrolleePage()
        {
            this.InitializeComponent();
            CheckValid();
        }
        public string program;
        public string year;
        private void CheckValid()
        {

            {

                // Check if all fields are filled
                bool isValid = !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
                               !string.IsNullOrWhiteSpace(AddressTextBox.Text) &&
                               !string.IsNullOrWhiteSpace(ContactTextBox.Text) &&
                               ProgramComboBox.SelectedItem is ComboBoxItem programItem &&
                               YearLevelComboBox.SelectedItem is ComboBoxItem yearItem &&
                               BirthdayDatePicker.SelectedDate.HasValue; // Make sure a date is selected

                // Enable/Disable the Submit button based on validity
                SubmitButton.IsEnabled = isValid;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Collecting values from the input fields
            string name = NameTextBox.Text;
            string address = AddressTextBox.Text;
            string contact = ContactTextBox.Text;  // You might want to handle errors here (e.g., try-catch)
            string birthday = BirthdayDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");  // Ensure the user selected a date
            int units = 0; //placeholder only
            var lastStudentResponse = await App.SupabaseClient
            .From<Student>()
            .Order("id", Supabase.Postgrest.Constants.Ordering.Descending) // true means descending order to get the last inserted record
            .Limit(1)
            .Get();

            int newId = 1;  // Default to 1 if no students exist yet

            if (lastStudentResponse.Models.Count > 0)
            {
                // Increment the last student's ID
                var lastStudent = lastStudentResponse.Models[0];
                newId = lastStudent.Id + 1;
            }

            // Create a new student object
            var newStudent = new Student
            {
                Id = newId,
                Name = name,
                Address = address,
                Contact = contact,
                Year = year,
                Fees = 123123,
                Program = program,
                Password = newId.ToString(),
                Birthday = birthday,
                Enrolled = false,  // You can change the default based on your requirement
                Paid = false,      // You can change this as needed
                Regular = false    // You can change this as needed
            };

            // Inserting the new student into the database
            var response = App.SupabaseClient
                .From<Student>()
                .Insert(newStudent);
            await response;

            var dialog = new ContentDialog
            {
                Title = "Submission Successful",
                Content = $"Your Student ID is {newId}. Remember this for your Login.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot // Required in WinUI 3 to show dialog properly
            };

            await dialog.ShowAsync();

            NameTextBox.Text = "";
            AddressTextBox.Text = "";
            ContactTextBox.Text = "";
            ProgramComboBox.SelectedItem = null;
            YearLevelComboBox.SelectedItem = null;
            ProgramComboBox.Header = "Enter Program";
            YearLevelComboBox.Header = "Enter Year Level";
            BirthdayDatePicker.SelectedDate = null;

            // Also reset internal tracking variables
            program = null;
            year = null;

            // Disable submit button again
            SubmitButton.IsEnabled = false;

            //Frame.GoBack(); gawin m to marky dear

        }
        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckValid();
        }

        private void Input_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            CheckValid();
        }

        private void YearLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YearLevelComboBox.SelectedItem is ComboBoxItem yearitem)
            {
                YearLevelComboBox.Header = yearitem.Content;
                year = yearitem.Content.ToString();
            }
            CheckValid();
        }

        private void Program_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProgramComboBox.SelectedItem is ComboBoxItem programitem)
            {
                ProgramComboBox.Header = programitem.Content;
                program = programitem.Content.ToString();
            }
            CheckValid();
        }
    }
}