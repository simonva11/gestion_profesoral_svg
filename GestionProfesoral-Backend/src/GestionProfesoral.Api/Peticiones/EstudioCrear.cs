using System.ComponentModel.DataAnnotations;

namespace GestionProfesoral.Api.Peticiones;

/// <summary>
/// El cuerpo del POST /api/docente/{cedula}/estudios.
///
/// FÍJESE EN LO QUE NO ESTÁ: no hay campo `docente`. La cédula del maestro
/// viene de la RUTA, y eso no es un ahorro de tecleo: es lo que impide que el
/// formulario mande un docente distinto al de la pantalla que se está viendo.
///
/// Tampoco hay `id`: lo asigna la base dentro de la transacción. Un número de
/// estudio no es algo que una persona deba inventar.
/// </summary>
public class EstudioCrear
{
    [Required(ErrorMessage = "El campo titulo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo titulo no puede exceder los 45 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo universidad es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El campo universidad no puede exceder los 50 caracteres.")]
    public string Universidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo fecha es obligatorio.")]
    public DateOnly? Fecha { get; set; }

    [Required(ErrorMessage = "El campo tipo es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo ciudad es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo ciudad no puede exceder los 45 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo insAcreditada es obligatorio.")]
    [Range(0, 1, ErrorMessage = "El campo insAcreditada solo admite 0 o 1.")]
    public byte? InsAcreditada { get; set; }

    [Required(ErrorMessage = "El campo metodologia es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo metodologia no puede exceder los 45 caracteres.")]
    public string Metodologia { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo perfilEgresado es obligatorio.")]
    public string PerfilEgresado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo pais es obligatorio.")]
    [MaxLength(45, ErrorMessage = "El campo pais no puede exceder los 45 caracteres.")]
    public string Pais { get; set; } = string.Empty;
}

/// <summary>
/// El cuerpo del PATCH: todo opcional, lo que no llega no se toca. El `docente`
/// tampoco está aquí — un estudio no se cambia de dueño con un PATCH; eso sería
/// retirarlo de uno y crearlo en otro.
/// </summary>
public class EstudioActualizar
{
    [MaxLength(45, ErrorMessage = "El campo titulo no puede exceder los 45 caracteres.")]
    public string? Titulo { get; set; }

    [MaxLength(50, ErrorMessage = "El campo universidad no puede exceder los 50 caracteres.")]
    public string? Universidad { get; set; }

    public DateOnly? Fecha { get; set; }

    [MaxLength(45, ErrorMessage = "El campo tipo no puede exceder los 45 caracteres.")]
    public string? Tipo { get; set; }

    [MaxLength(45, ErrorMessage = "El campo ciudad no puede exceder los 45 caracteres.")]
    public string? Ciudad { get; set; }

    [Range(0, 1, ErrorMessage = "El campo insAcreditada solo admite 0 o 1.")]
    public byte? InsAcreditada { get; set; }

    [MaxLength(45, ErrorMessage = "El campo metodologia no puede exceder los 45 caracteres.")]
    public string? Metodologia { get; set; }

    public string? PerfilEgresado { get; set; }

    [MaxLength(45, ErrorMessage = "El campo pais no puede exceder los 45 caracteres.")]
    public string? Pais { get; set; }
}

/// <summary>
/// El cuerpo del POST /api/docente/{cedula}/estudios/lote: VARIOS RENGLONES EN
/// UN SOLO ENVÍO.
///
/// Por qué existe este tipo y no se manda un arreglo pelado: un arreglo en la
/// raíz del JSON no puede crecer —no se le puede agregar un campo después sin
/// romper a quien lo lee—, y además así el 422 puede decir «el lote no puede
/// venir vacío» como lo que es: una validación de forma.
/// </summary>
public class EstudiosLote
{
    [Required(ErrorMessage = "El campo estudios es obligatorio.")]
    [MinLength(1, ErrorMessage = "El lote debe traer al menos un estudio.")]
    public List<EstudioCrear> Estudios { get; set; } = new();
}
