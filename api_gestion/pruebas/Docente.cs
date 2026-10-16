using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Repositorios;
using ApiGestion.Servicios;

namespace ApiGestion.Pruebas;

/// <summary>
/// El repositorio de mentiras del docente: otra implementación de la MISMA
/// interfaz, que guarda las filas en una lista en memoria en vez de hablar con
/// SQL Server.
///
/// Como ServicioDocente solo conoce IRepositorioDocente, no se entera de la
/// diferencia. ESTA CLASE ES LA PRUEBA DE QUE LAS CAPAS EXISTEN: si el
/// servicio tuviera dentro una cadena de conexión o un EXEC, este archivo no
/// compilaría.
/// </summary>
public class RepositorioDocenteFalso : IRepositorioDocente
{
    private readonly List<Docente> _filas = new();

    /// <summary>Cuántos hijos activos finge que tiene cada docente. Permite
    /// probar la regla D2 sin tener la tabla de estudios.</summary>
    public Dictionary<int, int> DetallePorDocente { get; } = new();

    /// <summary>Cédulas que el «motor» va a rechazar por duplicadas, y líneas
    /// que no existen: así se prueba el 409 sin base de datos.</summary>
    public HashSet<int> LineasQueNoExisten { get; } = new();

    public Task<IEnumerable<Docente>> ObtenerTodos(int limite) =>
        Task.FromResult(_filas.Take(limite).AsEnumerable());

    public Task<Docente?> ObtenerPorId(int cedula) =>
        Task.FromResult(_filas.FirstOrDefault(d => d.Cedula == cedula));

    public Task<Docente> Crear(Docente docente)
    {
        // Reproduce lo que haría la base: la clave primaria duplicada y la
        // clave foránea inexistente suben como ConflictoExcepcion, igual que
        // si TraductorErroresSql hubiera traducido un 2627 o un 547.
        if (_filas.Any(d => d.Cedula == docente.Cedula))
        {
            throw new ConflictoExcepcion("Ya existe un docente con esa clave.");
        }

        if (docente.LineaInvestigacionPrincipal is int linea
            && LineasQueNoExisten.Contains(linea))
        {
            throw new ConflictoExcepcion(
                "No se pudo guardar el docente: el registro relacionado que indicó no existe o está retirado.");
        }

        _filas.Add(docente);
        return Task.FromResult(docente);
    }

    public Task<int> Reemplazar(Docente docente)
    {
        var i = _filas.FindIndex(d => d.Cedula == docente.Cedula);
        if (i == -1) return Task.FromResult(0);
        _filas[i] = docente;
        return Task.FromResult(1);
    }

    public Task<int> ActualizarParcial(int cedula, DocenteCampos campos)
    {
        var d = _filas.FirstOrDefault(x => x.Cedula == cedula);
        if (d == null) return Task.FromResult(0);

        if (campos.Nombres != null) d.Nombres = campos.Nombres;
        if (campos.Apellidos != null) d.Apellidos = campos.Apellidos;
        if (campos.Genero != null) d.Genero = campos.Genero;
        if (campos.Cargo != null) d.Cargo = campos.Cargo;
        if (campos.FechaNacimiento != null) d.FechaNacimiento = campos.FechaNacimiento.Value;
        if (campos.Correo != null) d.Correo = campos.Correo;
        if (campos.Telefono != null) d.Telefono = campos.Telefono;
        if (campos.UrlCvlac != null) d.UrlCvlac = campos.UrlCvlac;
        if (campos.FechaActualizacion != null) d.FechaActualizacion = campos.FechaActualizacion.Value;
        if (campos.Escalafon != null) d.Escalafon = campos.Escalafon;
        if (campos.Perfil != null) d.Perfil = campos.Perfil;
        if (campos.CatMinciencia != null) d.CatMinciencia = campos.CatMinciencia;
        if (campos.ConvMinciencia != null) d.ConvMinciencia = campos.ConvMinciencia;
        if (campos.Nacionalidaad != null) d.Nacionalidaad = campos.Nacionalidaad;

        // El orden importa: limpiar gana, porque es una intención explícita.
        if (campos.LineaInvestigacionPrincipal != null)
            d.LineaInvestigacionPrincipal = campos.LineaInvestigacionPrincipal;
        if (campos.LimpiarLinea)
            d.LineaInvestigacionPrincipal = null;

        return Task.FromResult(1);
    }

