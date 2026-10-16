# Guion palabra por palabra

> Lo que está en **cursiva entre corchetes** es lo que haces, no lo que dices.
> Todo lo demás se lee tal cual. Habla despacio y haz pausa en los puntos.

---

## 1. APERTURA

> «Buenas. Voy a presentar la **versión 2** del módulo de Gestión Profesoral.
>
> Lo voy a hacer en este orden: primero qué es el sistema, después cómo está
> organizado el código, después les muestro dónde está escrito lo que
> prometí, y al final lo corro en vivo para que vean que hace lo que dice.»

---

## 2. QUÉ ES EL SISTEMA

> «El sistema son **tres programas separados** que hablan entre sí.
>
> El primero es una **pantalla**, que es lo que ve la persona que trabaja con
> esto: un coordinador que mantiene las hojas de vida de los profesores.
>
> El segundo es una **API**. Una API es un programa sin pantalla: recibe
> peticiones y devuelve datos. Es el que tiene las reglas.
>
> Y el tercero es la **base de datos**, donde la información queda guardada.
>
> Lo importante es esto: **la pantalla nunca habla con la base de datos**.
> Siempre le pide todo a la API. Eso es a propósito. Si los dos pudieran
> escribir en la base, habría dos sitios distintos con las reglas, y el día
> que se contradigan nadie sabría cuál manda.»

---

## 3. LA DIFERENCIA ENTRE UNA CLASE Y UNA INTERFAZ

> «Antes de mostrar el código necesito explicar dos palabras, porque todo lo
> demás se apoya en ellas: **clase** e **interfaz**.
>
> Una **clase** es el código que **hace** el trabajo de verdad. Tiene las
> instrucciones adentro.
>
> Una **interfaz** es una **lista de lo que se puede pedir**, sin decir cómo
> se hace. Es un contrato: dice *"aquí se puede pedir listar, consultar,
> crear, actualizar y eliminar"*, y nada más.
>
> La comparación que uso es la del **enchufe de la pared**. El enchufe es la
> interfaz: define la forma, cuántos huecos tiene y qué va a salir por ahí.
> No le importa si lo que conectas es un televisor o una licuadora. El
> televisor es la clase: es el aparato concreto que hace algo.
>
> Y lo que gana uno con eso es que **puede cambiar el aparato sin romper la
> pared**.»

*[ABRES: `src/GestionProfesoral.Domain/Contratos/IRepositorioDocente.cs`]*

> «Esta es una interfaz. Se nota en dos cosas: empieza por **I** mayúscula,
> que es la costumbre para nombrarlas, y los métodos **no tienen cuerpo** —
> dicen el nombre y lo que devuelven, y se acaban en punto y coma. Aquí dice
> *se puede pedir obtener todos, obtener por cédula, crear, reemplazar,
> actualizar y eliminar*. **No dice cómo.**»

*[ABRES: `src/GestionProfesoral.Infrastructure/Persistencia/RepositorioDocenteSqlServer.cs`]*

> «Y esta es la clase que **sí** lo hace. Fíjense en la primera línea: dice
> `RepositorioDocenteSqlServer` **dos puntos** `IRepositorioDocente`. Esos dos
> puntos se leen *"cumple con"*: esta clase se compromete a tener todo lo que
> la interfaz pidió. Y aquí sí hay cuerpo: cada método abre llave y hace algo.
>
> El nombre termina en **SqlServer** a propósito, porque esta clase sí sabe
> que detrás hay SQL Server. Si mañana cambiamos de base de datos, escribo
> otra clase —`RepositorioDocentePostgres`— que cumpla **la misma
> interfaz**, y todo lo demás sigue funcionando sin tocarse.
>
> Eso es lo que permite una interfaz: **cambiar el aparato sin romper la
> pared**.»

---

## 4. LAS CUATRO CAPAS

*[ABRES: la carpeta `GestionProfesoral-Backend/src/` en el explorador]*

