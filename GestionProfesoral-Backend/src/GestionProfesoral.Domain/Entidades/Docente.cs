namespace GestionProfesoral.Domain.Entidades;

/// <summary>
/// Entidad de dominio de la tabla docente: el maestro del que cuelga casi todo
/// el módulo (estudios, evaluaciones, reconocimientos, experiencia y tres
/// tablas puente).
///
/// No incluye Activo: el borrado lógico es un detalle interno del motor y no
/// forma parte de lo que la API expone (5_data_model.md §3).
///
/// A diferencia de Programa, aquí las fechas SÍ son fechas: el esquema las
/// declara DATE y no VARCHAR. Se usa DateOnly y no DateTime porque una fecha de
/// nacimiento no tiene hora, y arrastrar un «00:00:00» por el JSON obliga a
/// cada consumidor a recortarlo.
/// </summary>
public class Docente
{
    /// <summary>La cédula. Es la llave primaria, y la manda el formulario:
    /// no es IDENTITY (5_data_model.md §3).</summary>
    public int Cedula { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string Genero { get; set; } = string.Empty;

    public string Cargo { get; set; } = string.Empty;

    public DateOnly FechaNacimiento { get; set; }

    public string Correo { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string UrlCvlac { get; set; } = string.Empty;

    public DateOnly FechaActualizacion { get; set; }

    public string Escalafon { get; set; } = string.Empty;

    public string Perfil { get; set; } = string.Empty;

    /// <summary>Categoría de MinCiencias. Es el único texto que el esquema
    /// declara NULL: un docente sin categoría no tiene una cadena vacía, no
    /// tiene categoría.</summary>
    public string? CatMinciencia { get; set; }

    public string ConvMinciencia { get; set; } = string.Empty;

    /// <summary>Sí, con dos «a». Así se llama la columna en el esquema dado y
    /// renombrarla rompería el script del curso (ADR-07).</summary>
    public string Nacionalidaad { get; set; } = string.Empty;

    /// <summary>
    /// LA CLAVE FORÁNEA OPCIONAL de esta versión, y la razón de que sea `int?`
    /// y no `int`.
    ///
    /// En la pantalla es un desplegable con una opción «(ninguna)» que manda
    /// `null` — que NO es lo mismo que cadena vacía ni que 0: un 0 sería un
    /// código de línea que no existe, y la base lo rechazaría con un 409 por
    /// una línea que nadie quiso poner.
    /// </summary>
    public int? LineaInvestigacionPrincipal { get; set; }

    /// <summary>
    /// Cuántos estudios activos tiene. LA API NUNCA LO ESCRIBE: lo mantiene el
    /// disparador trg_contar_estudios dentro de la base.
    ///
    /// Por eso no aparece en DocenteCrear ni en DocenteActualizar: si el
    /// formulario pudiera mandarlo, el disparador no estaría demostrando nada
    /// (criterio 4).
    /// </summary>
    public int TotalEstudios { get; set; }
}
