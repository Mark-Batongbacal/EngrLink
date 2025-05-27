using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Supabase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Markup;

namespace EngrLink.Main_Window.Department_Chairman.SubPages
{
    [ContentProperty(Name = "Content")]
    public sealed partial class ViewSchedulePage : Page
    {
        public Faculty Fac { get; set; }

        public ViewSchedulePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Faculty faculty)
            {
                Fac = faculty;
                DataContext = Fac;
            }
        }

        private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
