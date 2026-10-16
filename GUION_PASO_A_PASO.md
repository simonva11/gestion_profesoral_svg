# Guion paso a paso — dónde le doy y qué voy diciendo

> **30 pasos en orden.** Cada uno dice exactamente dónde hacer clic y qué
> decir. Lo de **DICES** se lee tal cual; lo de **LE DAS** es lo que tocas.
>
> Duración: unos 18 minutos sin preguntas.

---

# ANTES DE ENTRAR

### PASO 0 · Preparar la máquina

**LE DAS:** abres una terminal y escribes:

```bash
cd gpsvg
docker compose up -d --build
```

Esperas a que termine. Luego:

| Abres | En qué |
|---|---|
| `http://localhost:8075/docentes` | Navegador, **pestaña 1** |
| `http://localhost:8074/swagger` | Navegador, **pestaña 2** |
| `github.com/simonva11/gestion_profesoral_svg` | Navegador, **pestaña 3** |
| La carpeta `gpsvg` | **VS Code** |
| Una terminal vacía | Abajo, lista para escribir |

**Comprobación:** la pestaña 1 debe mostrar la tabla de docentes. Si dice
«El servicio no está disponible», **recargas** y listo.

---

# BLOQUE 1 — EL SISTEMA · 2 minutos

### PASO 1 · Abrir con la pantalla a la vista

**LE DAS:** pestaña 1 del navegador (la pantalla de docentes).

**DICES:**
> «Buenas. Esto es la versión 2 del módulo de Gestión Profesoral.
>
> Lo que están viendo es una de las tres piezas del sistema. Son tres
> programas separados: esta **pantalla**, una **API** y una **base de
> datos**.»

### PASO 2 · Explicar la separación

**LE DAS:** nada, sigues en la misma pantalla.

**DICES:**
> «La pantalla es lo que usa el coordinador. La API es un programa sin
> pantalla que recibe peticiones y devuelve datos: ahí están las reglas. Y la
> base de datos es donde queda guardado.
>
> Lo importante: **la pantalla nunca habla con la base de datos**. Todo se lo
> pide a la API. Si las dos pudieran escribir en la base habría dos sitios con
> las reglas, y el día que se contradigan nadie sabe cuál manda.»

---

# BLOQUE 2 — CLASE E INTERFAZ · 3 minutos

### PASO 3 · Cambiar a VS Code

**LE DAS:** VS Code → panel izquierdo → despliegas
`GestionProfesoral-Backend` → `src`.

**DICES:**
> «Voy a abrir el código. Antes de nada necesito explicar dos palabras,
> porque todo lo demás se apoya en ellas: **clase** e **interfaz**.»

### PASO 4 · La comparación

**DICES:**
> «Una **clase** es el código que hace el trabajo de verdad.
>
> Una **interfaz** es la lista de lo que se puede pedir, sin decir cómo se
> hace. Es un contrato.
>
> Yo lo comparo con el enchufe de la pared. El **enchufe** es la interfaz:
> define la forma y qué sale por ahí, y no le importa si conectas un televisor
> o una licuadora. El **televisor** es la clase: el aparato concreto.
>
> ¿Para qué sirve? Para poder **cambiar el aparato sin romper la pared**.»

### PASO 5 · Mostrar una interfaz

**LE DAS:** `src` → `GestionProfesoral.Domain` → `Contratos` →
**`IRepositorioDocente.cs`**

**DICES:**
> «Esto es una interfaz. Se reconoce en dos cosas: el nombre empieza por **I**
> mayúscula, y los métodos **no tienen cuerpo** — dicen el nombre y se acaban
> en punto y coma.
>
> Aquí dice: se puede pedir obtener todos, obtener por cédula, crear,
> reemplazar, actualizar y eliminar. **No dice cómo se hace ninguna.**»

### PASO 6 · Mostrar la clase que la cumple

