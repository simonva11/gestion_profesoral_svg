using GestionProfesoral.Domain.Entidades;

namespace GestionProfesoral.Domain.Contratos;

/// <summary>
/// El contrato de datos del catálogo de líneas de investigación.
///
/// Vive en el DOMINIO y no en Infrastructure, y esa es la regla de esta
/// arquitectura: la capa de adentro declara lo que necesita, y la de afuera
/// lo implementa. Por eso Domain no referencia a nadie y todos lo referencian
/// a él.
/// </summary>
public interface IRepositorioLineaInvestigacion
{
    Task<IEnumerable<LineaInvestigacion>> ObtenerTodas(int limite);
}
