# Guía de IA — Versión 2

> **El eslabón del medio.** La especificación dice **qué** construir, el
> código es **lo construido**, y esta guía es **cómo se pasó de uno al otro**:
> los prompts que se usaron y, sobre todo, lo que hubo que corregirle a la IA.
>
> **Modalidad:** IDE agéntico, sobre este mismo repositorio.
> **Autor:** _______________________ · rama `rama-simon`.

---

## 1. Criterios de trabajo con IA

| | |
|---|---|
| **Qué se delega** | La escritura mecánica: una rebanada vertical ya especificada es el mismo molde con otros nombres |
| **Qué no se delega** | Las decisiones del dominio, qué entra al repositorio y la verificación de lo generado |
| **Las ambigüedades se marcan** | Cuando la IA propone un valor por defecto para algo que la spec no define, se detiene y la respuesta se escribe en el `2_spec.md` |
| **Los comentarios dicen por qué** | Un comentario que repite el nombre del método no aporta; el que dice la razón, sí |
| **Todo se comprueba corriendo** | La IA documenta lo que cree que hizo. Lo que vale es haberlo levantado y haberlo hecho fallar |

---

## 2. Prompt 1 — encuadre sobre el método y el esquema

```text
Contexto: módulo Gestión Profesoral de un proyecto académico, construido con
Spec-Driven Development. Te paso tres fuentes, en este orden de autoridad:

  1. 0_METODOLOGIA.md — el método, los diagramas obligatorios y la rúbrica
  2. VERSION_2.md     — qué se espera exactamente de esta versión
  3. db/gestion_profesoral.sql — el esquema, que es la fuente de verdad de
     las tablas: no inventes columnas ni cambies nombres

Stack: API en C# / ASP.NET Core sobre SQL Server con Dapper, front en Blazor
Server, todo en Docker.

Tarea: escribir el spec kit de la v2 en docs/spec_kit/versiones/v2_docente/,
con los nueve documentos del molde y la guía de IA, describiendo solo el
delta sobre la v1. El detalle de cada documento y los diagramas obligatorios
están en PROMPT_GENERAR_SPEC_KIT.md, en esta misma carpeta.
```

**Por qué el encuadre va primero.** Pedirle código antes de darle el método
produce una estructura plausible pero sin los diagramas que la rúbrica exige
y sin los criterios de aceptación. El orden no es cortesía: es lo que hace
que la spec sirva para decidir.

### Correcciones

| Lo generado | Problema | Corrección aplicada |
|---|---|---|
| Empezó el spec kit con la estructura del ejemplo, sin haber leído la metodología | Habrían faltado los diagramas obligatorios de la §4 y los criterios de interpretabilidad | Se le pasó `0_METODOLOGIA.md` y se rehízo el plan: historias con secuencia, tres compuertas y doce criterios |
| Asumió .NET 8 por la antigüedad del ejemplo | El `.csproj` del repositorio declara `net10.0` | «Verifica el `TargetFramework` en el `.csproj` antes de fijar la versión» |

---

## 3. Prompt 2 — la rebanada vertical de `docente`

```text
Implementa la fase 1 del 8_tasks.md: la rebanada vertical completa de la
tabla docente, según el 5_data_model.md y el 6_contracts.md.

Restricciones que no se pueden incumplir:

- linea_investigacion_principal es NULLABLE: la propiedad es int? y el
  desplegable manda null cuando se elige «(ninguna)». null no es cadena
  vacía ni 0.
- activo no viaja en el JSON de respuesta ni se acepta en el cuerpo.
- total_estudios lo escribe un disparador: la API lo lee y nunca lo envía.
- El repositorio no puede contener SQL: solo CommandType.StoredProcedure
  con parámetros.
- El controlador no puede importar Microsoft.Data.SqlClient: la traducción
  de SqlException a 409 vive en la capa de datos.

Comenta cada decisión no obvia explicando por qué, no qué.
```

Con la spec escrita, el prompt se vuelve corto: el `3_plan.md` ya define los
ocho archivos de la rebanada y el `6_contracts.md` ya define cada respuesta.

### Correcciones

| # | Lo generado | Problema | Corrección aplicada |
|---|---|---|---|
| 1 | Un segundo `Main` en el proyecto de pruebas | Dos puntos de entrada en un mismo proyecto no compilan | Las pruebas del docente pasaron a `Ejecutar()`, encadenado desde el `Main` existente |
| 2 | El alta por lote exigía que el id del estudio viniera del formulario | `estudios_realizados` no es `IDENTITY` en el esquema dado, y dos peticiones simultáneas colisionarían | El procedimiento asigna el id cuando no llega, con `UPDLOCK, HOLDLOCK` dentro de la transacción |
| 3 | `sp_eliminar_*` con `DELETE` | El borrado del proyecto es lógico: un `DELETE` pierde el historial y rompe el criterio 10 | `UPDATE … SET activo = 0 WHERE … AND activo = 1`, con el comentario de por qué lleva la segunda condición |

---

## 4. Prompt 3 — verificación con el sistema levantado