**LE DAS:** `src` → `GestionProfesoral.Infrastructure` → `Persistencia` →
**`RepositorioDocenteSqlServer.cs`** → busca la línea que dice
`public class RepositorioDocenteSqlServer : IRepositorioDocente`.

**DICES:**
> «Y esta es la clase que sí lo hace. Fíjense en los **dos puntos**: eso se lee
> *"cumple con"*. Esta clase se compromete a tener todo lo que la interfaz
> pidió, y aquí los métodos sí tienen cuerpo.
>
> El nombre termina en **SqlServer** a propósito, porque esta clase sí sabe
> que detrás hay SQL Server. Si mañana cambiamos de motor, escribo otra clase
> que cumpla la misma interfaz y nada más se entera.»

---

# BLOQUE 3 — LAS CUATRO CAPAS · 3 minutos

### PASO 7 · Mostrar las cuatro carpetas

**LE DAS:** VS Code → colapsas todo y dejas visible solo
`GestionProfesoral-Backend/src/` con sus cuatro carpetas.

**DICES:**
> «El código está partido en cuatro proyectos, uno por capa.
>
> **Domain** es el centro: dice qué es un docente y declara las interfaces. No
> depende de nada.
>
> **Application** decide: son las reglas del negocio.
>
> **Infrastructure** guarda: es lo único que sabe que hay SQL Server.
>
> **Api** traduce: es lo único que sabe que esto se habla por internet.»

### PASO 8 · La prueba de que las capas son reales

**LE DAS:** abres **`GestionProfesoral.Application.csproj`** (dentro de
`src/GestionProfesoral.Application/`) y señalas el bloque `ProjectReference`.

**DICES:**
> «Y aquí está lo que diferencia esto de cuatro carpetas con nombres bonitos.
>
> Este archivo dice a quién puede mirar la capa de reglas, y solo tiene **una**
> referencia: a Domain. **No referencia a Infrastructure.**
>
> ¿Qué significa en la práctica? Que yo, aunque quisiera, **no puedo escribir
> SQL en el servicio**: no compilaría. No es una buena intención, es una pared
> que pone el compilador.»

### PASO 9 · El ensamblador

**LE DAS:** `src/GestionProfesoral.Api/` → **`Program.cs`** → bajas hasta el
comentario que dice **EL ENSAMBLADOR** (línea 25 aproximadamente).

**DICES:**
> «Si el servicio solo conoce la interfaz, alguien tiene que decidir qué
> aparato se enchufa. Eso pasa aquí, y en ningún otro sitio del proyecto.
>
> Esta línea se lee: *cuando alguien pida un IRepositorioDocente, entrégale un
> RepositorioDocenteSqlServer*.
>
> Este archivo es el único donde una clase concreta aparece al lado de su
> interfaz. Por eso cambiar de base de datos es escribir una clase y cambiar
> **una línea**.»

---

# BLOQUE 4 — LOS REQUERIMIENTOS · 2 minutos

### PASO 10 · Abrir el spec

**LE DAS:** navegador, pestaña 3 (GitHub) →
`GestionProfesoral-Backend` → `docs` → `spec_kit` → `versiones` →
`v2_docente` → **`2_spec.md`** → bajas a **«4. Requisitos funcionales»**.

**DICES:**
> «Esto se escribió **antes** de programar. El método se llama desarrollo
> guiado por especificación: primero se escribe qué se va a construir.
>
> Si la especificación se escribe al final deja de ser una especificación y
> pasa a ser un informe de lo que uno hizo — y entonces no sirvió para decidir
> nada.»

### PASO 11 · Leer dos requisitos

**LE DAS:** señalas RF1 y bajas a RF7.

**DICES:**
> «Hay diez requisitos. Les leo dos.
>
> El **RF1** dice que todas las operaciones pasan por procedimientos
> almacenados.
>
> El **RF7** dice que cuando algo choca con lo que ya existe, la respuesta es
> **409** con un mensaje en castellano, **nunca un error 500**.»

### PASO 12 · Los criterios

**LE DAS:** mismo archivo → bajas a **«8. Criterios de aceptación»**.

