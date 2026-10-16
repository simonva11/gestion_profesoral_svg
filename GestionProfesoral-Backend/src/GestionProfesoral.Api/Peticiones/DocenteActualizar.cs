using System.ComponentModel.DataAnnotations;

namespace GestionProfesoral.Api.Peticiones;

/// <summary>
/// El cuerpo del PATCH /api/docente/{cedula}: solo lo que se quiere cambiar.
///
/// TODO es opcional —ninguna propiedad lleva [Required]—, y eso es lo que
/// distingue un PATCH de un PUT: lo que no llega, no se toca. Lo que sí se
/// valida es el LARGO de lo que llega, porque un texto de 200 caracteres en una
/// columna de 60 tiene que ser un 422 y no un 500 de la base.
///
/// El PATCH vacío NO se rechaza aquí: ninguna anotación puede decir «al menos
/// uno». Lo rechaza el servicio con un 400, porque «no mandó nada que cambiar»
/// es una regla, no una forma mal escrita (6_contracts.md §0).
/// </summary>
public class DocenteActualizar
{
    [MaxLength(60, ErrorMessage = "El campo nombres no puede exceder los 60 caracteres.")]
    public string? Nombres { get; set; }

    [MaxLength(60, ErrorMessage = "El campo apellidos no puede exceder los 60 caracteres.")]
    public string? Apellidos { get; set; }

    [MaxLength(12, ErrorMessage = "El campo genero no puede exceder los 12 caracteres.")]
    public string? Genero { get; set; }

    [MaxLength(30, ErrorMessage = "El campo cargo no puede exceder los 30 caracteres.")]
    public string? Cargo { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [MaxLength(70, ErrorMessage = "El campo correo no puede exceder los 70 caracteres.")]
    [EmailAddress(ErrorMessage = "El campo correo no tiene forma de correo electrónico.")]
    public string? Correo { get; set; }

    [MaxLength(20, ErrorMessage = "El campo telefono no puede exceder los 20 caracteres.")]
    public string? Telefono { get; set; }

    [MaxLength(128, ErrorMessage = "El campo urlCvlac no puede exceder los 128 caracteres.")]
    public string? UrlCvlac { get; set; }

    public DateOnly? FechaActualizacion { get; set; }

    [MaxLength(45, ErrorMessage = "El campo escalafon no puede exceder los 45 caracteres.")]
    public string? Escalafon { get; set; }

    public string? Perfil { get; set; }

    [MaxLength(45, ErrorMessage = "El campo catMinciencia no puede exceder los 45 caracteres.")]
    public string? CatMinciencia { get; set; }

    [MaxLength(45, ErrorMessage = "El campo convMinciencia no puede exceder los 45 caracteres.")]
    public string? ConvMinciencia { get; set; }

    [MaxLength(45, ErrorMessage = "El campo nacionalidaad no puede exceder los 45 caracteres.")]
    public string? Nacionalidaad { get; set; }

    /// <summary>Cambiar la línea por otra.</summary>
    public int? LineaInvestigacionPrincipal { get; set; }

    /// <summary>
    /// LA TRAMPA DEL PATCH, Y SU SOLUCIÓN EXPLÍCITA.
    ///
    /// En un PATCH, `null` significa «no lo mandé», así que mandar
    /// lineaInvestigacionPrincipal en null NO puede significar también
    /// «quítasela»: serían dos cosas distintas escritas igual.
    ///
    /// Por eso existe esta bandera. Quien quiera dejar al docente sin línea
    /// manda {"limpiarLinea": true}, que es una intención dicha en voz alta y
    /// no un vacío que hay que adivinar.
    /// </summary>
    public bool LimpiarLinea { get; set; } = false;
}
