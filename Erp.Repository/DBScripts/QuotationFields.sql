IF COL_LENGTH('dbo.Quotation_Master', 'PrintQuality') IS NULL
    ALTER TABLE dbo.Quotation_Master ADD PrintQuality VARCHAR(100) NULL;
GO

IF COL_LENGTH('dbo.Quotation_Master', 'Grade') IS NULL
    ALTER TABLE dbo.Quotation_Master ADD Grade VARCHAR(100) NULL;
GO

IF COL_LENGTH('dbo.Quotation_Master', 'NoofColours') IS NULL
    ALTER TABLE dbo.Quotation_Master ADD NoofColours NUMERIC(15, 2) NULL;
GO
