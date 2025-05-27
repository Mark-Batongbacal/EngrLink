using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Supabase;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EngrLink
{
    public partial class App : Application
    {
        

        public static Supabase.Client SupabaseClient;

        public App()
        {
            this.InitializeComponent();
        }
        public static Window? MainWindow { get; private set; }
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {

            bool supabaseReady = await TryInitializeSupabase();

            if (supabaseReady)
            {
                MainWindow = new MainWindow();
                MainWindow.Activate();
            }
            else
            {
                var errorWindow = new ConnectionErrorWindow();
                errorWindow.Activate();
            }
        }

        private async Task<bool> TryInitializeSupabase()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            try
            {
                SupabaseClient = new Supabase.Client(
                    "https://dpouedmzpftnpodbopbi.supabase.co",
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRwb3VlZG16cGZ0bnBvZGJvcGJpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDUwNDI2ODMsImV4cCI6MjA2MDYxODY4M30.XeZs98NROksWaNqE_q1HrgdxTLZ-Wmogwz4bWi4d_6s",
                    options
                );

                await SupabaseClient.InitializeAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Supabase init failed: " + ex.Message);
                SupabaseClient = null;
                return false;
            }
        }




        private Window? m_window;
    }
}
