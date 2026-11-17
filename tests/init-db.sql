-- Idempotent schema for GeekShop. Run by the `mssqltools` service in docker-compose.yml
-- (through init-db.sh) and by the integration tests, so the application never creates schema.
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GeekShop')
BEGIN
    EXEC ('CREATE DATABASE GeekShop');
END

GO

USE GeekShop
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name     NVARCHAR(200)    NOT NULL,
        Quantity BIGINT           NOT NULL,
        Price    FLOAT            NOT NULL);
END

GO
