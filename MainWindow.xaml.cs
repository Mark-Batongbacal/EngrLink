using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Supabase.Postgrest;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Collecting values from the input fields
            string name = NameTextBox.Text;
            string address = AddressTextBox.Text;
            string contact = ContactTextBox.Text;  // You might want to handle errors here (e.g., try-catch)
            string program = ProgramTextBox.Text;
            string birthday = BirthdayDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");  // Ensure the user selected a date
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
                Year = 2,
                Fees = 12312,
                Program = program,
                Birthday = birthday,
                Enrolled = false,  // You can change the default based on your requirement
                Paid = false,      // You can change this as needed
                Regular = false    // You can change this as needed
            };

            // Inserting the new student into the database
            var response = await App.SupabaseClient
                .From<Student>()
                .Insert(newStudent);
            Console.WriteLine("TITE");
            this.Close();
        }
    }
}
