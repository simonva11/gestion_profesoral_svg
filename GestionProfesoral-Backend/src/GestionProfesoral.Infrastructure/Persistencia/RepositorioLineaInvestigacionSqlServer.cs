using System.Data;
using GestionProfesoral.Domain.Entidades;
using Dapper;
using Microsoft.Data.SqlClient;

using GestionProfesoral.Domain.Contratos;

namespace GestionProfesoral.Infrastructure.Persistencia;

/// <summary>
/// Capa 3 del catálogo. Una llamada, un procedimiento: `sp_listar_linea_investigacion`,
/// que ya filtra los inactivos.
/// </summary>
public class RepositorioLineaInvestigacionSqlServer : IRepositorioLineaInvestigacion
{
    private readonly string _cadenaConexion;

    // Recibe la CADENA, no la configuración de ASP.NET: así esta capa no
    // sabe que existe un appsettings.json ni un servidor web. Quién la lee
    // es asunto del ensamblador (Program.cs).
    public RepositorioLineaInvestigacionSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    public async Task<IEnumerable<LineaInvestigacion>> ObtenerTodas(int limite)
    {
        using IDbConnection conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<LineaInvestigacion>(
            "sp_listar_linea_investigacion",
            new { Limite = limite },
            commandType: CommandType.StoredProcedure);
    }
}
