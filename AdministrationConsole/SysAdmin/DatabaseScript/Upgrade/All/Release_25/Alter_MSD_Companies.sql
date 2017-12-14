if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_Companies' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name = 'UseDBSlave')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [UseDBSlave] [bit] NOT NULL CONSTRAINT DF_Companies_UseDBSlave DEFAULT (0)
GO

UPDATE [dbo].[MSD_Companies] SET [UseDBSlave] = 0 WHERE [UseDBSlave] IS NULL
GO