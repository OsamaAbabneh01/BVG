using Domain.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class BackgroundJobRepository : IBackgroundJobRepository
    {
        private readonly AppDbContext _context;

        public BackgroundJobRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(
            IEnumerable<BackgroundJob> jobs,
            CancellationToken cancellationToken = default)
        {
            await _context.BackgroundJobs.AddRangeAsync(
                jobs,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        //public async Task<List<InvoiceIdRange>> GetInvoiceRangesAsync(
        //    int bulkSize,
        //    CancellationToken cancellationToken = default)
        //{
        //    var invoiceIds = await _context.Invoices
        //        .AsNoTracking()
        //        .Where(i => !i.InvoiceStatus)
        //        .OrderBy(i => i.Id)
        //        .Select(i => i.Id)
        //        .ToListAsync(cancellationToken);

        //    return invoiceIds
        //        .Chunk(bulkSize)
        //        .Select(chunk => new InvoiceIdRange
        //        {
        //            StartInvoiceId = chunk.First(),
        //            EndInvoiceId = chunk.Last()
        //        })
        //        .ToList();
        //}

        public async Task<List<InvoiceIdRange>> GetInvoiceRangesAsync(
            int bulkSize,
            CancellationToken cancellationToken = default)
        {
            return await _context.Database
                .SqlQuery<InvoiceIdRange>($"""
            WITH NumberedInvoices AS
            (
                SELECT
                    Id,
                    ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
                FROM dbo.Invoices
                WHERE InvoiceStatus = 0
            )
            SELECT
                MIN(Id) AS StartInvoiceId,
                MAX(Id) AS EndInvoiceId
            FROM NumberedInvoices
            GROUP BY (RowNumber - 1) / {bulkSize}
            ORDER BY MIN(Id);
            """)
                .ToListAsync(cancellationToken);
        }
    }
}
