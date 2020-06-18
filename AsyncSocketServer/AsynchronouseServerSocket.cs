using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.Generic;

namespace AsyncSocketServer
{
    public class AsynchronousSocketListener
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public delegate void onMessageReceivedComplete(object sender, string message);
        public delegate void onResponseMessageSent(object sender, string message);
        public static event onMessageReceivedComplete MessageReceivedComplete;
        public static event onResponseMessageSent ResponseMessageSent;
        public AsynchronousSocketListener()
        {
        }

        public async static Task StartListening(IPAddress pobj_IPAddress, int pi_Port)
        {
            try
            {

                //IPAddress ipAddress = IPAddress.Parse(pobj_IPAddress);
                IPEndPoint localEndPoint = new IPEndPoint(pobj_IPAddress, pi_Port);

                Socket listener = new Socket(pobj_IPAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Bind the socket to the local endpoint and listen for incoming connections.  
                listener.Bind(localEndPoint);
                listener.Listen(100);
                //ViewModelObjects.AppSettings.SocketStatus = ge_SocketStatus.e_Listening;
                await Task.Delay(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Debug.WriteLine("Waiting for a connection on " + pobj_IPAddress + " at port " + pi_Port.ToString() + "...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("StartListening Error" + e.ToString());
            }

            Debug.WriteLine("Read To end class");

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.  
                allDone.Set();

                // Get the socket that handles the client request.  
                Socket listener = (Socket)ar.AsyncState;
                //If we have shut down the socket dont do this.  
                Socket handler = listener.EndAccept(ar);

                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

            }
            catch (Exception e)
            {
                Debug.WriteLine("AcceptCallback Error" + e.ToString());
            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                string ls_ReceivedCommunicationContent = string.Empty;
                string ls_ReturnCommunicationContent = string.Empty;
                //string content = string.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read   
                    // more data.  
                    ls_ReceivedCommunicationContent = state.sb.ToString();
                    if (ls_ReceivedCommunicationContent.IndexOf("<EOF>") > -1)
                    {
                        //We need to take off the end of file marker
                        string ls_WorkContent = ls_ReceivedCommunicationContent.Replace("<EOF>", "");

                        ls_ReturnCommunicationContent = ls_WorkContent;

                        //Different than app
                        Device.BeginInvokeOnMainThread(() => {
                            MessageReceivedComplete(null, ls_WorkContent);
                        });

                        Send(handler, ls_ReturnCommunicationContent);
                    }
                    else
                    {
                        // Not all data received. Get more.  
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ReadCallback Error" + e.ToString());
            }
        }

        private static void Send(Socket handler, String data)
        {
            try
            {
                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);

                Device.BeginInvokeOnMainThread(() => {
                    ResponseMessageSent(null, data);
                });

            }
            catch (Exception e)
            {
                Debug.WriteLine("Send Error" + e.ToString());
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Debug.WriteLine("SendCallback Error" + e.ToString());
            }
        }

        //public static async Task StopListening()
        //{
        //    try
        //    {
        //        if (iobj_listener.Connected)
        //        {
        //            //Wait till the connection ends or 30 seconds - this is so any last messages can be processed.
        //            await Task.Delay(30000);

        //        }
        //        ViewModelObjects.AppSettings.SocketStatus = ge_SocketStatus.e_NotListening;
        //        iobj_listener.Close(1);

        //    }
        //    catch (Exception ex)
        //    {
        //        App.AppException(ex);
        //    }
        //}
    }
}
