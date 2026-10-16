# Guión de sustentación — Versión 2

> Cómo se sustenta todo: **la arquitectura, las capas, los diagramas de
> secuencia, el spec kit, los contratos, las ramas y los commits** — y sobre
> todo los **4 puntos de interpretabilidad**, que son los que no se pueden
> delegar.
>
> Los 20 puntos de la versión: **10 en equipo** (lo que el sistema hace,
> contra los criterios) + **10 individuales**, de los cuales **4 son
> interpretabilidad**.

---

## 1. Lo que se proyecta, en este orden

| # | Qué se abre | Cuánto | Qué se dice |
|---|---|---|---|
| 1 | `docs/spec_kit/versiones/0_mapa_versiones.md` | 1 min | Dónde está la v2 en la ruta y qué tablas le tocan |
| 2 | `v2_docente/2_spec.md` — **diagrama de contexto** | 1 min | El sistema, sus vecinos y qué viaja por cada flecha |
| 3 | `v2_docente/3_plan.md` — **despliegue y clases** | 2 min | Tres contenedores; las flechas apuntan a interfaces |
| 4 | **La aplicación en `localhost:8075`** | 5 min | El recorrido del §3 de aquí |
| 5 | **Swagger en `localhost:8074/swagger`** | 1 min | Los endpoints con nombre, uno por tabla |
| 6 | **Postman → carpeta «Deben fallar»** | 2 min | El 409, el 422, el 404 y el 204 |
| 7 | `0_historias_de_usuario.md` — **una secuencia** | 2 min | La que pida el profesor, leída contra el código |
| 8 | **GitHub: red de ramas, PR y tag `v2`** | 2 min | Nadie tocó `main`; cada quien tiene sus commits |
| 9 | **El archivo que elija el profesor** | 4 min | **Interpretabilidad** — §5 de aquí |

---

## 2. El resumen de 60 segundos (lo primero que se dice)

> «La v1 dejó montado el patrón sobre una tabla suelta: tres capas, borrado
> lógico y una pantalla. La v2 contesta lo que la v1 no tenía que contestar:
> **qué pasa cuando un registro depende de otro.**
>
> Tres cosas cambian. **Una:** todo el SQL se mudó a procedimientos
> almacenados, también el de la v1 — en los repositorios ya no hay SQL
> escrito a mano, solo llamadas con parámetros. **Dos:** aparecieron las
> claves foráneas, y con ellas el 409: un hijo con padre inexistente o una
> pareja repetida responden 409 en castellano, no 500. **Tres:** hay un
> disparador que mantiene el contador de estudios del docente, y escucha
> también el `UPDATE`, porque con borrado lógico retirar es un `UPDATE`.
>
> Y la v1 sigue en pie: su colección se volvió a correr completa.»

---

## 3. La demostración en vivo, paso por paso

| # | Qué se hace | Qué criterio demuestra | La frase que lo acompaña |
|---|---|---|---|
| 1 | Abrir **Docentes**, crear uno dejando la línea en **«(ninguna)»** | 5, 6 | «El desplegable muestra nombres y manda la clave. Como la FK es opcional, «(ninguna)» manda `null` — que no es cadena vacía: una cadena vacía donde va un número hace que el servidor conteste un error de conversión en inglés» |
| 2 | Abrir la **ficha** de ese docente | 9 | «El detalle está en la misma pantalla. No hay un menú aparte donde haya que volver a decir de qué docente se trata, porque un estudio no existe sin su docente» |
| 3 | Agregarle un estudio y **mostrar que el contador subió** | **4** | «Ese número **la API no lo mandó**: lo calculó el disparador dentro de la base» |
| 4 | **Retirar** ese estudio y mostrar que el contador **bajó** | **4** | «Y esta es la parte que se cae si el disparador solo oye `INSERT` y `DELETE`: con borrado lógico retirar es un `UPDATE`» |
| 5 | Cargar **tres estudios en un envío**, con el segundo malo | 7 | «409, y en la base no quedó **ninguno**: o entran los tres o no entra ninguno. Medio registro no es un estado que el negocio reconozca» |
| 6 | Asignar un docente a una red, **repetir la asignación** | 8 | «409, no 500. La base rechaza por llave primaria duplicada y nosotros lo traducimos» |
| 7 | **Retirar** un docente con estudios activos | 7 | «También 409, pero este nace en otro sitio: la base no lo habría impedido, porque retirar es un `UPDATE` y un `UPDATE` no viola ninguna clave foránea. Es una **regla de negocio** y vive en el servicio» |
| 8 | Retirar un docente sin detalle y **mostrarlo en la base con `activo = 0`** | 10 | «Desapareció del listado y **sigue en la base**. No se perdió: se retiró» |
| 9 | Correr `dotnet test` **con los contenedores apagados** | — | «La prueba del servicio pasa sin base de datos. Esa es la demostración de que las capas existen de verdad y no solo en el diagrama» |
| 10 | Correr la colección de la **v1** entera | **12** | «La regresión: la v2 incluye la v1, no la reemplaza» |

