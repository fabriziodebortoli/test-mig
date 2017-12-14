
--indexes on DMS_ArchivedDocument table
if exists (select * from dbo.sysindexes where name = 'IX_DMS_ArchivedDocument1' and id = object_id('[dbo].[DMS_ArchivedDocument]'))
DROP INDEX [dbo].[DMS_ArchivedDocument].[IX_DMS_ArchivedDocument1]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocument1] ON [dbo].[DMS_ArchivedDocument] ([Name] ASC, [ExtensionType] ASC, [CreationTimeUtc] ASC) ON [PRIMARY]
GO

if exists (select * from dbo.sysindexes where name = 'IX_DMS_ArchivedDocument2' and id = object_id('[dbo].[DMS_ArchivedDocument]'))
DROP INDEX [dbo].[DMS_ArchivedDocument].[IX_DMS_ArchivedDocument2]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocument2] ON [dbo].[DMS_ArchivedDocument] ([TBModified] ASC) ON [PRIMARY]
GO

if exists (select * from dbo.sysindexes where name = 'IX_DMS_ArchivedDocument3' and id = object_id('[dbo].[DMS_ArchivedDocument]'))
DROP INDEX [dbo].[DMS_ArchivedDocument].[IX_DMS_ArchivedDocument3]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocument3] ON [dbo].[DMS_ArchivedDocument] ([TBCreated] ASC) ON [PRIMARY]
GO

--indexes on DMS_ArchivedDocContent table
if exists (select * from dbo.sysindexes where name = 'IX_DMS_ArchivedDocContent1' and id = object_id('[dbo].[DMS_ArchivedDocContent]'))
DROP INDEX [dbo].[DMS_ArchivedDocContent].[IX_DMS_ArchivedDocContent1]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocContent1] ON [dbo].[DMS_ArchivedDocContent] ([ExtensionType] ASC) ON [PRIMARY]
GO

if exists (select * from dbo.sysindexes where name = 'IX_DMS_ArchivedDocContent2' and id = object_id('[dbo].[DMS_ArchivedDocContent]'))
DROP INDEX [dbo].[DMS_ArchivedDocContent].[IX_DMS_ArchivedDocContent2]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocContent2] ON [dbo].[DMS_ArchivedDocContent] ([ExtensionType] ASC) ON [PRIMARY]
GO

--indexes on DMS_ErpDocument table
if exists (select * from dbo.sysindexes where name = 'IX_DMS_ErpDocument1' and id = object_id('[dbo].[DMS_ErpDocument]'))
DROP INDEX [dbo].[DMS_ErpDocument].[IX_DMS_ErpDocument1]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_ErpDocument1] ON [dbo].[DMS_ErpDocument]([DocNamespace] ASC,[PrimaryKeyValue] ASC) ON [PRIMARY]
GO

--indexes on DMS_Attachment table
if exists (select * from dbo.sysindexes where name = 'IX_DMS_Attachment1' and id = object_id('[dbo].[DMS_Attachment]'))
DROP INDEX [dbo].[DMS_Attachment].[IX_DMS_Attachment1]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_Attachment1] ON [dbo].[DMS_Attachment] ([ErpDocumentID] ASC) ON [PRIMARY]
GO

if exists (select * from dbo.sysindexes where name = 'IX_DMS_Attachment2' and id = object_id('[dbo].[DMS_Attachment]'))
DROP INDEX [dbo].[DMS_Attachment].[IX_DMS_Attachment2]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_Attachment2] ON [dbo].[DMS_Attachment] ([TBCreated] ASC) ON [PRIMARY]
GO

if exists (select * from dbo.sysindexes where name = 'IX_DMS_Attachment3' and id = object_id('[dbo].[DMS_Attachment]'))
DROP INDEX [dbo].[DMS_Attachment].[IX_DMS_Attachment3]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_Attachment3] ON [dbo].[DMS_Attachment] ([TBModified] ASC) ON [PRIMARY]
GO

if exists (select * from dbo.sysindexes where name = 'IX_DMS_Attachment4' and id = object_id('[dbo].[DMS_Attachment]'))
DROP INDEX [dbo].[DMS_Attachment].[IX_DMS_Attachment4]
GO
CREATE NONCLUSTERED INDEX [IX_DMS_Attachment4] ON [dbo].[DMS_Attachment] ([CollectionID] ASC) ON [PRIMARY]
GO



