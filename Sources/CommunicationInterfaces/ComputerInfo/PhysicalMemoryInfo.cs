namespace NetworkOperator.Informants
{
    public class PhysicalMemoryInfo
    {
        public uint Modules { get; set; }
        public ulong Capacity { get; set; }//B
        public override string ToString()
        {
            return $"{Modules} modules of physical memory with total capacity {Capacity}B";
        }
    }
}