**DICES:**
> «Y estos son los doce criterios con los que se califica. Fíjense en la
> columna de la derecha: cada uno dice **cómo se comprueba**. No dice "debe
> funcionar bien", que no significa nada.»

---

# BLOQUE 5 — LOS ENDPOINTS · 2 minutos

### PASO 13 · Abrir el contrato

**LE DAS:** GitHub → botón **atrás** → **`6_contracts.md`** → sección
**«1. Las rutas de la v2»**.

**DICES:**
> «Un endpoint es una dirección a la que se le pide algo a la API. Este
> documento es el contrato: qué direcciones existen y qué responde cada una.
>
> También se escribió antes de programar, así que el código no decide el
> contrato: lo cumple.»

### PASO 14 · Los códigos

**LE DAS:** subes a **«Catálogo de códigos de la v2»**.

**DICES:**
> «Y estos son los números. **200** salió bien. **204** no hay nada que
> mostrar, y no es un error. **404** eso no existe. **409** choca con algo que
> ya está. **422** el formulario vino mal lleno.»

### PASO 15 · Swagger

**LE DAS:** pestaña 2 del navegador (`localhost:8074/swagger`). Despliegas
`GET /api/docente`.

**DICES:**
> «Y esto es lo mismo, pero vivo: lo genera la propia API. Si el contrato
> dijera algo que aquí no aparece, uno de los dos estaría mal.»

---

# BLOQUE 6 — LOS DIAGRAMAS DE SECUENCIA · 3 minutos

### PASO 16 · Abrirlos

**LE DAS:** GitHub → `GestionProfesoral-Backend` → `docs` → `dominio` →
**`DIAGRAMAS_SECUENCIA.md`**.

**DICES:**
> «Un diagrama de secuencia muestra el viaje de una petición: quién le habla a
> quién, en qué orden, y qué devuelve. Se lee de arriba hacia abajo.
>
> Y las cajas no son genéricas: son las clases que existen de verdad en el
> proyecto, con el mismo nombre. Si el diagrama dijera una cosa y el código
> hiciera otra, no estaría describiendo el sistema: lo estaría adornando.»

### PASO 17 · El 409 que viene de la base

**LE DAS:** bajas al **segundo** diagrama, el que se llama
«El 409 de integridad».

**DICES:**
> «Alguien intenta guardar un docente diciendo que pertenece a una línea de
> investigación que no existe.
>
> La base lo rechaza con un error número **547**, en inglés y lleno de
> tecnicismos. Si nadie lo atrapa, al usuario le sale un **500**, que
> significa *el servidor se cayó*. Y es mentira: el servidor está bien, el dato
> estaba mal. Quien lea ese 500 va a buscar un problema que no existe.
>
> Por eso lo traducimos: el 547 se convierte en **409** con un mensaje en
> castellano. Y la traducción está en infraestructura, porque es la única capa
> que conoce la base.»

### PASO 18 · El 409 que decide el negocio

**LE DAS:** bajas al **tercer** diagrama.

**DICES:**
> «Este responde el mismo 409 pero nace en otro sitio, y esa diferencia es la
> que yo preguntaría si fuera el profesor.
>
> Si intento retirar un docente que todavía tiene estudios, **la base no diría
> nada**: para nosotros retirar es poner una columna en cero, y eso a la base
> le parece un cambio normal.
>
> Así que esa regla la pusimos nosotros, en la capa de reglas. Es una decisión
> del negocio, no una restricción técnica, y está escrita en la especificación
> como la decisión D2.»

---

# BLOQUE 7 — EL CRUD CAPA POR CAPA · 4 minutos

### PASO 19 · Presentar el recorrido

**DICES:**
> «CRUD son las cuatro operaciones básicas: crear, leer, actualizar y borrar.
> Cada una atraviesa las cuatro capas. Voy a seguir una completa: **retirar un
> docente**.»

### PASO 20 · La capa 1

