IF COL_LENGTH('dbo.Invoice_Master', 'Quotation_Id') IS NULL
    ALTER TABLE dbo.Invoice_Master ADD Quotation_Id UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH('dbo.Invoice_Master', 'Invoice_Source') IS NULL
    ALTER TABLE dbo.Invoice_Master ADD Invoice_Source VARCHAR(20) NOT NULL
        CONSTRAINT DF_Invoice_Master_Source DEFAULT 'Manual';
GO

IF COL_LENGTH('dbo.Quotation_Master', 'Validity_Days') IS NULL
    ALTER TABLE dbo.Quotation_Master ADD Validity_Days INT NOT NULL
        CONSTRAINT DF_Quotation_Master_Validity_Days DEFAULT 30;
GO

IF COL_LENGTH('dbo.Quotation_Master', 'Valid_Until') IS NULL
    ALTER TABLE dbo.Quotation_Master ADD Valid_Until DATE NULL;
GO

IF COL_LENGTH('dbo.Quotation_Master', 'Conversion_Status') IS NULL
    ALTER TABLE dbo.Quotation_Master ADD Conversion_Status VARCHAR(20) NOT NULL
        CONSTRAINT DF_Quotation_Master_Conversion_Status DEFAULT 'NotConverted';
GO

IF COL_LENGTH('dbo.Quotation_Master', 'Converted_Invoice_Id') IS NULL
    ALTER TABLE dbo.Quotation_Master ADD Converted_Invoice_Id UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH('dbo.Quotation_Master', 'Converted_Date') IS NULL
    ALTER TABLE dbo.Quotation_Master ADD Converted_Date DATETIME NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Invoice_Detail')
           AND name = 'Outward_ID' AND is_nullable = 0)
    ALTER TABLE dbo.Invoice_Detail ALTER COLUMN Outward_ID UNIQUEIDENTIFIER NULL;
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Invoice_Detail')
           AND name = 'OutwardDetail_Id' AND is_nullable = 0)
    ALTER TABLE dbo.Invoice_Detail ALTER COLUMN OutwardDetail_Id UNIQUEIDENTIFIER NULL;
GO
