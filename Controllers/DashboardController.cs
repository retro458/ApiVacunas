using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using ApiVacunas.Data;
using ApiVacunas.DTOs;
using Oracle.ManagedDataAccess.Types;

namespace ApiVacunas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }
 private int ObtenerIdUsuario() =>
            Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt
                            .JwtRegisteredClaimNames.Sub)); 

        // ============================================================
        // GET api/dashboard/admin
        // Totales generales — solo admin
        // Consume sp_dashboard_admin
        // ============================================================
        [HttpGet("admin")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DashboardAdmin()
        {
            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_dashboard_admin";
                cmd.CommandType = CommandType.StoredProcedure;

                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                    { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCursor);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

                DashboardAdminDto? dashboard = null;

                if (await reader.ReadAsync())
                {
                    dashboard = new DashboardAdminDto
                    {
                        TotalUsuarios           = Convert.ToInt32(reader["TOTAL_USUARIOS"]),
                        TotalMiembros           = Convert.ToInt32(reader["TOTAL_MIEMBROS"]),
                        TotalVacunaciones       = Convert.ToInt32(reader["TOTAL_VACUNACIONES"]),
                        RecordatoriosPendientes = Convert.ToInt32(reader["RECORDATORIOS_PENDIENTES"]),
                        CampaniasActivas        = Convert.ToInt32(reader["CAMPANIAS_ACTIVAS"]),
                        CentrosActivos          = Convert.ToInt32(reader["CENTROS_ACTIVOS"])
                    };
                }

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = dashboard
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
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // GET api/dashboard/vacunaciones/{anio}
        // Vacunaciones por mes — solo admin
        // Consume sp_vacunaciones_por_mes
        // ============================================================
        [HttpGet("vacunaciones/{anio}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> VacunacionesPorMes(int anio)
        {
            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_vacunaciones_por_mes";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("p_anio", OracleDbType.Int32).Value = anio;

                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                    { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCursor);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var vacunaciones = new List<VacunacionMesDto>();

                using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

                while (await reader.ReadAsync())
                {
                    vacunaciones.Add(new VacunacionMesDto
                    {
                        MesNumero = reader["MES_NUMERO"].ToString()!,
                        MesNombre = reader["MES_NOMBRE"].ToString()!.Trim(),
                        Vacuna    = reader["VACUNA"].ToString()!,
                        Total     = Convert.ToInt32(reader["TOTAL"])
                    });
                }

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = vacunaciones
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
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // GET api/dashboard/cobertura
        // Cobertura por vacuna — solo admin
        // Consume sp_cobertura_por_vacuna
        // ============================================================
        [HttpGet("cobertura")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CoberturaPorVacuna()
        {
            var conn = (OracleConnection)_context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_cobertura_por_vacuna";
                cmd.CommandType = CommandType.StoredProcedure;

                var pCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                    { Direction = ParameterDirection.Output };
                var pMensaje = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 500)
                    { Direction = ParameterDirection.Output };

                cmd.Parameters.Add(pCursor);
                cmd.Parameters.Add(pMensaje);

                await cmd.ExecuteNonQueryAsync();

                var cobertura = new List<CoberturVacunaDto>();

                using var reader = ((OracleRefCursor)pCursor.Value).GetDataReader();

                while (await reader.ReadAsync())
                {
                    cobertura.Add(new CoberturVacunaDto
                    {
                        Vacuna            = reader["VACUNA"].ToString()!,
                        TipoVacuna        = reader["TIPO_VACUNA"].ToString()!,
                        Fabricante        = reader["FABRICANTE"] == DBNull.Value
                                            ? null : reader["FABRICANTE"].ToString(),
                        TotalAplicaciones = Convert.ToInt32(reader["TOTAL_APLICACIONES"]),
                        MiembrosVacunados = Convert.ToInt32(reader["MIEMBROS_VACUNADOS"])
                    });
                }

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = cobertura
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
            finally
            {
                await conn.CloseAsync();
            }
        }

        // ============================================================
        // GET api/dashboard/usuario
        // Resumen personal del usuario autenticado
        // ============================================================
        [HttpGet("usuario")]
        public async Task<IActionResult> DashboardUsuario()
        {
            var idUsuario   = ObtenerIdUsuario();
            var fechaLimite = DateTime.Today.AddDays(30);

            try
            {
                var totalMiembros = await _context.Miembros
                    .CountAsync(m => m.IdUsuario == idUsuario && m.Activo);

                var totalVacunaciones = await (
                    from h in _context.Historialvacunas
                    join m in _context.Miembros on h.IdMiembro equals m.Id
                    where m.IdUsuario == idUsuario
                    select h
                ).CountAsync();

                var recordatoriosPendientes = await (
                    from r in _context.Recordatorios
                    join h in _context.Historialvacunas on r.IdHistorial equals h.Id
                    join m in _context.Miembros on h.IdMiembro equals m.Id
                    where m.IdUsuario == idUsuario && r.Estado == "pendiente"
                    select r
                ).CountAsync();

                var proximasDosis = await (
                    from r in _context.Recordatorios
                    join h in _context.Historialvacunas on r.IdHistorial equals h.Id
                    join m in _context.Miembros on h.IdMiembro equals m.Id
                    where m.IdUsuario          == idUsuario
                       && r.Estado            == "pendiente"
                       && r.FechaRecordatorio >= DateTime.Today
                       && r.FechaRecordatorio <= fechaLimite
                    select r
                ).CountAsync();

                var campaniasProximas = await _context.Campaniavacunacions
                    .CountAsync(c => c.Activo 
                                  && c.Fecha >= DateTime.Today
                                  && c.Fecha <= fechaLimite);

                return Ok(new RespuestaDto
                {
                    Exito   = true,
                    Mensaje = "OK",
                    Data    = new DashboardUsuarioDto
                    {
                        TotalMiembros           = totalMiembros,
                        TotalVacunaciones       = totalVacunaciones,
                        RecordatoriosPendientes = recordatoriosPendientes,
                        ProximasDosis30Dias     = proximasDosis,
                        CampaniasProximas       = campaniasProximas
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