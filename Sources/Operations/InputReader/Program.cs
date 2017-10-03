using System;
using System.Threading;

namespace InputReader
{
    class Program
    {
        static void Main(string[] args)
        {
            InputMonitor monitor = new InputMonitor(Console.Out);
            bool running = true;
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                running = false;
            };
            while(running)
            {
                Thread.Sleep(100);
            }
            monitor.Dispose();
        }
    }
}
