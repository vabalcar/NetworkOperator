namespace NetworkOperator.Informants
{
    public class NetworkAdapterInfo
    {
        public ulong Speed { get; set; }//b/s
        public override string ToString()
        {
            return $"network adapter with bandwidth {Speed}b/s";
        }
    }
}
