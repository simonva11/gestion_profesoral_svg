using GestionProfesoral.Domain.Entidades;
using GestionProfesoral.Domain.Contratos;

using GestionProfesoral.Application.Contratos;

namespace GestionProfesoral.Application.Servicios;

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
