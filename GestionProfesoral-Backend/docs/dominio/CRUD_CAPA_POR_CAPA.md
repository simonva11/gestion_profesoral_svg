# Cómo se programa un CRUD, capa por capa

> El recorrido completo de **`docente`** —el recurso que ya está terminado—
> a través de los cuatro proyectos. Es el molde: las demás tablas son esto
> mismo con otros nombres.
>
> Si tiene que explicar el proyecto en voz alta, este documento es el orden
> en que conviene contarlo: **de adentro hacia afuera**, igual que se
> construye.

---

## 0. Los cuatro proyectos, y por qué son cuatro y no cuatro carpetas

```
src/
  GestionProfesoral.Domain/           ← no referencia a NADIE
  GestionProfesoral.Application/      ← referencia a Domain
  GestionProfesoral.Infrastructure/   ← referencia a Domain
  GestionProfesoral.Api/              ← referencia a Application (y a Infrastructure solo para registrar)
tests/
  GestionProfesoral.Tests/            ← referencia a Application y Domain, NO a Infrastructure
```

Con carpetas, «el servicio no debe tocar SQL» es un **acuerdo** que alguien
rompe el día que tiene prisa. Con proyectos es una **referencia que no
existe**: `Application` no puede nombrar `SqlConnection` porque no referencia
a `Infrastructure`, y el compilador lo detiene.

---

## 1. DOMAIN — qué es un docente y qué hace falta para guardarlo

Se empieza aquí porque es lo único que no depende de nada.

### 1.1 La entidad · `Domain/Entidades/Docente.cs`

```csharp
public class Docente
{
    public int Cedula { get; set; }
    public string Nombres { get; set; } = string.Empty;
    // …
    public int? LineaInvestigacionPrincipal { get; set; }  // la FK OPCIONAL: int? y no int
    public int TotalEstudios { get; set; }                 // lo escribe el DISPARADOR
}
```

**Dos decisiones que se sustentan aquí:**
- `int?` en la clave foránea: «(ninguna)» en la pantalla viaja como `null`, que
  no es `0` ni cadena vacía.
- **No hay propiedad `Activo`**: el borrado lógico es un detalle del motor y no
  forma parte de lo que la API expone.

### 1.2 El contrato de datos · `Domain/Contratos/IRepositorioDocente.cs`

```csharp
public interface IRepositorioDocente
{
    Task<IEnumerable<Docente>> ObtenerTodos(int limite);
    Task<Docente?> ObtenerPorId(int cedula);
    Task<Docente> Crear(Docente docente);
    Task<int> Reemplazar(Docente docente);
    Task<int> ActualizarParcial(int cedula, DocenteCampos campos);
    Task<int> EliminarLogico(int cedula);
    Task<int> ContarDetalleActivo(int cedula);
}
```

> **Lo importante es dónde vive esta interfaz.** La declara el **dominio**
> —la capa de adentro— y la implementa **Infrastructure** —la de afuera—. Así
> la dependencia apunta hacia adentro: el negocio dice qué necesita, y la
> infraestructura se acomoda. Al revés, cambiar de motor obligaría a tocar el
> negocio.

También aquí: `DocenteCampos`, el `record` con los campos opcionales del
PATCH, y las excepciones del dominio (`NoEncontradoExcepcion`,
`ConflictoExcepcion`), que **no mencionan HTTP**.

---

## 2. INFRASTRUCTURE — los cinco verbos, uno por procedimiento

`Infrastructure/Persistencia/RepositorioDocenteSqlServer.cs` implementa la
interfaz anterior. **Ni una línea de SQL**: solo el nombre del procedimiento y
sus parámetros.

| Verbo | Método | Procedimiento | Detalle que importa |
|---|---|---|---|
| **Listar** | `ObtenerTodos` | `sp_listar_docente` | el `WHERE activo = 1` vive en la base, no en C# |
| **Consultar** | `ObtenerPorId` | `sp_consultar_docente` | devuelve `null` si no hay filas: un hecho, no un 404 |
| **Crear** | `Crear` | `sp_crear_docente` | devuelve **la fila como quedó guardada** |
| **Actualizar** | `Reemplazar` / `ActualizarParcial` | `sp_actualizar_docente` | un solo procedimiento con `COALESCE` sirve para PUT y PATCH |
| **Eliminar** | `EliminarLogico` | `sp_eliminar_docente` | `UPDATE activo = 0`, nunca `DELETE` |

```csharp
public async Task<IEnumerable<Docente>> ObtenerTodos(int limite)
{
    using var conexion = CrearConexion();
    return await conexion.QueryAsync<Docente>(
        "sp_listar_docente",                        // el nombre, no el SQL
        new { Limite = limite },                    // parámetros, nunca texto pegado
        commandType: CommandType.StoredProcedure);  // esta línea es el criterio 3
}
```

Y el **traductor**, que es lo que convierte un 500 en un 409:

```csharp
catch (SqlException ex)
{
    throw TraductorErroresSql.Traducir(ex, "docente");  // 547 y 2627 → ConflictoExcepcion
}
```

> Este proyecto es el **único** que referencia a Dapper y a
> `Microsoft.Data.SqlClient`. Cambiar de motor es reescribir esta carpeta y
> una línea de `Program.cs`.

---

## 3. APPLICATION — las reglas, y nada más

`Application/Servicios/ServicioDocente.cs` recibe `IRepositorioDocente` por
constructor. **No sabe que existe SQL Server.**

