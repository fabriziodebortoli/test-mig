if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'AdministrationReference')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [AdministrationReference] [varchar] (20) NULL CONSTRAINT DF_CustSupp_AdminRefer_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[AdministrationReference] = '' WHERE [dbo].[MA_CustSupp].[AdministrationReference] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ImmediateLikeAccompanying')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [ImmediateLikeAccompanying] [char] (1) NULL CONSTRAINT DF_CustSupp_ImmLikeAcc_00 DEFAULT('0')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[ImmediateLikeAccompanying] = '0' WHERE [dbo].[MA_CustSupp].[ImmediateLikeAccompanying] IS NULL
GO
