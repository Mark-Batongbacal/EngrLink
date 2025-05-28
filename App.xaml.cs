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

namespace EngrLink
{
    public partial class App : Application
    {
        public static string CurrentProgram { get; set; } = "Current Program";

        public static Supabase.Client SupabaseClient;
        public static Window? MainWindow { get; set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            bool supabaseReady = await TryReinitializeSupabase();

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

        public static async Task<bool> TryReinitializeSupabase()
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
    }
}
