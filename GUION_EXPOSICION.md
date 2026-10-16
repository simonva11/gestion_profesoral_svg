# Guion de la exposición

> Para leer en el celular mientras manipula el computador.
> **HACES** = lo que abres o escribes. **DICES** = lo que cuentas en voz alta.
> No hay que decirlo literal: es para no quedarse en blanco.

---

## ANTES DE EMPEZAR — 5 minutos antes

**HACES:**

```bash
cd gpsvg
docker compose up -d --build
```

Espera a que respondan y deja **tres pestañas abiertas** en el navegador:

| Pestaña | Dirección |
|---|---|
| 1 · La pantalla | http://localhost:8075/docentes |
| 2 · Swagger | http://localhost:8074/swagger |
| 3 · GitHub | tu repositorio |

Y **VS Code abierto** en la carpeta del proyecto, con el panel de archivos a la vista.

> Si la pantalla dice «El servicio no está disponible», **recarga**: es el
> front que arrancó antes que la API. No toques nada.

---

## PARTE 1 — Qué es esto · 1 minuto

**DICES:**

> «Este es el módulo de Gestión Profesoral. Son **tres programas separados**
> que hablan entre sí: una **pantalla**, una **API** y una **base de datos**.
>
> La pantalla nunca habla con la base de datos: le pide todo a la API. Eso es
> a propósito, porque si los dos pudieran escribir en la base habría dos
> sitios con las reglas, y el día que se contradigan nadie sabe cuál manda.
>
> Lo que entrego hoy es la **versión 2**, que es la que agrega las tablas que
> dependen de otras.»

---

## PARTE 2 — La arquitectura · 2 minutos

**HACES:** VS Code → abre la carpeta `GestionProfesoral-Backend/src/`. Que se
vean las cuatro carpetas.

**DICES:**

> «El código está partido en **cuatro proyectos, uno por capa**:
>
> - **Domain** es el centro: dice qué es un docente. No depende de nada.
> - **Application** son las reglas del negocio.
> - **Infrastructure** es lo único que sabe que detrás hay SQL Server.
> - **Api** es lo único que sabe que esto se habla por internet.
>
> Y lo importante no es que sean cuatro carpetas, sino que son cuatro
> proyectos **con referencias**. Application **no referencia** a
> Infrastructure, así que el servicio **no puede** escribir SQL aunque yo
> quisiera: no compilaría. La regla no la cumple mi buena voluntad, la obliga
> el compilador.»

**HACES:** abre `src/GestionProfesoral.Application/GestionProfesoral.Application.csproj`
y señala que solo referencia a Domain.

**Si preguntan «¿y cómo se conectan entonces?»:**

**HACES:** abre `src/GestionProfesoral.Api/Program.cs`, línea ~40.

**DICES:**

> «Aquí, y solo aquí. Este archivo es el **ensamblador**: es el único sitio de
> todo el proyecto donde una clase concreta aparece junto a su interfaz. Si
> mañana cambiamos SQL Server por otro motor, se escribe otro repositorio y se
> cambia **una línea** de este archivo. Nada más se toca.»

---

## PARTE 3 — El spec kit: los requisitos · 2 minutos

**HACES:** GitHub →
`GestionProfesoral-Backend/docs/spec_kit/versiones/v2_docente/` →
abre **`2_spec.md`** → baja a la **sección 4**.

**DICES:**

> «Esto se escribió **antes** del código. Es el método del curso: primero la
> especificación, después se programa. Si se escribe al final es un informe,
> y entonces no sirvió para decidir nada.
>
> Aquí están los requisitos numerados, del RF1 al RF10. Por ejemplo el
> **RF1** dice que *todo el CRUD pasa por procedimientos almacenados*, y el
> **RF7** dice que *un error de integridad responde 409 y nunca 500*.»

**HACES:** baja a la **sección 8** del mismo archivo.

**DICES:**

> «Y aquí están los **doce criterios de aceptación**, cada uno con **cómo se
> comprueba**. No dice "debe funcionar bien": dice qué hay que abrir y qué
> tiene que pasar.»

