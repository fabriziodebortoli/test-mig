if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MSD_Companies' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name = 'IsSecurityLightMigrated')
ALTER TABLE [dbo].[MSD_Companies] 
ADD [IsSecurityLightMigrated] [bit] NOT NULL DEFAULT (1)
GO 

UPDATE [dbo].[MSD_Companies]
SET [IsSecurityLightMigrated] = 0 WHERE [IsSecurityLightMigrated] = 1
GO