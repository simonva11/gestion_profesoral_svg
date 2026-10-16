# Diagramas de secuencia — el flujo de la información

> Las cajas de estos diagramas son **clases reales del repositorio**, y los
> nombres son los que verá al abrir los archivos. Si un diagrama dijera
> «servicio → repositorio» y en el código el controlador hablara con la base,
> el diagrama no describiría el sistema: lo adornaría.
>
> Cada historia de usuario tiene el suyo en
> [`../spec_kit/0_historias_de_usuario.md`](../spec_kit/0_historias_de_usuario.md);
> los de **error** están en
> [`../spec_kit/versiones/v2_docente/6_contracts.md`](../spec_kit/versiones/v2_docente/6_contracts.md).
> Aquí están los tres que hay que saber contar de memoria.

## Los participantes, y en qué proyecto vive cada uno

| Caja | Proyecto | Archivo |
|---|---|---|
| `DocenteController` | **Api** | `src/GestionProfesoral.Api/Controllers/DocenteController.cs` |
| `ServicioDocente` | **Application** | `src/GestionProfesoral.Application/Servicios/ServicioDocente.cs` |
| `RepositorioDocenteSqlServer` | **Infrastructure** | `src/GestionProfesoral.Infrastructure/Persistencia/` |
| `sp_*` y `trg_contar_estudios` | **base de datos** | `db/gestion_profesoral.sql` |

---

## 1. El camino feliz — crear un docente

```mermaid
sequenceDiagram
    actor C as Coordinador
    participant F as Frontend
    participant CT as Api · DocenteController
    participant SV as Application · ServicioDocente
    participant RP as Infrastructure · RepositorioDocenteSqlServer
    participant DB as SQL Server

    C->>F: llena el formulario y elige la linea del desplegable
    F->>CT: POST /api/docente {..., lineaInvestigacionPrincipal: 1}
    Note over CT: [ApiController] ya valido DocenteCrear.<br/>Si faltara un campo, esto no se ejecutaria: 422
    CT->>CT: DocenteCrear (Api) -> Docente (Domain)
    CT->>SV: Crear(docente)
    SV->>RP: Crear(docente)
    RP->>DB: EXEC sp_crear_docente @Cedula, @Nombres, ...
    DB->>DB: INSERT + DEFAULT activo = 1
    DB-->>RP: SELECT de la fila COMO QUEDO GUARDADA
    RP-->>SV: Docente
    SV-->>CT: Docente
    CT-->>F: 200 {estado, mensaje, datos}
    F-->>C: «El docente quedó registrado»
```

**Cómo se lee:** la petición baja por las tres capas y la respuesta sube por
las mismas. Fíjese en que **el controlador traduce dos veces**: el cuerpo HTTP
a entidad al bajar, y la entidad a JSON al subir. Ni el servicio ni el
repositorio conocen el formato del JSON.

---

## 2. El 409 de integridad — el que nace en el motor

```mermaid
sequenceDiagram
    participant F as Frontend
    participant CT as Api · DocenteController
    participant SV as Application · ServicioDocente
    participant RP as Infrastructure · Repositorio
    participant TR as Infrastructure · TraductorErroresSql
    participant DB as SQL Server

    F->>CT: POST /api/docente {lineaInvestigacionPrincipal: 987654}
    Note over CT,SV: la FORMA del cuerpo es correcta: no es un 422
    CT->>SV: Crear(docente)
    SV->>RP: Crear(docente)
    RP->>DB: EXEC sp_crear_docente
    DB-->>RP: SqlException numero 547 — FOREIGN KEY
    RP->>TR: Traducir(ex, "docente")
    Note over TR: SIN ESTE PASO sale un 500.<br/>Y vive en Infrastructure porque es la unica<br/>capa que conoce SqlException
    TR-->>RP: ConflictoExcepcion(mensaje en castellano)
    RP-->>SV: ConflictoExcepcion
    SV-->>CT: ConflictoExcepcion
    CT-->>F: 409 {estado: 409, mensaje: "..."}
    Note over F: la pantalla lo muestra tal cual.<br/>No dice «FK», ni «547», ni «constraint»
```

**Cómo se lee:** el 409 **no lo inventa la API**: nace de un error del motor
que alguien tiene que traducir, y ese alguien es la capa de afuera. Hacia
adentro solo sube una excepción de dominio.

---

## 3. El 409 de regla de negocio — el que la base NO puede defender

```mermaid
sequenceDiagram
    participant F as Frontend
    participant CT as Api · DocenteController
    participant SV as Application · ServicioDocente
    participant RP as Infrastructure · Repositorio
    participant DB as SQL Server

    F->>CT: DELETE /api/docente/1017245
    CT->>SV: Eliminar(1017245)
    SV->>RP: ContarDetalleActivo(1017245)
    RP->>DB: EXEC sp_contar_detalle_docente
    DB-->>RP: 2
    RP-->>SV: 2
    Note over SV: La base NO habria dicho nada:<br/>retirar es UPDATE activo = 0,<br/>y un UPDATE no viola ninguna clave foranea.<br/>Por eso la regla vive en Application (decision D2)
    SV-->>CT: ConflictoExcepcion("...tiene 2 registros activos")
    CT-->>F: 409
```

**Cómo se lee:** compárelo con el anterior. **Los dos responden 409 y nacen en
capas distintas**: uno lo descubre el motor, el otro lo decide el negocio.
Saber distinguirlos es la pregunta más probable de la sustentación.

---

## 4. La regla de dependencias que sostiene los tres

```mermaid
flowchart LR
    API["<b>Api</b><br/>Controllers · Peticiones<br/>Program.cs"]
    APP["<b>Application</b><br/>Servicios · Contratos"]
    DOM["<b>Domain</b><br/>Entidades · Excepciones<br/>Contratos de repositorio"]
    INF["<b>Infrastructure</b><br/>Repositorios SQL Server<br/>TraductorErroresSql"]
    DB[(SQL Server)]

    API -->|referencia| APP
    APP -->|referencia| DOM
    INF -->|implementa lo que declara| DOM
    API -.->|"SOLO en Program.cs,<br/>para registrar"| INF
    INF --> DB

    style DOM fill:#e8f0fe
```

**Cómo se lee:** todas las flechas apuntan **hacia adentro**, al dominio, que
no referencia a nadie. La punteada es la única excepción y está confinada a
`Program.cs`. Esto **no es un acuerdo**: son referencias de proyecto, y el
compilador las hace cumplir — `Application` no puede nombrar a SQL Server
aunque alguien lo intente, porque no lo referencia.
