-- =====================================================================
--  VERSIÓN 2 — Módulo Gestión Profesoral
--  Procedimientos almacenados y disparador
--
--  Se agrega AL FINAL de db/gestion_profesoral.sql, después de las tablas.
--  Los objetos son CREATE OR ALTER: el script se puede volver a correr.
--
--  LA REGLA DE ESTA VERSIÓN: el SQL vive AQUÍ, con nombre y versionado.
--  En los repositorios de C# solo queda la LLAMADA, con sus parámetros.
-- =====================================================================
USE gestion_local;
GO

-- ---------------------------------------------------------------------
-- 0. La única modificación al esquema: la columna que mantiene el disparador
-- ---------------------------------------------------------------------
IF COL_LENGTH('docente', 'total_estudios') IS NULL
    ALTER TABLE docente ADD total_estudios INT NOT NULL DEFAULT 0;
GO

-- =====================================================================
--  1. docente — los cinco verbos
-- =====================================================================

-- LISTAR -------------------------------------------------------------
-- El WHERE activo = 1 vive aquí y no en C#: el borrado es lógico, así que
-- un listado sin este filtro mostraría lo retirado (criterio 10).
-- Los alias traducen snake_case a PascalCase para que Dapper mapee solo.
CREATE OR ALTER PROCEDURE sp_listar_docente
    @Limite INT = 1000
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Limite)
        cedula AS Cedula, nombres AS Nombres, apellidos AS Apellidos,
        genero AS Genero, cargo AS Cargo, fecha_nacimiento AS FechaNacimiento,
        correo AS Correo, telefono AS Telefono, url_cvlac AS UrlCvlac,
        fecha_actualizacion AS FechaActualizacion, escalafon AS Escalafon,
        perfil AS Perfil, cat_minciencia AS CatMinciencia,
        conv_minciencia AS ConvMinciencia, nacionalidaad AS Nacionalidaad,
        linea_investigacion_principal AS LineaInvestigacionPrincipal,
        total_estudios AS TotalEstudios
    FROM docente
    WHERE activo = 1
    ORDER BY apellidos, nombres;
END;
GO

-- CONSULTAR ----------------------------------------------------------
-- Un docente inactivo responde IGUAL que uno inexistente: 0 filas, y el
-- servicio lo traduce a 404. Quien retiró un registro no tiene por qué
-- poder confirmar que existió.
CREATE OR ALTER PROCEDURE sp_consultar_docente
    @Cedula INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        cedula AS Cedula, nombres AS Nombres, apellidos AS Apellidos,
        genero AS Genero, cargo AS Cargo, fecha_nacimiento AS FechaNacimiento,
        correo AS Correo, telefono AS Telefono, url_cvlac AS UrlCvlac,
        fecha_actualizacion AS FechaActualizacion, escalafon AS Escalafon,
        perfil AS Perfil, cat_minciencia AS CatMinciencia,
        conv_minciencia AS ConvMinciencia, nacionalidaad AS Nacionalidaad,
        linea_investigacion_principal AS LineaInvestigacionPrincipal,
        total_estudios AS TotalEstudios
    FROM docente
    WHERE cedula = @Cedula AND activo = 1;
END;
GO

-- CREAR --------------------------------------------------------------
-- Termina con un SELECT de la fila COMO QUEDÓ GUARDADA, no como la mandó
-- el formulario: así la respuesta trae el activo = 1 que puso el DEFAULT y
-- el total_estudios que mantiene el disparador.
-- @LineaInvestigacionPrincipal admite NULL a propósito: es la FK opcional
-- que en el front corresponde a la opción «(ninguna)».
CREATE OR ALTER PROCEDURE sp_crear_docente
    @Cedula INT, @Nombres VARCHAR(60), @Apellidos VARCHAR(60),
    @Genero VARCHAR(12), @Cargo VARCHAR(30), @FechaNacimiento DATE,
    @Correo VARCHAR(70), @Telefono VARCHAR(20), @UrlCvlac VARCHAR(128),
    @FechaActualizacion DATE, @Escalafon VARCHAR(45), @Perfil VARCHAR(MAX),
    @CatMinciencia VARCHAR(45) = NULL, @ConvMinciencia VARCHAR(45),
    @Nacionalidaad VARCHAR(45),
    @LineaInvestigacionPrincipal INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO docente (cedula, nombres, apellidos, genero, cargo,
        fecha_nacimiento, correo, telefono, url_cvlac, fecha_actualizacion,
        escalafon, perfil, cat_minciencia, conv_minciencia, nacionalidaad,
        linea_investigacion_principal)
    VALUES (@Cedula, @Nombres, @Apellidos, @Genero, @Cargo,
        @FechaNacimiento, @Correo, @Telefono, @UrlCvlac, @FechaActualizacion,
        @Escalafon, @Perfil, @CatMinciencia, @ConvMinciencia, @Nacionalidaad,
        @LineaInvestigacionPrincipal);

    EXEC sp_consultar_docente @Cedula;
