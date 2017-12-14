if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_Logins' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'PrivateAreaWebSiteAccess')
ALTER TABLE [dbo].[MSD_Logins] 
ADD [PrivateAreaWebSiteAccess] [bit] NOT NULL DEFAULT (0)
GO

UPDATE [dbo].[MSD_Logins] SET [PrivateAreaWebSiteAccess] = 0 WHERE [PrivateAreaWebSiteAccess] IS NULL
GO