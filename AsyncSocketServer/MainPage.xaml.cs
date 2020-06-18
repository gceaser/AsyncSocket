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
        
        public MainPage()
        {
            InitializeComponent();
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

                await AsynchronousSocketListener.StartListening(IPAddress.Parse("fe80::cda4:ea52:29f5:2c7c"), 8080);

                //await AsynchronousSocketListener.StartListening(IPAddress.Parse("192.168.1.19"), 8080);
            });

            //lblIPAddress.Text = "Listening on 192.168.1.19";
            //lblPort.Text = "Port: 8080";

            lblIPAddress.Text = "Listening on fe80::cda4:ea52:29f5:2c7c";
            lblPort.Text = "Port: 8080";
        }
    }
}