> «El código está partido en **cuatro proyectos, uno por capa**, y cada uno
> hace una sola cosa.
>
> **Domain** es el centro. Dice **qué es** un docente: que tiene cédula,
> nombres, apellidos. Y declara las interfaces, los contratos. No depende de
> nada: no sabe que existe internet ni que existe SQL Server.
>
> **Application** **decide**. Aquí están las reglas del negocio.
>
> **Infrastructure** **guarda**. Es la única parte que sabe que la base de
> datos es SQL Server.
>
> **Api** **traduce**. Es la única que sabe que esto se habla por internet.»

> «Y ahora lo importante, que es lo que diferencia esto de tener cuatro
> carpetas con nombres bonitos: **son cuatro proyectos separados, y cada uno
> declara a quién puede mirar**.»

*[ABRES: `src/GestionProfesoral.Application/GestionProfesoral.Application.csproj`]*

> «Este es el archivo de configuración de la capa de reglas. Fíjense en que
> solo tiene **una** referencia, a Domain. **No referencia a
> Infrastructure.**
>
> ¿Qué significa eso en la práctica? Que yo, aunque quisiera, **no puedo
> escribir SQL en el servicio**: el programa no compilaría. No es una
> recomendación que alguien cumple cuando tiene tiempo; es una pared que pone
> el compilador.»

---

## 5. ¿Y ENTONCES CÓMO SE CONECTAN?

*[ABRES: `src/GestionProfesoral.Api/Program.cs`, y baja hasta donde dice EL ENSAMBLADOR]*

> «Si el servicio solo conoce la interfaz, alguien tiene que decidir qué
> aparato se enchufa. Eso pasa **aquí, y en ningún otro sitio del proyecto**.
>
> Esta línea se lee así: *"cuando alguien pida un `IRepositorioDocente`,
> entrégale un `RepositorioDocenteSqlServer`"*.
>
> Este archivo se llama el **ensamblador**, y es el único lugar donde una
> clase concreta aparece al lado de su interfaz. Por eso cambiar de base de
> datos es escribir una clase nueva y cambiar **esta línea**. Nada más se
> toca: ni el servicio, ni el controlador, ni la pantalla.»

---

## 6. LOS REQUERIMIENTOS

*[ABRES en GitHub: `docs/spec_kit/versiones/v2_docente/2_spec.md`, sección 4]*

> «Esto se escribió **antes** de programar. El método del curso se llama
> desarrollo guiado por especificación: primero se escribe qué se va a
> construir, y después se construye. Si la especificación se escribe al final,
> ya no es una especificación: es un informe de lo que uno hizo, y entonces no
> sirvió para decidir nada.
>
> Aquí están los requisitos numerados, del RF1 al RF10. Les leo dos:
>
> El **RF1** dice que todas las operaciones pasan por **procedimientos
> almacenados** — ya les muestro qué es eso.
>
> El **RF7** dice que cuando alguien intente guardar algo que choca con lo que
> ya existe, el sistema responde con el código **409** y un mensaje en
> castellano, **nunca con un error 500**.»

*[BAJAS a la sección 8 del mismo archivo]*

> «Y aquí están los **doce criterios de aceptación**. Cada uno dice **cómo se
> comprueba**. No dice "el sistema debe funcionar bien", que no significa
> nada: dice qué hay que abrir y qué tiene que pasar.»

---

## 7. LOS ENDPOINTS

*[ABRES: `docs/spec_kit/versiones/v2_docente/6_contracts.md`, sección 1]*

> «Un **endpoint** es una dirección a la que se le puede pedir algo a la API.
> Este documento es el contrato: **qué direcciones existen y qué responde cada
> una**. También se escribió antes de programar, así que el código no decide
> el contrato: lo cumple.
>
> Por ejemplo: `GET /api/docente` devuelve la lista. `POST /api/docente` crea
> uno nuevo. `DELETE /api/docente/` con una cédula lo retira.»

*[SUBES al catálogo de códigos, sección 0]*

