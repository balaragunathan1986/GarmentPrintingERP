IF NOT EXISTS
(
    SELECT 1
    FROM sys.objects
    WHERE name = 'Payment_Master'
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[Payment_Master]
    (
        [PaymentID] INT IDENTITY(1,1) NOT NULL,
        [Payment_Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [PaymentNo] NVARCHAR(30) NOT NULL,
        [PaymentDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [Supplier_ID] UNIQUEIDENTIFIER NOT NULL,
        [Company_Id] INT NULL,
        [PaymentMode_Id] UNIQUEIDENTIFIER NULL,
        [Account_Id] UNIQUEIDENTIFIER NULL,
        [ReferenceNo] NVARCHAR(100) NULL,
        [ReferenceDate] DATETIME NULL,
        [PaymentAmount] DECIMAL(18,3) NOT NULL DEFAULT 0,
        [DeductionAmount] DECIMAL(18,3) NOT NULL DEFAULT 0,
        [NetAmount] DECIMAL(18,3) NOT NULL DEFAULT 0,
        [Remarks] NVARCHAR(500) NULL,
        [PaymentStatus] VARCHAR(20) NOT NULL DEFAULT 'Pending',
        [CreatedBy] INT NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedBy] INT NULL,
        [UpdatedDate] DATETIME NULL,
        [DeletedBy] INT NULL,
        [DeletedDate] DATETIME NULL,
        [ApprovedBy] INT NULL,
        [ApprovedDate] DATETIME NULL,
        [ApprovalStatus] VARCHAR(20) NOT NULL DEFAULT 'Pending',

        CONSTRAINT [PK_Payment_Master1]
            PRIMARY KEY CLUSTERED ([PaymentID]),
        CONSTRAINT [UQ_Payment_Id]
            UNIQUE ([Payment_Id]),
        CONSTRAINT [UQ_Payment_No]
            UNIQUE ([PaymentNo]),
        CONSTRAINT [FK_Payment_Supplier]
            FOREIGN KEY ([Supplier_ID])
            REFERENCES [dbo].[Supplier]([Supplier_ID]),
        CONSTRAINT [CK_Payment_Amount]
            CHECK
            (
                [PaymentAmount] >= 0
                AND [DeductionAmount] >= 0
                AND [NetAmount] >= 0
            ),
        CONSTRAINT [CK_Payment_Status]
            CHECK ([PaymentStatus] IN ('Pending', 'Partially Paid', 'Paid', 'Cancelled')),
        CONSTRAINT [CK_Payment_ApprovalStatus]
            CHECK ([ApprovalStatus] IN ('Pending', 'Approved', 'Rejected', 'Cancelled'))
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.objects
    WHERE name = 'Payment_Charge'
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[Payment_Charge]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Payment_Charge_Id] UNIQUEIDENTIFIER NOT NULL,
        [Charge_Id] UNIQUEIDENTIFIER NOT NULL,
        [Payment_Id] UNIQUEIDENTIFIER NOT NULL,
        [AddLessType] CHAR(1) NOT NULL,
        [Percentage] NUMERIC(5,2) NOT NULL,
        [Amount] NUMERIC(12,2) NOT NULL,
        [Remarks] NVARCHAR(1000) NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAt] DATETIME NULL,
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        [DeletedAt] DATETIME NULL,

        CONSTRAINT [PK_Payment_Charge]
            PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Payment_Charge_Id]
            UNIQUE ([Payment_Charge_Id]),
        CONSTRAINT [FK_Payment_Charge_Payment]
            FOREIGN KEY ([Payment_Id])
            REFERENCES [dbo].[Payment_Master]([Payment_Id]),
        CONSTRAINT [FK_Payment_Charge_Charge]
            FOREIGN KEY ([Charge_Id])
            REFERENCES [dbo].[Charge]([Charge_Id]),
        CONSTRAINT [UQ_Payment_Charge_Charge]
            UNIQUE ([Payment_Charge_Id], [Charge_Id])
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.objects
    WHERE name = 'Payment_Detail'
      AND type = 'U'
)
BEGIN
    CREATE TABLE [dbo].[Payment_Detail]
    (
        [PaymentDetailID] INT IDENTITY(1,1) NOT NULL,
        [PaymentDetail_Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Payment_Id] UNIQUEIDENTIFIER NOT NULL,
        [Invoice_Id] UNIQUEIDENTIFIER NOT NULL,
        [InvoiceAmount] DECIMAL(18,3) NOT NULL DEFAULT 0,
        [PreviousPaidAmount] DECIMAL(18,3) NOT NULL DEFAULT 0,
        [PaidAmount] DECIMAL(18,3) NOT NULL DEFAULT 0,
        [BalanceAmount] AS ([InvoiceAmount] - [PreviousPaidAmount] - [PaidAmount]) PERSISTED,
        [Remarks] NVARCHAR(500) NULL,
        [CreatedBy] NVARCHAR(100) NOT NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedBy] NVARCHAR(100) NULL,
        [UpdatedAt] DATETIME NULL,
        [DeletedBy] NVARCHAR(100) NULL,
        [DeletedAt] DATETIME NULL,

        CONSTRAINT [PK_Payment_Detail]
            PRIMARY KEY CLUSTERED ([PaymentDetailID]),
        CONSTRAINT [UQ_Payment_Detail_Id]
            UNIQUE ([PaymentDetail_Id]),
        CONSTRAINT [FK_Payment_Detail_Master]
            FOREIGN KEY ([Payment_Id])
            REFERENCES [dbo].[Payment_Master]([Payment_Id]),
        CONSTRAINT [FK_Payment_Detail_Invoice]
            FOREIGN KEY ([Invoice_Id])
            REFERENCES [dbo].[Invoice_Master]([Invoice_Id]),
        CONSTRAINT [CK_Payment_Detail_Amount]
            CHECK
            (
                [InvoiceAmount] >= 0
                AND [PreviousPaidAmount] >= 0
                AND [PaidAmount] >= 0
            )
    );
END;
GO
