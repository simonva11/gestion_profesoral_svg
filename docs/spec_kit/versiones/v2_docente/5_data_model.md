# Modelo de datos — Versión 2

> Las 11 tablas con clave foránea del módulo, cómo cuelgan unas de otras, y
> **qué columnas escribe la base y no la API**.

## 1. Entidad-relación de la v2

```mermaid
erDiagram
    linea_investigacion ||--o{ docente : "FK OPCIONAL"
    docente ||--o{ estudios_realizados : "maestro-detalle"
    docente ||--o{ evaluacion_docente : "detalle"
    docente ||--o{ reconocimiento : "detalle"
    docente ||--o{ experiecia : "detalle"
    docente ||--o{ docente_departamento : "puente"
    programa ||--o{ docente_departamento : "puente"
    docente ||--o{ intereses_futuros : "puente"
    termino_clave ||--o{ intereses_futuros : "puente"
    red ||--o{ red_docente : "puente"
    docente ||--o{ red_docente : "puente"
    estudios_realizados ||--o{ estudio_ac : "puente"
    area_conocimiento ||--o{ estudio_ac : "puente"
    estudios_realizados ||--o| apoyo_profesoral : "1 a 0..1"
    estudios_realizados ||--o| beca : "1 a 0..1"

    docente {
        int cedula PK
        varchar nombres
        varchar apellidos
        int linea_investigacion_principal FK "NULL permitido"
        int total_estudios "LO ESCRIBE EL DISPARADOR"
        bit activo "DEFAULT 1"
    }
    estudios_realizados {
        int id PK "lo manda la API, no es IDENTITY"
        varchar titulo
        int docente FK "NOT NULL"
        bit activo "DEFAULT 1"
    }
    red_docente {
        int red PK_FK
        int docente PK_FK
        date fecha_inicio
        bit activo "DEFAULT 1"
    }
    estudio_ac {
        int estudio PK_FK
        varchar area_conocimiento PK_FK
        bit activo
    }
    beca {
        int estudios PK_FK "1 a 1"
        varchar tipo
        bit activo
    }
```

**Cómo se lee:** `||--o{` es «uno a muchos»; `||--o|` es «uno a cero-o-uno»
(la extensión 1 a 1). Las tablas con **dos** columnas marcadas `PK_FK` son
las **puente**: su llave primaria es la pareja.

## 2. Las cuatro clases de tabla, y cómo se tratan

| Clase | Tablas | PK | En la interfaz |
|---|---|---|---|
| **Maestro con FK opcional** | `docente` | `cedula` (la manda el usuario) | Pantalla propia + desplegable «(ninguna)» para la línea |
| **Detalle** | `estudios_realizados`, `evaluacion_docente`, `reconocimiento`, `experiecia` | `id` | **Dentro de la ficha del docente** |
| **Puente** | `docente_departamento`, `intereses_futuros`, `red_docente`, `estudio_ac` | pareja compuesta | **Asignar / retirar**, sin editar |
| **Extensión 1 a 1** | `apoyo_profesoral`, `beca` | `estudios` (es PK y FK a la vez) | Panel dentro de la ficha del estudio |

## 3. Quién escribe cada columna — y esto es lo que se pregunta

| Columna | La escribe | Consecuencia |
|---|---|---|
| `activo` | **La base** (`DEFAULT 1` al crear; `sp_eliminar_*` al retirar) | **No viaja en el JSON de respuesta** ni se puede mandar en el cuerpo |
| `docente.total_estudios` | **El disparador** `trg_contar_estudios` | Si la API lo mandara, el disparador no estaría demostrando nada (criterio 4) |
| `evaluacion_docente.id`, `reconocimiento.id`, `experiecia.id` | **La base** (`IDENTITY(1,1)`) | No se manda al crear; el procedimiento lo devuelve con `SCOPE_IDENTITY()` |
| `docente.cedula`, `estudios_realizados.id` | **La API** (vienen del formulario) | Una cédula repetida la rechaza la PK → **409** |
| Todo lo demás | La API | — |

## 4. Las claves foráneas y de dónde salen sus desplegables

| Tabla | Columna FK | Obligatoria | Desplegable se llena con | Muestra |
|---|---|---|---|---|
| `docente` | `linea_investigacion_principal` | **No** | `GET /api/linea-investigacion` | nombre de la línea |
| `estudios_realizados` | `docente` | Sí | fijado por la ruta `/api/docente/{cedula}/estudios` | — |
| `evaluacion_docente` | `docente` | Sí | fijado por la ruta | — |
| `reconocimiento` | `docente` | Sí | fijado por la ruta | — |
| `experiecia` | `docente` | Sí | fijado por la ruta | — |
| `docente_departamento` | `docente` + `departamento` | Sí | `GET /api/docente` · `GET /api/programa` | nombres |
| `intereses_futuros` | `docente` + `termino_clave` | Sí | `GET /api/docente` · `GET /api/termino-clave` | nombres |
| `red_docente` | `red` + `docente` | Sí | `GET /api/red` · `GET /api/docente` | nombres |
| `estudio_ac` | `estudio` + `area_conocimiento` | Sí | ruta + `GET /api/area-conocimiento` | nombre del área |
| `apoyo_profesoral` | `estudios` | Sí | fijado por la ruta | — |
| `beca` | `estudios` | Sí | fijado por la ruta | — |

> **La regla:** cuando la FK está en la **ruta**, no hay desplegable — ya se
> sabe de qué padre se trata. Cuando está en el **cuerpo**, hay desplegable,
> **siempre**, y solo con padres **activos**.

## 5. Columna nueva que agrega la v2

```sql
ALTER TABLE docente ADD total_estudios INT NOT NULL DEFAULT 0;
```

Es la única modificación al esquema dado. Todo lo demás son procedimientos y
un disparador: **objetos nuevos, no cambios a las tablas**.

## 6. Lo que NO está en este modelo, a propósito

`usuario`, `rol` y `rol_usuario` **existen en la base desde la v1**
(Artículo 5 de la constitución), y **no se nombran en el código de la v2**:
son de la v3. Que una tabla exista no autoriza a usarla.
