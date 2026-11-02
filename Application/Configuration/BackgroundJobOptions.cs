namespace API.Options
{
    public class BackgroundJobOptions
    {
        public const string SectionName = "BackgroundJob";

        public int BulkSize { get; set; } = 1000;
    }
}