**LE DAS:** VS Code → `src/GestionProfesoral.Api/Controllers/` →
**`DocenteController.cs`** → `Ctrl+G` → escribes **225** → Enter.

**DICES:**
> «Paso uno, el controlador. Es lo primero que toca la petición cuando llega
> por internet.
>
> Miren lo poco que hace: llama al servicio, y según lo que pase devuelve un
> número — 200, 404 o 409. Y nada más.
>
> Si yo pusiera aquí una regla de negocio, esa regla solo valdría para quien
> entre por internet. Puesta en el servicio, vale siempre.»

### PASO 21 · La capa 2

**LE DAS:** `src/GestionProfesoral.Application/Servicios/` →
**`ServicioDocente.cs`** → `Ctrl+G` → **105**.

**DICES:**
> «Paso dos, el servicio. Aquí se decide.
>
> Pregunta cuántos registros activos tiene ese docente colgando. Si hay,
> lanza un conflicto diciendo cuántos son. Si no hay, manda a retirarlo.
>
> Y fíjense en lo que **no** hay en este archivo: ni una palabra de SQL, ni un
> solo número de los de internet. Este archivo no sabe que existe el 409:
> lanza un *"esto choca"* y el controlador lo traduce.»

### PASO 22 · La capa 3

**LE DAS:** `src/GestionProfesoral.Infrastructure/Persistencia/` →
**`RepositorioDocenteSqlServer.cs`** → `Ctrl+G` → **219**.

**DICES:**
> «Paso tres, el repositorio. Este guarda.
>
> Y aquí está lo que pide la entrega: **no hay SQL escrito**. Solo el nombre
> de un procedimiento y sus parámetros. Esta línea que dice
> `CommandType.StoredProcedure` es literalmente el criterio 3 de la rúbrica.»

### PASO 23 · La base de datos

**LE DAS:** `GestionProfesoral-Backend/db/` → **`gestion_profesoral.sql`** →
`Ctrl+G` → **712**.

**DICES:**
> «Paso cuatro, la base de datos. Este es el procedimiento, y hace un
> **UPDATE activo igual a cero**. No un DELETE.
>
> Eso se llama borrado lógico: el dato no se borra, se marca como retirado.
> Desaparece de las listas pero sigue ahí. Un docente que trabajó cinco años no
> se puede evaporar porque alguien tocó un botón.»

### PASO 24 · El disparador

**LE DAS:** mismo archivo → `Ctrl+G` → **867**.

**DICES:**
> «Y esto es un **disparador**: código que vive dentro de la base y se ejecuta
> solo, sin que nadie lo llame. Mantiene al día cuántos estudios tiene cada
> docente.
>
> Fíjense en esta línea: escucha inserciones, borrados **y actualizaciones**.
> Lo último es la trampa de esta versión: como el borrado es lógico, retirar un
> estudio **es una actualización**. Un disparador que no la escuchara dejaría
> el contador mal justo cuando alguien retira algo — el caso que nadie
> prueba.»

---

# BLOQUE 8 — LA DEMOSTRACIÓN · 4 minutos

### PASO 25 · Crear un docente

**LE DAS:** pestaña 1 → botón **Agregar** → abres el desplegable
**Línea de investigación principal**.

**DICES:**
> «Voy a crear un docente. Fíjense en este campo: **no es un cuadro de texto**,
> es una lista, y se llena desde la API.
>
> En la pantalla se lee *Ingeniería de Software*, pero lo que viaja en la
> petición es el número **1**. La persona reconoce nombres; la base necesita
> códigos. Si fuera un cuadro de texto, habría que adivinar qué códigos
> existen.»

**LE DAS:** dejas la opción **«(ninguna)»**, llenas los demás campos y le das
**Guardar**.

**DICES:**
> «Esta línea es opcional, así que la lista trae *(ninguna)*, que manda un
> valor **nulo**. Nulo no es lo mismo que vacío: texto vacío donde va un número
> haría que el servidor contestara un error de conversión en inglés.»

### PASO 26 · Provocar el error

