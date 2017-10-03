using System;
using System.Diagnostics;

using NetworkOperator.Informants;

namespace ComputerInfoDisplayer
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ComputerInfo.Current.PrintComputerInfo(Console.Out);
            sw.Stop();
            Console.WriteLine("Retriving information comleted.");
            Console.Write("Elapsed time: ");
            WriteLine($"{sw.ElapsedMilliseconds}ms", ConsoleColor.Green);
            Console.Write("Press any key to close . . . ");
            Console.ReadKey();
        }
        static void WriteLine(string line, ConsoleColor color) => Write($"{line}\n", color);
        static void Write(string s, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(s);
            Console.ForegroundColor = originalColor;
        }
    }
}
