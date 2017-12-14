IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = '$TablePhysicalName' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = '$FieldPhysicalName')
BEGIN
ALTER TABLE [dbo].[$TablePhysicalName]
	ADD [$FieldPhysicalName] $Type NULL CONSTRAINT $Constraint DEFAULT $DefaultValue
END
GO

