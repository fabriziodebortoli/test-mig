if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACodeForFEPA')
	ALTER TABLE [dbo].[MA_CustSuppBranches] 
	ADD [IPACodeForFEPA] [varchar] (6) NULL CONSTRAINT DF_CustSuppBranches_IPACodeFor_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_CustSuppBranches] SET [dbo].[MA_CustSuppBranches].[IPACodeForFEPA] = '' WHERE [dbo].[MA_CustSuppBranches].[IPACodeForFEPA] IS NULL
GO
