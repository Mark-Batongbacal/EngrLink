using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Supabase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EngrLink.Main_Window.Department_Chairman.SubPages
{
    public sealed partial class ViewSchedulePage : Page, INotifyPropertyChanged
    {
        private string facultyName;
        public string FacultyName
        {
            get => facultyName;
            set
            {
                if (facultyName != value)
                {
                    facultyName = value;
                    OnPropertyChanged(nameof(FacultyName));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public ObservableCollection<Subjects> Subjects { get; set; } = new ObservableCollection<Subjects>();

        public ViewSchedulePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Faculty faculty)
            {
                this.FacultyName = faculty.Program == "ARCHI" ? "Architect " + faculty.Name: "Engineer " + faculty.Name;

                _ = LoadSubjects(faculty.ProfCode);
            }
        }

        private async Task LoadSubjects(string profCode)
        {
            var client = App.SupabaseClient;

            if (client == null)
            {
                Debug.WriteLine("Error: SupabaseClient is not initialized.");
                return;
            }

            try
            {
                Debug.WriteLine($"Fetching subjects for ProfCode: {profCode}...");
                Debug.Write(this.FacultyName);
                var response = await client
                    .From<Subjects>()
                    .Filter("profcode", Supabase.Postgrest.Constants.Operator.Equals, profCode)
                    .Order("program", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Order("year", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Order("subject", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                if (response.Models != null)
                {
                    Subjects.Clear();
                    foreach (var subject in response.Models)
                    {
                        Subjects.Add(subject);
                    }

                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Error fetching subjects: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }
    }
}
