if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_Companies' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'Port')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [Port] [int] NOT NULL DEFAULT (0)
GO

UPDATE [dbo].[MSD_Companies] SET [Port] = 0 WHERE [Port] IS NULL
GO