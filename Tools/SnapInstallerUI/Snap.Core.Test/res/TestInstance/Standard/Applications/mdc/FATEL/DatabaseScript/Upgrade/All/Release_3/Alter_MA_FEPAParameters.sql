if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'DocStatusCheckError')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	ADD [DocStatusCheckError] [int]	     NULL CONSTRAINT DF_FEPAParame_DocStatusE_00  DEFAULT(30801920)
GO

	UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[DocStatusCheckError] = 30801920 WHERE [dbo].[MA_FEPAParameters].[DocStatusCheckError] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'SendItemCode')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	ADD  [SendItemCode] 		[char]	(1) NULL CONSTRAINT DF_FEPAParame_SendItemCo_00 DEFAULT('0')
GO

	UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[SendItemCode] = '0' WHERE [dbo].[MA_FEPAParameters].[SendItemCode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ItemCodeType')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	ADD  [ItemCodeType] 		[varchar]	(35) NULL CONSTRAINT DF_FEPAParame_ItemCodeTy_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[ItemCodeType] = '' WHERE [dbo].[MA_FEPAParameters].[ItemCodeType] IS NULL
GO