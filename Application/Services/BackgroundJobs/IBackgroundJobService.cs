using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.BackgroundJobs
{
    public interface IBackgroundJobService
    {
        Task<EnqueueBackgroundJobsResponse> EnqueueAsync(
        EnqueueBackgroundJobsRequest request,
        CancellationToken cancellationToken = default);
    }
}
