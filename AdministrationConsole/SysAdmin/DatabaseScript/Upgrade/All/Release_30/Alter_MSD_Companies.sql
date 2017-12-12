if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_Companies' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name = 'UseRowSecurity')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [UseRowSecurity] [bit] NOT NULL CONSTRAINT DF_Companies_UseRowSecurity DEFAULT (0)
GO

UPDATE [dbo].[MSD_Companies] SET [UseRowSecurity] = 0 WHERE [UseRowSecurity] IS NULL
GO
