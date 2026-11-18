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

IF OBJECT_ID(N'dbo.Catalogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Catalogs (
        Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Catalogs PRIMARY KEY,
        Name        NVARCHAR(200)    NOT NULL,
        Description NVARCHAR(500)    NULL);
END

GO

IF OBJECT_ID(N'dbo.CatalogProducts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CatalogProducts (
        CatalogId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_CatalogProducts_Catalogs REFERENCES dbo.Catalogs (Id) ON DELETE CASCADE,
        ProductId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_CatalogProducts_Products REFERENCES dbo.Products (Id),
        CONSTRAINT PK_CatalogProducts PRIMARY KEY (CatalogId, ProductId));
END

GO
