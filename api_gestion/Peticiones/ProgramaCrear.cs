using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// El cuerpo del POST. Diez campos obligatorios y uno opcional: fechaCierre, que
/// un programa abierto no tiene (C12). Si falta un obligatorio, el framework
/// responde 422 antes de que el negocio se entere (3_plan.md §4.1).
/// </summary>
public class ProgramaCrear
{
    [Required(ErrorMessage = "El campo id es obligatorio.")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El campo nombre es obligatorio.")]
    [MaxLength(60, ErrorMessage = "El campo nombre no puede exceder los 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo nivel es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo nivel no puede exceder los 45 caracteres.")]
    public string Nivel { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo fechaCreacion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo fechaCreacion no puede exceder los 45 caracteres.")]
    public string FechaCreacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo numeroCohortes es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo numeroCohortes no puede exceder los 45 caracteres.")]
    public string NumeroCohortes { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo cantGraduados es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo cantGraduados no puede exceder los 45 caracteres.")]
    public string CantGraduados { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo fechaActualizacion es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo fechaActualizacion no puede exceder los 45 caracteres.")]
    public string FechaActualizacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo ciudad no puede exceder los 45 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>El único opcional: un programa abierto no tiene fecha de cierre.</summary>
    [MaxLength(45, ErrorMessage = "El campo fechaCierre no puede exceder los 45 caracteres.")]
    public string? FechaCierre { get; set; }

    [Required(ErrorMessage = "El campo facultad es obligatorio.")]
    public int? Facultad { get; set; }
}
