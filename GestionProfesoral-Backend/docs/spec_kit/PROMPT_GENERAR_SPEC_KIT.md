# El prompt que genera el spec kit

> Este archivo es **reproducible**: con él y los documentos del curso, otra IA
> debería producir un spec kit equivalente al de `versiones/`. Es la mitad del
> «eslabón del medio» — la otra mitad, lo que hubo que corregirle, está en
> [`versiones/v2_docente/GUIA_IA2.md`](versiones/v2_docente/GUIA_IA2.md).

---

## Contexto que se le pega primero

```text
Vas a escribir el spec kit de la VERSIÓN 2 de un proyecto de aula de Diseño
de Software, módulo Gestión Profesoral. Se trabaja con Spec-Driven
Development: la especificación se escribe ANTES del código y manda sobre él.

LOS DOCUMENTOS QUE MANDAN (te los pego a continuación):
- 0_METODOLOGIA.md  → cómo se trabaja, los diagramas obligatorios y la rúbrica
- VERSION_2.md      → qué se espera exactamente de esta versión
- el esquema de la base (db/gestion_profesoral.sql), que es la fuente de
  verdad de las tablas: no inventes columnas ni cambies nombres
- el spec kit de la v1, que es el molde a seguir

STACK: API en C# / ASP.NET Core sobre SQL Server, con Dapper. Frontend en
Blazor Server. Todo en Docker. Arquitectura en cuatro proyectos:
Domain, Application, Infrastructure y Api.
```

## La petición

```text
Escribe la carpeta docs/spec_kit/versiones/v2_docente/ con los nueve
documentos del molde, describiendo SOLO EL DELTA sobre la v1:

  2_spec.md       propósito, alcance, requisitos funcionales numerados
                  (RF1, RF2…), las decisiones que el equipo debe tomar, las
                  clarificaciones, y los 12 criterios de aceptación
  3_plan.md       el plan técnico, el orden de construcción y el chequeo de
                  constitución artículo por artículo
  4_research.md   las decisiones de arquitectura, cada una con la alternativa
                  descartada y lo que se PIERDE con la elegida
  5_data_model.md las tablas de la versión y quién escribe cada columna
  6_contracts.md  los endpoints exactos, el catálogo de códigos y la tabla de
                  desenlaces por endpoint
  7_quickstart.md cómo se levanta y cómo se comprueba cada criterio
  8_tasks.md      las fases, cada una con su compuerta de verificación
  9_checklist.md  la lista que se firma antes de dar la versión por cerrada
  GUIA_IA2.md     los prompts usados y sus correcciones

Y amplía docs/spec_kit/0_historias_de_usuario.md con las historias de esta
versión, cada una con su criterio de aceptación.

DIAGRAMAS OBLIGATORIOS, en bloques ```mermaid dentro de los .md (nunca
imágenes), cada uno con 1-3 frases de guía de lectura debajo:
  0_historias_de_usuario.md  una SECUENCIA por historia
  2_spec.md                  diagrama de contexto
  3_plan.md                  despliegue + clases + secuencia del camino feliz
  5_data_model.md            entidad-relación
  6_contracts.md             las secuencias de ERROR (mínimo el 404 y el 422)
  7_quickstart.md            el ciclo de validación
  8_tasks.md                 las fases con sus compuertas

REGLAS DURAS
1. Las cajas de los diagramas son las CLASES REALES que va a tener el
   repositorio, con los mismos nombres. El diagrama es un contrato: si dice
   que el servicio lanza la excepción y el controlador la traduce, el código
   tiene que hacer exactamente eso.
2. Si algo no está definido en los documentos que te pegué, NO lo asumas:
   escribe «[NECESITA ACLARACIÓN: …]» y sigue. No rellenes con un valor por
   defecto.
3. Cada criterio de aceptación dice CÓMO SE COMPRUEBA, no solo qué se espera.
4. Todo en castellano, incluidos los nombres de clases y rutas.
5. No anticipes nada de la v3 ni de la v4: ni login, ni roles, ni dashboard.
```

## Después de generarlo: las tres compuertas

El spec kit **no está listo cuando los archivos existen**, sino cuando pasan
las tres compuertas de `0_METODOLOGIA.md` §3.1, y esas las pasa una persona:

| | Dónde | Qué se revisa |
|---|---|---|
| **1** | `2_spec.md` §7 | Que no quede ningún `[NECESITA ACLARACIÓN]`: cada uno se decide y la decisión se escribe |
| **2** | `3_plan.md` §5 | Que el plan respete la constitución, artículo por artículo. Si choca, se enmienda la constitución — no se deja pasar |
| **3** | `9_checklist.md` | Que cada requisito sea medible, único y verificable. **No se escribe código con la lista en rojo** |

> En esta versión la compuerta 2 encontró un choque real: la constitución
> decía que el SQL se escribe a mano con Dapper, y la v2 exige procedimientos
> almacenados. Se **enmendó el artículo 2** en vez de ignorarlo, y eso quedó
> escrito en el `3_plan.md`.
