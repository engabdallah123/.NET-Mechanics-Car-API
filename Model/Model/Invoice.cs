using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Invoice
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        public DateTime DateIssued { get; set; } = DateTime.Now;

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public bool IsPaid { get; set; } = false;

        public int WorkOrderId { get; set; }
        public virtual WorkOrder WorkOrder { get; set; }
    }
}
