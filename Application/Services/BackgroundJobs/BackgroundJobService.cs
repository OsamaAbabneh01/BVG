using API.Options;
using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.BackgroundJobs
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobRepository _repository;
        private readonly BackgroundJobOptions _options;

        public BackgroundJobService(
            IBackgroundJobRepository repository,
            IOptions<BackgroundJobOptions> options)
        {
            _repository = repository;
            _options = options.Value;
        }

        public async Task<EnqueueBackgroundJobsResponse> EnqueueAsync(
            EnqueueBackgroundJobsRequest request,
            CancellationToken cancellationToken = default)
        {
            var batchRunId = Guid.NewGuid();

            var invoiceRanges = await _repository.GetInvoiceRangesAsync(
                _options.BulkSize,
                cancellationToken);

            var jobs = invoiceRanges
                .Select(range => new BackgroundJob
                {
                    BatchRunId = batchRunId,
                    StartInvoiceId = range.StartInvoiceId,
                    EndInvoiceId = range.EndInvoiceId,
                    Status = 0,
                    Priority = 0,
                    AttemptCount = 0,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            await _repository.AddRangeAsync(jobs, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return new EnqueueBackgroundJobsResponse
            {
                BatchRunId = batchRunId,
                JobsCreated = jobs.Count
            };
        }
    }
}
