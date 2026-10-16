namespace ApiGestion.Modelos;

/// <summary>
/// Una línea de investigación. Es una tabla de la v1 —no tiene clave foránea—,
/// y en la v2 importa por una razón: es el PADRE de la clave foránea opcional
/// del docente.
///
/// Lo que el desplegable necesita de aquí son dos cosas: el `Nombre`, que es lo
/// que la persona lee, y el `Id`, que es lo que viaja en la petición.
/// </summary>
public class LineaInvestigacion
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
}
