using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Network.Client
{
    internal class StateObject {  
        // Client socket.  
        public Socket WorkSocket;  
        // Size of receive buffer.  
        public int BufferSize = 256;  
        // Receive buffer.  
        public readonly byte[] Buffer = new byte[256];  
        // Received data string.  
        public readonly StringBuilder Sb = new StringBuilder();  
    }

    public class ClientAsync
    {
        /// <summary>
        /// Client socket
        /// </summary>
        private Socket Client { get; set; }
        
        /// <summary>
        /// Host ip
        /// </summary>
        private string Host { get; set; }
        
        /// <summary>
        /// Dest port
        /// </summary>
        private int Port { get; set; }
        
        /// <summary>
        /// Is the client connected
        /// </summary>
        public bool IsConnected => Client.Connected;
        
        /// <summary>
        /// Invokes when client is connected
        /// </summary>
        public event EventHandler Connected;
        
        /// <summary>
        /// Invokes when a message sent
        /// </summary>
        public event EventHandler<int> Sent;
        
        /// <summary>
        /// Invokes when received message
        /// </summary>
        public event EventHandler<string> Received;
        
        /// <summary>
        /// Invokes when error accrued
        /// </summary>
        public event EventHandler<ErrorEventArgs> ErrorAccrued; 

        public ClientAsync(string ip="127.0.01", int port=65432)
        {
            IPHostEntry host = Dns.GetHostEntry(ip);
            Host = ip;
            Port = port;
            Client = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public ClientAsync(Socket socket)
        {
            Client = socket;
        }
        
        /// <summary>
        /// Connect to host (not async)
        /// </summary>
        public void Connect()
        {
            if (Client.Connected) { return; }
            try
            {
                Client.Connect(Host, Port);
                Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e));
            }
        }
        
        /// <summary>
        /// Connect to host. When a connection is made, the connect event invokes
        /// </summary>
        public void ConnectAsync()
        {  
            if (Client.Connected) { return; } // abort if already connected 
            Client.BeginConnect(Host, Port, ConnectCallback, Client);
        }
        
        /// <summary>
        /// Start sending a message. When done, the sent event invokes
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <param name="encoding">Encoding to use</param>
        public void Send(String data, Encoding encoding)
        {  
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = encoding.GetBytes(data);  
  
            // Begin sending the data to the remote device.  
            Client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,  
                SendCallback, Client);  
        }  
        
        /// <summary>
        /// Start receiving a message. When done, the received event invokes
        /// </summary>
        /// <param name="bytes">Number of bytes to read</param>
        public void Receive(int bytes)
        {  
            try
            {  
                // Create the state object.  
                StateObject state = new StateObject {WorkSocket = Client, BufferSize = bytes};
                
                // Begin receiving the data from the remote device.  
                Client.BeginReceive( state.Buffer, 0,
                    state.BufferSize,
                    0, ReceiveCallback, state);  
            } 
            catch (Exception e)
            {  
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e)); // in case of an error, invoke ErrorAccrued event
            }
        }
        
        /// <summary>
        /// Close connection and socket
        /// </summary>
        public void Close()
        {
            try
            {
                if (!Client.Connected) { return; }
                Client.Close();
            }
            catch (SocketException e)
            {
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e)); // in case of an error, invoke ErrorAccrued event
            }
        }
        
        /// <summary>
        /// Callback to Receive function
        /// </summary>
        private void ReceiveCallback(IAsyncResult ar) 
        {  
            try {  
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject) ar.AsyncState;  
                Client = state.WorkSocket;  
                // Read data from the remote device.  
                int bytesRead = Client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.Sb.Append(Encoding.ASCII.GetString(state.Buffer,0,bytesRead));
                }
                if (bytesRead < state.BufferSize) 
                {
                    //  Get the rest of the data.  
                    Client.BeginReceive(state.Buffer,0, state.BufferSize,0,  
                                        ReceiveCallback, state);  
                } 
                else 
                {  
                    // All the data has arrived invoke Received event
                    if (state.Sb.Length >= state.BufferSize)
                    {  
                        Received?.Invoke(this, state.Sb.ToString());
                    }
                }  
            } catch (Exception e) {  
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e)); // in case of an error, invoke ErrorAccrued event
            }  
        }  
        
        /// <summary>
        /// Callback to Send function
        /// </summary>
        private void SendCallback(IAsyncResult ar)
        {  
            try {  
                // Retrieve the socket from the state object.  
                Client = (Socket) ar.AsyncState;  
  
                // Complete sending the data to the remote device.  
                int bytesSent = Client.EndSend(ar);  
                Sent?.Invoke(this, bytesSent);
            } catch (Exception e) {  
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e)); // in case of an error, invoke ErrorAccrued event
            }  
        } 
        
        /// <summary>
        /// Callback to Connect function
        /// </summary>
        private void ConnectCallback(IAsyncResult ar) 
        {  
            try {  
                // Retrieve the socket from the state object.  
                Client = (Socket) ar.AsyncState;
                // Complete the connection.  
                Client.EndConnect(ar);  
                Connected?.Invoke(this, EventArgs.Empty);
            } catch (Exception e) {  
                ErrorAccrued?.Invoke(this, new ErrorEventArgs(e)); // in case of an error, invoke ErrorAccrued event
            }  
        } 
    }
}