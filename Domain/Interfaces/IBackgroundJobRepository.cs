using Domain.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IBackgroundJobRepository
    {
        Task AddRangeAsync(
        IEnumerable<BackgroundJob> jobs,
        CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);

        Task<List<InvoiceIdRange>> GetInvoiceRangesAsync(
            int bulkSize,
            CancellationToken cancellationToken = default);
    }
}
