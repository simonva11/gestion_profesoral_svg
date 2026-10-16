# Guía de IA — Versión 2

> **Obligatoria en cada versión.** Lleva los prompts que **de verdad se
> usaron** para generar el código a partir del spec kit, **uno por
> integrante, con su nombre**, y **lo que hubo que corregirle a la IA**.
>
> No es burocracia: el spec kit dice **qué** construir, el código es **lo
> construido**, y el prompt es **cómo se pasó de uno al otro**. Entregar los
> dos extremos y no el medio es entregar un resultado sin su procedimiento.
>
> **El profesor vuelve a ejecutar este archivo + el spec kit en otra IA y
> compara con el repositorio.** Si lo que sale no se parece, es que falta una
> de las tres piezas.

---

## 1. Las reglas con las que este equipo trabaja con IA

| | |
|---|---|
| **Se puede usar IA** | Sí. Nadie va a preguntar quién escribió la línea |
| **Lo que NO se puede** | Entregar algo que no se pueda contar. Si la IA lo escribió y usted no lo entendió, **no está entregado: está pegado** |
| **La ambigüedad se MARCA** | Si la IA dice «asumo que…» o «por defecto voy a…», **se la para**: eso es una ambigüedad de la spec disfrazada de detalle. La respuesta va a la spec, no solo al chat |
| **Los comentarios dicen por qué** | «recorre la lista» no vale. «el detalle va en un solo envío porque un fallo a mitad dejaría medio registro» sí |
| **Se comprueba lo que comenta** | La IA comenta lo que *cree* que hizo. Hay que hacerlo fallar y mirar |

---

## 2. El prompt base del equipo (el contexto que todos pegan primero)

```text
Eres un desarrollador de C# / ASP.NET Core 8 trabajando en el módulo
Gestión Profesoral de un proyecto académico. Vas a trabajar con
Spec-Driven Development: la especificación manda, y no se construye
nada que la spec no pida.

CONTEXTO DEL REPOSITORIO
- API: ASP.NET Core 8 + Dapper + SQL Server 2022, en Docker (puerto 8074).
- Front: Blazor Server (puerto 8075). El front NUNCA toca la base.
- Tres capas, y cada una depende de la INTERFAZ de la siguiente:
  Controllers/ -> Servicios/ -> Repositorios/ -> SQL Server.
  El único sitio donde una clase concreta aparece junto a su interfaz
  es Program.cs.
- Todo en castellano: nombres de clases, rutas, mensajes y comentarios.
- Borrado lógico: activo = 0, y todos los listados filtran activo = 1.

LA REGLA NUEVA DE LA V2
Los cinco verbos de TODAS las tablas pasan por procedimientos almacenados.
En los repositorios NO puede quedar SQL escrito a mano: solo llamadas con
CommandType.StoredProcedure y parámetros, nunca texto concatenado.

DOCUMENTOS QUE SON LA FUENTE DE VERDAD (los pego a continuación):
2_spec.md, 3_plan.md, 5_data_model.md, 6_contracts.md.

REGLAS DE TRABAJO
1. Si algo no está en esos documentos, NO lo asumas: dime
   "[NECESITA ACLARACIÓN: …]" y espera.
2. Comenta diciendo POR QUÉ, no qué. Si el comentario solo repite el
   nombre del método, bórralo.
3. No anticipes nada de la v3 ni de la v4: ni login, ni token, ni roles,
   ni dashboard.
```

---

## 3. Los prompts de cada integrante

> **Plantilla.** Cada integrante copia esta ficha y la llena con **lo que de
> verdad escribió**, incluidos los intentos que salieron mal. *La corrección
> es lo que más vale.*

### 3.1 Integrante: ____________________ · rama `rama-______` · Fase 0

**Prompt que usó (chat / IDE agéntico):**

