using System;
using System.Collections.Generic;
using System.IO;

namespace CodeDataSizeCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Argument error");
                return;
            }
            bool verboseOutput = false;
            string fullPath = string.Empty;
            string excludesFile = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        if(!Directory.Exists(args[i]))
                        {
                            Console.WriteLine("Invalid directory for searching for files");
                            return;
                        }
                        fullPath = Path.GetFullPath(args[i]);
                        break;
                    case 1:
                        if (!File.Exists(args[i]))
                        {
                            Console.WriteLine("Invalid excludes file");
                            return;
                        }
                        excludesFile = Path.GetFullPath(args[i]);
                        break;
                    case 2:
                        if (args[i] == "-v")
                        {
                            verboseOutput = true;
                        }
                        else
                        {
                            goto default;
                        }
                        break;
                    default:
                        Console.SetWindowSize(Console.LargestWindowWidth * 9 / 10, Console.LargestWindowHeight - 1);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Looking for *.{args[i]} files...");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"Total size of *.{args[i]} files in {fullPath} and all its subdirectories is ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(CountDataMetrics(fullPath, $".{args[i]}", GetExcludes(excludesFile), verboseOutput));
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine('.');
                        break;
                }
            }
            Console.Write("Press any key to close to program . . . ");
            Console.ReadKey();
            Console.WriteLine();
        }
        static DataMetrics CountDataMetrics(string path, string extension, IList<string> excludes, bool verboseOutput)
        {
            var metrics = new DataMetrics();
            var directoryInfo = new DirectoryInfo(path);
            if (verboseOutput)
            {
                Console.WriteLine(directoryInfo.FullName);
            }
            foreach (var file in directoryInfo.GetFiles())
            {
                if(file.Extension == extension && !excludes.Contains(file.Name.Substring(0, file.Name.IndexOf('.'))))
                {
                    metrics.Length += file.Length;
                    metrics.Lines += CountLines(file);
                    if (verboseOutput)
                    {
                        Console.WriteLine($"\t{file.Name}: {file.Length / 1024}kB");
                    }
                }
            }
            var directories = new Stack<DirectoryInfo>();
            foreach (var directory in directoryInfo.GetDirectories())
            {
                if (!excludes.Contains(directory.Name))
                {
                    directories.Push(directory);
                }
            }
            while(directories.Count != 0)
            {
                var directoryMetrics = CountDataMetrics(directories.Pop().FullName, extension, excludes, verboseOutput);
                metrics.Length += directoryMetrics.Length;
                metrics.Lines += directoryMetrics.Lines;
            }
            return metrics;
        }
        static long CountLines(FileInfo file)
        {
            long lines = 0;
            foreach (var line in File.ReadLines(file.FullName))
            {
                ++lines;
            }
            return lines;
        }

        static IList<string> GetExcludes(string excludesFile)
        {
            List<string> excludes = new List<string>();
            foreach (var line in File.ReadLines(excludesFile))
            {
                excludes.Add(line);
            }
            return excludes;
        }
    }
    class DataMetrics
    {
        public long Length { get; set; }
        public long Lines { get; set; }
        public override string ToString() => $"{Length / 1024}kB ({Lines} {nameof(Lines)})";
    }
}
