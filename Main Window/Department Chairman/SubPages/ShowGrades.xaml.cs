using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink.Main_Window.Department_Chairman.SubPages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ShowGrades : Page
{
   
    protected async override void OnNavigatedTo(NavigationEventArgs e)
    {
        int Id = 0;
        base.OnNavigatedTo(e);
        
        if (e.Parameter is int studentId)
        {
            // Now you have the ID and can load student grades
            Debug.WriteLine($"Navigated with Student ID: {studentId}");
            Id = studentId;

            var client = App.SupabaseClient;

            // Only get students where Enrolled is false
            var response = await client
                .From<Subjects>()
                .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, Id)
                .Get();

            StudentsListView.ItemsSource = response.Models;
        }
    }

    private async void LoadStudents()
    {
        
    }

    public ShowGrades()
    {
        InitializeComponent();
    }
}