```text
[Prompt base] +

Toma RepositorioProgramaSqlServer.cs, que hoy escribe SQL con Dapper, y
conviértelo a llamadas de procedimiento almacenado. Entrégame DOS cosas:

1. El script SQL con los cinco procedimientos de la tabla programa
   (sp_listar_programa, sp_consultar_programa, sp_crear_programa,
   sp_actualizar_programa, sp_eliminar_programa), siguiendo estas reglas:
   - el de listar filtra activo = 1 y recibe @Limite;
   - el de crear termina con un SELECT de la fila como quedó guardada;
   - el de eliminar hace UPDATE activo = 0, NO un DELETE;
   - los alias de las columnas van en el procedimiento (fecha_creacion AS
     FechaCreacion), para que Dapper mapee sin configuración.

2. El repositorio reescrito, con CommandType.StoredProcedure y sin una
   sola línea de SQL.

No cambies las interfaces: IRepositorioPrograma no se toca, porque el
servicio no se tiene que enterar de que el motor cambió por dentro.
```

**Qué salió mal y cómo se corrigió:**

| Lo que hizo la IA | Por qué estaba mal | Lo que se le escribió |
|---|---|---|
| *(ejemplo real que hay que reemplazar)* Generó `sp_eliminar_programa` con `DELETE FROM programa` | El borrado es lógico: un `DELETE` pierde la historia y rompe el criterio 10 | «El borrado es lógico en todo el proyecto. Cambia el DELETE por `UPDATE programa SET activo = 0 WHERE id = @Id AND activo = 1`, y explícame en un comentario por qué lleva `AND activo = 1`» |
| | | |

**Qué aprendió el integrante con esta corrección:** ______________________

---

### 3.2 Integrante: ____________________ · rama `rama-______` · Fase 1 (`docente`)

```text
[Prompt base] +

Genera la rebanada vertical completa de la tabla docente según el
5_data_model.md y el 6_contracts.md que te pegué: Modelos/Docente.cs,
Peticiones/DocenteCrear.cs y DocenteActualizar.cs, IRepositorioDocente,
RepositorioDocenteSqlServer, IServicioDocente, ServicioDocente y
DocenteController.

Tres cosas que NO puedes equivocar:
- linea_investigacion_principal es NULLABLE: la propiedad es int? y el
  desplegable del front manda null cuando la persona elige «(ninguna)».
  null NO es cadena vacía.
- activo no viaja en el JSON de respuesta ni se acepta en el cuerpo.
- total_estudios lo escribe un disparador: la API lo LEE y nunca lo manda.

Y crea Repositorios/TraductorErroresSql.cs: una clase estática que
convierta SqlException 547 en ConflictoExcepcion("El docente indicado no
existe.") y 2627/2601 en ConflictoExcepcion("Ya existe un registro con esa
clave."). El controlador traduce ConflictoExcepcion a 409. El controlador
NO puede importar Microsoft.Data.SqlClient.
```

**Qué salió mal y cómo se corrigió:**

| Lo que hizo la IA | Por qué estaba mal | Lo que se le escribió |
|---|---|---|
| | | |

---

### 3.3 Integrante: ____________________ · rama `rama-______` · Fase 2 (estudios + disparador)

```text
[Prompt base] +

1) Escribe trg_contar_estudios sobre estudios_realizados que mantenga
   docente.total_estudios.
   Tiene que escuchar INSERT, UPDATE **y** DELETE: como el borrado es
   lógico, retirar un hijo es un UPDATE, y un disparador que solo oiga
   INSERT y DELETE deja el contador mal justo cuando alguien retira algo.
   Cuenta solo los hijos con activo = 1, y usa inserted UNION deleted para
   recalcular los docentes afectados — no uno solo, porque un UPDATE puede
   tocar varias filas.

2) Escribe ServicioEstudios.CrearVarios y su repositorio, que inserte un
   arreglo de estudios en UNA transacción: BeginTransaction, un EXEC por
   renglón, Commit al final y Rollback en el catch. Si el tercero falla no
   puede quedar ninguno guardado.

3) EstudiosController con rutas anidadas bajo /api/docente/{cedula}: la
   cédula sale de la RUTA, nunca del cuerpo, porque un estudio no existe
   sin su docente.
```

**Qué salió mal y cómo se corrigió:**

