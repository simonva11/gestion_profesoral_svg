using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FrontGestion.Servicios;

/// <summary>
/// Docente, tal como el FRONT lo maneja.
///
/// Es una clase del front, no de la API. Se parece a la de allá porque el
/// contrato es el mismo, y aun así son dos clases distintas en dos proyectos
/// distintos: si compartieran una biblioteca, los dos procesos dejarían de ser
/// independientes y el front podría romperse por un cambio interno de la API.
///
/// Lo único que los une es el JSON.
/// </summary>
public class Docente
{
    public int Cedula { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string UrlCvlac { get; set; } = string.Empty;
    public DateOnly FechaActualizacion { get; set; }
    public string Escalafon { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string? CatMinciencia { get; set; }
    public string ConvMinciencia { get; set; } = string.Empty;
    public string Nacionalidaad { get; set; } = string.Empty;

    /// <summary>La clave foránea opcional: null cuando el desplegable está en
    /// «(ninguna)».</summary>
    public int? LineaInvestigacionPrincipal { get; set; }

    /// <summary>Solo lectura para el front: lo calcula el disparador.</summary>
    public int TotalEstudios { get; set; }
}

/// <summary>Una línea, tal como la necesita el desplegable: un texto que se lee
/// y una clave que viaja.</summary>
public class LineaInvestigacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>
/// LA CAPA DE DATOS DEL FRONT para el recurso docente.
///
/// Es al front lo que el repositorio es a la API: la única pieza que habla
/// HTTP, y la única que sabe que los datos viven en la API —nunca en la base—.
///
/// Las páginas no ven códigos de estado: preguntan «¿salió bien?» y reciben un
/// Resultado. Por eso el 409 de la v2 llega a la pantalla como una frase en
/// castellano y no como un número (criterio 11).
/// </summary>
public class ServicioDocente
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

    public ServicioDocente(HttpClient http)
    {
        _http = http;
    }

    // ------------------------------------------------------------------
    // Listar — 204 no es un error
    // ------------------------------------------------------------------
    public async Task<Resultado<List<Docente>>> Listar(int limite = 1000)
    {
        try
        {
            var r = await _http.GetAsync($"/api/docente?limite={limite}");

            // «No hay ninguno» NO es un fallo: la pantalla muestra un recuadro
            // que lo dice, no un aviso rojo.
            if (r.StatusCode == HttpStatusCode.NoContent)
            {
                return Resultado<List<Docente>>.Bien(new());
            }

            if (!r.IsSuccessStatusCode)
            {
                return Resultado<List<Docente>>.Mal(await Mensajes(r));
            }

            // EL SOBRE DEL CONTRATO: { tabla, limite, total, datos[] }.
            // Leerlo mal —esperando una lista pelada— deja la pantalla vacía y
            // SIN NINGÚN ERROR, que es la trampa más silenciosa de esta versión.
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                .Deserialize<List<Docente>>(_opciones) ?? new();

            return Resultado<List<Docente>>.Bien(datos);
        }
        catch (HttpRequestException)
        {
            return Resultado<List<Docente>>.Mal(NoDisponible);
        }
        catch (TaskCanceledException)
        {
            return Resultado<List<Docente>>.Mal(NoDisponible);
        }
    }

    // ------------------------------------------------------------------
    // Las opciones del desplegable (criterio 5)
    // ------------------------------------------------------------------
    // Este método es el que hace que la clave foránea NO se digite. Se llama
    // ANTES de mostrar el formulario: sin él, el <select> saldría vacío y la
    // persona no tendría de dónde elegir.
    public async Task<Resultado<List<LineaInvestigacion>>> ListarLineas()
    {
        try
        {
            var r = await _http.GetAsync("/api/linea-investigacion");

            if (r.StatusCode == HttpStatusCode.NoContent)
            {
                return Resultado<List<LineaInvestigacion>>.Bien(new());
            }

            if (!r.IsSuccessStatusCode)
            {
                return Resultado<List<LineaInvestigacion>>.Mal(await Mensajes(r));
            }

            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();
            var datos = sobre.GetProperty("datos")
                .Deserialize<List<LineaInvestigacion>>(_opciones) ?? new();

            return Resultado<List<LineaInvestigacion>>.Bien(datos);
        }
        catch (HttpRequestException)
        {
            return Resultado<List<LineaInvestigacion>>.Mal(NoDisponible);
        }
    }

    // ------------------------------------------------------------------
    // Crear
    // ------------------------------------------------------------------
    public async Task<Resultado<bool>> Crear(Docente docente)
    {
        try
        {
            var r = await _http.PostAsJsonAsync("/api/docente", docente);

            return r.IsSuccessStatusCode
                ? Resultado<bool>.Bien(true)
                : Resultado<bool>.Mal(await Mensajes(r));
        }
        catch (HttpRequestException)
        {
            return Resultado<bool>.Mal(NoDisponible);
        }
    }

    // ------------------------------------------------------------------
    // Reemplazar (el botón «Guardar cambios» de la pantalla)
    // ------------------------------------------------------------------
    public async Task<Resultado<bool>> Reemplazar(int cedula, Docente docente)
    {
        try
        {
            var r = await _http.PutAsJsonAsync($"/api/docente/{cedula}", docente);

            return r.IsSuccessStatusCode
                ? Resultado<bool>.Bien(true)
                : Resultado<bool>.Mal(await Mensajes(r));
        }
        catch (HttpRequestException)
        {
            return Resultado<bool>.Mal(NoDisponible);
        }
    }

    // ------------------------------------------------------------------
    // Retirar (borrado lógico)
    // ------------------------------------------------------------------
    public async Task<Resultado<bool>> Retirar(int cedula)
    {
        try
        {
            var r = await _http.DeleteAsync($"/api/docente/{cedula}");

            // Aquí es donde llega el 409 de la regla D2: «todavía tiene 2
            // registros activos». La pantalla lo muestra tal cual, porque ya
            // viene escrito para que lo lea una persona.
            return r.IsSuccessStatusCode
                ? Resultado<bool>.Bien(true)
                : Resultado<bool>.Mal(await Mensajes(r));
        }
        catch (HttpRequestException)
        {
            return Resultado<bool>.Mal(NoDisponible);
        }
    }

    // ------------------------------------------------------------------
    // El sobre de error del contrato, traducido a frases para la pantalla
    // ------------------------------------------------------------------
    private static async Task<List<string>> Mensajes(HttpResponseMessage r)
    {
        try
        {
            var sobre = await r.Content.ReadFromJsonAsync<JsonElement>();

            // En el 422 vienen varios: uno por campo mal escrito.
            if (sobre.TryGetProperty("errores", out var errores)
                && errores.ValueKind == JsonValueKind.Array)
            {
                var lista = errores.EnumerateArray()
                    .Select(e => e.GetString() ?? "")
                    .Where(e => e != "")
                    .ToList();

                if (lista.Count > 0) return lista;
            }

            if (sobre.TryGetProperty("mensaje", out var mensaje))
            {
                return new List<string> { mensaje.GetString() ?? "No se pudo completar la operación." };
            }
        }
        catch (JsonException)
        {
            // La respuesta no traía el sobre: no se inventa un motivo.
        }

        return new List<string> { "No se pudo completar la operación." };
    }
}
