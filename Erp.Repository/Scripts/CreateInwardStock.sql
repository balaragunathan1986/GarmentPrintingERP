IF NOT EXISTS
(
    SELECT 1
    FROM sys.objects
    WHERE name = 'Inward_Stock'
      AND schema_id = SCHEMA_ID('dbo')
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[Inward_Stock]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Inward_Stock_Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [DF_Inward_Stock_Inward_Stock_Id] DEFAULT NEWSEQUENTIALID(),
        [Inward_Item_Id] UNIQUEIDENTIFIER NOT NULL,
        [Inward_Id] UNIQUEIDENTIFIER NOT NULL,
        [Stock_Id] UNIQUEIDENTIFIER NOT NULL,
        [Quantity] NUMERIC(12,3) NOT NULL,
        [Uom_Id] UNIQUEIDENTIFIER NULL,
        [Remarks] NVARCHAR(MAX) NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAt] DATETIME NULL
            CONSTRAINT [DF_Inward_Stock_CreatedAt] DEFAULT GETDATE(),
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        [DeletedAt] DATETIME NULL,

        CONSTRAINT [PK_Inward_Stock] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Inward_Stock_Inward_Stock_Id] UNIQUE ([Inward_Stock_Id]),
        CONSTRAINT [FK_Inward_Stock_Uom] FOREIGN KEY ([Uom_Id])
            REFERENCES [dbo].[Unit_Of_Measurement] ([Uom_Id]),
        CONSTRAINT [FK_Inward_Stock_StockDetail] FOREIGN KEY ([Stock_Id])
            REFERENCES [dbo].[StockDetail] ([Stock_Id]),
        CONSTRAINT [FK_Inward_Stock_Inward_Master] FOREIGN KEY ([Inward_Id])
            REFERENCES [dbo].[Inward_Master] ([Inward_Id]),
        CONSTRAINT [FK_Inward_Stock_Inward_Item] FOREIGN KEY ([Inward_Item_Id])
            REFERENCES [dbo].[Inward_Item] ([Inward_Item_Id])
    );

    CREATE INDEX [IX_Inward_Stock_Inward_Id]
        ON [dbo].[Inward_Stock] ([Inward_Id]);

    CREATE INDEX [IX_Inward_Stock_Stock_Id]
        ON [dbo].[Inward_Stock] ([Stock_Id]);
END;
