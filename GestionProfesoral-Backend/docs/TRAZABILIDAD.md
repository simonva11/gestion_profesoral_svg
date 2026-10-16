# Trazabilidad — de la spec al código, y del contrato a la prueba

> **La regla de esta entrega:** si el `2_spec.md` dice que hay un requisito,
> aquí está **dónde se cumple y cómo se demuestra**. Si el `6_contracts.md`
> declara un endpoint, aquí está **la petición que lo prueba**.
>
> Y al revés: lo que no esté cumplido se dice. Una spec que promete lo que el
> código no hace es peor que una spec corta — el profesor abre el archivo y lo
> ve en treinta segundos.
>
> **Estado de hoy:** la rebanada de `docente` está completa; las demás tablas
> están especificadas y no construidas. Eso se marca ⚠️ y no ✅.

---

## 1. Los requisitos funcionales del `2_spec.md`

| RF | Lo que promete la spec | Dónde se cumple en el código | Cómo se demuestra | Estado |
|---|---|---|---|---|
| **RF1** | CRUD de las tablas con FK, **todo por procedimiento almacenado** | `Infrastructure/Persistencia/RepositorioDocenteSqlServer.cs` — 7 × `CommandType.StoredProcedure` · `db/gestion_profesoral.sql:583-736` | `grep -rE "SELECT \|INSERT \|UPDATE " src/GestionProfesoral.Infrastructure/` → **sin resultados** | ⚠️ hecho en `docente`; faltan 10 tablas y la migración de la v1 |
| **RF2** | El crear devuelve **la fila como quedó guardada** | `sp_crear_docente` termina con `EXEC sp_consultar_docente` (`db:632`) | `POST /api/docente` → la respuesta trae `totalEstudios: 0`, que la API **no envió** | ✅ |
| **RF3** | **Borrado lógico** y listados que filtran | `sp_eliminar_docente` → `UPDATE activo = 0` (`db:712`); el `WHERE activo = 1` vive en cada `sp_listar_*` | Se retira un docente, desaparece del listado y la fila **sigue en la base**: `SELECT cedula, activo FROM docente` | ✅ |
| **RF4** | La FK se elige de un **desplegable cargado de la API**, con «(ninguna)» → `null` | `GestionProfesoral-Frontend/Components/Pages/Docentes.razor:99` (la opción vacía) y `:300` (la conversión a `null`) · `Api/Controllers/LineaInvestigacionController.cs` | Se abre el formulario: el `<select>` muestra «Ingeniería de Software» y la petición lleva `1`. Guardando con «(ninguna)» la respuesta trae `null` | ✅ |
| **RF5** | **Maestro-detalle en una sola pantalla** | — | — | ❌ **no construido** (fase 2) |
| **RF6** | Alta múltiple en **un solo envío**, transaccional | `Api/Peticiones/EstudioCrear.cs` declara `EstudiosLote`; falta el repositorio | — | ❌ **no construido** (fase 2) |
| **RF7** | Integridad referencial: **409, nunca 500** | `Infrastructure/Persistencia/TraductorErroresSql.cs:30-31` (547 y 2627) → `Api/Controllers/DocenteController.cs:135` | Postman → «Deben fallar»: cédula repetida y línea inexistente → **409** en castellano | ✅ |
| **RF8** | **Al menos un disparador** funcionando | `db/gestion_profesoral.sql:867` — `trg_contar_estudios`, escucha `INSERT, UPDATE, DELETE` | Se agrega un estudio y `docente.total_estudios` sube; se **retira** y baja. La API nunca mandó ese número | ✅ comprobado por `sqlcmd`; falta hacerlo **por la interfaz** (fase 2) |
| **RF9** | Las puente **se asignan y se retiran**, no se editan | `db:895-940` (`sp_*_red_docente`); sin controlador todavía | — | ⚠️ procedimientos sí, API y pantalla no |
| **RF10** | **Regresión de la v1** | `postman/coleccion_v1.postman_collection.json` | Se corre la colección entera | ⚠️ pasa, pero la v1 todavía tiene SQL a mano (criterio 3) |

### Requisitos no funcionales

| Qué promete | Dónde | Cómo se demuestra | Estado |
|---|---|---|---|
| Secretos por variable de entorno | `docker-compose.yml` (`ConnectionStrings__SqlServer`) · `Api/Program.cs:21` lo lee en un solo sitio · `.env.example` | `git ls-files \| grep "^\.env$"` → vacío | ✅ |
| Todo el camino `async` | Las cuatro capas devuelven `Task<…>` | — | ✅ |
| La prueba corre **sin base de datos** | `tests/GestionProfesoral.Tests/` — **no referencia a Infrastructure** | `docker compose exec api-gestion dotnet run --project tests/…` con la base apagada | ✅ |
| La interfaz no habla en jerga | `Docentes.razor` | Se recorren las pantallas: no dice «PUT», «422» ni «FK» | ✅ |

