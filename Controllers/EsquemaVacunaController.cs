using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using ApiVacunas.Models;
using System.IdentityModel.Tokens.Jwt;


[ApiController]
[Route("api/[controller]")]
[Authorize]

public class EsquemaVacunaController : ControllerBase
{
    private readonly AppDbContext _context;

    public EsquemaVacunaController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CrearEsquema([FromBody] CrearEsquemaDto dto)
    {
        var vacunaexiste = await _context.Vacunas.FirstOrDefaultAsync(v => v.Id == dto.IdVacuna && v.Activo) != null;
        if (vacunaexiste)
        {
            return NotFound(new RespuestaDto { Exito = false, Mensaje = "Vacuna no encontrada o inactiva" });
        }

        var esquema = new Esquemavacuna
        {
            IdVacuna = dto.IdVacuna,
            NumeroDosis = dto.NumeroDosis,
            IntervaloDias = dto.IntervaloDias,
            EdadMinimaDias = dto.EdadMinimaDias,
            Descripcion = dto.Descripcion
        };

        _context.Esquemavacunas.Add(esquema);
        await _context.SaveChangesAsync();

        var esquemaDto = new EsquemaVacunaDto
        {
            Id = esquema.Id,
            IdVacuna = esquema.IdVacuna,
            NumeroDosis = esquema.NumeroDosis,
            IntervaloDias = esquema.IntervaloDias,
            EdadMinimaDias = esquema.EdadMinimaDias,
            Descripcion = esquema.Descripcion
        };

        return CreatedAtAction(nameof(ObtenerEsquema), new { id = esquema.Id }, esquemaDto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerEsquema(int id)
    {
        var esquema = await _context.Esquemavacunas.FindAsync(id);
        if (esquema == null)
        {
            return NotFound(new RespuestaDto { Exito = false, Mensaje = "Esquema de vacuna no encontrado" });
        }

        var esquemaDto = new EsquemaVacunaDto
        {
            Id = esquema.Id,
            IdVacuna = esquema.IdVacuna,
            NumeroDosis = esquema.NumeroDosis,
            IntervaloDias = esquema.IntervaloDias,
            EdadMinimaDias = esquema.EdadMinimaDias,
            Descripcion = esquema.Descripcion
        };

        return Ok(esquemaDto);
    }

    [HttpGet("vacuna/{idVacuna}")]
    public async Task<IActionResult> ObtenerEsquemasPorVacuna(int idVacuna)
    {
        var esquemas = await _context.Esquemavacunas
            .Where(e => e.IdVacuna == idVacuna)
            .Select(e => new EsquemaVacunaDto
            {
                Id = e.Id,
                IdVacuna = e.IdVacuna,
                NumeroDosis = e.NumeroDosis,
                IntervaloDias = e.IntervaloDias,
                EdadMinimaDias = e.EdadMinimaDias,
                Descripcion = e.Descripcion
            })
            .ToListAsync();

        return Ok(esquemas);
    }
}