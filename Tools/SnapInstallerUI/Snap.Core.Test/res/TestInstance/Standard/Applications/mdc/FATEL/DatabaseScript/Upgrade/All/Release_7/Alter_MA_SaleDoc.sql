if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_SaleDoc_FEPALetter_00')
	ALTER TABLE [dbo].[MA_SaleDoc] 
	DROP CONSTRAINT DF_SaleDoc_FEPALetter_00
GO

IF EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
    dbo.sysobjects.name = 'MA_SaleDoc' AND dbo.sysobjects.id = dbo.syscolumns.id
    AND dbo.syscolumns.name = 'FEPALetter770')
BEGIN
ALTER TABLE [dbo].[MA_SaleDoc]
   ALTER COLUMN [FEPALetter770] [varchar] (2)
END
GO

if not exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_SaleDoc_FEPALetter_00')
	ALTER TABLE [dbo].[MA_SaleDoc] 
	ADD CONSTRAINT DF_SaleDoc_FEPALetter_00 DEFAULT ('') FOR [FEPALetter770]
GO
