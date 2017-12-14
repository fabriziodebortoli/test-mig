if exists (select dbo.sysobjects.name from dbo.sysobjects where
    dbo.sysobjects.name = 'DF_DMS_ArchivedDocContentt_TextContent')
BEGIN
ALTER TABLE [dbo].[DMS_ArchivedDocContent]
   DROP CONSTRAINT [DF_DMS_ArchivedDocContentt_TextContent]
END
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ArchivedDocContent' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('TextContent'))
ALTER TABLE [dbo].[DMS_ArchivedDocContent] 
DROP COLUMN [TextContent]
GO

--ExtensionType to be equal to DMS_ArchivedDoc.ExtensionType
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ArchivedDocContent' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('ExtensionType'))
ALTER TABLE [dbo].[DMS_ArchivedDocContent] 
ADD [ExtensionType] [varchar] (10) NULL CONSTRAINT DF_DMS_ArchivedDocContent_ExtensionType DEFAULT('')
GO

UPDATE dbo.DMS_ArchivedDocContent set ExtensionType = Y.ExtensionType from
DMS_ArchivedDocContent X JOIN dbo.DMS_ArchivedDocument Y ON X.ArchivedDocID = Y.ArchivedDocID
GO

--OCRProcess
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ArchivedDocContent' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('OCRProcess'))
ALTER TABLE [dbo].[DMS_ArchivedDocContent] 
ADD [OCRProcess] [int] NULL CONSTRAINT DF_DMS_ArchivedDocContent_OCRProcess DEFAULT(0)
GO

UPDATE [dbo].[DMS_ArchivedDocContent] set [OCRProcess] = 0;
GO
