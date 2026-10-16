namespace ApiGestion.Peticiones;

/// <summary>
/// El cuerpo del PATCH: TODOS los campos son opcionales, y solo se escriben los
/// que lleguen.
///
/// La diferencia con ProgramaReemplazo es la lección del contrato: el MISMO
/// cuerpo responde 422 en PUT y 200 en PATCH, y no lo decide un if en el
/// servicio — lo decide el tipo.
/// </summary>
public class ProgramaActualizar
{
    public string? Nombre { get; set; }
    public string? Tipo { get; set; }
    public string? Nivel { get; set; }
    public string? FechaCreacion { get; set; }
    public string? FechaCierre { get; set; }
    public string? NumeroCohortes { get; set; }
    public string? CantGraduados { get; set; }
    public string? FechaActualizacion { get; set; }
    public string? Ciudad { get; set; }
    public int? Facultad { get; set; }
}