```text
Levanta el proyecto con docker compose, verifica que compile y comprueba
contra la API: el CRUD completo de docente, que el desplegable de la clave
foránea traiga opciones, y que los errores de integridad respondan 409 y no
500. Reporta lo que falle sin arreglar nada todavía.
```

Compiló a la primera, pero levantarlo mostró dos cosas que ninguna revisión
de código habría encontrado:

| Hallazgo | Decisión |
|---|---|
| `GET /api/linea-investigacion` respondía **204**: la tabla no tiene datos de referencia en el script, así que el desplegable quedaba vacío y el criterio 5 no era verificable | Se agregaron cuatro líneas de ejemplo en `db/v2/v2_semillas_linea_investigacion.sql`, marcadas como provisionales hasta cargar las del Excel |
| La pantalla mostró «El servicio no está disponible» | **No se modificó nada.** Era el front consultando la API durante el arranque; al recargar funcionaba. Cambiar código por un síntoma transitorio es la forma más rápida de romper lo que ya servía |

Y una comprobación hecha deliberadamente **por la interfaz** y no por
consola: al intentar registrar una cédula ya retirada, la pantalla respondió
«Ya existe un docente con esa clave». Eso valida los criterios 7 y 11 a la
vez — el 409 llegando desde el motor hasta el usuario, en castellano.

---

## 5. Prompt 4 — recortar el repositorio a los entregables

```text
Revisa qué hay en el repositorio contra la lista de entregables de
VERSION_2.md §11. Todo lo que no sea uno de los siete entregables sale:
material del curso heredado del ejemplo, documentos de apoyo y borradores.
Dime además qué entregable falta.
```

En el repositorio habían quedado el material del curso que venía en el
ejemplo (`ProyectosDeAula/`, el manual de marca, los logos, `PLAN_V1.md`) y
documentos de apoyo que no son entregables. Al recortarlo apareció lo que
faltaba: **la colección de Postman de la v2**, que sí es entregable.

> **Esta corrección es la más valiosa de las cuatro**, y conviene decir por
> qué: una IA propone agregar, nunca quitar. Decidir qué sobra es del autor,
> y es lo que deja un repositorio donde el evaluador encuentra lo que va a
> calificar sin tener que buscarlo.

---

## 6. Prompt 5 — reorganización en capas

```text
Reorganiza el backend en cuatro proyectos —Domain, Application,
Infrastructure y Api— más uno de pruebas, con estas referencias:

  Api -> Application -> Domain <- Infrastructure
  Api -> Infrastructure, solo para el registro en Program.cs

Domain no referencia a nada. Application no referencia a Infrastructure.
El proyecto de pruebas referencia a Application y Domain, no a
Infrastructure.

Los repositorios reciben la cadena de conexión como string, no
IConfiguration: la capa de datos no debe saber que existe un appsettings.

Al terminar: compilar, levantar y volver a correr las pruebas de humo y las
de capas.
```

**Qué cambia esto respecto de tener carpetas.** Con carpetas, «el servicio no
toca SQL» es un acuerdo. Con proyectos es una referencia que no existe: el
compilador lo impide. Esa es la razón de la reorganización, y es lo que se
sustenta.

### Correcciones

| Lo generado | Problema | Corrección aplicada |
|---|---|---|
| Al mover los archivos, las clases de Infrastructure perdieron el `using` de los contratos | No compilaba: cinco errores de tipo no encontrado | Se agregó `using …Domain.Contratos` en los cuatro archivos y se recompiló antes de dar nada por hecho |
| Las interfaces `IRepositorioLineaInvestigacion` e `IServicioLineaInvestigacion` quedaron declaradas dentro del archivo de su implementación | Un contrato que vive dentro de su implementación no es un contrato | Se separaron a `Domain/Contratos/` y `Application/Contratos/` |

---

## 7. Lo que no se delegó

| | Por qué |
|---|---|
| **Las decisiones D1–D6 del `2_spec.md`** | Son del dominio. La D2 —si se puede retirar un maestro con detalle activo— es una regla de negocio, no un detalle de implementación |
| **Qué sobra en el repositorio** | Ver §5 |
| **La firma del `9_checklist.md`** | Una IA puede ayudar a evaluar; no puede auto-aprobarse |
| **Esta guía** | Si la escribiera la IA, el eslabón del medio sería inventado |

---

## 8. Ambigüedades detectadas y dónde se resolvieron

| Lo que la IA propuso asumir | Dónde quedó la decisión |
|---|---|
| Que al retirar un docente se retire su detalle en cascada | Decisión **D3**: no hay cascada; se responde 409 |
| Que el id de `estudios_realizados` fuera autoincremental | Aclaración **A3**: no lo es; lo asigna el procedimiento |
| Responder 500 ante un rechazo de la base | `6_contracts.md` §3.3: los errores de integridad son **409** |
| Corregir `experiecia` a `experiencia` | **ADR-07**: el nombre viene del esquema dado y no se renombra; se corrige el texto de la pantalla |

---

## 9. Pendiente antes de la entrega final

- [ ] Nombre del autor en el encabezado.
- [ ] Prompts de las fases 2 a 6, con la misma estructura: el prompt y su corrección.
- [ ] Si se usa también la modalidad de chat, dejar las dos.