> «Y estos son los números que responde. Los importantes:
>
> **200** es *salió bien*. **204** es *no hay nada que mostrar* — y no es un
> error: es que la lista está vacía. **404** es *eso no existe*. **409** es
> *choca con algo que ya está*. Y **422** es *el formulario vino mal
> lleno*.»

*[CAMBIAS a la pestaña de Swagger, localhost:8074/swagger]*

> «Y esto es lo mismo pero vivo: la documentación que genera la propia API,
> con cada dirección y cada campo. Si el contrato dijera algo que aquí no
> aparece, uno de los dos estaría mal.»

---

## 8. LOS DIAGRAMAS DE SECUENCIA

*[ABRES: `docs/dominio/DIAGRAMAS_SECUENCIA.md`]*

> «Un **diagrama de secuencia** muestra el viaje de una petición: quién le
> habla a quién, en qué orden, y qué devuelve. Se lee de arriba hacia abajo:
> arriba está el principio y abajo el final.
>
> Y las cajas no son genéricas: **son las clases que existen de verdad en el
> proyecto**, con el mismo nombre. Si el diagrama dijera una cosa y el código
> hiciera otra, el diagrama no estaría describiendo el sistema: lo estaría
> adornando.»

*[MUESTRAS el segundo diagrama: el 409 de integridad]*

> «Este es el más interesante. Alguien intenta guardar un docente diciendo que
> pertenece a una línea de investigación que no existe.
>
> La base de datos lo rechaza con un error número **547**, en inglés y lleno
> de palabras técnicas. Si nadie atrapa ese error, al usuario le sale un
> **error 500**, que significa *el servidor se cayó*. Y eso es mentira: el
> servidor está perfecto, lo que pasó es que el dato estaba mal. Quien lea ese
> 500 va a buscar un problema que no existe.
>
> Entonces nosotros lo traducimos: el **547 se convierte en un 409** con el
> mensaje *"el registro relacionado que indicó no existe"*. Y la traducción
> está en la capa de infraestructura, porque es la única que conoce la base de
> datos.»

*[MUESTRAS el tercer diagrama: el 409 de regla de negocio]*

> «Este responde **el mismo 409**, pero nace en un sitio distinto, y esa
> diferencia es la que más me preguntarían si yo fuera el profesor.
>
> Si intento retirar un docente que todavía tiene estudios registrados, **la
> base de datos no diría nada**. Porque para nosotros retirar no es borrar: es
> poner una columna en cero, y eso a la base le parece un cambio normal.
>
> Así que esa regla la pusimos nosotros, en la capa de reglas. **Es una
> decisión del negocio, no una restricción técnica**, y está escrita en la
> especificación como la decisión D2.»

---

## 9. EL CRUD, CAPA POR CAPA

> «**CRUD** son las cuatro operaciones básicas sobre un dato: crear, leer,
> actualizar y borrar. En mi proyecto son seis direcciones, y **cada una
> atraviesa las cuatro capas**.
>
> Voy a seguir una sola, completa: **retirar un docente**.»

*[ABRES: `src/GestionProfesoral.Api/Controllers/DocenteController.cs`, línea 225]*

> «**Paso uno: el controlador.** Esto es lo primero que toca la petición
> cuando llega por internet.
>
> Miren lo poco que hace: llama al servicio, y según lo que pase devuelve un
> número. Si salió bien, 200. Si no existe, 404. Si choca, 409.
>
> Y **no hace nada más**. No tiene reglas, no tiene SQL. Si yo pusiera aquí
> una regla de negocio, esa regla solo valdría para quien entre por internet.
> Puesta en el servicio, vale siempre.»

*[ABRES: `src/GestionProfesoral.Application/Servicios/ServicioDocente.cs`, línea 105]*

> «**Paso dos: el servicio.** Aquí se decide.
>
> Primero pregunta cuántos registros activos tiene ese docente colgando. Si
> hay alguno, lanza un conflicto con un mensaje que dice cuántos son. Si no
> hay ninguno, manda a retirarlo.
>
> Y fíjense en lo que **no** aparece en este archivo: ninguna palabra de SQL,
> ningún número de los de internet. Este archivo no sabe que existe un 409:
> lanza un *"esto choca"* y el controlador lo traduce.»

