using GestionProfesoral.Domain.Excepciones;
using GestionProfesoral.Domain.Entidades;
using GestionProfesoral.Api.Peticiones;
using GestionProfesoral.Domain.Contratos;
using GestionProfesoral.Application.Contratos;
using Microsoft.AspNetCore.Mvc;

namespace GestionProfesoral.Api.Controllers;

/// <summary>
/// CAPA 1 — HTTP. No contiene lógica de negocio ni SQL (Artículo 3).
///
/// Hace exactamente tres cosas:
///   1. traduce el cuerpo HTTP (Peticiones/) a la entidad (Modelos/);
///   2. llama a la capa 2;
///   3. traduce las excepciones del negocio a códigos de estado.
///
/// Fíjese en lo que NO importa este archivo: Microsoft.Data.SqlClient. El
/// controlador no sabe que detrás hay SQL Server, y por eso no puede escribir
/// un `catch (SqlException)`. Esa traducción ya ocurrió en la capa 3 (ADR-02).
///
/// La ruta se escribe COMPLETA y no con [controller]: ese token generaría
/// "Docente" con mayúscula y el contrato pide /api/docente (Artículo 10).
/// </summary>
[ApiController]
[Route("api/docente")]
public class DocenteController : ControllerBase
{
    private readonly IServicioDocente _servicio;

    // Recibe la INTERFAZ, no la clase. Quién se la da es asunto de Program.cs,
    // que es el único sitio del proyecto donde las dos se nombran juntas.
    public DocenteController(IServicioDocente servicio)
    {
        _servicio = servicio;
    }

