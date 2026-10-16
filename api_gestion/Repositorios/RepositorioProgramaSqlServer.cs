using System.Data;
using ApiGestion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiGestion.Repositorios;

/// <summary>
/// La capa 3 contra SQL Server, con Dapper (Artículo 2): el SQL se escribe a mano,
/// queda a la vista y SIEMPRE va parametrizado (@parametro).
///
/// Todas las consultas filtran por activo = 1: el borrado es lógico (Artículo 6).
/// </summary>
public class RepositorioProgramaSqlServer : IRepositorioPrograma
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // Los alias traducen los nombres de la tabla (snake_case) a los de la entidad
    // (PascalCase): Dapper mapea por nombre.
    private const string COLUMNAS = @"id AS Id, nombre AS Nombre, tipo AS Tipo,
            nivel AS Nivel, fecha_creacion AS FechaCreacion, fecha_cierre AS FechaCierre,
            numero_cohortes AS NumeroCohortes, cant_graduados AS CantGraduados,
            fecha_actualizacion AS FechaActualizacion, ciudad AS Ciudad,
            facultad AS Facultad";

    public async Task<IEnumerable<Programa>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        var sql = $@"
            SELECT TOP (@Limite) {COLUMNAS}
            FROM programa
            WHERE activo = 1
            ORDER BY id ASC";

        return await conexion.QueryAsync<Programa>(sql, new { Limite = limite });
    }

    public async Task<Programa?> ObtenerPorId(int id)
    {
        using var conexion = CrearConexion();
        // Un programa inactivo responde como inexistente (C8)
        var sql = $@"
            SELECT {COLUMNAS}
            FROM programa
            WHERE id = @Id AND activo = 1";

        return await conexion.QueryFirstOrDefaultAsync<Programa>(sql, new { Id = id });
    }

    public async Task Crear(Programa programa)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            INSERT INTO programa (id, nombre, tipo, nivel, fecha_creacion, fecha_cierre,
                                  numero_cohortes, cant_graduados, fecha_actualizacion,
                                  ciudad, facultad, activo)
            VALUES (@Id, @Nombre, @Tipo, @Nivel, @FechaCreacion, @FechaCierre,
                    @NumeroCohortes, @CantGraduados, @FechaActualizacion,
                    @Ciudad, @Facultad, 1)";

        await conexion.ExecuteAsync(sql, programa);
    }

    public async Task<int> Reemplazar(Programa programa)
    {
        using var conexion = CrearConexion();
        const string sql = @"
            UPDATE programa
            SET nombre = @Nombre, tipo = @Tipo, nivel = @Nivel,
                fecha_creacion = @FechaCreacion, fecha_cierre = @FechaCierre,
                numero_cohortes = @NumeroCohortes, cant_graduados = @CantGraduados,
                fecha_actualizacion = @FechaActualizacion, ciudad = @Ciudad,
                facultad = @Facultad
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, programa);
    }

    public async Task<int> ActualizarParcial(int id, ProgramaCampos campos)
    {
        using var conexion = CrearConexion();

        // El PATCH escribe solo lo que llegó, así que la consulta se compone.
        // OJO: lo que se compone son NOMBRES DE COLUMNA de una lista cerrada,
        // escrita aquí; los VALORES siempre viajan como @parametro (3_plan.md §4.8).
        var asignaciones = new List<string>();
        var parametros = new DynamicParameters();
        parametros.Add("Id", id);

        void Agregar(string columna, string parametro, object? valor)
        {
            if (valor == null) return;
            asignaciones.Add($"{columna} = @{parametro}");
            parametros.Add(parametro, valor);
        }

        Agregar("nombre", "Nombre", campos.Nombre);
        Agregar("tipo", "Tipo", campos.Tipo);
        Agregar("nivel", "Nivel", campos.Nivel);
        Agregar("fecha_creacion", "FechaCreacion", campos.FechaCreacion);
        Agregar("fecha_cierre", "FechaCierre", campos.FechaCierre);
        Agregar("numero_cohortes", "NumeroCohortes", campos.NumeroCohortes);
        Agregar("cant_graduados", "CantGraduados", campos.CantGraduados);
        Agregar("fecha_actualizacion", "FechaActualizacion", campos.FechaActualizacion);
        Agregar("ciudad", "Ciudad", campos.Ciudad);
        Agregar("facultad", "Facultad", campos.Facultad);

        if (asignaciones.Count == 0) return 0;

        var sql = $"UPDATE programa SET {string.Join(", ", asignaciones)} WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarLogico(int id)
    {
        using var conexion = CrearConexion();
        // Borrado LÓGICO en una sola consulta: cero filas afectadas significa
        // "no existe o ya estaba inactivo", que es el 404 del contrato (D-v1-4).
        const string sql = @"
            UPDATE programa
            SET activo = 0
            WHERE id = @Id AND activo = 1";

        return await conexion.ExecuteAsync(sql, new { Id = id });
    }
}
