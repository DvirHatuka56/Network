using System;
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
                    ThreadPool.QueueUserWorkItem(state => HandleClient?.Invoke(this, client));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void Close()
        {
            try
            {
                Listener.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine((e));
            }
        }
    }
}