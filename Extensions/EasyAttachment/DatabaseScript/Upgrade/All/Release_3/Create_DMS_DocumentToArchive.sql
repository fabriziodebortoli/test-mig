if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_DocumentToArchive]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_DocumentToArchive](
	[DocToArchiveID] [int] IDENTITY(1,1) NOT NULL,
	[DocNamespace] [varchar](256) NOT NULL,
	[PrimaryKeyValue] [varchar](256) NOT NULL,
	[Name] [varchar](256) NOT NULL,
	[Description] [varchar](256) NOT NULL,
	[BinaryContent] [varbinary](max) NOT NULL,
	[RowKey] [varchar](128) NULL,
	[AttachmentID] [int]  NULL,
	CONSTRAINT [PK_DMS_DocumentToArchive] PRIMARY KEY CLUSTERED 
	(
		[DocToArchiveID] ASC
	)) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_DocumentToArchive1] ON [dbo].[DMS_DocumentToArchive] ([Name] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_DocumentToArchive2] ON [dbo].[DMS_DocumentToArchive] ([DocNamespace] ASC,	[PrimaryKeyValue] ASC) ON [PRIMARY]
END
GO