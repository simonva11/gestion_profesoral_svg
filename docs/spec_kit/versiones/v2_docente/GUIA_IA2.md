# Guía de IA — Versión 2

> **El eslabón del medio.** El spec kit dice **qué** construir, el código es
> **lo construido**, y esto es **cómo se pasó de uno al otro**: los prompts que
> de verdad se usaron y, sobre todo, **lo que hubo que corregirle a la IA**.
>
> **Modalidad:** IDE agéntico (Claude Code), sobre este mismo repositorio.
> **Autor:** _______________________ · rama `rama-simon`.
>
> Este proyecto lo desarrolla **una sola persona**, así que aquí no hay un
> prompt por integrante: está la secuencia completa, en el orden en que
> ocurrió.

---

## 1. Las reglas con las que se trabajó

| | |
|---|---|
| **Se puede usar IA** | Sí. Nadie va a preguntar quién escribió la línea |
| **Lo que NO se puede** | Entregar algo que no se pueda contar. Si la IA lo escribió y uno no lo entendió, **no está entregado: está pegado** |
| **La ambigüedad se MARCA** | Cuando la IA dijo «asumo que…», se la paró y la respuesta se escribió **en el `2_spec.md`**, no solo en el chat |
| **Los comentarios dicen por qué** | «recorre la lista» no vale; «el detalle va en un solo envío porque un fallo a mitad dejaría medio registro» sí |
| **Se comprueba lo que comenta** | La IA comenta lo que *cree* que hizo. Por eso todo se levantó y se probó |

---

## 2. Prompt 1 — el encuadre: leer el ejemplo y la metodología antes de escribir nada

```text
https://github.com/ccastro2050/proyecto_gestion_profesoral1

Entra a este GitHub. Tengo que mostrar cómo se dio la arquitectura del
código, los diagramas de secuencia, el spec kit, y que si ahí decimos que
hace una funcionalidad, en el código se vea reflejado.

Lo que tengo que hacer es la VERSIÓN DOS de este proyecto: cuáles son las
ramas y los commits, los diagramas de secuencia, el spec kit, el contract,
las capas — sustentar todo. La API que vamos a tener debe conectar toda la
data a la base de datos, y debemos tener una interfaz donde se haga el
CRUD. Importante: la arquitectura.
```

Y a continuación, el documento que manda:

```text
https://github.com/ccastro2050/proyecto_diseno_de_software4/blob/main/
ProyectosDeAula/docs/0_METODOLOGIA.md

Acá está la metodología y es lo que se espera de la versión dos.
```

**Por qué este prompt primero y no «escríbeme el código».** Porque el método
es SDD: la especificación manda. Darle el repositorio y la metodología **antes**
de pedirle una línea de código es lo que hizo que la spec saliera con los
diagramas obligatorios de la §4 y con los doce criterios, en vez de un
`2_spec.md` inventado.

### Qué hubo que corregirle

| Lo que hizo la IA | Por qué estaba mal | Lo que se le escribió |
|---|---|---|
| Empezó a escribir el spec kit **antes** de leer la metodología | Habría salido un spec kit con la estructura del ejemplo pero sin los diagramas obligatorios ni la interpretabilidad | Se le pasó el enlace de `0_METODOLOGIA.md` a mitad de camino y **rehízo el plan**: historias con secuencia, compuertas y los 12 criterios |
| Dio por hecho que la API era .NET 8 | El `.csproj` del repositorio dice **net10.0** | «Mira el `.csproj` antes de afirmar la versión» |

---

## 3. Prompt 2 — la fase 1, la rebanada vertical de `docente`

```text
Empecemos a construir el código de la fase 1 (docente): los 8 archivos de
la rebanada vertical más sus procedimientos.
```

Con el encuadre ya puesto por el spec kit, ese prompt corto alcanzó porque el
`3_plan.md` §4.2 ya decía **cuáles son los ocho archivos** y el `6_contracts.md`
ya decía **qué responde cada endpoint**. Ese es el punto del método: cuando la
spec está escrita, el prompt se vuelve corto.

Las reglas que sí hubo que repetirle explícitamente:

```text
Tres cosas que NO puedes equivocar:

- linea_investigacion_principal es NULLABLE: la propiedad es int? y el
  desplegable manda null cuando la persona elige «(ninguna)». null NO es
  cadena vacía ni cero.
- activo no viaja en el JSON ni se acepta en el cuerpo.
- total_estudios lo escribe un disparador: la API lo LEE y nunca lo manda.

Y el repositorio NO puede tener una línea de SQL: solo
CommandType.StoredProcedure con parámetros. El controlador NO puede importar
Microsoft.Data.SqlClient: la traducción de SqlException a 409 vive en la
capa 3.
```

### Qué hubo que corregirle