---

## 2. Los endpoints del `6_contracts.md`

| Endpoint | Implementado en | Petición que lo prueba | Estado |
|---|---|---|---|
| `GET /` | `Api/Program.cs` (diagnóstico) | Humo #1 | ✅ |
| `GET /api/docente[?limite=N]` | `DocenteController.ObtenerTodos` | Humo #4 · Postman «Listar» | ✅ |
| `GET /api/docente/{cedula}` | `.ObtenerPorId` | Humo #5 · 404 en Humo #9 | ✅ |
| `POST /api/docente` | `.Crear` | Humo #3 (200), #7 (409), #8 (422) | ✅ |
| `PUT /api/docente/{cedula}` | `.Reemplazar` | Pantalla: botón «Guardar» en modo edición | ✅ |
| `PATCH /api/docente/{cedula}` | `.ActualizarParcial` | Humo #6 (200) y #11 (400 sin campos) | ✅ |
| `DELETE /api/docente/{cedula}` | `.Eliminar` | Humo #13-15 | ✅ |
| `GET /api/linea-investigacion` | `LineaInvestigacionController` | Humo #2 — y es lo que llena el desplegable | ✅ |
| `GET\|POST /api/docente/{c}/estudios` | — | — | ❌ fase 2 |
| `POST /api/docente/{c}/estudios/lote` | — | — | ❌ fase 2 |
| Rutas de tablas puente (`/api/red/{i}/docentes`…) | — | — | ❌ fase 4 |
| Rutas 1 a 1 (`/estudios-realizados/{id}/apoyo`…) | — | — | ❌ fase 5 |

> **Los siete endpoints de la v1** (`/api/programa`) siguen respondiendo:
> colección `coleccion_v1`.

---

## 3. Los códigos de respuesta del contrato

| Código | Quién lo decide | Petición que lo demuestra |
|---|---|---|
| **200** | Controlador | `GET /api/docente` con filas |
| **204** | Controlador — vacío no es error | `GET /api/docente` sobre tabla vacía |
| **400** | Servicio (regla rota) | `?limite=0` · `PATCH {}` |
| **404** | Servicio (`NoEncontradoExcepcion`) | `GET /api/docente/999999` |
| **409** | Infrastructure (traductor) **o** Application (regla D2) | cédula repetida · línea inexistente · retirar con detalle activo |
| **422** | `[ApiController]`, antes del controlador | `POST` con cuerpo incompleto |
| **500** | Nadie lo atrapó | solo si la base está caída |

---

## 4. Las decisiones del `2_spec.md` §6, y dónde se ven

| Decisión | Dónde está escrita en el código |
|---|---|
| **D1** borrado lógico | todos los `sp_eliminar_*` hacen `UPDATE` |
| **D2** no se retira un maestro con detalle activo | `Application/Servicios/ServicioDocente.cs:106-122` |
| **D3** sin cascada lógica | no existe ningún `UPDATE` en cadena: es la ausencia, y es deliberada |
| **D4** no se reactiva | no hay endpoint que ponga `activo = 1` |
| **D6** disparador = propuesta A | `db:867` |

---

## 5. Cómo se verifica todo esto en cinco minutos

```bash
docker compose up -d --build                      # 1. levantar
bash GestionProfesoral-Backend/postman/humo_docente_v2.sh   # 2. 15 comprobaciones
docker compose exec api-gestion dotnet run \
  --project tests/GestionProfesoral.Tests/GestionProfesoral.Tests.csproj   # 3. las capas
grep -rE "SELECT |INSERT |UPDATE " GestionProfesoral-Backend/src/GestionProfesoral.Infrastructure/  # 4. criterio 3
```

Y a mano, por la interfaz (http://localhost:8075/docentes): crear con
«(ninguna)», ver el desplegable lleno, retirar y comprobar que desaparece de
la lista pero sigue en la base.

---

## 6. Lo que hoy NO se puede mostrar, y por qué

| Pendiente | Qué bloquea |
|---|---|
| `RepositorioProgramaSqlServer` todavía tiene SQL a mano | **criterio 3** |
| 10 tablas de la v2 sin rebanada | criterios 2, 8, 9 |
| Maestro-detalle y lote | criterios 9 y la demostración del disparador por la interfaz |
| `GUIA_IA2.md` con los prompts de las fases 2 a 6 | entregable de §11 |
| Tag `v2` | se pone cuando los doce criterios pasen |

**Decir esto es parte de la entrega.** Una matriz con todo en verde que no
resiste una prueba en vivo cuesta más que una matriz honesta.