| Lo que hizo la IA | Por qué estaba mal | Lo que se le escribió |
|---|---|---|
| *(típico)* El disparador solo con `AFTER INSERT, DELETE` | Con borrado lógico el contador no baja al retirar: criterio 4 fallado en el caso que nadie prueba | «Agrega UPDATE al AFTER y explícame en un comentario por qué hace falta con borrado lógico» |
| *(típico)* `CrearVarios` con un `foreach` de `await` **sin** transacción | Tres peticiones disfrazadas de una: si la tercera falla quedan dos guardadas | «Envuélvelo en una transacción de Dapper y pásala en cada ExecuteAsync; si no la pasas, cada EXEC corre por su cuenta» |
| | | |

---

### 3.4 Integrante: ____________________ · rama `rama-______` · Fases 3 y 5

```text
[Prompt base] +

Replica el patrón de estudios_realizados para evaluacion_docente,
reconocimiento y experiecia. Ojo con dos cosas:
- esas tres tablas SÍ son IDENTITY(1,1): el id NO se manda al crear, y el
  procedimiento lo devuelve con SCOPE_IDENTITY();
- la tabla se llama «experiecia» (sin n) en el esquema dado y así se queda;
  la ruta es /api/experiencia y la pantalla dice «Experiencia».

Después, apoyo_profesoral y beca: son extensiones 1 a 1 de
estudios_realizados, su PK es también su FK, y van como panel dentro de la
ficha del estudio, en /api/estudios-realizados/{id}/apoyo y /beca.
```

**Qué salió mal y cómo se corrigió:**

| Lo que hizo la IA | Por qué estaba mal | Lo que se le escribió |
|---|---|---|
| *(típico)* «Corrigió» `experiecia` a `experiencia` | Rompe el script del profesor: la tabla de la base no se llama así | «No renombres la tabla. El nombre viene del esquema dado; lo que se corrige es el texto de la pantalla» |
| | | |

---

### 3.5 Integrante: ____________________ · rama `rama-______` · Fase 4 (puentes)

```text
[Prompt base] +

Genera las cuatro tablas puente: docente_departamento, intereses_futuros,
red_docente y estudio_ac. Reglas:
- la llave primaria es la PAREJA, así que las dos claves van en la RUTA;
- NO generes PUT ni PATCH para la relación: una pareja existe o no existe,
  no se edita. Los atributos propios de la relación (fecha_inicio,
  dedicacion) sí se actualizan, pero identificando la fila por las dos
  claves;
- repetir la pareja tiene que responder 409 (SqlException 2627 traducida),
  nunca 500;
- en el front, los dos desplegables se llenan de la API y solo con activos:
  ofrecer un padre retirado es ofrecer una opción que la base va a rechazar.
```

**Qué salió mal y cómo se corrigió:**

| Lo que hizo la IA | Por qué estaba mal | Lo que se le escribió |
|---|---|---|
| *(típico)* Generó un `PUT /api/red-docente/{id}` con un id inventado | La tabla no tiene id: su identidad es la pareja | «La tabla no tiene columna id. Identifica la fila por (red, docente) y quita el PUT» |
| | | |

---

## 4. Lo que NO se le pidió a la IA, y por qué

| | |
|---|---|
| **Las decisiones D1–D6 del `2_spec.md`** | Son del equipo. Una IA habría «asumido» una y seguido, y la ambigüedad se marca, no se rellena |
| **Marcar el `9_checklist.md`** | Una IA puede ayudar a evaluar; no puede auto-aprobarse |
| **Esta guía** | Si la escribiera la IA, el eslabón del medio sería inventado |

## 5. Registro de ambigüedades que la IA intentó rellenar

> Cada vez que una IA dijo «asumo que…», se paró y la respuesta se escribió
> **en la spec**. El chat se cierra; la spec queda.

| Lo que la IA asumió | Dónde quedó la respuesta |
|---|---|
| «Asumo que al retirar un docente se retiran sus estudios» | Decisión **D3** del `2_spec.md`: **no** hay cascada lógica → 409 |
| «Asumo que el id de estudios_realizados es autoincremental» | Aclaración **A3**: no lo es; lo manda la API |
| «Por defecto respondo 500 si falla la base» | `6_contracts.md` §3.3: los errores de integridad son **409** |
| | |