| # | Lo que pasó | Por qué importaba | La corrección |
|---|---|---|---|
| 1 | El proyecto de pruebas **ya tenía un `Main`** y la IA escribió otro en el archivo nuevo | Dos `Main` en un mismo proyecto no compilan | Se convirtió la prueba del docente en `Ejecutar()` y se **encadenó** desde el `Main` que ya existía |
| 2 | La tabla `estudios_realizados` **no es `IDENTITY`** en el esquema dado | El alta por lote habría obligado a que alguien inventara los id a mano, y dos peticiones simultáneas chocarían | Se le pidió que el **procedimiento** asignara el id cuando no llega, con `UPDLOCK, HOLDLOCK` para que dos peticiones no calculen el mismo número |
| 3 | Dejó el `DELETE` físico en un procedimiento de eliminar | El borrado es lógico en todo el proyecto: un `DELETE` pierde la historia y rompe el criterio 10 | «Cámbialo por `UPDATE ... SET activo = 0 WHERE ... AND activo = 1`, y explica en un comentario por qué lleva el `AND activo = 1`» |

---

## 4. Prompt 3 — levantarlo y comprobarlo, que es donde se cae lo que «parecía» bien

```text
Ya prendí Docker Desktop. Levanta el proyecto, verifica que compile y
comprueba el CRUD de docente, el desplegable y el 409.
```

**Esto es lo que más corrigió el trabajo.** Compiló a la primera, pero al
probarlo aparecieron dos cosas que ningún repaso de código habría mostrado:

| Lo que apareció al correrlo | Qué se hizo |
|---|---|
| **El desplegable salía vacío.** `linea_investigacion` no tiene semillas en el script del profesor, así que `GET /api/linea-investigacion` respondía **204** y el `<select>` solo ofrecía «(ninguna)» | Se agregaron cuatro líneas de ejemplo en `db/v2/v2_semillas_linea_investigacion.sql`, **marcadas como provisionales** hasta cargar las del Excel. Sin ellas el **criterio 5 no se puede ni mirar** |
| **La pantalla dijo «El servicio no está disponible».** | **No se arregló nada**, y esa fue la decisión correcta: era el front arrancando antes que la API. Al recargar, funcionaba. Cambiar código por un error que no existe es la forma más rápida de romper algo que andaba bien |

Y una comprobación que sí valió la pena hacer **por la interfaz** y no por
consola: al guardar un docente con una cédula ya retirada, la pantalla
respondió **«Ya existe un docente con esa clave.»** — el 409 viajando desde el
motor hasta el usuario, en castellano. Eso confirmó los criterios 7 y 11 de una
sola vez.

---

## 5. Prompt 4 — la corrección más útil de todas: quitar

```text
Concéntrate en el spec kit y el código. La mayoría de cosas que creaste
como ProyectosDeAula y eso no importa. Estás metiendo cosas innecesarias.
```

La IA había dejado en el repositorio el material del curso que venía en el
ejemplo (`ProyectosDeAula/`, el manual de marca, los logos, el `PLAN_V1.md`) y
documentos de apoyo que **no son entregables**.

**Qué se le pidió:** dejar exactamente los siete entregables de
`VERSION_2.md` §11 y nada más. Al hacerlo apareció lo que de verdad faltaba:
**la colección de Postman de la v2**, que es entregable y no existía.

> **Por qué esta corrección vale más que las otras:** la IA tiende a **agregar**.
> Nunca propone quitar. Decidir qué sobra es del autor, y es lo que deja un
> repositorio donde el profesor encuentra lo que va a calificar sin buscarlo.

---

## 6. Lo que NO se le delegó a la IA, y por qué

| | |
|---|---|
| **Las decisiones D1–D6 del `2_spec.md`** | La IA las habría «asumido» y seguido. La ambigüedad se marca, no se rellena. La D2 —si se puede retirar un maestro con detalle activo— es una regla de negocio, no un detalle de implementación |
| **Qué sobra en el repositorio** | Ver §5 |
| **Marcar el `9_checklist.md`** | Una IA puede ayudar a evaluar; no puede auto-aprobarse |
| **Esta guía** | Si la escribiera la IA, el eslabón del medio sería inventado |

---

## 7. Registro de ambigüedades que la IA intentó rellenar

> Cada «asumo que…» se paró, y la respuesta quedó **en la spec**. El chat se
> cierra; la spec queda.

| Lo que la IA asumió | Dónde quedó la respuesta |
|---|---|
| «Asumo que al retirar un docente se retiran sus estudios» | Decisión **D3**: **no** hay cascada lógica → 409 |
| «Asumo que el id de `estudios_realizados` es autoincremental» | Aclaración **A3**: no lo es; lo asigna el procedimiento |
| «Por defecto respondo 500 si la base rechaza» | `6_contracts.md` §3.3: los errores de integridad son **409** |
| «Corrijo `experiecia` a `experiencia`» | **ADR-07**: no se renombra la tabla del esquema dado; lo que se corrige es el texto de la pantalla |

---

## 8. Para completar antes de entregar

- [ ] Poner el nombre del autor en el encabezado.
- [ ] Agregar los prompts de las fases que falten (2 a 6), con la misma
      estructura: **el prompt, y qué hubo que corregirle**.
- [ ] Si en alguna se usó el **chat** además del IDE agéntico, dejar los dos:
      la metodología pide las dos modalidades.
