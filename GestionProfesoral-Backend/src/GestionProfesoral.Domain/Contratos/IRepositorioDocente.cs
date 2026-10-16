using GestionProfesoral.Domain.Entidades;

namespace GestionProfesoral.Domain.Contratos;

/// <summary>
/// CAPA 3 — el contrato de datos.
///
/// El servicio conoce ESTA interfaz y nada más: no sabe que detrás hay SQL
/// Server, ni que las operaciones son procedimientos almacenados, ni cómo se
/// llaman. Por eso se le puede enchufar un repositorio de mentiras y probarlo
/// sin base de datos (Artículo 3, y es el criterio con el que se demuestra que
/// las capas existen de verdad).
///
/// Fíjese en lo que esta interfaz NO tiene: ni IActionResult, ni códigos HTTP,
/// ni SqlException. Hacia arriba solo suben entidades, números de filas y
/// excepciones de dominio.
/// </summary>
public interface IRepositorioDocente
{
    Task<IEnumerable<Docente>> ObtenerTodos(int limite);

    Task<Docente?> ObtenerPorId(int cedula);

    /// <summary>Crea y devuelve la fila COMO QUEDÓ GUARDADA: con lo que puso el
    /// DEFAULT de la base y lo que calculó el disparador.</summary>
    Task<Docente> Crear(Docente docente);

    Task<int> Reemplazar(Docente docente);

    Task<int> ActualizarParcial(int cedula, DocenteCampos campos);

    Task<int> EliminarLogico(int cedula);

    /// <summary>
    /// Cuántos registros de detalle ACTIVOS cuelgan del docente.
    ///
    /// Existe solo para que el servicio pueda aplicar la decisión D2 («no se
    /// retira un maestro con detalle activo»). Devuelve un número, no una
    /// decisión: decidir qué hacer con ese número es de la capa 2.
    /// </summary>
    Task<int> ContarDetalleActivo(int cedula);
}

/// <summary>
/// Los campos que un PATCH puede traer, todos opcionales.
///
/// Con quince columnas, pasarlos sueltos daría una firma de quince parámetros
/// —ilegible y fácil de equivocar al llamarla—. Este tipo los agrupa SIN que la
/// capa 2 ni la 3 conozcan las clases de Peticiones/, que son la frontera HTTP.
///
/// LimpiarLinea no es un campo de la tabla: es una INTENCIÓN. Hace falta porque
/// en un PATCH `null` ya significa «no lo mandé», así que quitar la línea
/// necesita decirse de otra forma (ver DocenteActualizar).
/// </summary>
public record DocenteCampos(
    string? Nombres = null,
    string? Apellidos = null,
    string? Genero = null,
    string? Cargo = null,
    DateOnly? FechaNacimiento = null,
    string? Correo = null,
    string? Telefono = null,
    string? UrlCvlac = null,
    DateOnly? FechaActualizacion = null,
    string? Escalafon = null,
    string? Perfil = null,
    string? CatMinciencia = null,
    string? ConvMinciencia = null,
    string? Nacionalidaad = null,
    int? LineaInvestigacionPrincipal = null,
    bool LimpiarLinea = false)
{
    /// <summary>¿Llegó algo que cambiar? Si no, el PATCH es un 400 y no un 404:
    /// sin esta comprobación el procedimiento afectaría 0 filas, y 0 filas
    /// significa «no existe» en toda la demás lógica.</summary>
    public bool HayAlguno =>
        Nombres != null || Apellidos != null || Genero != null || Cargo != null
        || FechaNacimiento != null || Correo != null || Telefono != null
        || UrlCvlac != null || FechaActualizacion != null || Escalafon != null
        || Perfil != null || CatMinciencia != null || ConvMinciencia != null
        || Nacionalidaad != null || LineaInvestigacionPrincipal != null
        || LimpiarLinea;
}
