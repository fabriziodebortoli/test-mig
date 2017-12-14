if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'LinkFEPA')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	ADD  [LinkFEPA] 		[varchar]	(256) NULL CONSTRAINT DF_FEPAParame_LinKFEPA_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[LinkFEPA] = '' WHERE [dbo].[MA_FEPAParameters].[LinkFEPA] IS NULL
GO