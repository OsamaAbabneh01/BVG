using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task ProcessInvoicesAsync(
            long startInvoiceId,
            long endInvoiceId,
            CancellationToken cancellationToken = default)
        {
            await _context.Invoices
                .Where(i =>
                    i.Id >= startInvoiceId &&
                    i.Id <= endInvoiceId &&
                    !i.InvoiceStatus)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        i => i.InvoiceStatus,
                        true),
                    cancellationToken);
        }
    }
}
