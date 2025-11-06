using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Invoice
{
    public class getInvoiceOrder
    {
        public int InvoiceId { get; set; }
        public bool IsPaid { get; set; }
    }
}
