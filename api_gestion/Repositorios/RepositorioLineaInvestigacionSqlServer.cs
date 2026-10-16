using System.Data;
using ApiGestion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiGestion.Repositorios;

/// <summary>El contrato de datos del catálogo de líneas.</summary>
public interface IRepositorioLineaInvestigacion
{
    Task<IEnumerable<LineaInvestigacion>> ObtenerTodas(int limite);
}

/// <summary>
/// Capa 3 del catálogo. Una llamada, un procedimiento: `sp_listar_linea_investigacion`,
/// que ya filtra los inactivos.
/// </summary>
public class RepositorioLineaInvestigacionSqlServer : IRepositorioLineaInvestigacion
{
    private readonly string _cadenaConexion;

    public RepositorioLineaInvestigacionSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
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
