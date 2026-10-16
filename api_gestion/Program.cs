using ApiGestion.Repositorios;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// La API escucha en el 8074 también DENTRO del contenedor, para que el Dockerfile,
// el docker-compose y los contratos digan todos el mismo número (3_plan.md §5.2).
builder.WebHost.UseUrls("http://0.0.0.0:8074");

// ============================================================
// EL ENSAMBLADOR (Artículo 3)
//
// ESTE ES EL ÚNICO LUGAR DE TODO EL PROYECTO donde una clase concreta aparece
// junto a su interfaz. Todo lo demás recibe interfaces por constructor, y por
// eso cambiar de motor de base de datos se hace cambiando UNA línea de aquí.
//
// Y hay una pareja de líneas POR RECURSO, no un registro genérico: el
// compilador revisa que cada servicio tenga su repositorio, cosa que un
// diccionario de tablas no podría revisar (Artículo 10.1).
// ============================================================

// --- v1 ---
builder.Services.AddScoped<IRepositorioPrograma, RepositorioProgramaSqlServer>();
builder.Services.AddScoped<IServicioPrograma, ServicioPrograma>();

// --- v2, fase 1 ---
builder.Services.AddScoped<IRepositorioDocente, RepositorioDocenteSqlServer>();
builder.Services.AddScoped<IServicioDocente, ServicioDocente>();

// Catálogo de solo lectura: es lo que llena el desplegable de la clave foránea
// opcional del docente (criterio 5).
builder.Services.AddScoped<IRepositorioLineaInvestigacion, RepositorioLineaInvestigacionSqlServer>();
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

// RF7 — Diagnóstico: dice quién es y qué versión, sin tocar la base de datos.
app.MapGet("/", () => Results.Ok(new
{
    mensaje = "API Gestión Profesoral — módulo de programas y docentes",
    version = "v2",
    contratos = "/swagger"
}));

app.MapControllers();

app.Run();
