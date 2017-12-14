if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSDocument' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('FiscalYear'))
BEGIN
ALTER TABLE [dbo].[DMS_SOSDocument] ADD [FiscalYear] [varchar] (4)
END
GO
UPDATE [dbo].[DMS_SOSDocument] SET [FiscalYear] = (SELECT f.FieldValue FROM DMS_SearchFieldIndexes f, dbo.DMS_AttachmentSearchIndexes i 
WHERE i.AttachmentID = DMS_SOSDocument.AttachmentID AND i.SearchIndexID = f.SearchIndexID and f.FieldName = 'FiscalYear')
GO