if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_Items' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EITypeCode')
	ALTER TABLE [dbo].[MA_Items] 
	ADD [EITypeCode] [varchar] (35) NULL CONSTRAINT DF_Items_EITypCod_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_Items] SET [dbo].[MA_Items].[EITypeCode] = '' WHERE [dbo].[MA_Items].[EITypeCode] IS NULL
GO


if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_Items' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EIValueCode')
	ALTER TABLE [dbo].[MA_Items] 
	ADD [EIValueCode] [varchar] (35) NULL CONSTRAINT DF_Items_EIValCod_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_Items] SET [dbo].[MA_Items].[EIValueCode] = '' WHERE [dbo].[MA_Items].[EIValueCode] IS NULL
GO
