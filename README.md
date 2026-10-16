# Gestión Profesoral — versión 2

Módulo **Gestión Profesoral** del proyecto de aula. Este README es **el mapa
de la sustentación**: dónde está cada cosa que hay que mostrar.

> La base de la v1 se retomó del ejemplo del profesor
> ([ccastro2050/proyecto_gestion_profesoral1](https://github.com/ccastro2050/proyecto_gestion_profesoral1)),
> como permite la metodología. La **v2** —spec kit, procedimientos
> almacenados, disparador, el 409 de integridad y la rebanada de `docente`—
> es trabajo de este equipo.

---

## 1. Los lenguajes

| Pieza | Lenguaje | Dónde |
|---|---|---|
| **API** | **C# · ASP.NET Core · .NET 10**, con **Dapper** | `api_gestion/` |
| **Interfaz** | **Razor / Blazor Server** (C# + HTML) | `front_blazor/` |
| **Base de datos** | **T-SQL** sobre **SQL Server 2022**: tablas, **procedimientos almacenados** y **disparador** | `db/` |
| **Spec kit y diagramas** | **Markdown + Mermaid** (el diagrama es texto, se versiona y lo lee la IA) | `docs/spec_kit/` |
| **Pruebas** | C# (prueba de capas) y Bash + curl (humo) | `api_gestion/pruebas/`, `pruebas_humo/` |
| **Orquestación** | YAML (Docker Compose) y Dockerfile | `docker-compose.yml` |

---

## 2. La programación por capas: dónde se ve en el código

```
Navegador → front_blazor (8075) → api_gestion (8074) → SQL Server (1433)
```

| Capa | Archivo exacto | Qué hace | Qué tiene PROHIBIDO |
|---|---|---|---|
| **1 · HTTP** | [`api_gestion/Controllers/DocenteController.cs`](api_gestion/Controllers/DocenteController.cs) | Traduce cuerpo ↔ entidad y excepciones ↔ códigos (200/204/400/404/409/422) | Reglas de negocio · SQL · conocer `SqlException` |
| **2 · Negocio** | [`api_gestion/Servicios/ServicioDocente.cs`](api_gestion/Servicios/ServicioDocente.cs) | Las reglas: `limite ≤ 0`, PATCH vacío, y **no se retira un docente con detalle activo** | Conocer HTTP, `IActionResult` o las clases de `Peticiones/` |
| **3 · Datos** | [`api_gestion/Repositorios/RepositorioDocenteSqlServer.cs`](api_gestion/Repositorios/RepositorioDocenteSqlServer.cs) | **Llamar** a los procedimientos (`CommandType.StoredProcedure`) y traducir el error del motor | **Escribir SQL a mano** · decidir |
| **Base** | [`db/gestion_profesoral.sql`](db/gestion_profesoral.sql) | El SQL: `SELECT`, `INSERT`, `JOIN` y el **disparador** | — |

**Las interfaces son las que hacen que las capas existan de verdad:**

| Contrato | Archivo |
|---|---|
| `IServicioDocente` | [`api_gestion/Servicios/IServicioDocente.cs`](api_gestion/Servicios/IServicioDocente.cs) |
| `IRepositorioDocente` | [`api_gestion/Repositorios/IRepositorioDocente.cs`](api_gestion/Repositorios/IRepositorioDocente.cs) |
| **El ensamblador** — el único sitio donde una clase concreta aparece junto a su interfaz | [`api_gestion/Program.cs`](api_gestion/Program.cs) |
| La traducción `SqlException` → **409** | [`api_gestion/Repositorios/TraductorErroresSql.cs`](api_gestion/Repositorios/TraductorErroresSql.cs) |

> **La demostración, no la afirmación:** `api_gestion/pruebas/Docente.cs`
> prueba el servicio con un repositorio **de mentiras** y **corre con los
> contenedores apagados**. Si el servicio tuviera dentro una cadena de
> conexión, no compilaría.
>
> ```bash
> docker compose exec api-gestion dotnet run --project pruebas/PruebaCapas.csproj
> ```

---

## 3. Los diagramas de secuencia: el flujo de la información

**Todos en Mermaid, dentro de los `.md`** (ninguna imagen pegada), y las
cajas tienen **los nombres reales de las clases**.

| Qué muestra | Archivo |
|---|---|
| **Una secuencia por historia de usuario** (6): listar, crear con desplegable, maestro-detalle, lote transaccional, tabla puente y la regla de retiro | [`docs/spec_kit/0_historias_de_usuario.md`](docs/spec_kit/0_historias_de_usuario.md) |
| **El camino feliz completo**, de la pantalla a la base y de vuelta, con el disparador | [`docs/spec_kit/versiones/v2_docente/3_plan.md`](docs/spec_kit/versiones/v2_docente/3_plan.md) §3 |
| **Las secuencias de ERROR**: el 404, el 422, el **409 de integridad** y el **409 de regla de negocio** | [`docs/spec_kit/versiones/v2_docente/6_contracts.md`](docs/spec_kit/versiones/v2_docente/6_contracts.md) §3 |
| Contexto, despliegue, clases, entidad-relación, ciclo de validación y fases | `2_spec.md`, `3_plan.md`, `5_data_model.md`, `7_quickstart.md`, `8_tasks.md` |

---

## 4. El spec kit: requisitos y endpoints

| Documento | Qué contesta |
|---|---|
| [**`2_spec.md`**](docs/spec_kit/versiones/v2_docente/2_spec.md) | **El documento número dos**: propósito, alcance, **los 10 requisitos funcionales (RF1–RF10)**, las decisiones D1–D6, las aclaraciones y **los 12 criterios de aceptación** |
| [**`6_contracts.md`**](docs/spec_kit/versiones/v2_docente/6_contracts.md) | **Los endpoints exactos**, el catálogo de códigos y la tabla de desenlaces por endpoint |
| [`0_historias_de_usuario.md`](docs/spec_kit/0_historias_de_usuario.md) | Las historias con su criterio de aceptación y su secuencia |
| [`3_plan.md`](docs/spec_kit/versiones/v2_docente/3_plan.md) | El plan técnico y el chequeo de constitución |
| [`4_research.md`](docs/spec_kit/versiones/v2_docente/4_research.md) | Las 8 decisiones de arquitectura, con su alternativa descartada |
| [`5_data_model.md`](docs/spec_kit/versiones/v2_docente/5_data_model.md) | Las 11 tablas y **quién escribe cada columna** |
| [`7_quickstart.md`](docs/spec_kit/versiones/v2_docente/7_quickstart.md) | Cómo se comprueba cada criterio |
| [`8_tasks.md`](docs/spec_kit/versiones/v2_docente/8_tasks.md) | Las fases con compuertas y el reparto por integrante |
| [`9_checklist.md`](docs/spec_kit/versiones/v2_docente/9_checklist.md) | La compuerta 3, que firma una persona |
| [`GUIA_IA2.md`](docs/spec_kit/versiones/v2_docente/GUIA_IA2.md) | Los prompts, con lo que hubo que corregirle a la IA |

### Los endpoints de la fase 1 (funcionando)

```
GET    /api/docente[?limite=N]      GET    /api/docente/{cedula}
POST   /api/docente                 PUT    /api/docente/{cedula}
PATCH  /api/docente/{cedula}        DELETE /api/docente/{cedula}
GET    /api/linea-investigacion     ← llena el desplegable de la clave foránea
```

Y los de la v1 (`/api/programa`) siguen pasando. Todo en
**http://localhost:8074/swagger**.

---

## 5. Arrancar y comprobar

```bash
docker compose up -d --build
```

| Qué | Dónde |
|---|---|
| **La pantalla** | **http://localhost:8075** |
| Swagger | http://localhost:8074/swagger |

```bash
bash pruebas_humo/humo_docente_v2.sh    # 15 comprobaciones, con las que deben fallar
```

> Para recargar la base con los procedimientos: `docker compose down -v && docker compose up -d --build`
> (sin el `-v` el script no se vuelve a ejecutar).

---

## 6. Ramas y commits

- **Nadie trabaja en `main`**: cada integrante en su `rama-<nombre>`.
- Todo entra por **Pull Request**, y el merge lo hace **solo el encargado**.
- Commits pequeños y descriptivos en castellano; cada versión cierra con su **tag**.
- Secretos por variable de entorno: copie `.env.example` a `.env` (que no se sube).

Las reglas, con ejemplos de mensajes y la plantilla de PR, en
[`docs/RAMAS_Y_COMMITS.md`](docs/RAMAS_Y_COMMITS.md).
El guión de lo que se proyecta y se responde, en
[`docs/GUIA_DE_SUSTENTACION.md`](docs/GUIA_DE_SUSTENTACION.md).
