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
                bool isValid = !string.IsNullOrWhiteSpace(NameTextBox.Text) &&
                               !string.IsNullOrWhiteSpace(AddressTextBox.Text) &&
                               !string.IsNullOrWhiteSpace(ContactTextBox.Text) &&
                               ProgramComboBox.SelectedItem is ComboBoxItem programItem &&
                               YearLevelComboBox.SelectedItem is ComboBoxItem yearItem &&
                               BirthdayDatePicker.SelectedDate.HasValue;

                SubmitButton.IsEnabled = isValid;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;

            if (Frame.CanGoBack)
                Frame.GoBack();
        }
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string address = AddressTextBox.Text;
            string contact = ContactTextBox.Text; 
            string birthday = BirthdayDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"); 
            int units = 0;
            var lastStudentResponse = await App.SupabaseClient
            .From<Student>()
            .Order("id", Supabase.Postgrest.Constants.Ordering.Descending) 
            .Limit(1)
            .Get();

            int newId = 1; 

            if (lastStudentResponse.Models.Count > 0)
            {
               
                var lastStudent = lastStudentResponse.Models[0];
                newId = lastStudent.Id + 1;
            }

            var fee = FeeCalculators.GetFee(year, program);

            var newStudent = new Student()
            {
                Id = newId,
                Name = name,
                Address = address,
                Contact = contact,
                Year = year,
                Fees = fee,
                Total = fee,
                Program = program,
                Password = newId.ToString(),
                Birthday = birthday,
                Enrolled = false,  
                Paid = false,    
                  
            };

            var response = App.SupabaseClient
                .From<Student>()
                .Insert(newStudent);
            await response;

            var dialog = new ContentDialog
            {
                Title = "Submission Successful",
                Content = $"Your Student ID is {newId}. Remember this for your Login.\n" +
                          $"Your Total Balance is ₱{fee}\n" +
                          $"Please pay a minimum amount of ₱5000\n",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
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

            program = null;
            year = null;

            SubmitButton.IsEnabled = false;
            Frame.GoBack(); 

        }
        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckValid();
        }

        private void NumberOnly_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            int caretIndex = textBox.SelectionStart;

            string filteredText = new string(textBox.Text.Where(char.IsDigit).ToArray());

            if (textBox.Text != filteredText)
            {
                textBox.Text = filteredText;
                textBox.SelectionStart = Math.Min(caretIndex, textBox.Text.Length);
            }
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

            if (program == "ARCHI")
                FifthYearItem.Visibility = Visibility.Visible;
            
            else
            {
                if (YearLevelComboBox.SelectedItem == FifthYearItem)
                {
                    YearLevelComboBox.SelectedIndex = -1;
                    YearLevelComboBox.Header = "Enter Year Level";
                    year = null;
                }
                FifthYearItem.Visibility = Visibility.Collapsed;
            }
                

            CheckValid();
        }
    }
}