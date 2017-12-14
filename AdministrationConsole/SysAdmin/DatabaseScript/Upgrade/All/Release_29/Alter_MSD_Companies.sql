if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where dbo.sysobjects.name = 'MSD_Companies' and 
dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'UseDataSynchro')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [UseDataSynchro] [bit] NOT NULL CONSTRAINT DF_Companies_UseDataSynchro DEFAULT (0)
GO

UPDATE [dbo].[MSD_Companies] SET [UseDataSynchro] = 0 WHERE [UseDataSynchro] IS NULL
GO
