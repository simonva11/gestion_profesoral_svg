using System.Data;
using ApiGestion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiGestion.Repositorios;

/// <summary>
/// CAPA 3 contra SQL Server — y ESTE ES EL ARCHIVO QUE CAMBIA EN LA VERSIÓN 2.
///
/// En la v1 aquí se escribía el SQL a mano con Dapper. Ahora no hay una sola
/// consulta: solo LLAMADAS a procedimientos almacenados, con
/// CommandType.StoredProcedure y los valores viajando como parámetros.
///
/// QUÉ SE GANA (criterio 3, y ADR-01):
///   · el SQL queda en UN sitio, con nombre y versionado en db/gestion_profesoral.sql;
///   · cambiar una consulta no obliga a recompilar la API;
///   · no hay texto que pegar, así que no hay por dónde inyectar SQL.
///
/// QUÉ SE PIERDE, dicho sin adornos: para entender una operación hay que
/// mirar dos archivos, este y el script. Es el precio, y hay que saber decirlo.
///
/// Lo que NO hace esta clase: decidir. No sabe qué es un 404 ni un 409; lanza
/// excepciones de dominio y quien decide es el servicio.
/// </summary>
public class RepositorioDocenteSqlServer : IRepositorioDocente
{
    private readonly string _cadenaConexion;

    // Se registra UNA vez por proceso. DateOnly es un tipo de C#; el motor solo
    // conoce DATE, así que alguien tiene que traducir, y ese alguien es la capa
    // que conoce el motor.
    static RepositorioDocenteSqlServer()
    {
        SqlMapper.AddTypeHandler(new ManejadorDateOnly());
    }

    public RepositorioDocenteSqlServer(IConfiguration configuracion)
    {
        // El secreto NO está en el código: llega por variable de entorno
        // (ConnectionStrings__SqlServer en el docker-compose.yml).
        _cadenaConexion = configuracion.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");
    }

    private IDbConnection CrearConexion() => new SqlConnection(_cadenaConexion);

