using Data;
using Domain.DTO.Invoice;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Services.IReposetory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;
using Domain.DTO.Vehicle;


namespace Services.Repository
{
    public class InvoiceRepo : IInvoiceRepo
    {
        private readonly DataContext db;

        public InvoiceRepo(DataContext db)
        {
            this.db = db;
        }
        public async Task<InvoiceDTO> GenerateInvoiceAsync(int workOrderId)
        {
            var workOrder = await db.WorkOrders
                .FirstOrDefaultAsync(w => w.Id == workOrderId);

            if (workOrder == null)
                throw new Exception("Work order not found");

            var tasks = workOrder.WorkOrderRepairTasks.Select(t => new InvoiceTaskDTO
            {
                TaskName = t.RepairTask.Name,
                LaborCost = t.RepairTask.LaborCost,
                Parts = t.RepairTask.Parts.Select(p => new InvoicePartDTO
                {
                    PartName = p.Name,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice
                }).ToList()
            }).ToList();

            var subtotal = tasks.Sum(t => t.LaborCost + t.Parts.Sum(p => p.UnitPrice * p.Quantity));
            var tax = subtotal * 0.15m;
            var total = subtotal + tax;

            var invoice = new Invoice
            {
                WorkOrderId = workOrder.Id,
                Subtotal = subtotal,
                Discount = 0,
                Tax = tax,
                Total = total
            };

            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            return new InvoiceDTO
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                DateIssued = invoice.DateIssued,
                IsPaid = invoice.IsPaid,
                Subtotal = invoice.Subtotal,
                Discount = invoice.Discount,
                Tax = invoice.Tax,
                Total = invoice.Total,
                CustomerName = workOrder.Customer.Name,
                VehicleName = workOrder.Vehicle.VehicleModel.Make.Name,
                LicencePlate = workOrder.Vehicle.LicensePlate,
                Tasks = tasks
            };

        }
        public async Task<InvoiceDTO?> GetInvoiceAsync(int invoiceId)
        {
            var invoice = await db.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null) return null;

