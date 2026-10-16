namespace ApiGestion.Modelos;

/// <summary>
/// Entidad de dominio que representa la tabla programa: un programa académico.
/// Es lo que viaja entre las capas.
///
/// No incluye Activo: el borrado lógico es un detalle interno del motor y no
/// forma parte de lo que la API expone (5_data_model.md §4).
///
/// Las fechas son STRING y no DateTime, porque así están declaradas en el
/// esquema dado (VARCHAR(45)). La v1 no hace aritmética de fechas, así que no
/// se corrige — queda como deuda anotada en D-v1-5.
/// </summary>
public class Programa
{
    /// <summary>El código del programa. Es la llave primaria.</summary>
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Pregrado, posgrado…</summary>
    public string Tipo { get; set; } = string.Empty;

    public string Nivel { get; set; } = string.Empty;

    public string FechaCreacion { get; set; } = string.Empty;

    /// <summary>El ÚNICO campo que admite nulos: un programa abierto no tiene
    /// fecha de cierre (C12).</summary>
    public string? FechaCierre { get; set; }

    public string NumeroCohortes { get; set; } = string.Empty;

    public string CantGraduados { get; set; } = string.Empty;

    public string FechaActualizacion { get; set; } = string.Empty;

    public string Ciudad { get; set; } = string.Empty;

    /// <summary>Un número sin clave foránea: la tabla facultad no existe en este
    /// módulo, así que no hay integridad referencial que imponer (C7).</summary>
    public int Facultad { get; set; }
}