---

## PARTE 4 — El contrato: los endpoints · 2 minutos

**HACES:** abre **`6_contracts.md`** → sección 1.

**DICES:**

> «Este documento es el contrato de la API: **qué direcciones existen y qué
> responde cada una**. Está escrito antes de programar, así que el código no
> decide el contrato: lo cumple.
>
> Por ejemplo `GET /api/docente` lista, `POST /api/docente` crea,
> `DELETE /api/docente/{cédula}` retira.»

**HACES:** baja al **catálogo de códigos** (sección 0).

**DICES:**

> «Y aquí está lo que responde en cada caso: 200 si salió bien, **204 si no
> hay nada** —que no es un error—, 404 si no existe, **409 si choca con algo
> que ya está**, y 422 si el formulario venía mal lleno.»

**HACES:** cambia a la pestaña de **Swagger**.

**DICES:**

> «Y esto es lo mismo, pero vivo: cada endpoint con sus campos. Si el contrato
> dice algo que Swagger no muestra, uno de los dos está mal.»

---

## PARTE 5 — Los diagramas de secuencia · 2 minutos

**HACES:** GitHub → `GestionProfesoral-Backend/docs/dominio/DIAGRAMAS_SECUENCIA.md`

**DICES:**

> «Un diagrama de secuencia muestra **el viaje de una petición**: quién llama
> a quién, en qué orden, y qué devuelve.
>
> Las cajas **no son genéricas**: son las clases que existen en el
> repositorio, con el mismo nombre. Si el diagrama dijera una cosa y el código
> hiciera otra, el diagrama no describiría el sistema, lo adornaría.»

**HACES:** muestra el **segundo** diagrama (el 409 de integridad).

**DICES:**

> «Este es el que más me gusta. Alguien manda un docente con una línea de
> investigación que no existe. La base lo rechaza con un error número 547, en
> inglés, lleno de jerga. Si nadie lo atrapa, al usuario le sale un **error
> 500**, que significa *me caí*, y quien lo lea va a buscar un problema que no
> existe.
>
> Entonces lo traducimos: **547 se convierte en 409 con un mensaje en
> castellano**. Y la traducción está en la capa de infraestructura, porque es
> la única que conoce la base de datos.»

**HACES:** muestra el **tercer** diagrama (el 409 de regla de negocio).

**DICES:**

> «Y este responde el mismo 409, pero nace en otro sitio. Si intento retirar
> un docente que todavía tiene estudios activos, **la base no diría nada**:
> retirar para nosotros es poner una columna en cero, y eso no rompe ninguna
> regla de la base. Así que esa regla la pusimos nosotros, en la capa de
> negocio. Es una decisión, no una restricción técnica — y está escrita en la
> spec como la decisión D2.»

> **Hay uno por cada historia de usuario** en
> `docs/spec_kit/0_historias_de_usuario.md`.

---

## PARTE 6 — El índice del CRUD: cómo se programó, capa por capa · 3 minutos

**HACES:** abre `docs/dominio/CRUD_CAPA_POR_CAPA.md`, y ve abriendo los
archivos de la tabla mientras hablas.

### La tabla que puedes leer del celular

| Verbo | Ruta | **Api** · DocenteController.cs | **Application** · ServicioDocente.cs | **Infrastructure** · RepositorioDocenteSqlServer.cs | Procedimiento en la base |
|---|---|---|---|---|---|
| **Listar** | `GET /api/docente` | `:40` | `:35` | `:56` | `sp_listar_docente` |
| **Consultar** | `GET /api/docente/{c}` | `:76` | `:47` | `:68` | `sp_consultar_docente` |
| **Crear** | `POST /api/docente` | `:95` | `:61` | `:84` | `sp_crear_docente` |
| **Reemplazar** | `PUT /api/docente/{c}` | `:145` | `:71` | `:132` | `sp_actualizar_docente` |
| **Modificar** | `PATCH /api/docente/{c}` | `:188` | `:86` | `:175` | `sp_actualizar_docente` |
| **Retirar** | `DELETE /api/docente/{c}` | `:225` | `:105` | `:219` | `sp_eliminar_docente` |

