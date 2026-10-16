using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>
/// El contrato de la capa de datos. El servicio conoce ESTA interfaz y nada más:
/// no sabe que detrás hay SQL Server, y por eso se le puede enchufar un
/// repositorio de mentiras para probarlo sin base de datos (Artículo 3).
/// </summary>
public interface IRepositorioPrograma
{
    Task<IEnumerable<Programa>> ObtenerTodos(int limite);
    Task<Programa?> ObtenerPorId(int id);
    Task Crear(Programa programa);
    Task<int> Reemplazar(Programa programa);
    Task<int> ActualizarParcial(int id, ProgramaCampos campos);
    Task<int> EliminarLogico(int id);
}

/// <summary>
/// Los campos que un PATCH puede traer, todos opcionales.
///
/// Con once columnas, pasarlos sueltos daría una firma de once parámetros
/// —ilegible y fácil de equivocar al llamarla—. Este tipo los agrupa SIN que la
/// capa 2 ni la 3 conozcan las clases de Peticiones/, que son la frontera HTTP
/// (3_plan.md §4.7).
/// </summary>
public record ProgramaCampos(
    string? Nombre = null,
    string? Tipo = null,
    string? Nivel = null,
    string? FechaCreacion = null,
    string? FechaCierre = null,
    string? NumeroCohortes = null,
    string? CantGraduados = null,
    string? FechaActualizacion = null,
    string? Ciudad = null,
    int? Facultad = null)
{
    /// <summary>¿Llegó algún campo? Si no, el PATCH es un 400 y no un 404.</summary>
    public bool HayAlguno =>
        Nombre != null || Tipo != null || Nivel != null || FechaCreacion != null
        || FechaCierre != null || NumeroCohortes != null || CantGraduados != null
        || FechaActualizacion != null || Ciudad != null || Facultad != null;
}
