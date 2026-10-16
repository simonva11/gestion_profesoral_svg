using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>
/// CAPA 2 — las reglas de negocio del docente.
///
/// Depende SOLO de la interfaz del repositorio, así que no sabe qué motor hay
/// detrás ni que las operaciones son procedimientos almacenados. Por eso se
/// puede probar sin base de datos (pruebas/Docente.cs), y esa prueba es la
/// demostración de que las capas no son un dibujo.
///
/// Las tres reglas que viven aquí y en ningún otro sitio:
///   1. limite <= 0 no es una consulta válida               → 400
///   2. un PATCH sin campos no es una modificación          → 400
///   3. NO SE RETIRA UN DOCENTE CON DETALLE ACTIVO (D2)     → 409
///
/// La tercera es la interesante: la base NO la puede defender. Con borrado
/// lógico, retirar es `UPDATE activo = 0`, y un UPDATE no viola ninguna clave
/// foránea. Si no estuviera escrita aquí, no estaría escrita en ninguna parte.
/// </summary>
public class ServicioDocente : IServicioDocente
{
    private readonly IRepositorioDocente _repositorio;

    public ServicioDocente(IRepositorioDocente repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IEnumerable<Docente>> ObtenerTodos(int limite)
    {
        // La FORMA del dato es correcta —sí es un entero—, así que esto es 400
        // y no 422: lo que está mal no es cómo se escribió, es lo que pide.
        if (limite <= 0)
        {
            throw new ArgumentException("El parámetro limite debe ser un número mayor a 0.");
        }

        return await _repositorio.ObtenerTodos(limite);
    }

    public async Task<Docente> ObtenerPorId(int cedula)
    {
        var docente = await _repositorio.ObtenerPorId(cedula);

        // El repositorio dijo «no hay filas», que es un hecho. Decir «no
        // existe» es una interpretación, y es de esta capa (6_contracts.md §3.1).
        if (docente == null)
        {
            throw new NoEncontradoExcepcion($"No existe un docente con la cédula {cedula}.");
        }

        return docente;
    }

    public async Task<Docente> Crear(Docente docente)
    {
        // No se comprueba aquí que la línea de investigación exista, y es a
        // propósito: entre el SELECT que lo comprobara y el INSERT, alguien
        // pudo retirarla. La integridad la defiende la base, que es la única
        // que puede hacerlo sin carreras; el repositorio traduce su rechazo a
        // ConflictoExcepcion y el controlador responde 409.
        return await _repositorio.Crear(docente);
    }

    public async Task<int> Reemplazar(int cedula, Docente docente)
    {
        // La cédula identifica la fila y viene de la RUTA, no del cuerpo: así
        // no hay dos fuentes para el mismo dato.
        docente.Cedula = cedula;

        var filasAfectadas = await _repositorio.Reemplazar(docente);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un docente con la cédula {cedula}.");
        }

        return filasAfectadas;
    }

    public async Task<int> ActualizarParcial(int cedula, DocenteCampos campos)
    {
        // Sin esta comprobación el procedimiento afectaría 0 filas, y 0 filas
        // significa «no existe» en toda la demás lógica: responderíamos 404 a
        // un docente que sí existe, en vez del 400 que exige el contrato.
        if (!campos.HayAlguno)
        {
            throw new ArgumentException("No se envió ningún campo para actualizar.");
        }

        var filasAfectadas = await _repositorio.ActualizarParcial(cedula, campos);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe un docente con la cédula {cedula}.");
        }

        return filasAfectadas;
    }

    public async Task<int> Eliminar(int cedula)
    {
        // ─────────────────────────────────────────────────────────────
        // LA REGLA D2, Y POR QUÉ ESTÁ AQUÍ Y NO EN LA BASE
        //
        // Retirar es UPDATE activo = 0. Para el motor eso es una fila que
        // cambia un bit: ninguna clave foránea se viola, así que la base
        // dejaría huérfanos activos colgando de un maestro retirado y no diría
        // nada. La decisión de impedirlo es del negocio, y por eso se escribió
        // en el 2_spec.md antes de programarla.
        //
        // Tampoco se retira el detalle en cascada (ADR-06): eso borraría en
        // silencio, y reactivar no está en el alcance de la v2.
        // ─────────────────────────────────────────────────────────────
        var detalleActivo = await _repositorio.ContarDetalleActivo(cedula);
        if (detalleActivo > 0)
        {
            throw new ConflictoExcepcion(
                $"No se puede retirar el docente: todavía tiene {detalleActivo} " +
                $"{(detalleActivo == 1 ? "registro activo" : "registros activos")} " +
                "(estudios, evaluaciones, reconocimientos o experiencia). Retírelos primero.");
        }

        var filasAfectadas = await _repositorio.EliminarLogico(cedula);
        if (filasAfectadas == 0)
        {
            // Retirar dos veces: la segunda no encuentra nada activo que
            // retirar, y para la API eso ya no existe.
            throw new NoEncontradoExcepcion($"No existe un docente con la cédula {cedula}.");
        }

        return filasAfectadas;
    }
}