    // ------------------------------------------------------------------
    // RF1 — Listar (sp_listar_docente)
    // ------------------------------------------------------------------
    // El WHERE activo = 1 NO está aquí: está dentro del procedimiento. Si un
    // día hay que cambiar qué se considera «activo», se cambia en la base y
    // ningún repositorio se entera.
    public async Task<IEnumerable<Docente>> ObtenerTodos(int limite)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<Docente>(
            "sp_listar_docente",
            new { Limite = limite },
            commandType: CommandType.StoredProcedure);
    }

    // ------------------------------------------------------------------
    // RF2 — Consultar (sp_consultar_docente)
    // ------------------------------------------------------------------
    public async Task<Docente?> ObtenerPorId(int cedula)
    {
        using var conexion = CrearConexion();

        // Devuelve null si no hay filas. Un docente RETIRADO responde igual que
        // uno inexistente, porque el procedimiento filtra activo = 1: quien
        // retiró un registro no tiene por qué poder confirmar que existió.
        return await conexion.QueryFirstOrDefaultAsync<Docente>(
            "sp_consultar_docente",
            new { Cedula = cedula },
            commandType: CommandType.StoredProcedure);
    }

    // ------------------------------------------------------------------
    // RF3 — Crear (sp_crear_docente)
    // ------------------------------------------------------------------
    public async Task<Docente> Crear(Docente docente)
    {
        using var conexion = CrearConexion();
        try
        {
            // El procedimiento termina con un SELECT de la fila recién
            // insertada, así que esto devuelve el docente COMO QUEDÓ GUARDADO:
            // con el activo = 1 que puso el DEFAULT y el totalEstudios = 0 que
            // mantiene el disparador. Devolver lo que mandó el formulario sería
            // devolver lo que ya sabíamos.
            var guardado = await conexion.QueryFirstAsync<Docente>(
                "sp_crear_docente",
                new
                {
                    docente.Cedula,
                    docente.Nombres,
                    docente.Apellidos,
                    docente.Genero,
                    docente.Cargo,
                    docente.FechaNacimiento,
                    docente.Correo,
                    docente.Telefono,
                    docente.UrlCvlac,
                    docente.FechaActualizacion,
                    docente.Escalafon,
                    docente.Perfil,
                    docente.CatMinciencia,
                    docente.ConvMinciencia,
                    docente.Nacionalidaad,
                    docente.LineaInvestigacionPrincipal
                },
                commandType: CommandType.StoredProcedure);

            return guardado;
        }
        catch (SqlException ex)
        {
            // Cédula repetida (2627) o línea de investigación inexistente (547).
            // Aquí nace el 409 del criterio 7: sin esta línea saldría un 500.
            throw TraductorErroresSql.Traducir(ex, "docente");
        }
    }

    // ------------------------------------------------------------------
    // RF4 — Reemplazar (sp_actualizar_docente con TODOS los campos)
    // ------------------------------------------------------------------
    // Un solo procedimiento sirve para el PUT y para el PATCH: usa COALESCE, y
    // lo que llega en null no se cambia. El PUT simplemente manda todo.
    public async Task<int> Reemplazar(Docente docente)
    {
        using var conexion = CrearConexion();
        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "sp_actualizar_docente",
                new
                {
                    docente.Cedula,
                    docente.Nombres,
                    docente.Apellidos,
                    docente.Genero,
                    docente.Cargo,
                    docente.FechaNacimiento,
                    docente.Correo,
                    docente.Telefono,
                    docente.UrlCvlac,
                    docente.FechaActualizacion,
                    docente.Escalafon,
                    docente.Perfil,
                    docente.CatMinciencia,
                    docente.ConvMinciencia,
                    docente.Nacionalidaad,
                    docente.LineaInvestigacionPrincipal,
                    // En un PUT, mandar la línea en null SÍ significa quitarla:
                    // reemplazar es reemplazar.
                    LimpiarLinea = docente.LineaInvestigacionPrincipal == null
                },
                commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            throw TraductorErroresSql.Traducir(ex, "docente");
        }
    }

    // ------------------------------------------------------------------
    // RF5 — Actualización parcial (el mismo procedimiento, con menos campos)
    // ------------------------------------------------------------------
    // Compare esto con la v1: allí el PATCH componía el UPDATE pegando nombres
    // de columna en un string. Aquí no se compone nada — lo que no se manda
    // llega como null y el COALESCE del procedimiento lo deja como estaba.
    public async Task<int> ActualizarParcial(int cedula, DocenteCampos campos)
    {
        using var conexion = CrearConexion();
        try
        {
            return await conexion.ExecuteScalarAsync<int>(
                "sp_actualizar_docente",
                new
                {
                    Cedula = cedula,
                    campos.Nombres,
                    campos.Apellidos,
                    campos.Genero,
                    campos.Cargo,
                    campos.FechaNacimiento,
                    campos.Correo,
                    campos.Telefono,
                    campos.UrlCvlac,
                    campos.FechaActualizacion,
                    campos.Escalafon,
                    campos.Perfil,
                    campos.CatMinciencia,
                    campos.ConvMinciencia,
                    campos.Nacionalidaad,
                    campos.LineaInvestigacionPrincipal,
                    campos.LimpiarLinea
                },
                commandType: CommandType.StoredProcedure);
        }
        catch (SqlException ex)
        {
            throw TraductorErroresSql.Traducir(ex, "docente");
        }
    }

    // ------------------------------------------------------------------
    // RF6 — Borrado lógico (sp_eliminar_docente)
    // ------------------------------------------------------------------
    // El procedimiento hace UPDATE activo = 0, no DELETE. Devuelve las filas
    // afectadas: 0 significa «no existe o ya estaba retirado», y eso lo
    // traduce el servicio a 404.
    //
    // Lo que este método NO comprueba es si el docente tiene detalle activo:
    // eso es una regla de negocio (D2) y vive en el servicio.
    public async Task<int> EliminarLogico(int cedula)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(
            "sp_eliminar_docente",
            new { Cedula = cedula },
            commandType: CommandType.StoredProcedure);
    }

    // ------------------------------------------------------------------
    // Apoyo a la decisión D2 (sp_contar_detalle_docente)
    // ------------------------------------------------------------------
    public async Task<int> ContarDetalleActivo(int cedula)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(
            "sp_contar_detalle_docente",
            new { Cedula = cedula },
            commandType: CommandType.StoredProcedure);
    }
}

/// <summary>
/// Traductor entre el DateOnly de C# y el DATE de SQL Server.
///
/// Está en la capa 3 por la misma razón que el traductor de errores: es la
/// única que conoce el motor. Gracias a esto el resto del sistema maneja fechas
/// sin hora, y el JSON dice "1980-04-12" en vez de "1980-04-12T00:00:00".
/// </summary>
public class ManejadorDateOnly : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parametro, DateOnly valor)
    {
        parametro.DbType = DbType.Date;
        parametro.Value = valor.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object valor) =>
        DateOnly.FromDateTime((DateTime)valor);
}
