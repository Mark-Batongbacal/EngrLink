using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginStudent : Page
    {
        public LoginStudent()
        {
            this.InitializeComponent();
        }

        private void CheckValid()
        {
            {
                // Check if all fields are filled
                bool isValid = !string.IsNullOrWhiteSpace(StudentID.Text) &&
                               !string.IsNullOrWhiteSpace(Password.Password);

                // Enable/Disable the Submit button based on validity
                SubmitButton.IsEnabled = isValid;
            }
        }
        private void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            CheckValid();
        }

        private void SubmitButton_Click (object sender, RoutedEventArgs e)
        {

        }
    }
}
