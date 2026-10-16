namespace GestionProfesoral.Domain.Excepciones;

/// <summary>
/// LA EXCEPCIÓN QUE ESTRENA LA VERSIÓN 2.
///
/// La v1 no la necesitaba: sin claves foráneas no hay conflictos de
/// integridad. Con ellas aparecen tres situaciones que no son «no existe»
/// (404) ni «está mal escrito» (422), sino «lo que pide choca con lo que ya
/// hay»:
///
///   1. Crear un hijo cuyo padre no existe        → la base lo rechaza (547)
///   2. Repetir la pareja de una tabla puente     → la base lo rechaza (2627)
///   3. Retirar un maestro con detalle activo     → LA BASE NO LO IMPIDE:
///      es una regla de negocio (decisión D2), porque con borrado lógico
///      retirar es un UPDATE y un UPDATE no viola ninguna clave foránea
///
/// Los tres salen como 409. Los dos primeros nacen en el repositorio, que los
/// traduce con TraductorErroresSql; el tercero nace en el servicio.
///
/// Y como la de 404, esta excepción NO menciona HTTP: dice qué pasó en el
/// dominio, y traducirlo a un número es trabajo del controlador (Artículo 3).
/// Si aquí dijera «409», el servicio tendría una opinión sobre un protocolo
/// que no conoce.
/// </summary>
public class ConflictoExcepcion : Exception
{
    public ConflictoExcepcion(string mensaje) : base(mensaje)
    {
    }
}