END;
GO

-- ACTUALIZAR ---------------------------------------------------------
-- Parámetros con DEFAULT NULL + COALESCE: lo que no se manda, no se cambia.
-- Así UN procedimiento sirve para el PUT y para el PATCH.
--
-- OJO con la trampa: con este patrón no se puede poner una columna EN NULL
-- a propósito (mandar NULL significa «no lo cambies»). Para la FK opcional
-- va @LimpiarLinea, que es la forma explícita de decir «quítale la línea».
CREATE OR ALTER PROCEDURE sp_actualizar_docente
    @Cedula INT,
    @Nombres VARCHAR(60) = NULL, @Apellidos VARCHAR(60) = NULL,
    @Genero VARCHAR(12) = NULL, @Cargo VARCHAR(30) = NULL,
    @FechaNacimiento DATE = NULL, @Correo VARCHAR(70) = NULL,
    @Telefono VARCHAR(20) = NULL, @UrlCvlac VARCHAR(128) = NULL,
    @FechaActualizacion DATE = NULL, @Escalafon VARCHAR(45) = NULL,
    @Perfil VARCHAR(MAX) = NULL, @CatMinciencia VARCHAR(45) = NULL,
    @ConvMinciencia VARCHAR(45) = NULL, @Nacionalidaad VARCHAR(45) = NULL,
    @LineaInvestigacionPrincipal INT = NULL,
    @LimpiarLinea BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE docente SET
        nombres = COALESCE(@Nombres, nombres),
        apellidos = COALESCE(@Apellidos, apellidos),
        genero = COALESCE(@Genero, genero),
        cargo = COALESCE(@Cargo, cargo),
        fecha_nacimiento = COALESCE(@FechaNacimiento, fecha_nacimiento),
        correo = COALESCE(@Correo, correo),
        telefono = COALESCE(@Telefono, telefono),
        url_cvlac = COALESCE(@UrlCvlac, url_cvlac),
        fecha_actualizacion = COALESCE(@FechaActualizacion, fecha_actualizacion),
        escalafon = COALESCE(@Escalafon, escalafon),
        perfil = COALESCE(@Perfil, perfil),
        cat_minciencia = COALESCE(@CatMinciencia, cat_minciencia),
        conv_minciencia = COALESCE(@ConvMinciencia, conv_minciencia),
        nacionalidaad = COALESCE(@Nacionalidaad, nacionalidaad),
        linea_investigacion_principal =
            CASE WHEN @LimpiarLinea = 1 THEN NULL
                 ELSE COALESCE(@LineaInvestigacionPrincipal,
                               linea_investigacion_principal) END
    WHERE cedula = @Cedula AND activo = 1;

    -- Filas afectadas: 0 significa «no existe o está inactivo», y el
    -- servicio lo traduce a 404.
    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO

-- ELIMINAR (LÓGICO) --------------------------------------------------
-- UPDATE, no DELETE. El AND activo = 1 hace que retirar dos veces devuelva
-- 0 filas, y eso es un 404: no se puede retirar lo que ya estaba retirado.
--
-- Lo que este procedimiento NO hace: comprobar si el docente tiene detalle
-- activo. Esa es una REGLA DE NEGOCIO (decisión D2) y vive en
-- ServicioDocente, porque la base no la puede defender: un UPDATE no viola
-- ninguna clave foránea.
CREATE OR ALTER PROCEDURE sp_eliminar_docente
    @Cedula INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE docente SET activo = 0 WHERE cedula = @Cedula AND activo = 1;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO

-- APOYO A LA REGLA D2 ------------------------------------------------
-- El servicio llama a esto ANTES de retirar: si devuelve > 0, responde 409.
CREATE OR ALTER PROCEDURE sp_contar_detalle_docente
    @Cedula INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM estudios_realizados WHERE docente = @Cedula AND activo = 1)
      + (SELECT COUNT(*) FROM evaluacion_docente  WHERE docente = @Cedula AND activo = 1)
      + (SELECT COUNT(*) FROM reconocimiento      WHERE docente = @Cedula AND activo = 1)
      + (SELECT COUNT(*) FROM experiecia          WHERE docente = @Cedula AND activo = 1)
        AS DetalleActivo;
