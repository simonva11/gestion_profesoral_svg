using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ApiGestion.Controllers;

/// <summary>
/// La capa 1: HTTP. No contiene lógica de negocio ni SQL (Artículo 3). Traduce las
/// excepciones del negocio a códigos de estado, y las peticiones a lo que la capa 2
/// entiende (3_plan.md §4.7).
///
/// La ruta se escribe COMPLETA y no con [controller]: ese token generaría "Programa"
/// con mayúscula y el contrato pide /api/programa (Artículo 10).
/// </summary>
[ApiController]
[Route("api/programa")]
public class ProgramaController : ControllerBase
{
    private readonly IServicioPrograma _servicio;

    public ProgramaController(IServicioPrograma servicio)
    {
        _servicio = servicio;
    }

    /// <summary>RF1 — Listar programas activos.</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int limite = 1000)
    {
        try
        {
            var datos = await _servicio.ObtenerTodos(limite);
            var lista = datos.ToList();

            // Vacío NO es error: 204 sin cuerpo. Es además el estado inicial del
            // sistema, porque la tabla arranca sin datos (C5).
            if (lista.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "programa",
                limite = limite,
                total = lista.Count,
                datos = lista
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF2 — Obtener un programa por su código.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var programa = await _servicio.ObtenerPorId(id);
            return Ok(programa);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Programa no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF3 — Crear un programa.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProgramaCrear peticion)
    {
        try
        {
            // El controlador traduce: la capa 2 recibe la entidad, no el cuerpo HTTP
            var programa = new Programa
            {
                Id = peticion.Id!.Value,
                Nombre = peticion.Nombre,
                Tipo = peticion.Tipo,
                Nivel = peticion.Nivel,
                FechaCreacion = peticion.FechaCreacion,
                FechaCierre = peticion.FechaCierre,
                NumeroCohortes = peticion.NumeroCohortes,
                CantGraduados = peticion.CantGraduados,
                FechaActualizacion = peticion.FechaActualizacion,
                Ciudad = peticion.Ciudad,
                Facultad = peticion.Facultad!.Value
            };

            await _servicio.Crear(programa);
            return Ok(new { estado = 200, mensaje = "Programa creado exitosamente." });
        }
        catch (SqlException ex)
        {
            // Código duplicado: la llave la defiende la base, no la API (C11)
            return StatusCode(500, new { estado = 500, mensaje = "Error al insertar en la base de datos.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF4 — Reemplazar completamente un programa.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Reemplazar(int id, [FromBody] ProgramaReemplazo peticion)
    {
        try
        {
            var programa = new Programa
            {
                Id = id,
                Nombre = peticion.Nombre,
                Tipo = peticion.Tipo,
                Nivel = peticion.Nivel,
                FechaCreacion = peticion.FechaCreacion,
                FechaCierre = peticion.FechaCierre,
                NumeroCohortes = peticion.NumeroCohortes,
                CantGraduados = peticion.CantGraduados,
                FechaActualizacion = peticion.FechaActualizacion,
                Ciudad = peticion.Ciudad,
                Facultad = peticion.Facultad!.Value
            };

            var filas = await _servicio.Reemplazar(id, programa);
            return Ok(new { estado = 200, mensaje = "Programa reemplazado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Programa no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF5 — Actualizar parcialmente un programa.</summary>
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> ActualizarParcial(int id, [FromBody] ProgramaActualizar peticion)
    {
        try
        {
            var campos = new ProgramaCampos(
                peticion.Nombre, peticion.Tipo, peticion.Nivel, peticion.FechaCreacion,
                peticion.FechaCierre, peticion.NumeroCohortes, peticion.CantGraduados,
                peticion.FechaActualizacion, peticion.Ciudad, peticion.Facultad);

            var filas = await _servicio.ActualizarParcial(id, campos);
            return Ok(new { estado = 200, mensaje = "Programa actualizado.", filasAfectadas = filas });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Programa no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF6 — Borrado lógico.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var filas = await _servicio.Eliminar(id);
            return Ok(new { estado = 200, mensaje = "Programa eliminado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Programa no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
