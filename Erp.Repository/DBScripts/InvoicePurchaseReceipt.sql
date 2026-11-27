IF COL_LENGTH('dbo.Invoice_Detail', 'PurchaseReceipt_Detail_Id') IS NULL
    ALTER TABLE dbo.Invoice_Detail ADD PurchaseReceipt_Detail_Id UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH('dbo.Invoice_Detail', 'PurchaseReceipt_Id') IS NULL
    ALTER TABLE dbo.Invoice_Detail ADD PurchaseReceipt_Id UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH('dbo.Invoice_Master', 'InvoiceType') IS NULL
    ALTER TABLE dbo.Invoice_Master ADD InvoiceType VARCHAR(1) NULL;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Invoice_Detail')
      AND name = 'Outward_ID'
      AND is_nullable = 0
)
    ALTER TABLE dbo.Invoice_Detail ALTER COLUMN Outward_ID UNIQUEIDENTIFIER NULL;
GO

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Invoice_Detail')
      AND name = 'OutwardDetail_Id'
      AND is_nullable = 0
)
    ALTER TABLE dbo.Invoice_Detail ALTER COLUMN OutwardDetail_Id UNIQUEIDENTIFIER NULL;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Invoice_Detail_PurchaseReceipt_Detail'
      AND parent_object_id = OBJECT_ID('dbo.Invoice_Detail')
)
    ALTER TABLE dbo.Invoice_Detail WITH CHECK
        ADD CONSTRAINT FK_Invoice_Detail_PurchaseReceipt_Detail
        FOREIGN KEY (PurchaseReceipt_Detail_Id)
        REFERENCES dbo.PurchaseReceipt_Detail (PurchaseReceipt_Detail_Id);
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Invoice_Detail_PurchaseReceipt_Master'
      AND parent_object_id = OBJECT_ID('dbo.Invoice_Detail')
)
    ALTER TABLE dbo.Invoice_Detail WITH CHECK
        ADD CONSTRAINT FK_Invoice_Detail_PurchaseReceipt_Master
        FOREIGN KEY (PurchaseReceipt_Id)
        REFERENCES dbo.PurchaseReceipt_Master (PurchaseReceipt_Id);
GO

UPDATE detail
SET Outward_ID = NULL,
    OutwardDetail_Id = NULL
FROM dbo.Invoice_Detail AS detail
INNER JOIN dbo.Invoice_Master AS invoice
    ON invoice.Invoice_Id = detail.Invoice_Id
WHERE invoice.InvoiceType = 'G'
  AND (detail.Outward_ID IS NOT NULL OR detail.OutwardDetail_Id IS NOT NULL);
GO