*[ABRES: `src/GestionProfesoral.Infrastructure/Persistencia/RepositorioDocenteSqlServer.cs`, línea 219]*

> «**Paso tres: el repositorio.** Este guarda.
>
> Y aquí está el detalle que pide la entrega: **no hay SQL escrito**. Lo único
> que hay es el **nombre** de un procedimiento y sus parámetros. Esta línea
> que dice `CommandType.StoredProcedure` es, literalmente, el criterio 3.»

*[ABRES: `db/gestion_profesoral.sql`, línea 712]*

> «**Paso cuatro: la base de datos.** Este es el procedimiento, y hace un
> `UPDATE activo = 0`. **No un DELETE.**
>
> Eso se llama **borrado lógico**: el dato no se borra, se marca como
> retirado. Desaparece de las listas, pero sigue ahí. Un docente que trabajó
> cinco años no se puede evaporar porque alguien le dio a un botón.»

---

## 10. LOS PROCEDIMIENTOS Y EL DISPARADOR

> «Un **procedimiento almacenado** es una instrucción guardada **dentro** de
> la base de datos, con nombre propio. En vez de que el programa le mande el
> SQL cada vez, le dice: *"ejecuta el que se llama así"*.
>
> ¿Por qué se hace así? Por dos razones. Una, el SQL queda en **un solo
> sitio**, con nombre y versionado, en vez de repartido por el código. Y dos,
> seguridad: los datos viajan como parámetros, aparte. Cuando uno arma una
> consulta pegando textos, está abriendo la puerta a lo que se llama
> **inyección de SQL**, que es la forma más común de robarse una base de
> datos.»

*[VAS a la línea 867 del mismo archivo]*

> «Y esto es un **disparador**. Es código que vive dentro de la base y se
> ejecuta **solo**, sin que nadie lo llame, cuando pasa algo.
>
> Este mantiene al día cuántos estudios tiene cada docente. Nadie tiene que
> acordarse de actualizar ese número: se actualiza solo.
>
> Y fíjense en esta línea: escucha **inserciones, borrados y
> actualizaciones**. Lo último es la trampa de esta versión. Como nuestro
> borrado es lógico, retirar un estudio **es una actualización**. Un disparador
> que solo escuchara inserciones y borrados dejaría el contador mal justo
> cuando alguien retira algo — que es el caso que nadie prueba.»

---

## 11. LA DEMOSTRACIÓN EN VIVO

*[VAS a localhost:8075/docentes y le das a «Agregar»]*

> «Esta es la pantalla. Voy a crear un docente.
>
> Fíjense en el campo de la línea de investigación: **no es un cuadro de
> texto**, es una lista.»

*[ABRES el desplegable]*

> «Y esta lista **se llena desde la API**. Eso importa porque en la pantalla
> se lee *Ingeniería de Software*, pero lo que viaja en la petición es el
> número **1**. La persona reconoce nombres; la base de datos necesita
> códigos.
>
> Si esto fuera un cuadro de texto, la persona tendría que adivinar qué
> códigos existen, escribiría uno que no está, y el sistema le respondería un
> error sin decirle cuál era el bueno.»

*[DEJAS «(ninguna)» y llenas el resto. GUARDAS]*

> «Esta línea es opcional, así que la lista trae la opción *(ninguna)*, que
> manda un valor **nulo**. Nulo no es lo mismo que vacío: si mandáramos texto
> vacío donde va un número, el servidor contestaría un error de conversión en
> inglés en vez de aceptar que simplemente no hay valor.
>
> Y quedó registrado.»

*[INTENTAS crear la misma cédula otra vez]*

> «Ahora el caso malo. Intento crear el mismo docente otra vez.
>
> Y miren el mensaje: *"Ya existe un docente con esa clave"*. En castellano.
> **No dice error 500, ni dice constraint, ni dice clave primaria.** La
> pantalla no habla en jerga de programadores — ese es el criterio 11.»

