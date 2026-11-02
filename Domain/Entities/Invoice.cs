using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Invoice
    {
        public long Id { get; set; }

        public bool InvoiceStatus { get; set; }

        public string AccountNo { get; set; } = string.Empty;
    }
}