**DICES, mientras abres los tres archivos de la fila «Retirar»:**

> «Sigamos una sola operación: retirar un docente.
>
> **Uno.** El controlador recibe la petición y **no hace nada más** que
> llamar al servicio y traducir el resultado a un número: 200, 404 o 409.
>
> **Dos.** El servicio decide. Primero pregunta cuántos registros activos
> cuelgan de ese docente; si hay, lanza un conflicto. Esta es la regla de
> negocio, y vive aquí porque la base no puede defenderla.
>
> **Tres.** El repositorio **no escribe SQL**: llama a un procedimiento
> almacenado por su nombre. Esta línea —`CommandType.StoredProcedure`— es
> literalmente el criterio 3 de la entrega.
>
> **Cuatro.** El procedimiento está en la base, y hace un `UPDATE activo = 0`,
> no un `DELETE`. El dato no se pierde: se retira.»

**HACES:** abre `GestionProfesoral-Backend/db/gestion_profesoral.sql`, línea **712**.

---

## PARTE 7 — Los procedimientos y el disparador · 2 minutos

**HACES:** en el mismo archivo, ve a la línea **867**.

**DICES:**

> «Esto es un **disparador**: un pedazo de código que vive dentro de la base y
> se ejecuta solo. Mantiene al día cuántos estudios tiene cada docente, sin
> que nadie se acuerde de actualizarlo.
>
> Y escucha tres cosas: cuando se inserta, cuando se borra **y cuando se
> actualiza**. Eso último es la trampa: como nuestro borrado es lógico,
> retirar un estudio es una actualización. Un disparador que no escuche las
> actualizaciones deja el contador mal **justo cuando alguien retira algo**,
> que es el caso que nadie prueba.»

---

## PARTE 8 — La demostración en vivo · 4 minutos

**HACES:** pestaña 1, `localhost:8075/docentes`.

| # | Qué haces | Qué dices |
|---|---|---|
| 1 | Clic en **Agregar** | «Fíjense en el campo de la línea de investigación: **no es un cuadro de texto**» |
| 2 | Abre el **desplegable** | «Es una lista que se llena desde la API. En la pantalla se lee *Ingeniería de Software*; en la petición viaja el número 1. La persona reconoce nombres; la base necesita códigos» |
| 3 | Deja **«(ninguna)»** y llena el resto | «Esta línea es opcional, así que hay una opción vacía que manda un *nulo*. No una cadena vacía: eso haría que el servidor conteste un error de conversión en inglés» |
| 4 | **Guardar** | «Y queda registrado» |
| 5 | Intenta crear **la misma cédula** otra vez | «Aquí está el criterio 7: dice **"ya existe un docente con esa clave"**, en castellano. No dice error 500, ni *constraint*, ni *primary key*. La pantalla no habla en jerga» |
| 6 | **Retirar** un docente | «Desaparece de la lista…» |
| 7 | Terminal: consulta la base | «…pero **sigue ahí**, con la columna activo en cero. No se borró, se retiró» |

**Comando del paso 7** (tenlo copiado antes):

```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Aplicacionweb123!' -C -d gestion_local -Q "SELECT cedula, nombres, activo FROM docente"
```

---

## PARTE 9 — Las pruebas · 2 minutos

**HACES:**

```bash
bash GestionProfesoral-Backend/postman/humo_docente_v2.sh
```

**DICES:**

> «Quince comprobaciones contra la API. Y fíjense en la mitad de abajo: son
> **las peticiones que tienen que fallar**. Un criterio solo está probado si
> se probó también el caso malo.»

**HACES:**

```bash
docker compose exec api-gestion dotnet run --project tests/GestionProfesoral.Tests/GestionProfesoral.Tests.csproj
```

