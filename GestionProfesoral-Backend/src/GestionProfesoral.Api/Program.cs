using GestionProfesoral.Application.Contratos;
using GestionProfesoral.Application.Servicios;
using GestionProfesoral.Domain.Contratos;
using GestionProfesoral.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// La API escucha en el 8074 también DENTRO del contenedor, para que el Dockerfile,
// el docker-compose y los contratos digan todos el mismo número.
builder.WebHost.UseUrls("http://0.0.0.0:8074");

// ============================================================
// EL SECRETO, LEÍDO EN UN SOLO SITIO
//
// Llega por variable de entorno (ConnectionStrings__SqlServer en el
// docker-compose.yml). Lo lee ESTE archivo y nadie más: los repositorios
// reciben la cadena ya resuelta, así que la capa de infraestructura no sabe
// que existe un appsettings.json ni un servidor web.
// ============================================================
var cadenaSqlServer = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'SqlServer'.");

// ============================================================
// EL ENSAMBLADOR (Artículo 3)
//
// ESTE ES EL ÚNICO ARCHIVO DEL PROYECTO donde una clase concreta aparece
// junto a su interfaz — y el único que referencia a Infrastructure.
//
// Es lo que hace que la regla de dependencias sea REAL y no un dibujo:
//
//     Api ──→ Application ──→ Domain ←── Infrastructure
//      └───────────────────────────────────┘  (solo aquí, para registrar)
//
// Application NO referencia a Infrastructure. Por eso el servicio no puede
// nombrar a SQL Server ni aunque alguien lo intente: no compila.
// ============================================================

// --- v1 ---
builder.Services.AddScoped<IRepositorioPrograma>(_ => new RepositorioProgramaSqlServer(cadenaSqlServer));
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();

// --- v2, fase 1 ---
builder.Services.AddScoped<IRepositorioDocente>(_ => new RepositorioDocenteSqlServer(cadenaSqlServer));
builder.Services.AddScoped<IServicioDocente, ServicioDocente>();

// Catálogo de solo lectura: es lo que llena el desplegable de la clave
// foránea opcional del docente (criterio 5).
builder.Services.AddScoped<IRepositorioLineaInvestigacion>(_ => new RepositorioLineaInvestigacionSqlServer(cadenaSqlServer));
builder.Services.AddScoped<IServicioLineaInvestigacion, ServicioLineaInvestigacion>();

builder.Services.AddControllers();

// ============================================================
// EL 422 DEL CONTRATO (6_contracts.md §3.2)
//
// Con [ApiController], un cuerpo inválido corta la petición ANTES de entrar al
// método y responde 400 con ProblemDetails. El contrato exige 422 con el sobre
// {estado, mensaje, errores[]}: hay que reemplazar la fábrica de respuestas.
// ============================================================
builder.Services.Configure<ApiBehaviorOptions>(opciones =>
{
    opciones.InvalidModelStateResponseFactory = contexto =>
    {
        var errores = contexto.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(x => x.ErrorMessage))
            .ToList();

        return new ObjectResult(new
        {
            estado = 422,
            mensaje = "Datos inválidos.",
            errores
        })
        { StatusCode = 422 };
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Diagnóstico: dice quién es y qué versión, sin tocar la base de datos.
app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API Gestión Profesoral — módulo de programas y docentes",
    version = "v2",
    contratos = "/swagger"
}));

app.MapControllers();

app.Run();
