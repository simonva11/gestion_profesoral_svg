using System.ComponentModel.DataAnnotations;

namespace GestionProfesoral.Api.Peticiones;

/// <summary>
/// El cuerpo del PUT /api/docente/{cedula}: un reemplazo COMPLETO.
///
/// Es igual a DocenteCrear menos la cédula, y eso no es repetición por descuido:
/// la cédula identifica la fila y viene de la RUTA. Si viniera también en el
/// cuerpo habría dos fuentes para el mismo dato, y el día que no coincidan hay
/// que decidir cuál manda — una decisión que no hace falta tomar si el cuerpo
/// no la trae.
///
/// Diferencia con DocenteActualizar: aquí TODO es obligatorio. Un PUT que
/// omitiera un campo lo dejaría en blanco, porque reemplazar es reemplazar.
/// Quien solo quiere cambiar una cosa usa PATCH.
/// </summary>
public class DocenteReemplazo
{
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

    [Required(ErrorMessage = "El campo perfil es obligatorio.")]
    public string Perfil { get; set; } = string.Empty;

    [MaxLength(45, ErrorMessage = "El campo catMinciencia no puede exceder los 45 caracteres.")]
    public string? CatMinciencia { get; set; }

    [Required(ErrorMessage = "El campo convMinciencia es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo convMinciencia no puede exceder los 45 caracteres.")]
    public string ConvMinciencia { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo nacionalidaad es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo nacionalidaad no puede exceder los 45 caracteres.")]
    public string Nacionalidaad { get; set; } = string.Empty;

    /// <summary>Sigue siendo opcional: mandarlo en null en un PUT es la forma
    /// de decir «este docente ya no tiene línea principal».</summary>
    public int? LineaInvestigacionPrincipal { get; set; }
}
