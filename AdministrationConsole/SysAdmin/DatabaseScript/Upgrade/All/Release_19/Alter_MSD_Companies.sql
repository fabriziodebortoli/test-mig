if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Companies' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'DatabaseCulture')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [DatabaseCulture] [int] NOT NULL DEFAULT (0)
GO

UPDATE [dbo].[MSD_Companies]
SET [DatabaseCulture] = 0 WHERE [DatabaseCulture] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Companies' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'SupportColumnCollation')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [SupportColumnCollation] [bit] NOT NULL DEFAULT (0)
GO

UPDATE [dbo].[MSD_Companies]
SET [SupportColumnCollation] = 0
GO