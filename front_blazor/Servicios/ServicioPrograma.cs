using System.Net.Http.Json;
using System.Text.Json;

namespace FrontGestion.Servicios;

/// <summary>
/// Programa, tal como el front lo maneja.
///
/// **Es una clase del front, no de la API.** Se parece a la de allá porque el
/// contrato es el mismo, y aun así son dos clases distintas en dos proyectos
/// distintos: si compartieran una biblioteca, los dos procesos dejarían de ser
/// independientes y el front podría romperse por un cambio interno de la API.
///
/// Lo único que los une es el JSON.
/// </summary>
public class Programa
{
    /// <summary>Código</summary>
    public int Id { get; set; }

    /// <summary>Nombre</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tipo</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Nivel</summary>
    public string Nivel { get; set; } = string.Empty;

    /// <summary>Fecha de creación</summary>
    public string FechaCreacion { get; set; } = string.Empty;

    /// <summary>Fecha de cierre</summary>
    public string? FechaCierre { get; set; }

    /// <summary>Número de cohortes</summary>
    public string NumeroCohortes { get; set; } = string.Empty;

    /// <summary>Cantidad de graduados</summary>
    public string CantGraduados { get; set; } = string.Empty;

    /// <summary>Fecha de actualización</summary>
    public string FechaActualizacion { get; set; } = string.Empty;

    /// <summary>Ciudad</summary>
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>Facultad</summary>
    public int Facultad { get; set; }
}

/// <summary>
/// Lo que devuelve cada operación: si salió bien, qué trajo, y qué errores hay
/// que mostrar.
///
/// Existe para que las páginas **no vean códigos de estado**. Una página
/// pregunta «¿salió bien?», no «¿fue 200 o 204?».
/// </summary>
public record Resultado<T>(bool Ok, T? Datos, List<string> Errores)
{
    public static Resultado<T> Bien(T datos) => new(true, datos, new());
    public static Resultado<T> Mal(List<string> errores) => new(false, default, errores);
}