**DICES:**

> «Y esta es la prueba de las capas. Trece comprobaciones del servicio… **sin
> base de datos**. Le enchufo un repositorio falso que guarda en memoria, y el
> servicio ni se entera.
>
> Eso es la demostración de que las capas existen de verdad: si el servicio
> tuviera una conexión a SQL Server adentro, esto **ni siquiera
> compilaría**.»

---

## PARTE 10 — Git: ramas y commits · 1 minuto

**HACES:** GitHub → pestaña **Insights → Network**, o la lista de commits.

**DICES:**

> «Nadie trabaja en la rama principal. El trabajo va en mi rama y entra a
> `main` por **Pull Request**.
>
> Y los commits son pequeños y dicen qué cambió: *"el repositorio llama
> procedimientos y traduce el 409"*, *"el servicio, con la regla que la base
> no puede defender"*. No hay un commit que diga *avances*.»

---

## PARTE 11 — Lo que falta · 1 minuto · **dilo tú primero**

**HACES:** abre `GestionProfesoral-Backend/docs/TRAZABILIDAD.md`.

**DICES:**

> «Y para terminar, lo que **no** está. Este documento es la trazabilidad:
> cada requisito de la spec, dónde se cumple y cómo se comprueba — y los que
> todavía no, marcados.
>
> La rebanada de docente está completa de punta a punta. Faltan las otras
> tablas, que son este mismo molde con otros nombres, y falta migrar el código
> de la versión 1 a procedimientos.
>
> Prefiero decirlo yo a que una matriz toda en verde no resista una prueba en
> vivo.»

---

# RESPUESTAS RÁPIDAS

| Si preguntan… | Contestas |
|---|---|
| **¿Por qué tres capas?** | «Porque cambian por razones distintas: el protocolo, las reglas y el motor. Cambiar de base de datos toca una clase y una línea de Program.cs» |
| **¿Cómo pruebas que hay capas?** | «Corriendo las pruebas con la base apagada» |
| **¿Por qué procedimientos almacenados?** | «Para que el SQL quede en un solo sitio, con nombre y versionado. Y porque los parámetros viajan como parámetros: una consulta armada pegando texto es por donde entra una inyección de SQL» |
| **¿Qué se pierde con eso?** | «Que hay que mirar dos archivos para entender una operación. Es el precio» |
| **¿De dónde sale el 409?** | «De dos sitios distintos: el de integridad lo descubre el motor y lo traduce la capa de datos; el de la regla de negocio lo decide el servicio, porque la base no podría» |
| **¿Qué pasa si esto falla?** | «Quien llamó recibe un código y un mensaje en castellano; la persona ve ese mensaje en la pantalla, sin jerga» |
| **¿Qué cambiarías?** | «Si agregamos un campo: el procedimiento y la entidad. Si cambiamos de motor: el repositorio y una línea. **Y no se toca** ni el servicio, ni el controlador, ni la pantalla» |
| **¿Por qué el disparador y no el código?** | «Porque también tiene que valer para quien entre directo por el administrador de la base y no pase por la API» |
| **¿Esto lo hizo una IA?** | «Sí, con mis prompts, y están entregados en la GUIA_IA2 con lo que tuve que corregirle. Por ejemplo, generó un borrado físico donde va un borrado lógico, y puso el disparador sin escuchar las actualizaciones» |

---

## SI ALGO SE CAE

| Problema | Qué haces |
|---|---|
| La pantalla dice «servicio no disponible» | Recargar. Es el front que arrancó primero |
| Un endpoint responde raro | `docker compose logs api-gestion --tail 30` |
| Nada responde | `docker compose ps` — mira que los tres estén *Up* |
| Perdiste los datos de la demo | Es normal si corriste `down -v`. Crea uno nuevo en 20 segundos desde la pantalla |

**Y la frase para salir de cualquier bache:**

> «Eso no lo tengo resuelto todavía, y está anotado en la trazabilidad como
> pendiente. Lo que sí puedo mostrar es…»
