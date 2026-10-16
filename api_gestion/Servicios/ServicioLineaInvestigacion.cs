using ApiGestion.Modelos;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

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

public class ServicioLineaInvestigacion : IServicioLineaInvestigacion
{
    private readonly IRepositorioLineaInvestigacion _repositorio;

    public ServicioLineaInvestigacion(IRepositorioLineaInvestigacion repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<LineaInvestigacion>> ObtenerTodas(int limite)
    {
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodas(limite);
    }
}