> **El paso 4 y el paso 7 son los que suben la nota**, porque son los dos
> casos que casi nadie prueba.

---

## 4. Las preguntas que van a hacer, con su respuesta

### Sobre la arquitectura y las capas

| Pregunta | Respuesta |
|---|---|
| **¿Por qué tres capas?** | Porque tienen tres razones de cambio distintas: el protocolo (HTTP), las reglas y el motor. Cambiar de SQL Server a PostgreSQL toca **una** clase y **una línea** de `Program.cs` |
| **¿Cómo demuestra que las capas existen?** | Corriendo la prueba con los contenedores apagados. Si el servicio tuviera una cadena de conexión dentro, no compilaría |
| **¿Por qué esta flecha no puede existir en su diagrama de dependencias?** | Porque va de una capa a una **clase concreta** de otra. Todas las flechas apuntan a interfaces; el único sitio donde una clase concreta aparece junto a su interfaz es el ensamblador |
| **¿Por qué el front no habla con la base?** | Porque entonces habría dos sitios con reglas de negocio, y el día que se contradigan nadie sabe cuál manda. El front solo conoce el JSON |

### Sobre la base de datos

| Pregunta | Respuesta |
|---|---|
| **¿Por qué procedimientos almacenados?** | Para que el SQL quede en **un solo sitio**, con nombre y versionado en el script. Y porque los parámetros viajan como parámetros: una consulta armada pegando texto es por donde entra una inyección de SQL |
| **¿Qué se pierde con eso?** | Que la lógica queda repartida en dos lenguajes, y hay que saber decir qué hay en cada uno. Lo digo: el **SQL** está en la base; las **reglas** están en el servicio |
| **¿Por qué el disparador y no el servicio?** | Porque el disparador vale también para quien entre por SQL Server Management Studio y no pase por la API. Si lo hiciera el servicio, serían dos operaciones y la segunda podría fallar |
| **¿Por qué el procedimiento de crear termina con un `SELECT`?** | Para devolver la fila **como quedó guardada**, con lo que puso el `DEFAULT` y lo que calculó el disparador — no como la mandó el formulario |

### Sobre los errores

| Pregunta | Respuesta |
|---|---|
| **Léame su secuencia del 404** | «Tres capas y tres responsabilidades sobre el mismo hecho: el repositorio dice *no hay filas*, el servicio dice *no existe*, el controlador dice *404*. `null` es un hecho; 404 es una interpretación» |
| **¿De dónde sale el 409?** | De dos sitios distintos, y hay que distinguirlos: el de **integridad** nace de un `SqlException` 547 que traduce la capa 3; el de **regla de negocio** lo decide el servicio, porque la base no lo podría defender |
| **¿Por qué el 422 y no el 400?** | 400 es *la regla se rompió* (`limite ≤ 0`); 422 es *la forma del dato está mal* (falta un campo). El 422 corta **antes** del controlador, en `[ApiController]` |
| **Si esto se cae, ¿qué ve la persona?** | Un mensaje en castellano. La pantalla no dice «PUT», «422», «FK» ni «constraint»: eso es jerga nuestra, no del coordinador |

