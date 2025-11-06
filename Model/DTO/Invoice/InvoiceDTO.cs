using Domain.DTO.Vehicle;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Invoice
{
    public class InvoiceDTO
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime DateIssued { get; set; }
        public bool IsPaid { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public string CustomerName { get; set; }
        public string TechnicianName { get; set; }



        public decimal Tax {get; set; }
        public List<InvoiceTaskDTO> Tasks { get; set; } = new();
        public string VehicleName { get; set; }
        public string LicencePlate { get; set; }
    }

}
