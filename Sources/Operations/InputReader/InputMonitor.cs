using NetworkOperator.IO;
using System;
using System.IO;

namespace InputReader
{
    class InputMonitor : IDisposable
    {
        private Mouse mouse = new Mouse(25);
        private Keyboard keyboard = new Keyboard();
        private TextWriter outputWriter;

        public InputMonitor(TextWriter outputWriter)
        {
            this.outputWriter = outputWriter;
            mouse.OnMouseEvent += e => PrintTimeStamp(e.ToString());
            mouse.OnMoved += e => PrintTimeStamp(e.ToString());
            keyboard.OnKeyStateChanged += e => PrintTimeStamp(e.ToString());
        }
        private void PrintTimeStamp(string message) => outputWriter.WriteLine($"{DateTime.Now}: {message}");
        public void Dispose()
        {
            mouse.Dispose();
            keyboard.Dispose();
        }
    }
}
