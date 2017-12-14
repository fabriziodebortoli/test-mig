--adding barcodetype to manage more than one type
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ArchivedDocContent' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('StorageFile'))
ALTER TABLE [dbo].[DMS_ArchivedDocContent] 
ADD [StorageFile] [varchar] (512) NULL CONSTRAINT DF_DMS_ArchivedDocContent_StorageFile DEFAULT(NULL)
GO
UPDATE [dbo].[DMS_ArchivedDocContent] SET [dbo].[DMS_ArchivedDocContent].[StorageFile] = ''
WHERE [dbo].[DMS_ArchivedDocContent].[StorageFile] IS NULL
GO
UPDATE [dbo].[DMS_ArchivedDocument] SET [dbo].[DMS_ArchivedDocument].[StorageType] = 0
WHERE [dbo].[DMS_ArchivedDocument].[StorageType] IS NULL
GO

