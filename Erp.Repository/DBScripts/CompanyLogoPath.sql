IF COL_LENGTH('dbo.Company', 'CompanyLogoPath') IS NULL
BEGIN
    ALTER TABLE [dbo].[Company]
    ADD [CompanyLogoPath] NVARCHAR(500) NULL;
END;
GO