            return new InvoiceDTO
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                DateIssued = invoice.DateIssued,
                IsPaid = invoice.IsPaid,
                Subtotal = invoice.Subtotal,
                Discount = invoice.Discount,
                Tax = invoice.Tax,
                Total = invoice.Total,
                CustomerName = invoice.WorkOrder.Customer.Name,
                VehicleName = invoice.WorkOrder.Vehicle?.VehicleModel.Make.Name,
                LicencePlate = invoice.WorkOrder.Vehicle?.LicensePlate,
                Tasks = invoice.WorkOrder.WorkOrderRepairTasks.Select(t => new InvoiceTaskDTO
                {
                    TaskName = t.RepairTask.Name,
                    LaborCost = t.RepairTask.LaborCost,
                    Parts = t.RepairTask.Parts.Select(p => new InvoicePartDTO
                    {
                        PartName = p.Name,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice
                    }).ToList()
                }).ToList()
            };
        }
        public async Task<InvoiceDTO?> MarkAsPaidAsync(int invoiceId)
        {
            var invoice = await db.Invoices.FindAsync(invoiceId);
            if (invoice == null) return null;

            invoice.IsPaid = true;
            await db.SaveChangesAsync();

            return await GetInvoiceAsync(invoiceId);
        }
        public byte[] GeneratePdf(InvoiceDTO invoice)
        {
            // Register a license (if you have one)
            // QuestPDF.Settings.License = LicenseType.Professional; 

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Calibri)); // Or another clean font

                    // 1. HEADER
                    page.Header().Row(row =>
                    {
                        // --- Column 1: Logo and Company Name ---
                        row.RelativeColumn().Row(rowLeft =>
                        {
                            // Logo Placeholder
                            rowLeft.ConstantColumn(40).Container()
                                .Width(40).Height(40)
                                .Background("#D32F2F") // Red square
                                .AlignCenter().AlignMiddle()
                                .Text("🔧") // Placeholder wrench emoji
                                .FontSize(24).FontColor("#FFFFFF");
                            // TODO: Replace the emoji above with your actual logo:
                            // .Image("logo.png") or .SvgImage(svgData)

                            // Company Name
                            rowLeft.RelativeColumn().PaddingLeft(10).AlignMiddle()
                                .Text(text =>
                                {
                                    // Colors picked from your screenshot
                                    text.Span("Mechanic ").FontSize(22).Bold().FontColor("#26C6DA"); // Cyan
                                    text.Span("Shop").FontSize(22).Bold().FontColor("#EC407A"); // Pink
                                });
                        });

                        // --- Column 2: Invoice Details ---
                        row.RelativeColumn().AlignRight().Column(colRight =>
                        {
                            colRight.Spacing(2);
                            colRight.Item().AlignRight().Text("INVOICE").FontSize(24).Bold().FontColor(Colors.Grey.Darken3);
                            colRight.Item().AlignRight().Text($"#{invoice.InvoiceNumber}").FontSize(14).FontColor(Colors.Grey.Darken1);
                            colRight.Item().AlignRight().Text($"Date: {invoice.DateIssued:MMMM dd, yyyy}").FontSize(10).FontColor(Colors.Grey.Darken1);
                            colRight.Item().AlignRight().Text($"Status: {(invoice.IsPaid ? "Paid" : "Unpaid")}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // 2. CONTENT
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // --- Black Divider Line ---
                        col.Item().LineHorizontal(1).LineColor(Colors.Black);

                        // --- Invoice Table ---
                        col.Item().Table(table =>
                        {
                            // Define Table Columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(6); // Description
                                columns.ConstantColumn(50); // QTY
                                columns.RelativeColumn(2); // Unit Price
                                columns.RelativeColumn(2); // Line Total
                            });

                            // --- Table Header ---
                            table.Header(header =>
                            {
                                // Helper for styling header cells
                                static IContainer HeaderCellStyle(IContainer container) =>
                                    container.Background(Colors.Grey.Darken4)
                                             .PaddingVertical(5).PaddingHorizontal(10)
                                             .DefaultTextStyle(x => x.Bold().FontColor(Colors.White).FontSize(10));

                                header.Cell().Element(HeaderCellStyle).Text("DESCRIPTION");
                                header.Cell().Element(HeaderCellStyle).AlignCenter().Text("QTY");
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("UNIT PRICE");
                                header.Cell().Element(HeaderCellStyle).AlignRight().Text("LINE TOTAL");
                            });

                            // --- Table Body (Line Items) ---
                            int i = 1;
                            foreach (var task in invoice.Tasks)
                            {
                                // Calculate task totals
                                decimal unitPrice = task.LaborCost + task.Parts.Sum(p => p.UnitPrice * p.Quantity);
                                decimal lineTotal = unitPrice * 1; // Assuming Qty is always 1 for the whole task

                                // -- Column 1: Description --
                                table.Cell().Padding(10).Column(descCol =>
                                {
                                    descCol.Spacing(4);
                                    descCol.Item().Text($"{i++}: {task.TaskName}").Bold();

                                    // Sub-details
                                    descCol.Item().PaddingLeft(10).Column(subCol =>
                                    {
                                        subCol.Item().Text($"Labor = ${task.LaborCost:N2}").FontSize(10).FontColor(Colors.Grey.Darken2);
                                        subCol.Item().Text("Parts:").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
                                        foreach (var p in task.Parts)
                                        {
                                            subCol.Item().PaddingLeft(10).Text($"• {p.PartName} x{p.Quantity} @ ${p.UnitPrice:N2}").FontSize(10).FontColor(Colors.Grey.Darken2);
                                        }
                                    });
                                });

                                // -- Column 2: QTY --
                                table.Cell().Padding(10).AlignCenter().AlignMiddle().Text("1");

                                // -- Column 3: Unit Price --
                                table.Cell().Padding(10).AlignRight().AlignMiddle().Text($"${unitPrice:N2}");

                                // -- Column 4: Line Total --
                                table.Cell().Padding(10).AlignRight().AlignMiddle().Text($"${lineTotal:N2}");

                                // -- Row Divider --
                                table.Cell().ColumnSpan(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        // --- Totals Section ---
                        col.Item().PaddingTop(10).AlignRight().Container()
                            .Background(Colors.Grey.Lighten4) // Light grey background
                            .Padding(15)
                            .Width(250) // Fixed width for the totals box
                            .Column(totalsCol =>
                            {
                                totalsCol.Spacing(5);

                                // Helper for a totals row
                                void TotalsRow(string label, string value, TextStyle? style = null)
                                {
                                    totalsCol.Item().Row(row =>
                                    {
                                        row.RelativeColumn().Text(label).Style(style ?? TextStyle.Default);
                                        row.RelativeColumn().AlignRight().Text(value).Style(style ?? TextStyle.Default);
                                    });
                                }

                                // Subtotal
                                TotalsRow("Subtotal:", $"${invoice.Subtotal:N2}");

                                // Tax
                                TotalsRow("Tax:", $"${invoice.Tax:N2}");

                                // Divider
                                totalsCol.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                                // Total
                                var totalStyle = TextStyle.Default.Bold().FontSize(14).FontColor("#00897B"); // Teal color from image
                                TotalsRow("TOTAL:", $"${invoice.Total:N2}", totalStyle);
                            });
                    });

                    // 3. FOOTER
                    // The target design does not have a footer, so the 
                    // original page.Footer() section is removed.

                });
            });

            return document.GeneratePdf();
        }
    }
}
