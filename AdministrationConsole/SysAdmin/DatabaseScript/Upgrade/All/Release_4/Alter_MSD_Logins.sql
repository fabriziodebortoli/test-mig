if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Logins' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'Locked')
ALTER TABLE [dbo].[MSD_Logins] 
ADD [Locked] [bit] NOT NULL DEFAULT (0)
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Logins' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'LoginFailedCount')
ALTER TABLE [dbo].[MSD_Logins] 
ADD [LoginFailedCount] [int] NOT NULL DEFAULT (0) 
GO