/// <summary>
/// ==========================================================================
/// LA CAPA DE DATOS DEL FRONT — y por qué es de `programa` y no «de cualquier
/// tabla»
/// ==========================================================================
///
/// Este servicio es al front lo que el repositorio es a la API: la ÚNICA pieza
/// que sabe dónde viven los datos —en la API, nunca en la base de datos— y la
/// única que habla HTTP.
///
/// **Y es específico de un recurso, no genérico.** Podría escribirse un
/// `ApiService.Listar("programa")` que sirviera para cualquier tabla, y sería
/// más corto. No se hace, por el Artículo 10.1 y por lo mismo que del lado de
/// la API: un método `Listar(string tabla)` no le dice a nadie qué recursos
/// existen, y el compilador deja de revisar si esa tabla es una de las que hay.
///
/// Cuando el proyecto tenga más recursos habrá un servicio como este por cada
/// uno. Se van a parecer mucho — y cada uno va a decir sus campos, sus
/// mensajes y sus operaciones, que es justamente lo que un molde único borra.
///
/// ==========================================================================
/// LO QUE ESTE ARCHIVO NO SABE, Y NO LE HACE FALTA
/// ==========================================================================
///
/// No sabe que la API está en C#. Da la casualidad de que sí lo está —el front
/// también— pero en ninguna línea se aprovecha: todo viaja como JSON por HTTP,
/// igual que si la API estuviera en Python.
///
/// Y no sabe que detrás hay SQL Server. Eso es asunto de la API.
/// </summary>
public class ServicioPrograma
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly List<string> NoDisponible = new()
    {
        "El servicio no está disponible. ¿Está arriba la API?"
    };

    public ServicioPrograma(HttpClient http)
    {
        _http = http;
    }

    // ------------------------------------------------------------------
    // RF1 — Listar
    // ------------------------------------------------------------------
    public async Task<Resultado<List<Programa>>> Listar(int limite = 1000)
    {
        try
        {
            var r = await _http.GetAsync($"/api/programa?limite={limite}");

            // 204 es «no hay ninguno», y NO es un error: la pantalla muestra
            // un recuadro que lo dice, no un aviso rojo.
            if (r.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return Resultado<List<Programa>>.Bien(new());
            }

            if (!r.IsSuccessStatusCode)
            {
                return Resultado<List<Programa>>.Mal(await Mensajes(r));
            }

            // El sobre del contrato: { tabla, limite, total, datos[] }
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                .Deserialize<List<Programa>>(_opciones) ?? new();

            return Resultado<List<Programa>>.Bien(datos);
        }
        catch (HttpRequestException)
        {
            return Resultado<List<Programa>>.Mal(NoDisponible);
        }
        catch (TaskCanceledException)
        {
            return Resultado<List<Programa>>.Mal(NoDisponible);
        }
    }

    // ------------------------------------------------------------------
    // RF2 — Obtener uno
    // ------------------------------------------------------------------
    public async Task<Resultado<Programa>> Obtener(int id)
    {
        try
        {
            var r = await _http.GetAsync($"/api/programa/{id}");
            if (!r.IsSuccessStatusCode)
            {
                return Resultado<Programa>.Mal(await Mensajes(r));
            }

            var ficha = await r.Content.ReadFromJsonAsync<Programa>(_opciones);
            return Resultado<Programa>.Bien(ficha!);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<Programa>.Mal(NoDisponible);
        }
    }

    // ------------------------------------------------------------------
    // RF3 — Crear
    // ------------------------------------------------------------------
    public async Task<Resultado<bool>> Crear(Programa entidad)
    {
        return await Enviar(HttpMethod.Post, "/api/programa", entidad);
    }

    // ------------------------------------------------------------------
    // RF4 — Reemplazar: «guardar la ficha completa»
    //
    // La llave NO va en el cuerpo: identifica la fila y viaja en la ruta.
    // ------------------------------------------------------------------
    public async Task<Resultado<bool>> Reemplazar(int id, Programa entidad)
    {
        var cuerpo = new
        {
            nombre = entidad.Nombre,
            tipo = entidad.Tipo,
            nivel = entidad.Nivel,
            fechaCreacion = entidad.FechaCreacion,
            fechaCierre = entidad.FechaCierre,
            numeroCohortes = entidad.NumeroCohortes,
            cantGraduados = entidad.CantGraduados,
            fechaActualizacion = entidad.FechaActualizacion,
            ciudad = entidad.Ciudad,
            facultad = entidad.Facultad
        };
        return await Enviar(HttpMethod.Put, $"/api/programa/{id}", cuerpo);
    }

    // ------------------------------------------------------------------
    // RF5 — Actualizar: «guardar solo lo que cambié»
    //
    // Solo viaja lo diligenciado. Un campo en blanco NO se envía — no es que
    // se envíe vacío: sencillamente no va, y la API deja ese campo como estaba.
    //
    // El diccionario es de `object?` y no de `string`: hay campos que el
    // contrato pide como NÚMERO, y un número entre comillas la API lo
    // rechazaría aunque el valor fuera correcto.
    // ------------------------------------------------------------------
    public async Task<Resultado<bool>> Actualizar(
        int id,
        string? nombre,
        string? tipo,
        string? nivel,
        string? fechaCreacion,
        string? fechaCierre,
        string? numeroCohortes,
        string? cantGraduados,
        string? fechaActualizacion,
        string? ciudad,
        int? facultad)
    {
        var cuerpo = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(nombre)) cuerpo["nombre"] = nombre;
        if (!string.IsNullOrWhiteSpace(tipo)) cuerpo["tipo"] = tipo;
        if (!string.IsNullOrWhiteSpace(nivel)) cuerpo["nivel"] = nivel;
        if (!string.IsNullOrWhiteSpace(fechaCreacion)) cuerpo["fechaCreacion"] = fechaCreacion;
        if (!string.IsNullOrWhiteSpace(fechaCierre)) cuerpo["fechaCierre"] = fechaCierre;
        if (!string.IsNullOrWhiteSpace(numeroCohortes)) cuerpo["numeroCohortes"] = numeroCohortes;
        if (!string.IsNullOrWhiteSpace(cantGraduados)) cuerpo["cantGraduados"] = cantGraduados;
        if (!string.IsNullOrWhiteSpace(fechaActualizacion)) cuerpo["fechaActualizacion"] = fechaActualizacion;
        if (!string.IsNullOrWhiteSpace(ciudad)) cuerpo["ciudad"] = ciudad;
        if (facultad.HasValue) cuerpo["facultad"] = facultad.Value;

        return await Enviar(HttpMethod.Patch, $"/api/programa/{id}", cuerpo);
    }

    // ------------------------------------------------------------------
    // RF6 — Retirar del uso (la API lo hace lógico: la fila no se borra)
    // ------------------------------------------------------------------
    public async Task<Resultado<bool>> Eliminar(int id)
    {
        return await Enviar(HttpMethod.Delete, $"/api/programa/{id}", null);
    }

    // ------------------------------------------------------------------
    // Lo común a las cuatro operaciones que escriben
    // ------------------------------------------------------------------
    private async Task<Resultado<bool>> Enviar(HttpMethod metodo, string ruta, object? cuerpo)
    {
        try
        {
            var peticion = new HttpRequestMessage(metodo, ruta);
            if (cuerpo != null)
            {
                peticion.Content = JsonContent.Create(cuerpo);
            }

            var r = await _http.SendAsync(peticion);
            return r.IsSuccessStatusCode
                ? Resultado<bool>.Bien(true)
                : Resultado<bool>.Mal(await Mensajes(r));
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            return Resultado<bool>.Mal(NoDisponible);
        }
    }

    /// <summary>
    /// Traduce a texto los errores que produce ESTA API.
    ///
    /// El sobre es plano y tiene dos formas:
    ///   { estado, mensaje, detalle }   → 400, 404, 500
    ///   { estado, mensaje, errores[] } → 422, cuando el cuerpo no cumple
    ///
    /// **Este método es el único sitio del front que conoce ese formato.** Si
    /// mañana la API cambia el sobre, se cambia aquí y en ninguna página.
    /// </summary>
    private static async Task<List<string>> Mensajes(HttpResponseMessage r)
    {
        try
        {
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();

            if (sobre.TryGetProperty("errores", out var errores)
                && errores.ValueKind == JsonValueKind.Array
                && errores.GetArrayLength() > 0)
            {
                return errores.EnumerateArray()
                    .Select(x => x.GetString() ?? "")
                    .Where(x => x.Length > 0)
                    .ToList();
            }

            var partes = new List<string>();
            if (sobre.TryGetProperty("mensaje", out var m)) partes.Add(m.GetString() ?? "");
            if (sobre.TryGetProperty("detalle", out var dt)) partes.Add(dt.GetString() ?? "");
            partes.RemoveAll(string.IsNullOrWhiteSpace);

            return partes.Count > 0
                ? partes
                : new List<string> { "No se pudo completar la operación." };
        }
        catch
        {
            // Un 500 puede devolver HTML en vez de JSON.
            return new List<string> { "No se pudo completar la operación." };
        }
    }
}
