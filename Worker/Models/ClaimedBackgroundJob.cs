using System;
using System.Collections.Generic;
using System.Text;

namespace Workers.Models
{
    public class ClaimedBackgroundJob
    {
        public long Id { get; set; }

        public Guid BatchRunId { get; set; }

        public long StartInvoiceId { get; set; }

        public long EndInvoiceId { get; set; }

        public byte Status { get; set; }

        public int AttemptCount { get; set; }

        public string WorkerId { get; set; } = string.Empty;

        public DateTime LockedUntil { get; set; }
    }
}
