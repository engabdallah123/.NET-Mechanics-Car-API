namespace Domain.DTO.Invoice
{
    public class InvoiceTaskDTO
    {
        public string TaskName { get; set; }
        public decimal LaborCost { get; set; }
        public List<InvoicePartDTO> Parts { get; set; } = new();
    }

}
