-- =====================================================================
--  Semillas mínimas de `linea_investigacion`
--
--  POR QUÉ ESTÁN AQUÍ: el desplegable de la clave foránea del docente se
--  llena con esta tabla. Vacía, el <select> solo ofrecería «(ninguna)» y el
--  criterio 5 no se podría ni mirar.
--
--  SON DE EJEMPLO. El equipo las reemplaza por las del Excel de referencia
--  (`ProyectosDeAula/Mapa_conocimiento/`), que es la fuente de verdad de los
--  catálogos.
--
--  Es idempotente: el NOT EXISTS evita duplicar si el script se vuelve a
--  correr sobre una base que ya las tiene.
-- =====================================================================
USE gestion_local;
GO

IF NOT EXISTS (SELECT 1 FROM linea_investigacion)
BEGIN
    INSERT INTO linea_investigacion (nombre, descripcion) VALUES
        ('Ingenieria de Software',
         'Metodos, procesos y herramientas para construir software con calidad.'),
        ('Ciencia de Datos',
         'Analitica, aprendizaje automatico y explotacion de grandes volumenes de datos.'),
        ('Redes y Telecomunicaciones',
         'Infraestructura de comunicaciones, protocolos y seguridad de red.'),
        ('Educacion y TIC',
         'Uso de la tecnologia en procesos de ensenanza y aprendizaje.');
END;
GO