**LE DAS:** **Agregar** otra vez → pones **la misma cédula** → **Guardar**.

**DICES:**
> «Ahora el caso malo, que es el que de verdad prueba algo.
>
> Miren el mensaje: *"Ya existe un docente con esa clave"*. En castellano. No
> dice error 500, ni dice *constraint*, ni dice clave primaria duplicada. La
> pantalla no habla en jerga de programadores: ese es el criterio 11.»

### PASO 27 · El borrado lógico

**LE DAS:** botón **Retirar** en una fila. Luego vas a la terminal y pegas:

```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Aplicacionweb123!' -C -d gestion_local -Q "SELECT cedula, nombres, activo FROM docente"
```

**DICES:**
> «Desapareció de la lista. Pero voy a mirar la base directamente… y ahí está,
> con la columna **activo en cero**. No se borró: se retiró.»

---

# BLOQUE 9 — LAS PRUEBAS · 2 minutos

### PASO 28 · La prueba de humo

**LE DAS:** terminal:

```bash
bash GestionProfesoral-Backend/postman/humo_docente_v2.sh
```

**DICES:**
> «Quince comprobaciones automáticas contra la API. Y fíjense en la mitad de
> abajo: son **las peticiones que tienen que fallar**. La cédula repetida, la
> línea que no existe, el formulario incompleto.
>
> Un criterio solo está probado cuando se probó también el caso malo.»

### PASO 29 · La prueba de capas

**LE DAS:** terminal:

```bash
docker compose exec api-gestion dotnet run --project tests/GestionProfesoral.Tests/GestionProfesoral.Tests.csproj
```

**DICES:**
> «Y esta es la que más me importa: trece comprobaciones de las reglas de
> negocio **sin base de datos**.
>
> Lo que hago es enchufarle al servicio un repositorio **falso** que guarda en
> memoria. Y el servicio ni se entera, porque él solo conoce la interfaz: el
> enchufe, no el aparato.
>
> Esa es la demostración de que las capas existen de verdad. Si el servicio
> tuviera una conexión a la base adentro, esto ni siquiera compilaría.»

---

# BLOQUE 10 — SOLID · 1 minuto

### PASO 30 · Los cinco principios

**LE DAS:** vuelves a VS Code con las cuatro carpetas de `src/` a la vista.

**DICES:**
> «Todo lo que mostré responde a los cinco principios SOLID.
>
> **Responsabilidad única:** el servicio decide y el repositorio guarda. Dos
> razones de cambio, dos clases.
>
> **Abierto y cerrado:** para cambiar de base de datos agrego una clase nueva
> y cambio una línea; no modifico lo que ya funciona.
>
> **Sustitución:** el repositorio falso reemplaza al real y el servicio se
> comporta igual.
>
> **Segregación de interfaces:** una interfaz por recurso, pequeña, en vez de
> una gigante.
>
> **Inversión de dependencias:** el servicio depende de la interfaz, nunca de
> la clase concreta. Y la interfaz vive en el centro, en Domain.
>
> Si tuviera que resumirlo en una frase: **todas las flechas apuntan hacia
> adentro**.»

---

# BLOQUE 11 — EL MÉTODO Y LOS PROMPTS · 2 minutos

### PASO 31 · Los commits

**LE DAS:** GitHub → pestaña **Commits**.

**DICES:**
> «Sobre cómo se trabajó: nadie toca la rama principal directamente. Todo entra
> por Pull Request.
>
> Y los commits son pequeños y dicen qué cambió: *"el repositorio llama
> procedimientos y traduce el 409"*. Ninguno dice *avances*.»

### PASO 32 · La guía de IA

**LE DAS:** GitHub → `docs` → `spec_kit` → `versiones` → `v2_docente` →
**`GUIA_IA2.md`**.

**DICES:**
> «Y aquí está la parte del método que más me sirvió: la guía de IA.
>
> La especificación dice **qué** construir y el código es **lo construido**.
> Esto es el eslabón del medio: **cómo se pasó de uno al otro**. Están los
> prompts que usé, en orden.»

