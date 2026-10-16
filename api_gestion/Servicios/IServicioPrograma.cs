using ApiGestion.Modelos;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>
/// El contrato de la capa de negocio. Solo conoce Modelos/ y el tipo de campos
/// parciales: las clases de Peticiones/ pertenecen a la frontera HTTP y no cruzan
/// a esta capa (3_plan.md §4.7).
///
/// Los problemas se comunican con excepciones, que el controlador traduce:
///   ArgumentException      → 400
///   NoEncontradoExcepcion  → 404
/// </summary>
public interface IServicioPrograma
{
    /// <summary>Hasta 'limite' programas activos. ArgumentException si limite &lt;= 0.</summary>
    Task<IEnumerable<Programa>> ObtenerTodos(int limite);

    /// <summary>El programa con ese código. NoEncontradoExcepcion si no existe o está inactivo.</summary>
    Task<Programa> ObtenerPorId(int id);

    /// <summary>Crea el programa. El cuerpo ya fue validado por ProgramaCrear.</summary>
    Task Crear(Programa programa);

    /// <summary>Reemplazo completo. NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> Reemplazar(int id, Programa programa);

    /// <summary>Escribe solo los campos enviados. ArgumentException si no llegó ninguno ·
    /// NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> ActualizarParcial(int id, ProgramaCampos campos);

    /// <summary>Borrado lógico. NoEncontradoExcepcion si no existe o ya estaba inactivo.</summary>
    Task<int> Eliminar(int id);
}
