using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using ApiVacunas.Models;
using ApiVacunas.Services;

namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CertificadoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICertificadoPdfService _pdfService;

        public CertificadoController(AppDbContext context, ICertificadoPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // ============================================================
        // GET api/certificado/miembro/{idMiembro}
        // Certificados de un miembro
        // ============================================================
        [HttpGet("miembro/{idMiembro}")]
        public async Task<IActionResult> ObtenerCertificados(int idMiembro)
        {
            try
            {
                var certificados = await (
                    from c in _context.Certificados
                    join m in _context.Miembros on c.IdMiembro equals m.Id
                    where c.IdMiembro == idMiembro
                    orderby c.FechaEmision descending
                    select new CertificadoDto
                    {
                        Id            = c.Id,
                        IdMiembro     = c.IdMiembro,
                        CodigoQr      = c.CodigoQr,
                        FechaEmision  = c.FechaEmision,
                        UrlPdf        = c.UrlPdf,
                        NombreMiembro = m.Nombre
                    }
                ).ToListAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = certificados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
        }

        // ============================================================
        // POST api/certificado
        // Generar certificado/carnet digital para un miembro
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> GenerarCertificado([FromBody] CrearCertificadoDto dto)
        {
            if (dto.IdMiembro == 0)
                return BadRequest(new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "IdMiembro es obligatorio."
                });

            try
            {
                // Verificar que el miembro existe
                var miembro = await _context.Miembros
                    .FirstOrDefaultAsync(m => m.Id == dto.IdMiembro);

                if (miembro == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Miembro no encontrado."
                    });

                // Generar código QR único si no viene uno
                var codigoQr = string.IsNullOrEmpty(dto.CodigoQr) || dto.CodigoQr.ToLower() == "string"
                     ? 
                    $"VAC-{dto.IdMiembro}-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}"
                    : dto.CodigoQr;

                var certificado = new Certificado
                {
                    IdMiembro    = dto.IdMiembro,
                    CodigoQr     = codigoQr,
                    FechaEmision = DateTime.Now,
                    UrlPdf       = dto.UrlPdf
                };

                _context.Certificados.Add(certificado);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Certificado generado correctamente.",
                    Data    = new
                    {
                        IdCertificado = certificado.Id,
                        CodigoQr      = codigoQr,
                        FechaEmision  = certificado.FechaEmision
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
        }

        // ============================================================
        // DELETE api/certificado/{id}
        // Eliminar certificado
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCertificado(int id)
        {
            try
            {
                var certificado = await _context.Certificados
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (certificado == null)
                    return NotFound(new RespuestaDto
                    {
                        Exito   = false,
                        Mensaje = "Certificado no encontrado."
                    });

                _context.Certificados.Remove(certificado);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "Certificado eliminado correctamente."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito   = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
        }

        [HttpGet("descargar-pdf/{idMiembro}")]
        [AllowAnonymous] 
        public async Task<IActionResult> DescargarCertificadoPdf(int idMiembro)
        {
            try
            {
                var resultado = await _pdfService.GenerarCarnetPdfAsync(idMiembro);

                if (resultado == null)
                    return NotFound(new RespuestaDto 
                    { 
                        Exito = false, 
                        Mensaje = "No se encontró el miembro o no posee un carnet digital emitido." 
                    });

                string nombreArchivo = $"Carnet_{resultado.Value.NombreMiembro.Replace(" ", "_")}.pdf";
                
                // Retorna el archivo binario directamente al cliente
                return File(resultado.Value.PdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto 
                { 
                    Exito = false, 
                    Mensaje = "Error interno al procesar el documento: " + ex.Message 
                });
            }
        }
    }
}