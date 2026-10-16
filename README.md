# Gestión Profesoral — Versión 2

Proyecto de aula de Diseño de Software, módulo **Gestión Profesoral**.
Dos proyectos separados, orquestados con Docker.

```
GestionProfesoral-Backend/     API REST en C# / ASP.NET Core 10 + SQL Server
  docs/                        el spec kit, los diagramas y la trazabilidad
  src/                         cuatro proyectos, uno por capa
  tests/                       la prueba de capas, sin base de datos
  db/                          el script: tablas, procedimientos y disparador
  postman/                     las colecciones, con las peticiones que deben fallar

GestionProfesoral-Frontend/    la interfaz en Blazor Server · nunca toca la base

docker-compose.yml             los tres contenedores
```

## Arrancar

```bash
docker compose up -d --build
```

| Qué | Dónde |
|---|---|
| **La interfaz** — empiece por aquí | **http://localhost:8075** |
| API — diagnóstico | http://localhost:8074/ |
| Swagger | http://localhost:8074/swagger |

> Para recargar la base con los procedimientos:
> `docker compose down -v && docker compose up -d --build` — sin el `-v` el
> script no se vuelve a ejecutar.

## Comprobar

```bash
bash GestionProfesoral-Backend/postman/humo_docente_v2.sh
docker compose exec api-gestion dotnet run --project tests/GestionProfesoral.Tests/GestionProfesoral.Tests.csproj
```

La segunda corre **con la base apagada**: es la demostración de que las capas
existen de verdad.

## Por dónde empezar a leer

| Pregunta | Documento |
|---|---|
| ¿Cómo está organizado el código, capa por capa? | [`docs/dominio/CRUD_CAPA_POR_CAPA.md`](GestionProfesoral-Backend/docs/dominio/CRUD_CAPA_POR_CAPA.md) |
| ¿Cómo fluye la información? | [`docs/dominio/DIAGRAMAS_SECUENCIA.md`](GestionProfesoral-Backend/docs/dominio/DIAGRAMAS_SECUENCIA.md) |
| ¿Qué promete la spec y dónde se cumple? | [`docs/TRAZABILIDAD.md`](GestionProfesoral-Backend/docs/TRAZABILIDAD.md) |
| ¿Qué construye la versión 2? | [`docs/spec_kit/versiones/v2_docente/2_spec.md`](GestionProfesoral-Backend/docs/spec_kit/versiones/v2_docente/2_spec.md) |
| ¿Qué endpoints hay? | [`docs/spec_kit/versiones/v2_docente/6_contracts.md`](GestionProfesoral-Backend/docs/spec_kit/versiones/v2_docente/6_contracts.md) |

## Cómo se trabaja

Nadie trabaja en `main`: el trabajo va en `rama-simon` y entra por **Pull
Request**. Commits pequeños y descriptivos en castellano. Cada versión cierra
con su **tag**. Los secretos van por variable de entorno — copie
`.env.example` a `.env`, que no se sube.

> La base de la v1 se retomó del ejemplo del profesor
> ([ccastro2050/proyecto_gestion_profesoral1](https://github.com/ccastro2050/proyecto_gestion_profesoral1)),
> como permite la metodología. La v2 y la reorganización en capas son trabajo
> de este proyecto.
