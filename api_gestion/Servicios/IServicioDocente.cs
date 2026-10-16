using ApiGestion.Modelos;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>
/// CAPA 2 — el contrato del negocio.
///
/// Solo conoce Modelos/ y DocenteCampos: las clases de Peticiones/ pertenecen a
/// la frontera HTTP y NO cruzan a esta capa. Si cruzaran, cambiar el formato
/// del JSON obligaría a tocar las reglas de negocio.
///
/// Los problemas se comunican con EXCEPCIONES, que el controlador traduce:
///   ArgumentException      → 400   (una regla se rompió)
///   NoEncontradoExcepcion  → 404   (no existe, o está retirado)
///   ConflictoExcepcion     → 409   (choca con lo que ya hay)
///
/// Esta interfaz no menciona HTTP en ninguna línea, y eso es lo que permite
/// probarla con un repositorio de mentiras y sin levantar un servidor.
/// </summary>
public interface IServicioDocente
{
    /// <summary>Hasta 'limite' docentes activos. ArgumentException si limite &lt;= 0.</summary>
    Task<IEnumerable<Docente>> ObtenerTodos(int limite);

    /// <summary>El docente con esa cédula. NoEncontradoExcepcion si no existe o está retirado.</summary>
    Task<Docente> ObtenerPorId(int cedula);

    /// <summary>Crea y devuelve la fila guardada. ConflictoExcepcion si la cédula
    /// ya existe o la línea indicada no existe.</summary>
    Task<Docente> Crear(Docente docente);

    /// <summary>Reemplazo completo. NoEncontradoExcepcion si no existe · devuelve filas afectadas.</summary>
    Task<int> Reemplazar(int cedula, Docente docente);

    /// <summary>Escribe solo los campos enviados. ArgumentException si no llegó
    /// ninguno · NoEncontradoExcepcion si no existe.</summary>
    Task<int> ActualizarParcial(int cedula, DocenteCampos campos);

    /// <summary>Borrado lógico. NoEncontradoExcepcion si no existe o ya estaba
    /// retirado · ConflictoExcepcion si todavía tiene detalle activo (D2).</summary>
    Task<int> Eliminar(int cedula);
}
