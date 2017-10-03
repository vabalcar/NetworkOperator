using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NetworkOperator.Core
{
    public class ConfigurationManager
    {
        public const string CONFIGURATION_DIRECTORY = "Configuration";
        public const char SEPARATOR = ';';
        private static ConfigurationManager current;
        public static ConfigurationManager Current
        {
            get
            {
                if (current == null)
                {
                    current = new ConfigurationManager();
                }
                return current;
            }
        }
        private ConfigurationManager()
        {
            if (!Directory.Exists(CONFIGURATION_DIRECTORY))
            {
                Directory.CreateDirectory(CONFIGURATION_DIRECTORY);
            }
        }

        public void StoreConfiguration<T>(IEnumerable<T> configuration, [CallerFilePath] string callerPath = "")
            => StoreConfiguration(configuration, ProcessCallerPath(callerPath), true);
        public void StoreConfiguration<T>(IEnumerable<T> configuration, string configurationFileName, bool autoCompleteName)
        {
            IEnumerable<string> AutoConvertingEnumeration()
            {
                foreach (var item in configuration)
                {
                    yield return item.ToString();
                }
            };
            StoreLines(AutoConvertingEnumeration(), configurationFileName, autoCompleteName);
        }

        public void StoreConfiguration<T>(T[,] configuration, [CallerFilePath] string callerPath = "")
            => StoreConfiguration(configuration, ProcessCallerPath(callerPath), true);
        public void StoreConfiguration<T>(T[,] configuration, string configurationFileName, bool autoCompleteName)
        {
            IEnumerable<string> AutoConvertingEnumeration()
            {
                yield return $"{configuration.GetLength(0)}{SEPARATOR}{configuration.GetLength(1)}";
                for (int i = 0; i < configuration.GetLength(0); i++)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < configuration.GetLength(1); j++)
                    {
                        sb.Append(configuration[i, j]);
                        if (j < configuration.GetLength(1) - 1)
                        {
                            sb.Append(SEPARATOR);
                        }
                    }
                    yield return sb.ToString();
                }
            };
            StoreLines(AutoConvertingEnumeration(), configurationFileName, autoCompleteName);
        }

        private void StoreLines(IEnumerable<string> lines, string fileName, bool autoCompleteName)
        {
            StreamWriter writer = new StreamWriter(new FileStream(GetPath(fileName, autoCompleteName), FileMode.Create));
            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
            writer.Flush();
            writer.Dispose();
        }

        public bool IsConfigurationAvailable([CallerFilePath] string callerPath = "")
            => IsConfigurationAvailable(ProcessCallerPath(callerPath), true);
        public bool IsConfigurationAvailable(string configurationFileName, bool autoCompleteName)
            => File.Exists(GetPath(configurationFileName, autoCompleteName));

        public List<string> LoadList<T>([CallerFilePath] string callerPath = "")
            => LoadList(s => s.ToString(), ProcessCallerPath(callerPath), true);
        public List<T> LoadList<T>(Func<string, T> parser, [CallerFilePath] string callerPath = "")
            => LoadList(parser, ProcessCallerPath(callerPath), true);
        public List<T> LoadList<T>(Func<string, T> parser, string configurationFileName, bool autoCompleteName)
        {
            VerifyFileExistence(configurationFileName, autoCompleteName);
            var list = new List<T>();
            foreach (var line in File.ReadLines(GetPath(configurationFileName, autoCompleteName)))
            {
                list.Add(parser(line));
            }
            return list;
        }

        public string[,] LoadMatrix<T>([CallerFilePath] string callerPath = "")
            => LoadMatrix(s => s.ToString(), ProcessCallerPath(callerPath), true);
        public T[,] LoadMatrix<T>(Func<string, T> parser, [CallerFilePath] string callerPath = "")
            => LoadMatrix(parser, ProcessCallerPath(callerPath), true);
        public T[,] LoadMatrix<T>(Func<string, T> parser, string configurationFileName, bool autoCompleteName)
        {
            VerifyFileExistence(configurationFileName, autoCompleteName);
            T[,] matrix = null;
            bool firstFileLine = true;
            int matrixRow = 0;
            foreach (var line in File.ReadLines(GetPath(configurationFileName, autoCompleteName)))
            {
                string[] tokens;
                if (firstFileLine)
                {
                    firstFileLine = false;
                    tokens = line.Split(SEPARATOR);
                    if (tokens.Length == 2 
                        && int.TryParse(tokens[0], out int matrixHeight) 
                        && int.TryParse(tokens[1], out int matrixWidth))
                    {
                        matrix = new T[matrixHeight, matrixWidth];
                        continue;
                    }
                    else
                    {
                        ThrowParseException(configurationFileName, autoCompleteName);
                    }
                }
                tokens = line.Split(SEPARATOR);
                if (tokens.Length != matrix.GetLength(1))
                {
                    ThrowParseException(configurationFileName, autoCompleteName);
                }
                for (int i = 0; i < tokens.Length; i++)
                {
                    matrix[matrixRow, i] = parser(tokens[i]);
                }
                ++matrixRow;
            }
            return matrix;
        }

        private void ThrowParseException(string wronglyFormatedFileName, bool autoCompleteName)
            => throw new FormatException($"Configuration file {GetPath(wronglyFormatedFileName, autoCompleteName)} is in wrong format!");
        private string ProcessCallerPath(string callerPath)
        {
            int lastSlashIndex = callerPath.LastIndexOf('\\');
            int lastDotIndex = callerPath.LastIndexOf('.');
            return callerPath.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);
        }
        private string GetPath(string configurationFileName, bool autoCompleteName)
        {
            if (autoCompleteName)
            {
                return $"{CONFIGURATION_DIRECTORY}//{configurationFileName}.conf";
            }
            return $"{CONFIGURATION_DIRECTORY}//{configurationFileName}";
        }
        private void VerifyFileExistence(string file, bool autoCompleteName)
        {
            var path = GetPath(file, autoCompleteName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File {path} doesn't exist.");
            }
        }
    }
}
