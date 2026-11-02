using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IInvoiceRepository
    {
        Task ProcessInvoicesAsync(
            long startInvoiceId,
            long endInvoiceId,
            CancellationToken cancellationToken = default);
    }
}
