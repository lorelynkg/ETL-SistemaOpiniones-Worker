USE SistemaOpinionesDW;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @FilasInsertadas INT;

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Cargar clientes
    INSERT INTO dbo.DimClientes (IdCliente, Nombre, Email)
    SELECT C.IdCliente, C.Nombre, C.Email
    FROM SistemaOpiniones.dbo.Clientes C
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.DimClientes D
        WHERE D.IdCliente = C.IdCliente
    );

    SET @FilasInsertadas = @@ROWCOUNT;
    PRINT 'Clientes cargados: ' + CAST(@FilasInsertadas AS VARCHAR(10));

    -- 2. Cargar productos
    INSERT INTO dbo.DimProductos (IdProducto, Nombre, Categoria)
    SELECT P.IdProducto, P.Nombre, P.Categoria
    FROM SistemaOpiniones.dbo.Productos P
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.DimProductos D
        WHERE D.IdProducto = P.IdProducto
    );

    SET @FilasInsertadas = @@ROWCOUNT;
    PRINT 'Productos cargados: ' + CAST(@FilasInsertadas AS VARCHAR(10));

    -- 3. Cargar fuentes
    INSERT INTO dbo.DimFuentes (IdFuente, TipoFuente)
    SELECT F.IdFuente, F.TipoFuente
    FROM SistemaOpiniones.dbo.FuenteDatos F
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.DimFuentes D
        WHERE D.IdFuente = F.IdFuente
    );

    SET @FilasInsertadas = @@ROWCOUNT;
    PRINT 'Fuentes cargadas: ' + CAST(@FilasInsertadas AS VARCHAR(10));

    -- 4. Cargar fechas
    INSERT INTO dbo.DimFecha
        (IdFecha, Fecha, Dia, Mes, Trimestre, Anio)
    SELECT DISTINCT
        CONVERT(INT, CONVERT(CHAR(8), CAST(O.Fecha AS DATE), 112)),
        CAST(O.Fecha AS DATE),
        DAY(O.Fecha),
        MONTH(O.Fecha),
        DATEPART(QUARTER, O.Fecha),
        YEAR(O.Fecha)
    FROM SistemaOpiniones.dbo.Opiniones O
    WHERE O.Fecha IS NOT NULL
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.DimFecha D
          WHERE D.Fecha = CAST(O.Fecha AS DATE)
      );

    SET @FilasInsertadas = @@ROWCOUNT;
    PRINT 'Fechas cargadas: ' + CAST(@FilasInsertadas AS VARCHAR(10));

    COMMIT TRANSACTION;

    PRINT 'Proceso de carga finalizado correctamente.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Error durante la carga: ' + ERROR_MESSAGE();
    THROW;

END CATCH;
GO
