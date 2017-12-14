IF EXISTS (SELECT * FROM dbo.sysindexes WHERE NAME = '$IndexName' AND id = object_id('[dbo].[$TablePhysicalName]'))
BEGIN
	DROP INDEX [dbo].[$TablePhysicalName].[$IndexName]
END
GO
