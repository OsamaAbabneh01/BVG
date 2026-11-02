using System;
using System.Collections.Generic;
using System.Text;

namespace Workers
{
    public class BackgroundJobOptions
    {
        public const string SectionName = "BackgroundJob";

        public int MaxAttempts { get; set; } = 3;

        public int LeaseSeconds { get; set; } = 300;

        public int MaxConcurrentJobs { get; set; } = 3;

        public int BulkSize { get; set; } = 1000;
    }
}
