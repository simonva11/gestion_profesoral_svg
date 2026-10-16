# Backend — Gestión Profesoral

API REST en **C# / ASP.NET Core 10** sobre **SQL Server 2022**, con **Dapper**
llamando procedimientos almacenados. Cuatro proyectos, uno por capa.

```
src/
  GestionProfesoral.Domain/           entidades · excepciones · contratos de repositorio
  GestionProfesoral.Application/      los servicios: las reglas de negocio
  GestionProfesoral.Infrastructure/   los repositorios contra SQL Server
  GestionProfesoral.Api/              controladores · peticiones · Program.cs
tests/
  GestionProfesoral.Tests/            la prueba de capas, sin base de datos
db/                                   tablas, procedimientos y el disparador
docs/                                 spec kit, diagramas y trazabilidad
postman/                              colecciones y prueba de humo
```

## La regla de dependencias

```
Api ──→ Application ──→ Domain ←── Infrastructure
 └──────────────────────────────────┘  solo en Program.cs, para registrar
```

**Domain no referencia a nadie.** `Application` no referencia a
`Infrastructure`, así que el servicio **no puede** nombrar a SQL Server: no
compilaría. Las capas no son un acuerdo, son referencias de proyecto.

| Capa | Hace | Tiene prohibido |
|---|---|---|
| **Api** | Traduce cuerpo ↔ entidad y excepciones ↔ códigos HTTP | Reglas de negocio · SQL · conocer `SqlException` |
| **Application** | Las reglas (incluida la D2: no retirar un maestro con detalle activo) | Conocer HTTP o las clases de `Peticiones/` |
| **Infrastructure** | **Llamar** a los procedimientos y traducir los errores del motor | **Escribir SQL a mano** · decidir |
| **Domain** | Decir qué es un docente y qué contrato necesita | Depender de cualquier cosa |

El recorrido completo, con código: [`docs/dominio/CRUD_CAPA_POR_CAPA.md`](docs/dominio/CRUD_CAPA_POR_CAPA.md).

## Documentación

| Documento | Qué contesta |
|---|---|
| [`docs/TRAZABILIDAD.md`](docs/TRAZABILIDAD.md) | **Cada requisito y cada endpoint: dónde se cumple y cómo se demuestra** |
| [`docs/dominio/CRUD_CAPA_POR_CAPA.md`](docs/dominio/CRUD_CAPA_POR_CAPA.md) | Cómo se programa un CRUD a través de los cuatro proyectos |
| [`docs/dominio/DIAGRAMAS_SECUENCIA.md`](docs/dominio/DIAGRAMAS_SECUENCIA.md) | El flujo de la información, incluidos los dos 409 |
| [`docs/spec_kit/`](docs/spec_kit/) | La constitución, las historias con su secuencia y una carpeta por versión |
| [`docs/spec_kit/PROMPT_GENERAR_SPEC_KIT.md`](docs/spec_kit/PROMPT_GENERAR_SPEC_KIT.md) | El prompt que genera el spec kit |
| [`docs/spec_kit/versiones/v2_docente/2_spec.md`](docs/spec_kit/versiones/v2_docente/2_spec.md) | Los RF1–RF10 y los 12 criterios |
| [`docs/spec_kit/versiones/v2_docente/6_contracts.md`](docs/spec_kit/versiones/v2_docente/6_contracts.md) | Los endpoints y las secuencias de error |

## Base de datos

El script `db/gestion_profesoral.sql` trae las 19 tablas y, de la línea 528 en
adelante, **los objetos de la v2**: 15 procedimientos y el disparador
`trg_contar_estudios` (`:867`). Los de la v2 están también sueltos en
`db/v2/` para leerlos sin buscar.

> El SQL vive en la base, con nombre y versionado. En los repositorios solo
> hay **llamadas**: `commandType: CommandType.StoredProcedure`.

## Comprobar

```bash
# desde la raíz del repositorio
bash GestionProfesoral-Backend/postman/humo_docente_v2.sh
docker compose exec api-gestion dotnet run --project tests/GestionProfesoral.Tests/GestionProfesoral.Tests.csproj
grep -rE "SELECT |INSERT |UPDATE " src/GestionProfesoral.Infrastructure/   # debe salir vacío
```
