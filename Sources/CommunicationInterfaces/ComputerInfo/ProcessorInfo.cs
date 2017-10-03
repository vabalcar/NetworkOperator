namespace NetworkOperator.Informants
{
    public class ProcessorInfo
    {
        public uint NumberOfLogicalProcessors { get; set; }
        public uint MaxClockSpeed { get; set; }//MHz
        public override string ToString()
        {
            return $"processor with {NumberOfLogicalProcessors} logical cores working at {MaxClockSpeed}Mhz";
        }
    }
}
