IF COL_LENGTH('dbo.Outward_Detail', 'ExcessQty') IS NULL
BEGIN
    ALTER TABLE dbo.Outward_Detail
    ADD ExcessQty DECIMAL(18,2) NULL
        CONSTRAINT DF_Outward_Detail_ExcessQty DEFAULT (0) WITH VALUES;
END;

IF COL_LENGTH('dbo.Outward_Detail', 'ShortageQty') IS NULL
BEGIN
    ALTER TABLE dbo.Outward_Detail
    ADD ShortageQty DECIMAL(18,2) NULL
        CONSTRAINT DF_Outward_Detail_ShortageQty DEFAULT (0) WITH VALUES;
END;

IF COL_LENGTH('dbo.Outward_Detail', 'DamagedQty') IS NULL
BEGIN
    ALTER TABLE dbo.Outward_Detail
    ADD DamagedQty DECIMAL(18,2) NULL
        CONSTRAINT DF_Outward_Detail_DamagedQty DEFAULT (0) WITH VALUES;
END;