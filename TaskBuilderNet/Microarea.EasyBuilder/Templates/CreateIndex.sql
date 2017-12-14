IF NOT EXISTS (SELECT * FROM dbo.sysindexes WHERE NAME = '$IndexName' AND id = object_id('[dbo].[$TablePhysicalName]'))
BEGIN
	CREATE INDEX [$IndexName] ON [dbo].[$TablePhysicalName] ($IndexFields) ON [PRIMARY]
END
GO
