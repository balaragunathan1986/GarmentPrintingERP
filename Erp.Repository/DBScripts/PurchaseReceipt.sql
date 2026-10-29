IF NOT EXISTS
(
    SELECT 1
    FROM sys.objects
    WHERE name = 'PurchaseReceipt_Master'
      AND schema_id = SCHEMA_ID('dbo')
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[PurchaseReceipt_Master]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PurchaseReceipt_Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Master_PurchaseReceipt_Id] DEFAULT (NEWID()),
        [Receipt_No] VARCHAR(30) NOT NULL,
        [Receipt_Date] DATETIME2 NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Master_Receipt_Date] DEFAULT (SYSDATETIME()),
        [Company_Id] UNIQUEIDENTIFIER NOT NULL,
        [Supplier_Id] UNIQUEIDENTIFIER NOT NULL,
        [DC_No] VARCHAR(50) NULL,
        [DC_Date] DATE NULL,
        [Supplier_Invoice_No] VARCHAR(50) NULL,
        [Supplier_Invoice_Date] DATE NULL,
        [Remarks] NVARCHAR(1000) NULL,
        [IsApproved] BIT NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Master_IsApproved] DEFAULT (0),
        [IsCancelled] BIT NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Master_IsCancelled] DEFAULT (0),
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Master_CreatedAt] DEFAULT (SYSDATETIME()),
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        [DeletedAt] DATETIME2 NULL,

        CONSTRAINT [PK_PurchaseReceipt_Master] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_PurchaseReceipt_Master_Id] UNIQUE ([PurchaseReceipt_Id]),
        CONSTRAINT [UQ_PurchaseReceipt_Master_ReceiptNo]
            UNIQUE ([Company_Id], [Receipt_No]),
        CONSTRAINT [FK_PurchaseReceipt_Master_Company]
            FOREIGN KEY ([Company_Id]) REFERENCES [dbo].[Company] ([Company_Id]),
        CONSTRAINT [FK_PurchaseReceipt_Master_Supplier]
            FOREIGN KEY ([Supplier_Id]) REFERENCES [dbo].[Supplier] ([Supplier_Id])
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.objects
    WHERE name = 'StockDetail'
      AND schema_id = SCHEMA_ID('dbo')
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[StockDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Stock_Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [DF_StockDetail_Stock_Id] DEFAULT (NEWID()),
        [StockNo] VARCHAR(15) NOT NULL,
        [ItemId] UNIQUEIDENTIFIER NULL,
        [ColorId] UNIQUEIDENTIFIER NULL,
        [SizeId] UNIQUEIDENTIFIER NULL,
        [Qty] NUMERIC(14,3) NOT NULL
            CONSTRAINT [DF_StockDetail_Qty] DEFAULT (0),
        [Rate] NUMERIC(14,4) NULL,
        [AllotedQty] NUMERIC(14,3) NULL,
        [Product_Id] UNIQUEIDENTIFIER NULL,
        [SQty] NUMERIC(12,3) NULL,
        [BalQty] NUMERIC(14,3) NULL,
        [StockDate] DATETIME NULL,
        [Company_Id] UNIQUEIDENTIFIER NULL,
        [ReturnQty] NUMERIC(14,3) NULL,
        [Uom_Id] UNIQUEIDENTIFIER NULL,
        [processId] UNIQUEIDENTIFIER NULL,
        [supplierid] UNIQUEIDENTIFIER NULL,
        [Remarks] VARCHAR(1000) NULL,
        [MarkupRate] NUMERIC(14,4) NOT NULL
            CONSTRAINT [DF_StockDetail_MarkupRate] DEFAULT (0),
        [ItemCode] VARCHAR(30) NOT NULL
            CONSTRAINT [DF_StockDetail_ItemCode] DEFAULT (''),
        [IsActive] BIT NOT NULL
            CONSTRAINT [DF_StockDetail_IsActive] DEFAULT (1),
        [IsDeleted] BIT NOT NULL
            CONSTRAINT [DF_StockDetail_IsDeleted] DEFAULT (0),
        [CreatedBy] VARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME NOT NULL
            CONSTRAINT [DF_StockDetail_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedBy] VARCHAR(MAX) NULL,
        [UpdatedAt] DATETIME NULL,
        [DeletedBy] VARCHAR(MAX) NULL,
        [DeletedAt] DATETIME NULL,

        CONSTRAINT [PK_StockDetail] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_StockDetail_Stock_Id] UNIQUE ([Stock_Id]),
        CONSTRAINT [FK_StockDetail_Company]
            FOREIGN KEY ([Company_Id]) REFERENCES [dbo].[Company] ([Company_Id]),
        CONSTRAINT [FK_StockDetail_Supplier]
            FOREIGN KEY ([supplierid]) REFERENCES [dbo].[Supplier] ([Supplier_Id]),
        CONSTRAINT [FK_StockDetail_Uom]
            FOREIGN KEY ([Uom_Id])
            REFERENCES [dbo].[Unit_Of_Measurement] ([Uom_Id]),
        CONSTRAINT [FK_StockDetail_Item]
            FOREIGN KEY ([ItemId]) REFERENCES [dbo].[Item] ([Item_Id]),
        CONSTRAINT [FK_StockDetail_Color]
            FOREIGN KEY ([ColorId]) REFERENCES [dbo].[Color] ([Color_Id]),
        CONSTRAINT [FK_StockDetail_Size]
            FOREIGN KEY ([SizeId]) REFERENCES [dbo].[Size] ([Size_Id]),
        CONSTRAINT [CK_StockDetail_AllotedQty]
            CHECK ([AllotedQty] IS NULL OR [AllotedQty] >= 0),
        CONSTRAINT [CK_StockDetail_BalQty]
            CHECK ([BalQty] IS NULL OR [BalQty] >= 0),
        CONSTRAINT [CK_StockDetail_StockNo] CHECK ([StockNo] <> ''),
        CONSTRAINT [CK_StockDetail_Qty] CHECK ([Qty] >= 0)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.objects
    WHERE name = 'PurchaseReceipt_Detail'
      AND schema_id = SCHEMA_ID('dbo')
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[PurchaseReceipt_Detail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PurchaseReceipt_Detail_Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Detail_Id] DEFAULT (NEWID()),
        [PurchaseReceipt_Id] UNIQUEIDENTIFIER NOT NULL,
        [Purchase_Detail_Id] UNIQUEIDENTIFIER NOT NULL,
        [Received_Qty] NUMERIC(15,3) NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Detail_Received_Qty] DEFAULT (0),
        [Accepted_Qty] NUMERIC(15,3) NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Detail_Accepted_Qty] DEFAULT (0),
        [Rejected_Qty] NUMERIC(15,3) NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Detail_Rejected_Qty] DEFAULT (0),
        [Shortage_Qty] NUMERIC(15,3) NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Detail_Shortage_Qty] DEFAULT (0),
        [Rate] NUMERIC(15,5) NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Detail_Rate] DEFAULT (0),
        [Remarks] NVARCHAR(500) NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL
            CONSTRAINT [DF_PurchaseReceipt_Detail_CreatedAt] DEFAULT (SYSDATETIME()),
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        [DeletedAt] DATETIME2 NULL,

        CONSTRAINT [PK_PurchaseReceipt_Detail] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_PurchaseReceipt_Detail_Id] UNIQUE ([PurchaseReceipt_Detail_Id]),
        CONSTRAINT [FK_PurchaseReceipt_Detail_Master]
            FOREIGN KEY ([PurchaseReceipt_Id])
            REFERENCES [dbo].[PurchaseReceipt_Master] ([PurchaseReceipt_Id]),
        CONSTRAINT [FK_PurchaseReceipt_Detail_PurchaseDetail]
            FOREIGN KEY ([Purchase_Detail_Id])
            REFERENCES [dbo].[Purchase_Detail] ([Purchase_Detail_Id]),
        CONSTRAINT [CK_PurchaseReceipt_Detail_Quantities]
            CHECK
            (
                [Received_Qty] >= 0
                AND [Accepted_Qty] >= 0
                AND [Rejected_Qty] >= 0
                AND [Shortage_Qty] >= 0
                AND [Accepted_Qty] + [Rejected_Qty] <= [Received_Qty]
            ),
        CONSTRAINT [CK_PurchaseReceipt_Detail_Rate] CHECK ([Rate] >= 0)
    );
END;
GO
