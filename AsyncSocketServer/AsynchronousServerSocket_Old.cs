﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsyncSocketServer
{
    public class AsynchronousSocketListener_Old
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public delegate void onMessageReceivedComplete(object sender, string message);
        public delegate void onResponseMessageSent(object sender, string message);
        public static event onMessageReceivedComplete MessageReceivedComplete;
        public static event onResponseMessageSent ResponseMessageSent;

        public AsynchronousSocketListener_Old()
        {
        }

        public async static Task StartListening(string ps_IPAddress, int pi_Port)
        {
            IPAddress ipAddress = IPAddress.Parse(ps_IPAddress);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, pi_Port);

            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                await Task.Delay(100);
                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Debug.WriteLine("Waiting for a connection on " + ps_IPAddress + " at port " + pi_Port.ToString() + "...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

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
                Socket handler = listener.EndAccept(ar);

                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
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
                String content = String.Empty;

                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket.   
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read   
                    // more data.  
                    content = state.sb.ToString();
                    if (content.IndexOf("<EOF>") > -1)
                    {
                        // All the data has been read from the   
                        // client. Display it on the Debug.  
                        Debug.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                            content.Length, content);

                        Device.BeginInvokeOnMainThread(() => {
                            MessageReceivedComplete(null, content);
                        });

                        // Echo the data back to the client.  
                        Send(handler, content);
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
                string ls_SendMessage = data.Replace("<EOF>", " Received at " + DateTime.Now);

                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.ASCII.GetBytes(ls_SendMessage);

                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);

                Device.BeginInvokeOnMainThread(() => {
                    ResponseMessageSent(null, ls_SendMessage);
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
    }
}
