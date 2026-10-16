-- =====================================================================
--  Procedimiento del catálogo que llena el desplegable del docente
--
--  `linea_investigacion` es una tabla de la V1 (no tiene clave foránea), así
--  que su CRUD completo es de esa versión. Lo que la fase 1 de la v2 necesita
--  es el LISTADO, porque es de donde salen las opciones del desplegable.
--
--  Y filtra activo = 1: ofrecer una línea retirada sería ofrecer una opción
--  que la base va a rechazar con un 409 (criterio 5).
-- =====================================================================
USE gestion_local;
GO

CREATE OR ALTER PROCEDURE sp_listar_linea_investigacion
    @Limite INT = 1000
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Limite)
        id AS Id,
        nombre AS Nombre,
        descripcion AS Descripcion
    FROM linea_investigacion
    WHERE activo = 1
    ORDER BY nombre;
END;
GO
