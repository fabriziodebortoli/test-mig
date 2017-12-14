DECLARE @defname VARCHAR(100), @dropcmd VARCHAR(1000)

SET @defname = (SELECT name FROM sysobjects so JOIN sysconstraints sc
ON so.id = sc.constid WHERE object_name(so.parent_obj) = 'MSD_Companies' 
AND so.xtype = 'D' AND sc.colid = (SELECT colid FROM syscolumns 
 WHERE id = object_id('MSD_Companies') AND name = 'IsSecurityLightMigrated'))

SET @dropcmd = 'ALTER TABLE MSD_Companies DROP CONSTRAINT ' + @defname

EXEC(@dropcmd)
GO

ALTER TABLE MSD_Companies
ADD CONSTRAINT DF_Companies_IsSecLightMig DEFAULT(0) FOR IsSecurityLightMigrated
GO

UPDATE MSD_Companies SET IsSecurityLightMigrated = 0 WHERE UseSecurity = 0
GO