### Sobre el spec kit y el método

| Pregunta | Respuesta |
|---|---|
| **¿Cuándo escribieron la spec?** | Antes. Si se escribe al final es un informe de lo que se hizo, y entonces no sirvió para decidir nada |
| **¿Qué ambigüedad encontraron?** | Las del §7 del `2_spec.md`. La más seria: si al retirar un docente se retira su detalle. Se marcó, se decidió en equipo y quedó escrita como **D3** — no se resolvió improvisando en el código |
| **¿La constitución se cumplió?** | En el artículo 2 **no**, y por eso se **enmendó**: decía que el SQL se escribe a mano y queda a la vista; ahora dice que vive en la base, versionado. El principio de fondo se mantiene, cambia dónde se ve. *Nunca se deja pasar por esta vez* |
| **¿Y los prompts?** | En la `GUIA_IA2.md`, uno por integrante, **con lo que hubo que corregirle a la IA** — que es lo que más dice del equipo |

### Sobre Git

| Pregunta | Respuesta |
|---|---|
| **¿Quién hizo qué?** | `git shortlog -sn`: cada rama es una persona y cada fase del `8_tasks.md` tiene su dueño |
| **¿Por qué nadie tocó `main`?** | Porque `main` es lo que funciona. Todo entró por PR y el merge lo hizo solo el encargado |
| **¿Dónde está la v1?** | En el **tag `v1`** de este mismo repositorio. Las versiones no son repositorios |

---

## 5. Los 4 puntos de interpretabilidad (individual)

> El profesor abre **un archivo que él elige**, de lo que **usted** entregó,
> y usted lo cuenta. Las cuatro preguntas son siempre las mismas.

### Las cuatro preguntas, y cómo se preparan

| | Pregunta | Cómo se prepara |
|---|---|---|
| **1** | **¿Qué hace?** | Cuéntelo en 3 frases, **sin leer línea por línea**. Empiece por quién lo llama y qué devuelve |
| **2** | **¿Por qué así?** | Diga la alternativa que descartó **y lo que se pierde** con la elegida. Están en el `4_research.md` |
| **3** | **¿Qué pasa si falla?** | Dos respuestas en una: qué recibe **quien llamó** (el código HTTP) y qué ve **la persona** (el mensaje de la pantalla) |
| **4** | **¿Qué cambiaría?** | Diga dónde se toca **y dónde no**. Lo segundo es lo que demuestra que entendió las capas |

### Ensayo: el archivo más probable (`RepositorioEstudiosSqlServer.cs`)

> **Qué hace.** «Es la capa 3 de los estudios. No decide nada: recibe una
> entidad del servicio y llama al procedimiento que corresponde, pasando los
> campos como parámetros. Lo único que hace de más es traducir los errores
> del motor a excepciones de dominio, porque es la única capa que conoce
> `SqlException`.»
>
> **Por qué así.** «Podíamos dejar el SQL aquí, como en la v1 con Dapper.
> Se mudó a procedimientos para que el SQL quede en un sitio, con nombre y
> versionado. **Se pierde** que ahora hay que mirar dos archivos para
> entender una operación; **se gana** que cambiar una consulta no obliga a
> recompilar la API, y que no hay texto que pegar, así que no hay por dónde
> inyectar.»
>
> **Qué pasa si falla.** «Si el docente no existe, la base lanza el error
> 547. Si nadie lo atrapara, subiría hasta el `catch` genérico del
> controlador y saldría un **500** — que es decir *me caí*, y quien lo lea va
> a buscar un error que no existe. Por eso `TraductorErroresSql` lo convierte
> en `ConflictoExcepcion`, el controlador responde **409** y la pantalla dice
> *El docente indicado no existe*.»
>
> **Qué cambiaría.** «Si mañana agregamos una columna, se toca el
> procedimiento y el modelo. Si cambiamos de motor, se escribe otro
> repositorio y se cambia **una línea** de `Program.cs`. **Y no se toca** ni
> el servicio, ni el controlador, ni el front: ninguno sabe que detrás hay
> SQL Server.»