    public Task<int> EliminarLogico(int cedula)
    {
        // Sacarlo de la lista reproduce el EFECTO OBSERVABLE del borrado
        // lógico: deja de listarse y deja de encontrarse. La entidad no tiene
        // columna Activo —es detalle del motor—, así que aquí no hay nada que
        // marcar.
        var d = _filas.FirstOrDefault(x => x.Cedula == cedula);
        if (d == null) return Task.FromResult(0);
        _filas.Remove(d);
        return Task.FromResult(1);
    }

    public Task<int> ContarDetalleActivo(int cedula) =>
        Task.FromResult(DetallePorDocente.TryGetValue(cedula, out var n) ? n : 0);
}

/// <summary>
/// Las pruebas de la capa 2 del docente. CORREN SIN BASE DE DATOS: apague los
/// contenedores y vuelva a correrlas — esa es la demostración.
///
/// No tiene Main propio: el proyecto de pruebas ya tiene uno en Programa.cs.
/// Se encadena desde allá con una línea (ver LEEME del paquete).
/// </summary>
public class Docente_Pruebas
{
    private static bool _bien = true;

    private static void Revisar(bool condicion, string ok, string error)
    {
        Console.WriteLine(condicion ? $"[OK] {ok}" : $"[ERROR] {error}");
        if (!condicion) _bien = false;
    }

    private static Docente NuevoDocente(int cedula = 1017245, int? linea = null) => new()
    {
        Cedula = cedula,
        Nombres = "Ana",
        Apellidos = "Torres Gomez",
        Genero = "Femenino",
        Cargo = "Titular",
        FechaNacimiento = new DateOnly(1980, 4, 12),
        Correo = "ana.torres@umv.edu.co",
        Telefono = "3001234567",
        UrlCvlac = "https://cvlac.minciencias.gov.co/ana",
        FechaActualizacion = new DateOnly(2026, 1, 30),
        Escalafon = "Asociado",
        Perfil = "Doctora en Ingenieria",
        CatMinciencia = null,
        ConvMinciencia = "2023",
        Nacionalidaad = "Colombiana",
        LineaInvestigacionPrincipal = linea
    };

    public static async Task<bool> Ejecutar()
    {
        Console.WriteLine();
        Console.WriteLine("=== Prueba de capas: DOCENTE — SIN base de datos ===");

        var repoFalso = new RepositorioDocenteFalso();
        IServicioDocente servicio = new ServicioDocente(repoFalso);

        // 1. Arranca vacío
        Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "El sistema arranca sin docentes.",
            "Debía arrancar sin docentes.");

        // 2. Crear devuelve la fila guardada, y la línea en null sobrevive
        var guardado = await servicio.Crear(NuevoDocente());
        Revisar(guardado.LineaInvestigacionPrincipal == null,
            "Un docente sin línea principal se guarda con null, no con 0.",
            "La línea debía quedar en null: «(ninguna)» no es cero.");

        // 3. Listar lo encuentra
        var lista = (await servicio.ObtenerTodos(1000)).ToList();
        Revisar(lista.Count == 1 && lista[0].Nombres == "Ana",
            $"Docente creado y listado: {lista[0].Nombres} {lista[0].Apellidos}",
            "Debía haber exactamente un docente.");

        // 4. La cédula repetida es un CONFLICTO (→ 409), no un 500
        try
        {
            await servicio.Crear(NuevoDocente());
            Revisar(false, "", "Una cédula repetida debió lanzar ConflictoExcepcion.");
        }
        catch (ConflictoExcepcion)
        {
            Revisar(true, "Cédula repetida rechazada con ConflictoExcepcion (→ 409).", "");
        }

        // 5. Una línea de investigación inexistente también es 409
        repoFalso.LineasQueNoExisten.Add(777);
        try
        {
            await servicio.Crear(NuevoDocente(cedula: 1098332, linea: 777));
            Revisar(false, "", "Una línea inexistente debió lanzar ConflictoExcepcion.");
        }
        catch (ConflictoExcepcion)
        {
            Revisar(true, "Clave foránea inexistente rechazada con ConflictoExcepcion (→ 409).", "");
        }

