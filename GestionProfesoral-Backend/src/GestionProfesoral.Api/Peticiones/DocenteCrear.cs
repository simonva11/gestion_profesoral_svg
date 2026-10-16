using System.ComponentModel.DataAnnotations;

namespace GestionProfesoral.Api.Peticiones;

/// <summary>
/// El cuerpo del POST /api/docente.
///
/// Esta clase es LA FRONTERA: si falta un campo obligatorio o un texto se pasa
/// de largo, [ApiController] corta la petición ANTES de que el método del
/// controlador se ejecute y responde 422 con el sobre {estado, mensaje,
/// errores[]} (6_contracts.md §3.2). Ni una línea de negocio llega a correr, y
/// no se gasta una conexión a la base en un cuerpo que ya se sabe que está mal.
///
/// Los largos no son un capricho: son los del esquema. Si aquí dijeran 200 y la
/// columna aguanta 60, el error saldría de la base como un 500 en inglés en vez
/// de un 422 en castellano que dice qué campo fue.
/// </summary>
public class DocenteCrear
{
    // Es int? y no int a propósito: con `int`, un cuerpo sin cédula llegaría
    // como 0 y [Required] no se quejaría — 0 es un valor válido para un int.
    // Con `int?`, ausente es null, y el 422 sale.
    [Required(ErrorMessage = "El campo cedula es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo cedula debe ser un número mayor a 0.")]
    public int? Cedula { get; set; }

    [Required(ErrorMessage = "El campo nombres es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El campo nombres no puede exceder los 60 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo apellidos es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El campo apellidos no puede exceder los 60 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo genero es obligatorio.")]
    [MaxLength(12, ErrorMessage = "El campo genero no puede exceder los 12 caracteres.")]
    public string Genero { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo cargo es obligatorio.")]
    [MaxLength(30, ErrorMessage = "El campo cargo no puede exceder los 30 caracteres.")]
    public string Cargo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo fechaNacimiento es obligatorio.")]
    public DateOnly? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El campo correo es obligatorio.")]
    [MaxLength(70, ErrorMessage = "El campo correo no puede exceder los 70 caracteres.")]
    // Valida la FORMA, que es lo que corresponde a esta capa. Que el correo
    // exista de verdad no se sabe hasta que alguien le escriba.
    [EmailAddress(ErrorMessage = "El campo correo no tiene forma de correo electrónico.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo telefono es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El campo telefono no puede exceder los 20 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo urlCvlac es obligatorio.")]
    [MaxLength(128, ErrorMessage = "El campo urlCvlac no puede exceder los 128 caracteres.")]
    public string UrlCvlac { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo fechaActualizacion es obligatorio.")]
    public DateOnly? FechaActualizacion { get; set; }

    [Required(ErrorMessage = "El campo escalafon es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo escalafon no puede exceder los 45 caracteres.")]
    public string Escalafon { get; set; } = string.Empty;

    // Sin MaxLength: la columna es VARCHAR(MAX)
    [Required(ErrorMessage = "El campo perfil es obligatorio.")]
    public string Perfil { get; set; } = string.Empty;

    /// <summary>Opcional: la columna admite nulos.</summary>
    [MaxLength(45, ErrorMessage = "El campo catMinciencia no puede exceder los 45 caracteres.")]
    public string? CatMinciencia { get; set; }

    [Required(ErrorMessage = "El campo convMinciencia es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo convMinciencia no puede exceder los 45 caracteres.")]
    public string ConvMinciencia { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo nacionalidaad es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo nacionalidaad no puede exceder los 45 caracteres.")]
    public string Nacionalidaad { get; set; } = string.Empty;

    /// <summary>
    /// LA CLAVE FORÁNEA OPCIONAL. No lleva [Required] a propósito: un docente
    /// puede no tener línea principal, y el desplegable manda `null` cuando la
    /// persona elige «(ninguna)».
    ///
    /// Lo que NO se valida aquí es que la línea exista: eso lo defiende la base
    /// con su clave foránea, y el repositorio traduce el error a un 409
    /// (6_contracts.md §3.3). Comprobarlo antes con un SELECT sería una carrera:
    /// entre el SELECT y el INSERT alguien pudo retirar esa línea.
    /// </summary>
    public int? LineaInvestigacionPrincipal { get; set; }

    // Y aquí NO hay TotalEstudios: lo escribe el disparador. Un campo que el
    // formulario pudiera mandar sería un campo que el disparador no controla.
}