END;
GO

-- =====================================================================
--  2. estudios_realizados — el detalle
-- =====================================================================

-- El listado del DETALLE: por maestro. Es lo que llena la tabla de abajo
-- en la ficha del docente (criterio 9).
CREATE OR ALTER PROCEDURE sp_listar_estudios_por_docente
    @Docente INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id AS Id, titulo AS Titulo, universidad AS Universidad,
           fecha AS Fecha, tipo AS Tipo, ciudad AS Ciudad,
           docente AS Docente, ins_acreditada AS InsAcreditada,
           metodologia AS Metodologia, perfil_egresado AS PerfilEgresado,
           pais AS Pais
    FROM estudios_realizados
    WHERE docente = @Docente AND activo = 1
    ORDER BY fecha DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_consultar_estudios_realizados
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id AS Id, titulo AS Titulo, universidad AS Universidad,
           fecha AS Fecha, tipo AS Tipo, ciudad AS Ciudad,
           docente AS Docente, ins_acreditada AS InsAcreditada,
           metodologia AS Metodologia, perfil_egresado AS PerfilEgresado,
           pais AS Pais
    FROM estudios_realizados
    WHERE id = @Id AND activo = 1;
END;
GO

-- CREAR --------------------------------------------------------------
-- Si @Docente no existe, el INSERT falla con el error 547 (FOREIGN KEY).
-- Ese error NO se atrapa aquí a propósito: sube al repositorio, que lo
-- traduce a ConflictoExcepcion -> 409 en castellano (criterio 7).
-- Atraparlo aquí y devolver un código propio obligaría a inventar un
-- segundo lenguaje de errores encima del que ya tiene el motor.
CREATE OR ALTER PROCEDURE sp_crear_estudios_realizados
    @Id INT, @Titulo VARCHAR(45), @Universidad VARCHAR(50), @Fecha DATE,
    @Tipo VARCHAR(45), @Ciudad VARCHAR(45), @Docente INT,
    @InsAcreditada TINYINT, @Metodologia VARCHAR(45),
    @PerfilEgresado VARCHAR(MAX), @Pais VARCHAR(45)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO estudios_realizados (id, titulo, universidad, fecha, tipo,
        ciudad, docente, ins_acreditada, metodologia, perfil_egresado, pais)
    VALUES (@Id, @Titulo, @Universidad, @Fecha, @Tipo, @Ciudad, @Docente,
        @InsAcreditada, @Metodologia, @PerfilEgresado, @Pais);

    EXEC sp_consultar_estudios_realizados @Id;
END;
GO

CREATE OR ALTER PROCEDURE sp_actualizar_estudios_realizados
    @Id INT,
    @Titulo VARCHAR(45) = NULL, @Universidad VARCHAR(50) = NULL,
    @Fecha DATE = NULL, @Tipo VARCHAR(45) = NULL, @Ciudad VARCHAR(45) = NULL,
    @InsAcreditada TINYINT = NULL, @Metodologia VARCHAR(45) = NULL,
    @PerfilEgresado VARCHAR(MAX) = NULL, @Pais VARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE estudios_realizados SET
        titulo = COALESCE(@Titulo, titulo),
        universidad = COALESCE(@Universidad, universidad),
        fecha = COALESCE(@Fecha, fecha),
        tipo = COALESCE(@Tipo, tipo),
        ciudad = COALESCE(@Ciudad, ciudad),
        ins_acreditada = COALESCE(@InsAcreditada, ins_acreditada),
        metodologia = COALESCE(@Metodologia, metodologia),
        perfil_egresado = COALESCE(@PerfilEgresado, perfil_egresado),
        pais = COALESCE(@Pais, pais)
    WHERE id = @Id AND activo = 1;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO

-- Retirar un estudio es un UPDATE — y por eso el disparador de abajo
-- TIENE que escuchar el UPDATE, o el contador no baja nunca.
CREATE OR ALTER PROCEDURE sp_eliminar_estudios_realizados
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE estudios_realizados SET activo = 0 WHERE id = @Id AND activo = 1;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO

