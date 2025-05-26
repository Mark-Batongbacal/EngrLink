using EngrLink.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Supabase;
using Supabase.Postgrest;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace EngrLink.Main_Window.Instructor
{
    public sealed partial class FacultyPage : Page
    {
        public FacultyPage()
        {
            this.InitializeComponent();
        }

    }
}
