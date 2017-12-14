if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_CompanyLogins' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'EBDeveloper')
ALTER TABLE [dbo].[MSD_CompanyLogins] 
ADD [EBDeveloper] [bit] NOT NULL DEFAULT (0)
GO

UPDATE [dbo].[MSD_CompanyLogins] SET [EBDeveloper] = 0 WHERE [EBDeveloper] IS NULL
GO