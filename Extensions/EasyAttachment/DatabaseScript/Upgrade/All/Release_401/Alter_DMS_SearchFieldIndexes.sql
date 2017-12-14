--Change FieldValue size from max to 1024
if  exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SearchFieldIndexes' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('FieldValue'))
ALTER TABLE [dbo].DMS_SearchFieldIndexes ALTER COLUMN FieldValue [varchar](1024) NOT NULL
GO
--Change FormattedValue size from max to 1024
if  exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SearchFieldIndexes' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('FormattedValue'))
ALTER TABLE [dbo].DMS_SearchFieldIndexes ALTER COLUMN FormattedValue [varchar](1024) NULL
GO