if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_TaxCodes' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EICode')
	ALTER TABLE [dbo].[MA_TaxCodes] 
	ADD [EICode] [varchar] (8) NULL CONSTRAINT DF_TaxCodes_EICode_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_TaxCodes] SET [dbo].[MA_TaxCodes].[EICode] = '' WHERE [dbo].[MA_TaxCodes].[EICode] IS NULL
GO


