if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_Jobs' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EIJobCode')
	ALTER TABLE [dbo].[MA_Jobs] 
	ADD [EIJobCode] [varchar] (100) NULL CONSTRAINT DF_Jobs_EIJobCod_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_Jobs] SET [dbo].[MA_Jobs].[EIJobCode] = '' WHERE [dbo].[MA_Jobs].[EIJobCode] IS NULL
GO
