using ApiGestion.Modelos;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestion.Controllers;

/// <summary>
/// El catálogo de líneas de investigación.
///
/// ES DE SOLO LECTURA EN ESTE ARCHIVO, y eso tiene una razón que hay que saber
/// decir: `linea_investigacion` es una tabla de la **v1** (no tiene clave
/// foránea), así que su CRUD completo pertenece a esa versión. Lo que la v2
/// necesita de ella es **el listado**, porque es de donde salen las opciones
/// del desplegable del docente (criterio 5).
///
/// Si su equipo ya cerró la v1 con el CRUD de esta tabla, este archivo se
/// reemplaza por el que ya tienen: aquí está solo para que la fase 1 pueda
/// llenar su desplegable.
/// </summary>
[ApiController]
[Route("api/linea-investigacion")]
public class LineaInvestigacionController : ControllerBase
{
    private readonly IServicioLineaInvestigacion _servicio;

    public LineaInvestigacionController(IServicioLineaInvestigacion servicio)
    {
        _servicio = servicio;
    }

    /// <summary>
    /// Las líneas ACTIVAS. El desplegable solo puede ofrecer activas: ofrecer
    /// una línea retirada es ofrecer una opción que la base va a rechazar con
    /// un 409.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodas([FromQuery] int limite = 1000)
    {
        try
        {
            var lista = (await _servicio.ObtenerTodas(limite)).ToList();

            if (lista.Count == 0)
            {
                return NoContent();
            }

            return Ok(new
            {
                tabla = "linea_investigacion",
                limite,
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
}
