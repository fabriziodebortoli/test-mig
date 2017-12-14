if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DS_Providers' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('IAFModules'))
ALTER TABLE [dbo].[DS_Providers] 
	ADD [IAFModules] [varchar](256) NULL CONSTRAINT DF_Providers_IAFModules DEFAULT('')
GO

UPDATE [dbo].[DS_Providers] SET [IAFModules] = '' WHERE [IAFModules] IS NULL
GO

