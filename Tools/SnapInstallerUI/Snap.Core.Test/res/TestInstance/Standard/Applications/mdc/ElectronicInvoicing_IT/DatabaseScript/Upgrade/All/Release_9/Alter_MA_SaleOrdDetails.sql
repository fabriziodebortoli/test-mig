
IF EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
    dbo.sysobjects.name = 'MA_SaleOrdDetails' AND dbo.sysobjects.id = dbo.syscolumns.id
    AND dbo.syscolumns.name = 'ContractCode')
BEGIN
ALTER TABLE [dbo].[MA_SaleOrdDetails]
   ALTER COLUMN [ContractCode] [varchar] (15)
END
GO
