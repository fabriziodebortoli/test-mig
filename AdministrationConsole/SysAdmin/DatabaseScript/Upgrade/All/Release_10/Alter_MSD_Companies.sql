if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Companies' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'IsValid')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [IsValid] [bit] NOT NULL DEFAULT (1)
GO

UPDATE [dbo].[MSD_Companies] 
SET [dbo].[MSD_Companies].[IsValid] = 0
GO