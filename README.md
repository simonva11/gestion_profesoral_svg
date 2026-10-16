# Gestión Profesoral — proyecto de aula

Módulo **Gestión Profesoral** del proyecto de aula de Diseño de Software.
API en **C# / ASP.NET Core** sobre **SQL Server**, interfaz en **Blazor
Server**, todo en Docker.

> **De dónde parte este repositorio.** La base de la **v1** es el ejemplo de
> referencia del profesor
> ([ccastro2050/proyecto_gestion_profesoral1](https://github.com/ccastro2050/proyecto_gestion_profesoral1)),
> que la metodología permite retomar y completar: *«el equipo que tome este
> ejemplo lo revisa, y si está de acuerdo lo retoma y lo completa; si no, lo
> rehace a su manera»* (0_mapa_versiones.md). Su README original quedó en
> [`docs/README_ejemplo_profesor.md`](docs/README_ejemplo_profesor.md).
> Lo de la **v2** —spec kit, procedimientos almacenados, disparador y la
> rebanada de `docente`— es trabajo de este equipo.

---

## 1. Arranque: un solo comando

Solo hace falta **Docker Desktop**.

```bash
docker compose up -d --build
```

| Qué | Dónde |
|---|---|
| **La pantalla** — empiece por aquí | **http://localhost:8075** |
| API — diagnóstico | http://localhost:8074/ |
| Documentación interactiva (Swagger) | http://localhost:8074/swagger |
| Docentes (v2) | http://localhost:8074/api/docente |
| Programas (v1) | http://localhost:8074/api/programa |

> Para recargar la base con los **procedimientos y el disparador** de la v2
> hay que borrar el volumen, o el script no se vuelve a ejecutar:
>
> ```bash
> docker compose down -v && docker compose up -d --build
> ```

**La prueba de capas, sin base de datos** (apague los contenedores y
córrala: ahí está la demostración de que las capas existen):

```bash
cd api_gestion/pruebas && dotnet run
```

---

## 2. Dónde está cada cosa

| Carpeta | Qué hay |
|---|---|
| `api_gestion/` | **La API**. Tres capas: `Controllers/` → `Servicios/` → `Repositorios/` |
| `front_blazor/` | **La interfaz**. Consume la API por HTTP; **nunca** toca la base |
| `db/` | El script de la base: 19 tablas, **procedimientos almacenados** y el **disparador** |
| `db/v2/` | Los objetos de la v2 por separado, para leerlos sin buscar |
| `docs/spec_kit/` | **El spec kit**: la constitución, las historias de usuario y una carpeta por versión |
| `docs/*.md` | El material conceptual del curso (SOLID, capas, ACID, Docker, pruebas, SDD) |
| `ProyectosDeAula/` | El material del curso: metodología, documentos de módulo y scripts |
| `postman/` | Las colecciones de prueba, **incluidas las peticiones que deben fallar** |

---

## 3. La arquitectura, en una línea cada una

```
Navegador → front_blazor (8075) → api_gestion (8074) → SQL Server (1433)
```

| Capa | Carpeta | Hace | Tiene prohibido |
|---|---|---|---|
| 1 · HTTP | `api_gestion/Controllers/` | Traduce cuerpo ↔ entidad y excepciones ↔ códigos | Reglas de negocio · SQL |
| 2 · Negocio | `api_gestion/Servicios/` | Las reglas (incluido el 409 de la regla D2) | Conocer HTTP |
| 3 · Datos | `api_gestion/Repositorios/` | **Llamar** a los procedimientos almacenados | **Escribir SQL a mano** |
| Base | `db/gestion_profesoral.sql` | El SQL y el disparador | — |

Cada capa depende de la **interfaz** de la siguiente. El único sitio donde
una clase concreta aparece junto a su interfaz es `api_gestion/Program.cs`.

Está contada entera, con diagramas, en
[`docs/ARQUITECTURA_V2.md`](docs/ARQUITECTURA_V2.md).

---

## 4. Las versiones

| Versión | Qué agrega | Estado |
|---|---|---|
| v1 | CRUD de las tablas **sin** clave foránea | base retomada del ejemplo |
| **v2** | CRUD de las **11 tablas con clave foránea**, todo por **procedimientos almacenados**, **un disparador**, desplegables para las FK y **409** de integridad | **en construcción** |
| v3 | JWT, sesiones y control de acceso | sin especificar |
| v4 | Consultas multitabla, tablero, marca y publicación | sin especificar |

El detalle está en
[`docs/spec_kit/versiones/0_mapa_versiones.md`](docs/spec_kit/versiones/0_mapa_versiones.md).
**Las versiones no son repositorios**: para ver una anterior se abre su tag.

### El spec kit de la v2

| Documento | Qué contesta |
|---|---|
| [`0_historias_de_usuario.md`](docs/spec_kit/0_historias_de_usuario.md) | Las historias H5–H10, **cada una con su diagrama de secuencia** |
| [`2_spec.md`](docs/spec_kit/versiones/v2_docente/2_spec.md) | Qué construye la v2, el alcance y los 12 criterios |
| [`3_plan.md`](docs/spec_kit/versiones/v2_docente/3_plan.md) | Cómo se construye · despliegue, clases y camino feliz |
| [`4_research.md`](docs/spec_kit/versiones/v2_docente/4_research.md) | Las 8 decisiones, con lo que se gana y lo que se pierde |
| [`5_data_model.md`](docs/spec_kit/versiones/v2_docente/5_data_model.md) | Las tablas y **quién escribe cada columna** |
| [`6_contracts.md`](docs/spec_kit/versiones/v2_docente/6_contracts.md) | Los endpoints exactos y **las secuencias de error** |
| [`7_quickstart.md`](docs/spec_kit/versiones/v2_docente/7_quickstart.md) | Cómo se levanta y cómo se comprueba cada criterio |
| [`8_tasks.md`](docs/spec_kit/versiones/v2_docente/8_tasks.md) | Las fases con sus compuertas y el reparto por integrante |
| [`9_checklist.md`](docs/spec_kit/versiones/v2_docente/9_checklist.md) | La compuerta 3, que se firma a mano |
| [`GUIA_IA2.md`](docs/spec_kit/versiones/v2_docente/GUIA_IA2.md) | Los prompts, **con lo que hubo que corregirle a la IA** |

---

## 5. Cómo se trabaja aquí

- **Nadie trabaja en `main`.** Cada integrante tiene su rama `rama-<nombre>`.
- Todo llega a `main` por **Pull Request**, y el merge lo hace **solo el
  encargado del main**.
- Commits pequeños, frecuentes y descriptivos **en castellano**.
- Cada versión cierra con su **tag** (`v1`, `v2`, …).
- **Cero secretos en el código**: van por variable de entorno. Copie
  `.env.example` a `.env` — el `.env` no se sube.

Las reglas completas, con ejemplos de mensajes de commit y la plantilla de
PR, están en [`docs/RAMAS_Y_COMMITS.md`](docs/RAMAS_Y_COMMITS.md).