    /// <summary>RF1 — Listar docentes activos.</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int limite = 1000)
    {
        try
        {
            var datos = await _servicio.ObtenerTodos(limite);
            var lista = datos.ToList();

            // Vacío NO es error: 204 sin cuerpo. Una lista vacía con 200 haría
            // que la pantalla tuviera que distinguir «no hay» de «no cargó».
            if (lista.Count == 0)
            {
                return NoContent();
            }

            // El SOBRE del contrato, no una lista pelada: una lista no puede
            // crecer sin romper a quien la lee; el sobre sí.
            return Ok(new
            {
                tabla = "docente",
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

    /// <summary>RF2 — Obtener un docente por su cédula.</summary>
    [HttpGet("{cedula:int}")]
    public async Task<IActionResult> ObtenerPorId(int cedula)
    {
        try
        {
            var docente = await _servicio.ObtenerPorId(cedula);
            return Ok(docente);
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Docente no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF3 — Crear un docente.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] DocenteCrear peticion)
    {
        // Si faltara un campo o un texto fuera muy largo, esta línea no se
        // habría ejecutado: [ApiController] ya habría respondido 422
        // (6_contracts.md §3.2).
        try
        {
            var docente = new Docente
            {
                Cedula = peticion.Cedula!.Value,
                Nombres = peticion.Nombres,
                Apellidos = peticion.Apellidos,
                Genero = peticion.Genero,
                Cargo = peticion.Cargo,
                FechaNacimiento = peticion.FechaNacimiento!.Value,
                Correo = peticion.Correo,
                Telefono = peticion.Telefono,
                UrlCvlac = peticion.UrlCvlac,
                FechaActualizacion = peticion.FechaActualizacion!.Value,
                Escalafon = peticion.Escalafon,
                Perfil = peticion.Perfil,
                CatMinciencia = peticion.CatMinciencia,
                ConvMinciencia = peticion.ConvMinciencia,
                Nacionalidaad = peticion.Nacionalidaad,
                // Si la persona eligió «(ninguna)», aquí llega null y null es
                // lo que viaja hasta la base. No se convierte a 0 ni a "".
                LineaInvestigacionPrincipal = peticion.LineaInvestigacionPrincipal
            };

            var guardado = await _servicio.Crear(docente);

            // Se devuelve lo que GUARDÓ la base, no lo que mandó el formulario:
            // así la pantalla puede mostrar el totalEstudios que puso el
            // disparador sin pedir el docente otra vez.
            return Ok(new { estado = 200, mensaje = "Docente registrado.", datos = guardado });
        }
        catch (ConflictoExcepcion ex)
        {
            // EL 409 DE LA VERSIÓN 2 (criterio 7). Nació como un SqlException
            // 547 o 2627 y llegó hasta aquí ya traducido y en castellano.
            return Conflict(new { estado = 409, mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF4 — Reemplazar completamente un docente.</summary>
    [HttpPut("{cedula:int}")]
    public async Task<IActionResult> Reemplazar(int cedula, [FromBody] DocenteReemplazo peticion)
    {
        try
        {
            var docente = new Docente
            {
                Cedula = cedula,                       // de la RUTA, no del cuerpo
                Nombres = peticion.Nombres,
                Apellidos = peticion.Apellidos,
                Genero = peticion.Genero,
                Cargo = peticion.Cargo,
                FechaNacimiento = peticion.FechaNacimiento!.Value,
                Correo = peticion.Correo,
                Telefono = peticion.Telefono,
                UrlCvlac = peticion.UrlCvlac,
                FechaActualizacion = peticion.FechaActualizacion!.Value,
                Escalafon = peticion.Escalafon,
                Perfil = peticion.Perfil,
                CatMinciencia = peticion.CatMinciencia,
                ConvMinciencia = peticion.ConvMinciencia,
                Nacionalidaad = peticion.Nacionalidaad,
                LineaInvestigacionPrincipal = peticion.LineaInvestigacionPrincipal
            };

            var filas = await _servicio.Reemplazar(cedula, docente);
            return Ok(new { estado = 200, mensaje = "Docente reemplazado.", filasAfectadas = filas });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Docente no encontrado.", detalle = ex.Message });
        }
        catch (ConflictoExcepcion ex)
        {
            return Conflict(new { estado = 409, mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF5 — Actualizar parcialmente un docente.</summary>
    [HttpPatch("{cedula:int}")]
    public async Task<IActionResult> ActualizarParcial(int cedula, [FromBody] DocenteActualizar peticion)
    {
        try
        {
            // El controlador traduce la petición HTTP al tipo que entiende la
            // capa 2. DocenteActualizar no cruza: se queda en esta frontera.
            var campos = new DocenteCampos(
                peticion.Nombres, peticion.Apellidos, peticion.Genero, peticion.Cargo,
                peticion.FechaNacimiento, peticion.Correo, peticion.Telefono,
                peticion.UrlCvlac, peticion.FechaActualizacion, peticion.Escalafon,
                peticion.Perfil, peticion.CatMinciencia, peticion.ConvMinciencia,
                peticion.Nacionalidaad, peticion.LineaInvestigacionPrincipal,
                peticion.LimpiarLinea);

            var filas = await _servicio.ActualizarParcial(cedula, campos);
            return Ok(new { estado = 200, mensaje = "Docente actualizado.", filasAfectadas = filas });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { estado = 400, mensaje = "Parámetros inválidos.", detalle = ex.Message });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Docente no encontrado.", detalle = ex.Message });
        }
        catch (ConflictoExcepcion ex)
        {
            return Conflict(new { estado = 409, mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }

    /// <summary>RF6 — Borrado lógico.</summary>
    [HttpDelete("{cedula:int}")]
    public async Task<IActionResult> Eliminar(int cedula)
    {
        try
        {
            var filas = await _servicio.Eliminar(cedula);
            return Ok(new { estado = 200, mensaje = "Docente retirado.", filasAfectadas = filas });
        }
        catch (ConflictoExcepcion ex)
        {
            // EL OTRO 409, y nace en otro sitio: este lo decidió el servicio
            // (regla D2), porque la base no lo habría impedido.
            return Conflict(new { estado = 409, mensaje = ex.Message });
        }
        catch (NoEncontradoExcepcion ex)
        {
            return NotFound(new { estado = 404, mensaje = "Docente no encontrado.", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { estado = 500, mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }
}
