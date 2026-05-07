using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Security.Claims;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Types;

namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // todos los endpoints requieren JWT
    public class MiembrosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MiembrosController(AppDbContext context)
        {
            _context = context;
        }

        // Obtiene el id del usuario desde el JWT
        private int ObtenerIdUsuario() =>
            Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt
                            .JwtRegisteredClaimNames.Sub));

        // ============================================================
        // GET api/miembros
        // Lista todos los miembros del usuario autenticado
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerMiembros()
        {
            var idUsuario = ObtenerIdUsuario();
            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_obtener_miembros";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = idUsuario;

                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCursor);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var mensaje = pMensaje.Value.ToString()!;
                var miembros = new List<MiembroDto>();

                using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

                while (await reader.ReadAsync())
                {
                    miembros.Add(new MiembroDto
                    {
                        Id = Convert.ToInt32(reader["ID"]),
                        Nombre = reader["NOMBRE"].ToString()!,
                        Tipo = reader["TIPO"].ToString()!,
                        FechaNacimiento = reader["FECHA_NACIMIENTO"] == DBNull.Value
                                            ? null
                                            : Convert.ToDateTime(reader["FECHA_NACIMIENTO"]),
                        Genero = reader["GENERO"] == DBNull.Value
                                            ? null : reader["GENERO"].ToString(),
                        Especie = reader["ESPECIE"] == DBNull.Value
                                            ? null : reader["ESPECIE"].ToString(),
                        NumeroDocumento = reader["NUMERO_DOCUMENTO"] == DBNull.Value
                                            ? null : reader["NUMERO_DOCUMENTO"].ToString(),
                        FotoUrl = reader["FOTO_URL"] == DBNull.Value
                                            ? null : reader["FOTO_URL"].ToString(),
                        Edad = reader["EDAD"] == DBNull.Value
                                            ? null : Convert.ToInt32(reader["EDAD"])
                    });
                }

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = mensaje,
                    Data = miembros
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // POST api/miembros
        // Crear nuevo miembro
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> CrearMiembro([FromBody] CrearMiembroDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Tipo))
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Nombre y tipo son obligatorios."
                });
            }

            if (dto.Tipo != "persona" && dto.Tipo != "mascota")
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Tipo debe ser persona o mascota."
                });
            }

            var idUsuario = ObtenerIdUsuario();
            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_crear_miembro";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = idUsuario;
                cmd.Parameters.Add("p_nombre", OracleDbType.Varchar2).Value = dto.Nombre;
                cmd.Parameters.Add("p_tipo", OracleDbType.Varchar2).Value = dto.Tipo;
                cmd.Parameters.Add("p_fecha_nacimiento", OracleDbType.Date).Value =
                    dto.FechaNacimiento.HasValue ? dto.FechaNacimiento.Value : DBNull.Value;
                cmd.Parameters.Add("p_genero", OracleDbType.Varchar2).Value =
                    dto.Genero ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_especie", OracleDbType.Varchar2).Value =
                    dto.Especie ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_numero_documento", OracleDbType.Varchar2).Value =
                    dto.NumeroDocumento ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_foto_url", OracleDbType.Varchar2).Value =
                    dto.FotoUrl ?? (object)DBNull.Value;

                var pIdOut = new OracleParameter("p_id_out", OracleDbType.Int32)
                { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pIdOut);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var idOut = Convert.ToInt32(pIdOut.Value.ToString());
                var mensaje = pMensaje.Value.ToString()!;

                if (idOut == -1)
                {
                    return BadRequest(new RespuestaDto { Exito = false, Mensaje = mensaje });
                }

                return Ok(new RespuestaDto
                {
                    Exito = true,
                    Mensaje = mensaje,
                    Data = new { IdMiembro = idOut }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // PUT api/miembros/{id}
        // Actualizar miembro
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMiembro(int id, [FromBody] ActualizarMiembroDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return BadRequest(new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "El nombre es obligatorio."
                });
            }

            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_actualizar_miembro";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;
                cmd.Parameters.Add("p_nombre", OracleDbType.Varchar2).Value = dto.Nombre;
                cmd.Parameters.Add("p_fecha_nacimiento", OracleDbType.Date).Value =
                    dto.FechaNacimiento.HasValue ? dto.FechaNacimiento.Value : DBNull.Value;
                cmd.Parameters.Add("p_genero", OracleDbType.Varchar2).Value =
                    dto.Genero ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_especie", OracleDbType.Varchar2).Value =
                    dto.Especie ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_numero_documento", OracleDbType.Varchar2).Value =
                    dto.NumeroDocumento ?? (object)DBNull.Value;
                cmd.Parameters.Add("p_foto_url", OracleDbType.Varchar2).Value =
                    dto.FotoUrl ?? (object)DBNull.Value;

                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var mensaje = pMensaje.Value.ToString()!;

                if (mensaje.Contains("no encontrado"))
                {
                    return NotFound(new RespuestaDto { Exito = false, Mensaje = mensaje });
                }

                return Ok(new RespuestaDto { Exito = true, Mensaje = mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // DELETE api/miembros/{id}
        // Desactivar miembro (baja lógica)
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DesactivarMiembro(int id)
        {
            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_desactivar_miembro";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_id", OracleDbType.Int32).Value = id;

                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var mensaje = pMensaje.Value.ToString()!;

                if (mensaje.Contains("no encontrado"))
                {
                    return NotFound(new RespuestaDto { Exito = false, Mensaje = mensaje });
                }

                return Ok(new RespuestaDto { Exito = true, Mensaje = mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto
                {
                    Exito = false,
                    Mensaje = "Error interno: " + ex.Message
                });
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
        // ============================================================
        // GET api/miembros/{id}/perfil
        // Perfil completo del miembro con alergias, último IMC e historial
        // ============================================================
 
[HttpGet("{id}/perfil")]
public async Task<IActionResult> ObtenerPerfilCompleto(int id)
{
    var idUsuario = ObtenerIdUsuario();
 
    try
    {
        // Verificar que el miembro pertenece al usuario autenticado
        var miembro = await _context.Miembros
            .FirstOrDefaultAsync(m => m.Id == id && m.IdUsuario == idUsuario);
 
        if (miembro == null)
            return NotFound(new RespuestaDto
            {
                Exito   = false,
                Mensaje = "Miembro no encontrado."
            });
 
        // Datos básicos del miembro
        var miembroDto = new MiembroDto
        {
            Id               = miembro.Id,
            Nombre           = miembro.Nombre,
            Tipo             = miembro.Tipo,
            FechaNacimiento  = miembro.FechaNacimiento,
            Genero           = miembro.Genero,
            Especie          = miembro.Especie,
            NumeroDocumento  = miembro.NumeroDocumento,
            FotoUrl          = miembro.FotoUrl,
            Edad             = miembro.FechaNacimiento.HasValue
                                ? (int?)((DateTime.Today - miembro.FechaNacimiento.Value).Days / 365)
                                : null
        };
 
        // Alergias del miembro
        var alergias = await (
            from ma in _context.Miembroalergia
            join a in _context.Alergia on ma.IdAlergia equals a.Id
            where ma.IdMiembro == id
            orderby a.Nombre
            select new MiembroAlergiaDto
            {
                IdMiembroAlergia = ma.Id,
                IdAlergia        = a.Id,
                Nombre           = a.Nombre!
            }
        ).ToListAsync();
 
        // Último IMC registrado
        var ultimoImc = await _context.Imcs
            .Where(i => i.IdMiembro == id)
            .OrderByDescending(i => i.Fecha)
            .Select(i => new ImcDto
            {
                Id            = i.Id,
                IdMiembro     = i.IdMiembro,
                Peso          = i.Peso,
                Altura        = i.Altura,
                Resultado     = i.Resultado,
                Clasificacion = i.Resultado == null ? "Sin registrar" :
                                i.Resultado < 18.5m ? "Bajo peso" :
                                i.Resultado < 25m   ? "Normal"    :
                                i.Resultado < 30m   ? "Sobrepeso" : "Obesidad",
                Fecha         = i.Fecha
            })
            .FirstOrDefaultAsync();
 
        // Historial de vacunación
        var historial = await (
            from h in _context.Historialvacunas
            join v in _context.Vacunas on h.IdVacuna equals v.Id
            join c in _context.Centrovacunacions on h.IdCentro equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            join r in _context.Recordatorios on h.Id equals r.IdHistorial into rj
            from r in rj.DefaultIfEmpty()
            where h.IdMiembro == id
            orderby h.FechaAplicacion descending
            select new HistorialDto
            {
                Id                 = h.Id,
                FechaAplicacion    = h.FechaAplicacion,
                ProximaDosis       = h.ProximaDosis,
                DosisNumero        = h.DosisNumero,
                Lote               = h.Lote,
                NombreMedico       = h.NombreMedico,
                Observaciones      = h.Observaciones,
                Vacuna             = v.Nombre,
                Fabricante         = v.Fabricante,
                TipoVacuna         = v.Tipo,
                Centro             = c != null ? c.Nombre    : null,
                CentroDireccion    = c != null ? c.Direccion : null,
                RecordatorioEstado = r != null ? r.Estado    : null,
                FechaRecordatorio  = r != null ? r.FechaRecordatorio : null
            }
        ).ToListAsync();
 
        // Recordatorios pendientes
        var recordatorios = await (
            from r in _context.Recordatorios
            join h in _context.Historialvacunas on r.IdHistorial equals h.Id
            join v in _context.Vacunas on h.IdVacuna equals v.Id
            where h.IdMiembro == id && r.Estado == "pendiente"
            orderby r.FechaRecordatorio
            select new RecordatorioDto
            {
                Id                = r.Id,
                IdHistorial       = r.IdHistorial,
                FechaRecordatorio = r.FechaRecordatorio,
                Tipo              = r.Tipo,
                Mensaje           = r.Mensaje,
                Estado            = r.Estado,
                Vacuna            = v.Nombre,
                Miembro           = miembro.Nombre,
                DiasRestantes     = (int?)(r.FechaRecordatorio - DateTime.Today).TotalDays
            }
        ).ToListAsync();
 
        return Ok(new RespuestaDto
        {
            Exito   = true,
            Mensaje = "OK",
            Data    = new PerfilMiembroDto
            {
                Miembro       = miembroDto,
                Alergias      = alergias,
                UltimoImc     = ultimoImc,
                Historial     = historial,
                Recordatorios = recordatorios
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
    }
}