using System;
using System.Threading;

namespace Network.Server
{
    public class ThreadPoolServer : Server
    {
        private int MaxWorkers { get; }
        private int MinWorkers { get; }
        public override event EventHandler<Client> HandleClient;
        
        public ThreadPoolServer()
        {
            ThreadPool.GetMaxThreads(out int workerThreads, out _);
            MaxWorkers = workerThreads;
            ThreadPool.GetMinThreads(out workerThreads, out _);
            MinWorkers = workerThreads;
        }
        
        protected override void Listen()
        {
            try
            {
                while (true)
                {
                    ThreadPool.GetAvailableThreads(out int workerThreads, out _);
                    if (workerThreads == 0) { continue; }

                    var client = new Client(Listener.AcceptTcpClient());
                    Console.WriteLine($"Client connected No.{Connected()}");
                    ThreadPool.QueueUserWorkItem(state => HandleClient?.Invoke(this, client));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private int Connected()
        {
            ThreadPool.GetAvailableThreads(out int threads, out _);
            return MaxWorkers - threads;
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