*[RETIRAS un docente]*

> «Y ahora lo retiro. Desaparece de la lista… pero no se borró.»

*[EN LA TERMINAL]*

```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Aplicacionweb123!' -C -d gestion_local -Q "SELECT cedula, nombres, activo FROM docente"
```

> «Ahí está, con la columna activo en cero. **No se borró: se retiró.**»

---

## 12. LAS PRUEBAS

```bash
bash GestionProfesoral-Backend/postman/humo_docente_v2.sh
```

> «Quince comprobaciones automáticas contra la API. Y fíjense en la mitad de
> abajo: son **las peticiones que tienen que fallar**. La cédula repetida, la
> línea que no existe, el formulario incompleto.
>
> Un criterio solo está probado cuando se probó también el caso malo. Probar
> solo que funciona cuando todo sale bien no prueba casi nada.»

```bash
docker compose exec api-gestion dotnet run --project tests/GestionProfesoral.Tests/GestionProfesoral.Tests.csproj
```

> «Y esta es la prueba de las capas, que para mí es la más importante.
>
> Trece comprobaciones de las reglas del negocio… **sin base de datos**. Lo
> que hago es enchufarle al servicio un repositorio **falso**, que guarda en
> una lista en memoria en vez de en SQL Server. Y el servicio ni se entera,
> porque él solo conoce la interfaz — el enchufe, no el aparato.
>
> Eso es la demostración de que las capas existen de verdad: si el servicio
> tuviera una conexión a la base adentro, esto **ni siquiera compilaría**.»

---

## 13. SOLID

> «Todo lo que acabo de mostrar responde a los cinco principios SOLID:
>
> **S, responsabilidad única.** Cada clase cambia por una sola razón: el
> servicio decide, el repositorio guarda. Dos razones, dos clases.
>
> **O, abierto y cerrado.** Para cambiar de base de datos **agrego** una clase
> nueva y cambio una línea. No modifico lo que ya funciona.
>
> **L, sustitución.** El repositorio falso de las pruebas reemplaza al real y
> el servicio se comporta igual.
>
> **I, segregación de interfaces.** Una interfaz por recurso, pequeña, en vez
> de una gigante con todo adentro.
>
> **D, inversión de dependencias.** El servicio depende de la interfaz, nunca
> de la clase. Y la interfaz vive en el centro, en Domain: **la capa de
> adentro declara lo que necesita y la de afuera obedece**.
>
> Si tuviera que resumir toda la arquitectura en una frase sería esa: **todas
> las flechas apuntan hacia adentro**.»

---

## 14. GIT

*[ABRES la lista de commits en GitHub]*

> «Sobre el trabajo: nadie trabaja directo en la rama principal. Lo mío va en
> mi rama y entra por **Pull Request**.
>
> Y los commits son pequeños y dicen qué cambió: *"el repositorio llama
> procedimientos y traduce el 409"*, *"el servicio, con la regla que la base
> no puede defender"*. No hay ninguno que diga *avances*.»

---

## 15. LO QUE FALTA — dilo tú, antes de que pregunten

*[ABRES: `docs/TRAZABILIDAD.md`]*

> «Y para cerrar, lo que **no** está hecho.
>
> Este documento es la trazabilidad: cada requisito de la especificación, en
> qué archivo se cumple y cómo se comprueba. Y los que todavía no, marcados.
>
> La rebanada del docente está completa de punta a punta: pantalla, API,
> reglas, base de datos y pruebas. Faltan las otras tablas, que son este
> mismo molde con otros nombres, y falta migrar el código de la versión 1 a
> procedimientos almacenados.
>
> Prefiero decirlo yo a que una tabla toda en verde no resista una prueba en
> vivo.»

---

## 16. CIERRE

> «Eso es todo. En resumen: cuatro capas que el compilador obliga a respetar,
> el SQL dentro de la base, los errores traducidos a castellano, y una prueba
> que corre sin base de datos para demostrar que las capas no son un dibujo.
>
> Quedo atento a las preguntas.»
