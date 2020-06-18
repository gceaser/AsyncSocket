using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsyncSocketServer
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private string is_IPAddress;

        public MainPage()
        {
            InitializeComponent();
            is_IPAddress = App.IPV4Address;
            AsynchronousSocketListener.MessageReceivedComplete += App_MessageReceivedComplete;
            AsynchronousSocketListener.ResponseMessageSent += App_ResponseMessageSent;
        }

        private void App_ResponseMessageSent(object sender, string message)
        {
            string ls_NewLine;

            if (txtMessageSendBack.Text.Trim().Length > 0)
                ls_NewLine = Environment.NewLine;
            else
                ls_NewLine = "";

            txtMessageSendBack.Text += ls_NewLine + message;
        }

        private void App_MessageReceivedComplete(object sender, string message)
        {
            string ls_NewLine;

            if (txtMessageReceived.Text.Trim().Length > 0)
                ls_NewLine = Environment.NewLine;
            else
                ls_NewLine = "";

            txtMessageReceived.Text += ls_NewLine + message;
        }

        private void btnStart_Clicked(object sender, System.EventArgs e)
        {
            Task.Run(async () =>
            {

                await AsynchronousSocketListener.StartListening(IPAddress.Parse(is_IPAddress), 8080);

            });

            lblIPAddress.Text = "Listening on " + is_IPAddress;
            lblPort.Text = "Port: " + App.Port.ToString() ;
        }

        private void swIPV6_Toggled(object sender, ToggledEventArgs e)
        {
            if (swIPV6.IsToggled)
            {
                is_IPAddress = App.IPV6Address;
            }
            else
            {
                is_IPAddress = App.IPV4Address;
            }
        }
    }
}
