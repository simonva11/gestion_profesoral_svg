using ApiGestion.Excepciones;
using Microsoft.Data.SqlClient;

namespace ApiGestion.Repositorios;

/// <summary>
/// LA PIEZA QUE CONVIERTE UN 500 EN UN 409.
///
/// Cuando la base rechaza algo por integridad, lo que sube es una SqlException
/// con un número y un mensaje en inglés lleno de nombres de restricciones. Si
/// nadie la atrapa, llega al catch genérico del controlador y sale un **500**
/// — que es decir «me caí», y quien lo lea va a buscar un error que no existe
/// (criterio 7: un 500 es criterio fallado).
///
/// POR QUÉ ESTÁ EN LA CAPA 3 Y NO EN EL CONTROLADOR (ADR-02):
/// esta es la única capa que ya conoce el motor. Si la traducción estuviera
/// arriba, el controlador tendría que importar Microsoft.Data.SqlClient, y la
/// capa de datos se habría filtrado hasta el borde de HTTP. Hacia arriba solo
/// sube ConflictoExcepcion, que es de dominio y no habla de SQL.
///
/// QUÉ PASA SI NO ENCUENTRA EL NÚMERO: devuelve la excepción original, sin
/// disfrazarla. Un error que no entendemos tiene que seguir siendo un 500: un
/// 409 inventado diría que el usuario hizo algo mal cuando el que falló fue el
/// sistema.
/// </summary>
public static class TraductorErroresSql
{
    // Los números son del motor, no nuestros. Están aquí con nombre para que
    // el `switch` de abajo se pueda leer sin buscarlos en internet.
    private const int ClaveForaneaViolada = 547;   // el padre no existe, o tiene hijos
    private const int ClavePrimariaDuplicada = 2627;
    private const int IndiceUnicoDuplicado = 2601;

    /// <summary>
    /// Traduce el error del motor a una excepción de dominio, en castellano.
    /// </summary>
    /// <param name="ex">La excepción tal como la lanzó SQL Server.</param>
    /// <param name="recurso">
    /// Cómo se llama lo que se estaba guardando, para que el mensaje hable del
    /// negocio y no de la tabla: «docente», «estudio», «asignación a la red».
    /// </param>
    public static Exception Traducir(SqlException ex, string recurso = "registro")
    {
        return ex.Number switch
        {
            ClaveForaneaViolada => new ConflictoExcepcion(
                $"No se pudo guardar el {recurso}: el registro relacionado que indicó no existe o está retirado."),

            ClavePrimariaDuplicada or IndiceUnicoDuplicado => new ConflictoExcepcion(
                $"Ya existe un {recurso} con esa clave."),

            // No lo conocemos: que siga siendo un 500. Mentir aquí sería peor
            // que no traducir.
            _ => ex
        };
    }
}