### PASO 33 · Las correcciones — *esto es lo que vale*

**LE DAS:** bajas a las tablas que dicen **«Qué hubo que corregir»**.

**DICES:**
> «Pero lo que de verdad cuenta no son los prompts: son las correcciones.
>
> Tres ejemplos. Primero: generó un borrado físico donde el proyecto usa
> borrado lógico — eso habría perdido el historial y roto el criterio 10.
>
> Segundo: escribió el disparador escuchando solo inserciones y borrados. Con
> borrado lógico eso deja el contador mal justo cuando alguien retira algo.
>
> Y tercero, el más útil: tuve que pedir **quitar**. Una IA siempre agrega,
> nunca propone sacar; decidir qué sobra en el repositorio me tocó a mí.
>
> Por eso el repositorio tiene solo los siete entregables y no el material del
> curso que venía en el ejemplo.»

---

# BLOQUE 12 — CIERRE · 1 minuto

### PASO 34 · Lo que falta, dicho por mí

**LE DAS:** GitHub → `GestionProfesoral-Backend/docs/` →
**`TRAZABILIDAD.md`**.

**DICES:**
> «Y cierro con lo que **no** está hecho.
>
> Este documento es la trazabilidad: cada requisito de la especificación, en
> qué archivo se cumple y cómo se comprueba. Y los que todavía no, marcados.
>
> La rebanada del docente está completa de punta a punta. Faltan las demás
> tablas, que son este mismo molde con otros nombres, y migrar el código de la
> versión 1 a procedimientos almacenados.
>
> Prefiero decirlo yo a que una tabla toda en verde no resista una prueba en
> vivo.»

### PASO 35 · Última frase

**DICES:**
> «En resumen: cuatro capas que el compilador obliga a respetar, el SQL dentro
> de la base, los errores traducidos a castellano, y una prueba que corre sin
> base de datos para demostrar que las capas no son un dibujo.
>
> Quedo atento a las preguntas.»

---

# SI PREGUNTAN

| Pregunta | Respuesta |
|---|---|
| ¿Por qué cuatro capas? | «Cambian por razones distintas: el protocolo, las reglas y el motor. Cambiar de base de datos toca una clase y una línea» |
| ¿Cómo sé que el controlador no toca la base? | «Porque no podría: Application no referencia a Infrastructure. No compila» |
| ¿Dónde está la lógica? | «En Application. Por ejemplo, que no se pueda retirar un docente con estudios activos» |
| ¿Dónde está el SQL? | «En la base, en procedimientos. En el repositorio solo está el nombre y los parámetros» |
| ¿Por qué procedimientos y no SQL en el código? | «El SQL queda en un solo sitio y versionado. Y los parámetros viajan aparte, que es lo que evita la inyección de SQL» |
| ¿Qué se pierde con eso? | «Que para entender una operación hay que mirar dos archivos. Es el precio» |
| ¿De dónde sale el 409? | «De dos sitios: el de integridad lo descubre el motor y lo traduce la capa de datos; el de la regla lo decide el servicio» |
| ¿Qué pasa si falla? | «Quien llamó recibe un código y un mensaje en castellano; la persona ve ese mensaje, sin jerga» |
| ¿Qué cambiarías? | «Un campo: el procedimiento y la entidad. De motor: el repositorio y una línea. Y no se toca el servicio, ni el controlador, ni la pantalla» |

# SI ALGO SE CAE

| Pasa | Haces |
|---|---|
| «El servicio no está disponible» | Recargar |
| Un endpoint responde raro | `docker compose logs api-gestion --tail 30` |
| Nada responde | `docker compose ps` — los tres en *Up* |
| Se perdieron los datos | Normal tras `down -v`. Creas uno nuevo en veinte segundos |

**Frase para salir de cualquier bache:**
> «Eso está anotado en la trazabilidad como pendiente. Lo que sí puedo mostrar
> es…»