```csharp
public async Task<int> Eliminar(int cedula)
{
    // La base NO puede defender esta regla: retirar es UPDATE activo = 0,
    // y un UPDATE no viola ninguna clave foránea.
    var detalleActivo = await _repositorio.ContarDetalleActivo(cedula);
    if (detalleActivo > 0)
        throw new ConflictoExcepcion($"No se puede retirar el docente: todavía tiene {detalleActivo}…");

    var filas = await _repositorio.EliminarLogico(cedula);
    if (filas == 0)
        throw new NoEncontradoExcepcion($"No existe un docente con la cédula {cedula}.");

    return filas;
}
```

Las tres reglas que viven aquí y en ningún otro sitio:

| Regla | Qué pasa si no está |
|---|---|
| `limite <= 0` → `ArgumentException` | la base devolvería un error en inglés |
| PATCH sin campos → `ArgumentException` | el procedimiento afectaría 0 filas y responderíamos **404** a un docente que sí existe |
| No retirar un maestro con detalle activo | quedarían hijos activos colgando de un padre retirado, **y la base no diría nada** |

> **Fíjese en lo que NO hay aquí:** ni `IActionResult`, ni códigos de estado,
> ni `SqlException`. Por eso este archivo se prueba sin levantar un servidor
> ni una base de datos.

---

## 4. API — traducir, y solo traducir

`Api/Controllers/DocenteController.cs` hace exactamente tres cosas:

```csharp
[HttpDelete("{cedula:int}")]
public async Task<IActionResult> Eliminar(int cedula)
{
    try
    {
        var filas = await _servicio.Eliminar(cedula);                 // 2. llama al negocio
        return Ok(new { estado = 200, mensaje = "Docente retirado.", filasAfectadas = filas });
    }
    catch (ConflictoExcepcion ex)   { return Conflict(new { estado = 409, mensaje = ex.Message }); }
    catch (NoEncontradoExcepcion ex){ return NotFound(new { estado = 404, … }); }   // 3. traduce
}
```

1. Traduce el cuerpo HTTP (`Api/Peticiones/DocenteCrear.cs`) a la entidad del dominio.
2. Llama a `IServicioDocente`.
3. Traduce las excepciones del dominio a códigos HTTP.

**Las `Peticiones/` viven en Api y no bajan**, porque son la frontera HTTP: ahí
están los `[Required]` y los `[MaxLength]` que producen el **422 antes de que
el controlador se ejecute**.

Y el **ensamblador**, `Api/Program.cs`, el único archivo donde una clase
concreta aparece junto a su interfaz:

```csharp
builder.Services.AddScoped<IRepositorioDocente>(_ => new RepositorioDocenteSqlServer(cadenaSqlServer));
builder.Services.AddScoped<IServicioDocente, ServicioDocente>();
```

---

## 5. FRONTEND — un servicio por recurso, y cero SQL

`GestionProfesoral-Frontend/` es **otro proyecto y otro contenedor**. Solo
conoce el JSON:

| Archivo | Qué hace |
|---|---|
| `Servicios/ServicioDocente.cs` | la única pieza que habla HTTP; lee el sobre `{tabla, limite, total, datos}` |
| `Components/Pages/Docentes.razor` | la pantalla: tabla, formulario y el **desplegable** de la clave foránea |

```razor
<select @bind="campoLinea">
    <option value="">— (ninguna) —</option>
    @foreach (var linea in lineas)
    {
        <option value="@linea.Id">@linea.Nombre</option>   @* se LEE el nombre, VIAJA el id *@
    }
</select>
```

---

## 6. Y la prueba que demuestra que todo lo anterior es cierto

`tests/GestionProfesoral.Tests/Docente.cs` implementa `IRepositorioDocente`
con una lista en memoria y le pasa ese falso al servicio:

```csharp
var repoFalso = new RepositorioDocenteFalso();
IServicioDocente servicio = new ServicioDocente(repoFalso);
```

```bash
docker compose exec api-gestion dotnet run --project tests/GestionProfesoral.Tests/GestionProfesoral.Tests.csproj
```

Corre **con la base de datos apagada**. Y no podría ser de otra forma: el
proyecto de pruebas **no referencia a Infrastructure**.

---

## 7. Para agregar la siguiente tabla, en este orden

| # | Dónde | Qué |
|---|---|---|
| 1 | `db/gestion_profesoral.sql` | los 5 `sp_*_<tabla>` |
| 2 | `Domain/Entidades/` | la entidad |
| 3 | `Domain/Contratos/` | `IRepositorio<Tabla>` y su `record <Tabla>Campos` |
| 4 | `Infrastructure/Persistencia/` | `Repositorio<Tabla>SqlServer` con las 5 llamadas |
| 5 | `Application/Contratos/` + `Servicios/` | la interfaz y las reglas |
| 6 | `Api/Peticiones/` | `<Tabla>Crear` y `<Tabla>Actualizar` con sus validaciones |
| 7 | `Api/Controllers/` | el controlador con sus rutas |
| 8 | `Api/Program.cs` | **dos líneas** |
| 9 | `tests/` | el repositorio falso y las comprobaciones |
| 10 | `GestionProfesoral-Frontend/` | el servicio y la pantalla |

Si alguno de esos diez pasos sobra, es que la tabla no necesitaba una
rebanada; si falta, la versión no está cerrada.
