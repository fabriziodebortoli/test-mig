if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'AdministrationReference')
	ALTER TABLE [dbo].[MA_CustSuppBranches] 
	ADD [AdministrationReference] [varchar] (20) NULL CONSTRAINT DF_CustSuppBranches_AdminRefer_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_CustSuppBranches] SET [dbo].[MA_CustSuppBranches].[AdministrationReference] = '' WHERE [dbo].[MA_CustSuppBranches].[AdministrationReference] IS NULL
GO