        // 6. Buscar una cédula que no existe: 404
        try
        {
            await servicio.ObtenerPorId(999999);
            Revisar(false, "", "Debió lanzar NoEncontradoExcepcion.");
        }
        catch (NoEncontradoExcepcion)
        {
            Revisar(true, "Buscar una cédula inexistente lanza NoEncontradoExcepcion (→ 404).", "");
        }

        // 7. limite <= 0 es regla de negocio: 400, no 422
        try
        {
            await servicio.ObtenerTodos(0);
            Revisar(false, "", "Debió lanzar ArgumentException.");
        }
        catch (ArgumentException)
        {
            Revisar(true, "Límite menor o igual a cero rechazado con ArgumentException (→ 400).", "");
        }

        // 8. PATCH sin campos: 400, no 404
        try
        {
            await servicio.ActualizarParcial(1017245, new DocenteCampos());
            Revisar(false, "", "Debió lanzar ArgumentException por cuerpo vacío.");
        }
        catch (ArgumentException)
        {
            Revisar(true, "Actualización parcial sin campos rechazada con ArgumentException (→ 400).", "");
        }

        // 9. PATCH con un campo cambia solo ese
        await servicio.ActualizarParcial(1017245, new DocenteCampos(Cargo: "Emérito"));
        var tras = await servicio.ObtenerPorId(1017245);
        Revisar(tras.Cargo == "Emérito" && tras.Nombres == "Ana",
            "La actualización parcial cambió solo el cargo.",
            "La actualización parcial tocó campos que no debía.");

        // 10. LimpiarLinea quita la línea; mandar null NO la quita
        await servicio.ActualizarParcial(1017245, new DocenteCampos(LineaInvestigacionPrincipal: 3));
        await servicio.ActualizarParcial(1017245, new DocenteCampos(Cargo: "Titular"));
        tras = await servicio.ObtenerPorId(1017245);
        Revisar(tras.LineaInvestigacionPrincipal == 3,
            "Un PATCH que no manda la línea NO la borra: null significa «no lo mandé».",
            "La línea no debía perderse en un PATCH que no la mencionaba.");

        await servicio.ActualizarParcial(1017245, new DocenteCampos(LimpiarLinea: true));
        tras = await servicio.ObtenerPorId(1017245);
        Revisar(tras.LineaInvestigacionPrincipal == null,
            "LimpiarLinea sí la quita: la intención se dice en voz alta.",
            "LimpiarLinea debía dejar la línea en null.");

        // 11. LA REGLA D2: no se retira un docente con detalle activo
        repoFalso.DetallePorDocente[1017245] = 2;
        try
        {
            await servicio.Eliminar(1017245);
            Revisar(false, "", "Debió lanzar ConflictoExcepcion: el docente tiene detalle activo.");
        }
        catch (ConflictoExcepcion ex)
        {
            Revisar(ex.Message.Contains("2"),
                "No se puede retirar un docente con detalle activo (regla D2 → 409), y el mensaje dice cuántos.",
                "El mensaje del 409 debía decir cuántos registros activos quedan.");
        }

        // 12. Sin detalle, sí se retira
        repoFalso.DetallePorDocente[1017245] = 0;
        await servicio.Eliminar(1017245);
        Revisar(!(await servicio.ObtenerTodos(1000)).Any(),
            "Retirado el detalle, el docente sí se retira y deja de listarse.",
            "El docente debía poder retirarse.");

        // 13. Retirar dos veces: la segunda es 404
        try
        {
            await servicio.Eliminar(1017245);
            Revisar(false, "", "La segunda eliminación debió lanzar NoEncontradoExcepcion.");
        }
        catch (NoEncontradoExcepcion)
        {
            Revisar(true, "Segunda eliminación rechazada: para la API ya no existe (→ 404).", "");
        }

        Console.WriteLine(_bien
            ? "=== Docente: prueba de capas CON ÉXITO ==="
            : "=== Docente: prueba de capas FALLIDA: mire los [ERROR] ===");

        return _bien;
    }
}
