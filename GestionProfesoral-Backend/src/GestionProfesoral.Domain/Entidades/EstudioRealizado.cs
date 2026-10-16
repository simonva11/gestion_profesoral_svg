namespace GestionProfesoral.Domain.Entidades;

/// <summary>
/// Entidad de la tabla estudios_realizados: un título académico de un docente.
///
/// ES UN DETALLE, y eso cambia cómo se trata: **un estudio no existe sin su
/// docente**. No se crea suelto para después buscarle padre, y por eso su ruta
/// va anidada bajo el maestro (`/api/docente/{cedula}/estudios`) y la cédula
/// viaja en la RUTA, no en el cuerpo.
///
/// De esta tabla cuelgan a su vez `apoyo_profesoral` y `beca` (1 a 1) y la
/// puente `estudio_ac` — son de las fases 4 y 5.
/// </summary>
public class EstudioRealizado
{
    /// <summary>
    /// Llave primaria. La tabla NO es IDENTITY en el esquema dado, así que
    /// cuando no llega, lo asigna el procedimiento dentro de la transacción
    /// (ver sp_crear_estudios_realizados).
    /// </summary>
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Universidad { get; set; } = string.Empty;

    public DateOnly Fecha { get; set; }

    /// <summary>Pregrado, especialización, maestría, doctorado…</summary>
    public string Tipo { get; set; } = string.Empty;

    public string Ciudad { get; set; } = string.Empty;

    /// <summary>La clave foránea al maestro. Es obligatoria —el esquema la
    /// declara NOT NULL— y la pone la ruta, no el formulario.</summary>
    public int Docente { get; set; }

    /// <summary>¿La institución está acreditada? El esquema lo declara
    /// TINYINT, no BIT: se respeta y se expone como número, porque cambiar el
    /// tipo sería alterar el esquema dado.</summary>
    public byte InsAcreditada { get; set; }

    public string Metodologia { get; set; } = string.Empty;

    public string PerfilEgresado { get; set; } = string.Empty;

    public string Pais { get; set; } = string.Empty;
}
