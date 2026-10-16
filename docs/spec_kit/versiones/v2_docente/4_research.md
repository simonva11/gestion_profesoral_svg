# Investigación y decisiones (ADR) — Versión 2

> El registro de las decisiones: qué opciones había, con qué criterio se
> eligió y qué se pierde con lo elegido. **Sin diagrama obligatorio**: aquí
> una tabla comparativa vale más que un dibujo.
>
> Estas son las preguntas de la sustentación de la forma «¿por qué así y no
> de la otra manera?» (interpretabilidad, §2.2 de la metodología).

---

## ADR-01 — Dapper llamando procedimientos, no Entity Framework

**Contexto.** El criterio 3 exige que los cinco verbos de todas las tablas
pasen por procedimientos almacenados.

| Opción | A favor | En contra |
|---|---|---|
| **Dapper + `CommandType.StoredProcedure`** ✅ | La llamada se ve en una línea; el proyecto ya usa Dapper desde la v1; no hay capa que esconda nada | Hay que mapear a mano los parámetros |
| Entity Framework Core | Migraciones, LINQ, menos código repetido | **Genera su propio SQL**: escondería justo lo que hay que demostrar. Y `FromSqlRaw` para llamar procedimientos es EF peleando contra sí mismo |
| `SqlCommand` crudo de ADO.NET | Cero dependencias | 15 líneas donde Dapper usa 3, y el mapeo a mano en todas |

**Decisión.** Dapper. **Se pierde:** las migraciones automáticas; el esquema
se versiona a mano en el script, que es lo que el curso pide igual.

---

## ADR-02 — Dónde se traduce el error de SQL Server

**Contexto.** `SqlException 547` (clave foránea) y `2627/2601` (clave
duplicada) tienen que salir como **409 en castellano**.

| Opción | Consecuencia |
|---|---|
| En el **controlador**, con `catch (SqlException)` | La capa 1 tendría que importar `Microsoft.Data.SqlClient`: la capa de datos se filtra hasta arriba, y el controlador deja de ser probable sin base |
| En el **servicio** | Mismo problema, una capa más abajo |
| En el **repositorio**, con `TraductorErroresSql` ✅ | La única capa que ya conoce el motor es la que traduce; hacia arriba solo sube `ConflictoExcepcion`, que es de dominio |

**Decisión.** En el repositorio. **Se pierde:** un archivo más. Se gana que
cambiar de motor solo obligue a reescribir el traductor.

> **La v1 tenía este bug y la v2 lo arregla:** `ProgramaController.Crear`
> atrapa `SqlException` y responde **500** para una clave duplicada. En la v2
> eso pasa a ser **409**.

---

## ADR-03 — Rutas anidadas para el detalle

| Opción | Lectura |
|---|---|
| `GET /api/estudios?docente=1017245` | El docente es un **filtro**: el estudio podría existir sin él |
| `GET /api/docente/1017245/estudios` ✅ | El docente es **parte de la identidad**: un estudio no existe sin su docente |

**Decisión.** Anidadas para el detalle y para las puente. **Se pierde:**
rutas más largas y un controlador con dos niveles de parámetros. Se gana que
el formulario **no pueda** mandar un docente distinto al de la pantalla.

---

## ADR-04 — Un endpoint de lote, y no N peticiones

**Contexto.** VERSION_2.md §3: «cuando se registran varios renglones de una,
van en UN SOLO ENVÍO».

| Opción | Qué pasa si el tercero falla |
|---|---|
| Tres `POST` desde el front | Dos guardados y uno no: **medio registro**, un estado que el negocio no reconoce |
| Un `POST …/lote` con transacción ✅ | `ROLLBACK`: no quedó ninguno, y la pantalla dice por qué |

**Decisión.** Lote transaccional. **Se pierde:** no se puede mostrar progreso
parcial. **Se gana:** la **A** de ACID deja de ser teoría y pasa a verse en
`BeginTransaction()` / `Rollback()`.

---

## ADR-05 — El contador lo mantiene un disparador, no el servicio

| Opción | Problema |
|---|---|
| `ServicioEstudios` actualiza `docente.total_estudios` después de insertar | Son **dos** operaciones: si la segunda falla, el contador miente. Y quien entre por SQL Server Management Studio lo deja mal igual |
| `trg_contar_estudios` en la base ✅ | Vale para **todo el mundo**, incluido quien no pase por la API |

**Decisión.** Disparador. Y escucha `INSERT`, `UPDATE` **y** `DELETE`: con
borrado lógico, retirar un hijo es un `UPDATE`, y un disparador que no lo
oiga deja el contador mal **justo en el caso que nadie prueba**.

**Se pierde:** lógica repartida en dos sitios (C# y SQL), y hay que saber
decir dónde está cada cosa. Es exactamente lo que se pregunta.

---

## ADR-06 — Sin cascada en el borrado lógico

**Contexto.** Al retirar un docente, ¿se retira su detalle?

| Opción | Consecuencia |
|---|---|
| Cascada lógica: retirar el docente retira sus estudios | Borra en silencio. Y **no se puede deshacer en la v2**, porque reactivar no está en el alcance |
| **409 y que lo retire quien quiera retirarlo** ✅ | Obliga a una decisión consciente; el mensaje dice cuántos hijos activos hay |

**Decisión.** 409 (decisión D2 del `2_spec.md`). Es una **regla de negocio**,
no una restricción de la base: con borrado lógico el motor no la defendería.

---

## ADR-07 — `experiecia` se queda con su nombre

La tabla del esquema dado se llama `experiecia` (falta la `n`). Renombrarla
rompería el script del curso y los scripts de los otros módulos.

**Decisión.** La **tabla** y la **entidad** conservan el nombre del esquema;
la **ruta** es `/api/experiencia` y la **pantalla** dice «Experiencia». El
error queda documentado aquí, que es donde se buscan estas cosas.

**Se pierde:** una inconsistencia visible entre tabla y ruta, que hay que
saber explicar. **Se gana:** el script sigue siendo el del profesor.

---

## ADR-08 — Las tablas puente no se editan

Una pareja `(red, docente)` **es** su llave primaria. «Editarla» sería
cambiar la identidad de la fila, que es crear otra y borrar esta.

**Decisión.** Solo **asignar** y **retirar**. En la interfaz no hay botón de
editar en ninguna puente, y en la API no hay `PUT` ni `PATCH` en esas rutas.
Los atributos propios de la relación (`fecha_inicio`, `dedicacion`) sí se
pueden actualizar con `PATCH` **sobre las dos claves**, que no cambian.

---

## Lo que se consultó

| Tema | Dónde |
|---|---|
| Qué se espera de la v2 | `ProyectosDeAula/docs/VERSION_2.md` |
| El método, la rúbrica y los diagramas obligatorios | `ProyectosDeAula/docs/0_METODOLOGIA.md` |
| El patrón de capas ya montado | `docs/SOLID_CAPAS_PATRONES.md` · `api_gestion/` de la v1 |
| El recorrido de una petición | `docs/FLUJO_DE_UNA_PETICION.md` |
| Transacciones | `docs/PRINCIPIOS_ACID.md` |
| `async`/`await` | `docs/PROGRAMACION_ASINCRONICA.md` |
| El esquema | `db/gestion_profesoral.sql` · `ProyectosDeAula/db_scripts/sqlserver/` |
