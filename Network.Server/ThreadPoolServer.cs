﻿using System;
using System.Threading;

namespace Network.Server
{
    public class ThreadPoolServer : Server
    {
        public override event EventHandler<Client.Client> HandleClient;
        
        public ThreadPoolServer(string ip = "127.0.0.1", int port = 42069) : base(ip, port) { }
        
        protected override void Listen()
        {
            try
            {
                while (true)
                {
                    ThreadPool.GetAvailableThreads(out int workerThreads, out _);
                    if (workerThreads == 0) { continue; }

                    var client = new Client.Client(Listener.AcceptTcpClient());
                    Console.WriteLine($"Client connected No.{ConnectedClients()}");
                    ThreadPool.QueueUserWorkItem(state => HandleClient?.Invoke(this, client));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private int ConnectedClients()
        {
            ThreadPool.GetAvailableThreads(out int available, out _);
            ThreadPool.GetMaxThreads(out int max, out _);
            return max - available;
        }

        public override void Close()
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