-- =====================================================================
--  3. EL DISPARADOR (propuesta A de VERSION_2.md §5)
-- =====================================================================
-- Mantiene docente.total_estudios sin que nadie se acuerde de actualizarlo.
--
-- TRES DECISIONES QUE HAY QUE SABER EXPLICAR:
--
-- 1) Escucha INSERT, UPDATE **y** DELETE. Con borrado lógico, retirar un
--    estudio es un UPDATE: un disparador sordo al UPDATE deja el contador
--    mal justo en el caso que nadie prueba.
--
-- 2) Cuenta solo los activos (activo = 1), para que el contador diga lo
--    mismo que muestra el listado.
--
-- 3) Recalcula para TODOS los docentes afectados (inserted UNION deleted),
--    no para uno: un UPDATE puede tocar varias filas de golpe, y si el
--    estudio cambió de docente hay DOS contadores que arreglar — el del
--    docente nuevo (inserted) y el del viejo (deleted).
--
-- POR QUÉ ESTO ESTÁ EN LA BASE Y NO EN EL SERVICIO: así vale también para
-- quien entre por SQL Server Management Studio y no pase por la API.
CREATE OR ALTER TRIGGER trg_contar_estudios
ON estudios_realizados
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE d
    SET d.total_estudios = (
        SELECT COUNT(*)
        FROM estudios_realizados e
        WHERE e.docente = d.cedula AND e.activo = 1
    )
    FROM docente d
    WHERE d.cedula IN (SELECT docente FROM inserted
                       UNION
                       SELECT docente FROM deleted);
END;
GO

-- =====================================================================
--  4. red_docente — una tabla PUENTE
-- =====================================================================
-- La llave primaria es la PAREJA. Repetirla da el error 2627, que el
-- repositorio traduce a 409 (criterio 8).
-- NO hay sp_actualizar_red_docente para la pareja: una pareja existe o no
-- existe. Lo que sí se actualiza son sus atributos propios, identificando
-- la fila por las DOS claves.
CREATE OR ALTER PROCEDURE sp_listar_docentes_por_red
    @Red INT
AS
BEGIN
    SET NOCOUNT ON;
    -- El JOIN vive aquí, que es de lo que se trata la v2: el listado del
    -- puente muestra NOMBRES, porque una pantalla con cédulas sueltas no
    -- le dice nada a nadie.
    SELECT rd.red AS Red, rd.docente AS Docente,
           d.nombres + ' ' + d.apellidos AS NombreDocente,
           rd.fecha_inicio AS FechaInicio, rd.fecha_fin AS FechaFin,
           rd.act_destacadas AS ActDestacadas
    FROM red_docente rd
    INNER JOIN docente d ON d.cedula = rd.docente
    WHERE rd.red = @Red AND rd.activo = 1 AND d.activo = 1
    ORDER BY d.apellidos;
END;
GO

CREATE OR ALTER PROCEDURE sp_crear_red_docente
    @Red INT, @Docente INT, @FechaInicio DATE,
    @FechaFin VARCHAR(45) = NULL, @ActDestacadas VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO red_docente (red, docente, fecha_inicio, fecha_fin, act_destacadas)
    VALUES (@Red, @Docente, @FechaInicio, @FechaFin, @ActDestacadas);

    SELECT red AS Red, docente AS Docente, fecha_inicio AS FechaInicio,
           fecha_fin AS FechaFin, act_destacadas AS ActDestacadas
    FROM red_docente WHERE red = @Red AND docente = @Docente;
END;
GO

CREATE OR ALTER PROCEDURE sp_eliminar_red_docente
    @Red INT, @Docente INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Las DOS claves: es lo que identifica la fila (criterio 8)
    UPDATE red_docente SET activo = 0
    WHERE red = @Red AND docente = @Docente AND activo = 1;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END;
GO

-- =====================================================================
--  5. Lo que falta por escribir (mismo molde, otros nombres)
-- =====================================================================
-- Quedan, con exactamente el mismo patrón de arriba:
--
--   docente_departamento  · intereses_futuros · estudio_ac   (puente)
--   evaluacion_docente · reconocimiento · experiecia         (detalle, IDENTITY:
--                                        el crear devuelve SCOPE_IDENTITY())
--   apoyo_profesoral · beca                                  (1 a 1)
--
-- Y la MIGRACIÓN de la v1 (fase 0 del 8_tasks.md):
--   programa · area_conocimiento · termino_clave ·
--   linea_investigacion · red
--
-- Total al cerrar la v2: 5 procedimientos × 16 tablas + los de apoyo.
-- =====================================================================
