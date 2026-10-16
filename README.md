# Gestión Profesoral — Versión 2

Módulo **Gestión Profesoral**. Este repositorio contiene **lo que se entrega
de la v2** (VERSION_2.md §11) y nada más.

---

## Lo que se entrega, y dónde está

| Lo que pide la v2 | Dónde está aquí | Estado |
|---|---|---|
| **El spec kit de la v2** — los nueve documentos y la guía de IA | [`docs/spec_kit/versiones/v2_docente/`](docs/spec_kit/versiones/v2_docente/) | ✅ completo, escrito **antes** del código |
| **El script de la base** con los procedimientos y el disparador | [`db/gestion_profesoral.sql`](db/gestion_profesoral.sql) · los de la v2 sueltos en [`db/v2/`](db/v2/) | ⚠️ 15 procedimientos + `trg_contar_estudios`; faltan los de 12 tablas |
| **El código** — repositorios que **llaman** a los procedimientos | [`api_gestion/`](api_gestion/) | ⚠️ `docente` completo; `programa` (v1) todavía con SQL a mano |
| **La interfaz gráfica** | [`front_blazor/`](front_blazor/) | ⚠️ pantalla de `docente`; falta la ficha maestro-detalle |
| **La colección de pruebas**, con las que **tienen que fallar** | [`postman/coleccion_v2.postman_collection.json`](postman/) · y [`pruebas_humo/humo_docente_v2.sh`](pruebas_humo/) | ✅ 15 peticiones, 6 de ellas deben fallar |
| **Los prompts**, con lo que hubo que corregirle a la IA | [`docs/spec_kit/versiones/v2_docente/GUIA_IA2.md`](docs/spec_kit/versiones/v2_docente/GUIA_IA2.md) | ✅ los cuatro prompts reales y sus correcciones |
| **El tag `v2`** sobre el commit que pasa los doce criterios | — | ⬜ pendiente: faltan criterios |

---

## El spec kit

| Documento | Qué contesta |
|---|---|
| [`0_historias_de_usuario.md`](docs/spec_kit/0_historias_de_usuario.md) | Las historias H5–H10, **cada una con su diagrama de secuencia** |
| [`1_constitution.md`](docs/spec_kit/1_constitution.md) | Los principios, los mismos para las cuatro versiones |
| [`0_mapa_versiones.md`](docs/spec_kit/versiones/0_mapa_versiones.md) | Qué tabla entra en qué versión |
| [**`2_spec.md`**](docs/spec_kit/versiones/v2_docente/2_spec.md) | **Los requisitos RF1–RF10**, el alcance, las decisiones D1–D6 y los **12 criterios de aceptación** |
| [`3_plan.md`](docs/spec_kit/versiones/v2_docente/3_plan.md) | El plan · diagramas de **despliegue**, **clases** y **camino feliz** |
| [`4_research.md`](docs/spec_kit/versiones/v2_docente/4_research.md) | Las 8 decisiones de arquitectura, con su alternativa descartada |
| [`5_data_model.md`](docs/spec_kit/versiones/v2_docente/5_data_model.md) | Las 11 tablas y **quién escribe cada columna** |
| [**`6_contracts.md`**](docs/spec_kit/versiones/v2_docente/6_contracts.md) | **Los endpoints** y las **secuencias de error**: 404, 422 y los dos 409 |
| [`7_quickstart.md`](docs/spec_kit/versiones/v2_docente/7_quickstart.md) | Cómo se comprueba cada criterio |
| [`8_tasks.md`](docs/spec_kit/versiones/v2_docente/8_tasks.md) | Las fases con sus compuertas y el orden en que se hacen |
| [`9_checklist.md`](docs/spec_kit/versiones/v2_docente/9_checklist.md) | La compuerta 3, que firma una persona |
| [`GUIA_IA2.md`](docs/spec_kit/versiones/v2_docente/GUIA_IA2.md) | Los prompts y sus correcciones |

---

## La arquitectura: tres capas, y se ven en la ruta del archivo

```
Navegador → front_blazor (8075) → api_gestion (8074) → SQL Server (1433)
```

| Capa | Archivo | Qué tiene PROHIBIDO |
|---|---|---|
| **1 · HTTP** | [`api_gestion/Controllers/DocenteController.cs`](api_gestion/Controllers/DocenteController.cs) | Reglas de negocio · SQL · conocer `SqlException` |
| **2 · Negocio** | [`api_gestion/Servicios/ServicioDocente.cs`](api_gestion/Servicios/ServicioDocente.cs) | Conocer HTTP o las clases de `Peticiones/` |
| **3 · Datos** | [`api_gestion/Repositorios/RepositorioDocenteSqlServer.cs`](api_gestion/Repositorios/RepositorioDocenteSqlServer.cs) | **Escribir SQL a mano** |
| **Base** | [`db/gestion_profesoral.sql`](db/gestion_profesoral.sql) | — |

Cada capa depende de la **interfaz** de la siguiente (`IServicioDocente`,
`IRepositorioDocente`). El único sitio donde una clase concreta aparece junto
a su interfaz es [`api_gestion/Program.cs`](api_gestion/Program.cs).
El 409 nace en [`TraductorErroresSql.cs`](api_gestion/Repositorios/TraductorErroresSql.cs).

**La demostración de que las capas existen** — corre con la base apagada:

```bash
docker compose exec api-gestion dotnet run --project pruebas/PruebaCapas.csproj
```

**Lenguajes:** C# / ASP.NET Core / .NET 10 (API) · Razor-Blazor Server
(interfaz) · T-SQL sobre SQL Server 2022 (procedimientos y disparador) ·
Markdown + Mermaid (spec kit).

---

## Arrancar y comprobar

```bash
docker compose up -d --build
```

| Qué | Dónde |
|---|---|
| **La pantalla** | **http://localhost:8075** |
| Swagger | http://localhost:8074/swagger |

```bash
bash pruebas_humo/humo_docente_v2.sh
```

> Para recargar la base con los procedimientos:
> `docker compose down -v && docker compose up -d --build` — sin el `-v` el
> script no se vuelve a ejecutar.

---

## Lo que falta para cerrar la v2

Los doce criterios están en el [`2_spec.md`](docs/spec_kit/versiones/v2_docente/2_spec.md) §8.
Hoy faltan:

1. **Migrar los repositorios de la v1 a procedimientos** — `programa` todavía
   tiene SQL escrito a mano, y eso deja el **criterio 3 en rojo**.
2. Los **procedimientos y las rebanadas** de las 10 tablas restantes de la v2.
3. La **ficha maestro-detalle** y el alta por lote (criterio 9).
4. Firmar el **`9_checklist.md`** y poner el **tag `v2`**.

El orden y el reparto están en [`8_tasks.md`](docs/spec_kit/versiones/v2_docente/8_tasks.md).

---

## Cómo se trabaja

**Este proyecto lo desarrolla una sola persona.** Aun así se respeta la regla:
nadie trabaja en `main` — el trabajo va en `rama-simon` y entra por **Pull
Request**, aunque el autor y quien integra sean el mismo. Commits pequeños y
descriptivos en castellano. Los secretos van por variable de entorno: copie
`.env.example` a `.env`, que no se sube.

> La base de la v1 se retomó del ejemplo del profesor
> ([ccastro2050/proyecto_gestion_profesoral1](https://github.com/ccastro2050/proyecto_gestion_profesoral1)),
> como permite la metodología.
