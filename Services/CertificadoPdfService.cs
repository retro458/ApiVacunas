using ApiVacunas.Data;
using ApiVacunas.Models;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiVacunas.Services
{
    public class CertificadoPdfService : ICertificadoPdfService
    {
        private readonly AppDbContext _context;

        public CertificadoPdfService(AppDbContext context)
        {
            _context = context;
        }

        // AJUSTADO: Usamos PdfBytes para cumplir exactamente con tu interfaz
        public async Task<(byte[] PdfBytes, string NombreMiembro)?> GenerarCarnetPdfAsync(int idMiembro)
        {
            // 1. Buscar datos del Miembro
            var miembro = await _context.Miembros.FirstOrDefaultAsync(m => m.Id == idMiembro);
            if (miembro == null) return null;

            // 2. Buscar su último certificado generado
            var certificado = await _context.Certificados
                .Where(c => c.IdMiembro == idMiembro)
                .OrderByDescending(c => c.FechaEmision)
                .FirstOrDefaultAsync();

            if (certificado == null) return null;

            // 3. Consultar historial de vacunas
            var historial = await (
                from h in _context.Historialvacunas
                join v in _context.Vacunas on h.IdVacuna equals v.Id
                where h.IdMiembro == idMiembro
                orderby h.FechaAplicacion ascending
                select new
                {
                    VacunaNombre = v.Nombre,
                    v.Fabricante,
                    h.DosisNumero,
                    h.FechaAplicacion,
                    h.Lote
                }
            ).ToListAsync();

            // 4. Generar bytes del Código QR visual (Se queda aquí adentro, excelente)
            byte[] qrBytes;
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(certificado.CodigoQr!, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                qrBytes = qrCode.GetGraphic(15);
            }

            // 5. Construcción del Layout con QuestPDF
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                    // Encabezado
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("CARNET DE VACUNACIÓN DIGITAL")
                                .FontSize(22).Bold().FontColor(Colors.Blue.Darken3);
                            
                            col.Item().PaddingTop(10).Text($"Beneficiario: {miembro.Nombre}").FontSize(13).Bold();
                            if(miembro.Tipo?.Equals("mascota", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                string dui = string.IsNullOrWhiteSpace(miembro.NumeroDocumento) ? "N/A" : miembro.NumeroDocumento == "string"
                                ? "N/A" : miembro.NumeroDocumento;
                                col.Item().Text($"DUI: {dui}").FontSize(11);
                            }

                            col.Item().Text($"Tipo: {miembro.Tipo!.ToUpper()}").FontSize(11);
                            col.Item().Text($"Fecha Emisión: {certificado.FechaEmision:dd/MM/yyyy HH:mm}").FontSize(10).Italic();
                        });

                        row.ConstantItem(90).AlignMiddle().Image(qrBytes);
                    });

                    // Tabla de contenido
                    page.Content().PaddingVertical(1.5f, Unit.Centimetre).Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().Text("Dosis e Inmunizaciones Registradas").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);

                        if (!historial.Any())
                        {
                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10)
                                .Text("No se registran dosis aplicadas en el sistema hasta la fecha.").Italic();
                        }
                        else
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1.5f);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Vacuna").Bold().FontSize(10);
                                    header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Laboratorio").Bold().FontSize(10);
                                    header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Dosis").Bold().FontSize(10).AlignCenter();
                                    header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("F. Aplicación").Bold().FontSize(10);
                                    header.Cell().Background(Colors.Blue.Lighten4).Padding(6).Text("Lote").Bold().FontSize(10);
                                });

                                foreach (var registro in historial)
                                {
                                    table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(registro.VacunaNombre).FontSize(10);
                                    table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(registro.Fabricante ?? "N/A").FontSize(10);
                                    table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(registro.DosisNumero.ToString()).FontSize(10).AlignCenter();
                                    table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(registro.FechaAplicacion.ToString("dd/MM/yyyy")).FontSize(10);
                                    table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(registro.Lote ?? "N/A").FontSize(10);
                                }
                            });
                        }
                    });

                    // Pie de página
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"Firma Digital / ID de Verificación: {certificado.CodigoQr}")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                            row.RelativeItem().AlignRight().Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                                x.Span(" de ");
                                x.TotalPages();
                            });
                        });
                    });
                });
            }).GeneratePdf();

            // Retorna perfectamente el archivo maestro compilado y el string
            return (pdfBytes, miembro.Nombre);
        }
    }
}