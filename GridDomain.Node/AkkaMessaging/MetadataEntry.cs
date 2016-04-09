namespace GridDomain.Node.AkkaMessaging
{
    public class MetadataEntry
    {
        public string Who { get; }
        public string Why { get; }
        public string What { get; }

        public MetadataEntry(string who, string what, string why)
        {
            Who = who;
            What = what;
            Why = why;
        }
    }
}