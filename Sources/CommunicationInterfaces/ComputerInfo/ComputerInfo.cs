using System;
using System.Collections.Generic;
using System.IO;
using System.Management;

namespace NetworkOperator.Informants
{
    public sealed class ComputerInfo
    {
        private static ComputerInfo current;

        public static ComputerInfo Current
        {
            get
            {
                if (current == null)
                {
                    current = new ComputerInfo();
                }
                return current;
            }
        }

        private ComputerInfo()
        {
        }

        private object performanceIndexCounterLock = new object();
        private uint performanceIndex = 0;
        public uint PerformanceIndex
        {
            get
            {
                lock (performanceIndexCounterLock)
                {
                    if (performanceIndex == 0)
                    {
                        foreach (var processor in Processors)
                        {
                            performanceIndex += processor.NumberOfLogicalProcessors * processor.MaxClockSpeed / 10;
                        }
                        ulong maxNetworkAdapterSpeed = 0;
                        foreach (var networkAdapter in NetworkAdapters)
                        {
                            maxNetworkAdapterSpeed = Math.Max(maxNetworkAdapterSpeed, networkAdapter.Speed);
                        }
                        performanceIndex += (uint)(maxNetworkAdapterSpeed / 1000000);
                        performanceIndex *= (uint)(((double)Memory.Capacity) / (1024 * 1024 * 1024));
                    }
                }
                return performanceIndex;
            }
        }

        private List<ProcessorInfo> processors;
        public List<ProcessorInfo> Processors
        {
            get
            {
                if (processors == null)
                {
                    processors = GetProcessorsInfo();
                }
                return processors;
            }
            private set
            {
                processors = value;
            }
        }
        private List<NetworkAdapterInfo> networkAdapters;
        public List<NetworkAdapterInfo> NetworkAdapters
        {
            get
            {
                if (networkAdapters == null)
                {
                    networkAdapters = GetNeworkAdaptersInfo();
                }
                return networkAdapters;
            }
            private set
            {
                networkAdapters = value;
            }
        }
        private PhysicalMemoryInfo memory;
        public PhysicalMemoryInfo Memory
        {
            get
            {
                if (memory == null)
                {
                    memory = GetPhysicalMemoryInfo();
                }
                return memory;
            }
        }
        private List<ProcessorInfo> GetProcessorsInfo()
        {
            ManagementObjectSearcher processorsSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            var processorsInfo = new List<ProcessorInfo>();
            uint maxClockSpeed, numberOfLogicalProcessors;
            foreach (var processor in processorsSearcher.Get())
            {
                maxClockSpeed = (uint)processor["MaxClockSpeed"];
                numberOfLogicalProcessors = (uint)processor["NumberOfLogicalProcessors"];//hyperthreading considered
                processorsInfo.Add(new ProcessorInfo { MaxClockSpeed = maxClockSpeed, NumberOfLogicalProcessors = numberOfLogicalProcessors });
            }
            return processorsInfo;
        }
        private List<NetworkAdapterInfo> GetNeworkAdaptersInfo()
        {
            ManagementObjectSearcher networAdaptersSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
            var networkAdaptersInfo = new List<NetworkAdapterInfo>();
            ulong speed = 0;
            bool isPhysical;
            foreach (var networkAdapter in networAdaptersSearcher.Get())
            {
                object retrievedSpeed = networkAdapter["Speed"];
                object retrievedIsPhysical = networkAdapter["PhysicalAdapter"];
                if (retrievedSpeed == null || retrievedIsPhysical == null)
                {
                    continue;
                }
                isPhysical = (bool)retrievedIsPhysical;
                speed = (ulong)retrievedSpeed;
                if (isPhysical && speed <= 100000000000)
                {
                    networkAdaptersInfo.Add(new NetworkAdapterInfo { Speed = speed });
                }
            }
            return networkAdaptersInfo;
        }
        private PhysicalMemoryInfo GetPhysicalMemoryInfo()
        {
            ManagementObjectSearcher physicalMemorySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            ulong capacity = 0;
            uint modules = 0;
            foreach (var physicalMemory in physicalMemorySearcher.Get())
            {
                ++modules;
                capacity += (ulong)physicalMemory["capacity"];
            }
            return new PhysicalMemoryInfo { Modules = modules, Capacity = capacity };
        }

        public void PrintComputerInfo(TextWriter output)
        {
            output.WriteLine($"This computer contains:");
            foreach (var processor in Processors)
            {
                output.WriteLine($"  {processor}");
            }
            foreach (var networkAdapter in NetworkAdapters)
            {
                output.WriteLine($"  {networkAdapter}");
            }
            output.WriteLine($"  {Current.Memory}");
            output.WriteLine($"=> its {nameof(PerformanceIndex)} is {PerformanceIndex}");
        }

        public override string ToString()
        {
            return $"ComputerInfo about computer with {nameof(PerformanceIndex)} {PerformanceIndex}";
        }
    }
}
