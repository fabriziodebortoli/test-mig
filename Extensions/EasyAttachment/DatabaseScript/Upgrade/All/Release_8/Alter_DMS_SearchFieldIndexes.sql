if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SearchFieldIndexes' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('FormattedValue'))
BEGIN
ALTER TABLE [dbo].[DMS_SearchFieldIndexes] ADD [FormattedValue] [varchar](max) NULL
END
GO

UPDATE [dbo].[DMS_SearchFieldIndexes] SET [FormattedValue] = [FieldValue]
GO