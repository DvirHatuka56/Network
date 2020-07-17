using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Network.Server
{
    public class Server
    {
        protected TcpListener Listener { get; }
        public virtual event EventHandler<Client> HandleClient; 

        public Server(string ip = "127.0.0.1", int port = 42069)
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(ip);
                Listener = new TcpListener(localAddr, port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public Server(TcpListener listener)
        {
            Listener = listener;
        }

        public void Start()
        {
            try
            {
                Listener.Start();   
                Thread listen = new Thread(Listen);
                listen.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected virtual void Listen()
        {
            try
            {
                while (true)
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    Thread handle = new Thread(() => HandleClient?.Invoke(this, new Client(client)));
                    handle.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public virtual void Close()
        {
            try
            {
                Listener.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
    }
}