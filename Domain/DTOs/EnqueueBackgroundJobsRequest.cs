using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DTOs
{
    public class EnqueueBackgroundJobsRequest
    {
    }

    public class EnqueueBackgroundJobsResponse
    {
        public Guid BatchRunId { get; set; }

        public int JobsCreated { get; set; }
    }
}
