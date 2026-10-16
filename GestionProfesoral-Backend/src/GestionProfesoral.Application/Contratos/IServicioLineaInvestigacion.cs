using GestionProfesoral.Domain.Entidades;

namespace GestionProfesoral.Application.Contratos;

/// <summary>
/// La capa 2 del catálogo de líneas. Tiene una sola regla, y aun así existe:
/// si el controlador llamara directo al repositorio, la próxima regla
/// —filtrar por facultad, ordenar de otra forma— no tendría dónde ir, y
/// terminaría dentro del controlador o repetida en cada pantalla.
/// </summary>
public interface IServicioLineaInvestigacion
{
    Task<IEnumerable<LineaInvestigacion>> ObtenerTodas(int limite);
}
