using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsyncSocketServer
{
    public partial class App : Application
    {
        public const string IPV4Address = "192.168.1.19";
        public const string IPV6Address = "192.168.1.19";
        public const int Port = 8080;

        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static void ProcessException(Exception pobj_Exception)
        {
            Debug.WriteLine(pobj_Exception.Message);
        }
    }
}
