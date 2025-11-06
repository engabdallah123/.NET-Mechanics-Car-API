using Domain.DTO.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IReposetory
{
    public interface IInvoiceRepo
    {
        Task<InvoiceDTO> GenerateInvoiceAsync(int workOrderId);
        Task<InvoiceDTO?> GetInvoiceAsync(int invoiceId);
        Task<InvoiceDTO?> MarkAsPaidAsync(int invoiceId);
        byte[] GeneratePdf(InvoiceDTO invoice);
    }
}