### Los comentarios: lo que se califica

| ❌ No vale | ✅ Vale |
|---|---|
| `// recorre la lista de estudios` | `// el detalle viaja en UN envío y el repositorio lo abre dentro de una transacción: si fuera un INSERT por petición, un fallo a mitad dejaría medio registro` |
| `// valida el docente` | `// la FK la defiende la base y el 547 se traduce aquí: comprobar antes con un SELECT sería una carrera, porque entre el SELECT y el INSERT alguien pudo retirar al docente` |
| `// borra el estudio` | `// es UPDATE y no DELETE porque el borrado es lógico — y por eso el disparador tiene que escuchar el UPDATE` |

> **La prueba de fuego:** tápese el código. ¿El comentario sigue diciendo
> algo? Si solo repite el nombre del método, no aporta.
>
> **Y cuidado con los comentarios de la IA:** comenta lo que *cree* que hizo.
> Lo que se califica es que usted pueda decir **si es cierto**.

### Las tres cosas que hay que haber hecho antes de la sustentación

| | |
|---|---|
| **1** | **Hacer fallar su propio código a propósito** y mirar el error. Mande un docente que no existe. Apague la base. Quite el `AND activo = 1` y vea qué pasa |
| **2** | **Borrar una línea y ver qué se rompe.** Quite el `UPDATE` del disparador y compruebe que el contador no baja |
| **3** | **Contarle el archivo a un compañero** sin mirar la pantalla |

---

## 6. Las trampas de esta versión (repáselas la noche antes)

| | Qué pasa | Cómo se nota |
|---|---|---|
| **El disparador sordo al `UPDATE`** | Con borrado lógico, retirar es un `UPDATE` | El contador no baja al retirar |
| **El listado que no filtra** | Se olvidó el `WHERE activo = 1` | Lo retirado sigue apareciendo |
| **El desplegable con inactivos** | El listado que lo llena no filtra | Se puede elegir un padre retirado, que la base va a rechazar |
| **El 500 que debía ser 409** | No se tradujo el error de la base | Criterio 7 fallado |
| **El sobre leído mal** | La API devuelve `{tabla, limite, total, datos}`, no una lista pelada | Pantalla **vacía y sin ningún error** |
| **La puente con botón de editar** | Una pareja existe o no existe | — |
| **El procedimiento que no devuelve la fila** | La interfaz no puede mostrar lo que acaba de crear | — |
| **La jerga en la pantalla** | «Error 422», «violación de FK» | Criterio 11 |

---

## 7. Lo que NO se muestra, y hay que saber decir por qué

| Si preguntan por… | La respuesta |
|---|---|
| Inicio de sesión, usuarios, roles | **Es de la v3.** Adelantarlo no suma, resta: una v2 que ya trae media sesión le quita a la v3 la mitad de su razón de ser |
| Dashboard, gráficos, consultas multitabla | **Es de la v4** |
| Reactivar un registro retirado | **Fuera del alcance** de la v2, escrito en el `2_spec.md` §3 |

---

## 8. Checklist de la noche anterior

| | ✔ |
|---|---|
| `docker compose down -v && docker compose up -d --build` corre limpio | ☐ |
| Los doce criterios comprobados **hoy**, no la semana pasada | ☐ |
| La colección de la v1 corrida completa | ☐ |
| El tag `v2` puesto y empujado | ☐ |
| `9_checklist.md` firmado por una persona | ☐ |
| `GUIA_IA2.md` con el prompt y la corrección de **cada** integrante | ☐ |
| Cada quien ensayó las **cuatro preguntas** sobre **sus** archivos | ☐ |
| Cada quien hizo fallar su propio código y vio el error | ☐ |
| Ninguna pantalla dice «PUT», «422» ni «FK» | ☐ |
| `git ls-files \| grep "^\.env$"` da vacío | ☐ |
