IF EXISTS (SELECT dbo.sysobjects.name FROM dbo.sysobjects WHERE
	dbo.sysobjects.name = '$Constraint')
BEGIN
ALTER TABLE [dbo].[$TablePhysicalName]
   DROP CONSTRAINT [$Constraint]
END
GO

IF EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
    dbo.sysobjects.name = '$TablePhysicalName' AND dbo.sysobjects.id = dbo.syscolumns.id
    AND dbo.syscolumns.name = '$FieldPhysicalName')
BEGIN
ALTER TABLE [dbo].[$TablePhysicalName]
   DROP COLUMN [$FieldPhysicalName]
END
GO
