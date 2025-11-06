using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.IReposetory;
using Services.Repository;

namespace CarMaintenance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepo invoiceRepo;

        public InvoiceController(IInvoiceRepo invoiceRepo)
        {
            this.invoiceRepo = invoiceRepo;
        }

        [HttpPost("generate/{workOrderId}")]
        public async Task<IActionResult> GenerateInvoice(int workOrderId)
        {
            try
            {
                var invoice = await invoiceRepo.GenerateInvoiceAsync(workOrderId);
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetInvoice(int invoiceId)
        {
            var invoice = await invoiceRepo.GetInvoiceAsync(invoiceId);
            if (invoice == null)
                return NotFound("Invoice not found.");

            return Ok(invoice);
        }


        [HttpGet("pdf/{invoiceId}")]
        public async Task<IActionResult> GetInvoicePdf(int invoiceId)
        {
            var invoice = await invoiceRepo.GetInvoiceAsync(invoiceId);
            if (invoice == null)
                return NotFound("Invoice not found.");

            var pdfBytes = invoiceRepo.GeneratePdf(invoice);

            Response.Headers["Content-Disposition"] = "inline; filename=invoice.pdf";
            Response.Headers["Content-Type"] = "application/pdf";

            return File(pdfBytes, "application/pdf");
        }



        [HttpPut("mark-paid/{invoiceId}")]
        public async Task<IActionResult> MarkAsPaid(int invoiceId)
        {
            var invoice = await invoiceRepo.MarkAsPaidAsync(invoiceId);
            if (invoice == null)
                return NotFound("Invoice not found.");

            return Ok(invoice);
        }
    }
}
