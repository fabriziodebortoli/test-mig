if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TB_CustomMetadata]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[TB_CustomMetadata](
	[FileID] [int] IDENTITY(1,1) NOT NULL,
	[ParentID] [int] NULL,
	[PathName] [varchar](512) NOT NULL,
	[FileName] [varchar](256) NOT NULL,
	[CompleteFileName] [varchar](780) NOT NULL,
	[FileType] [varchar](20) NOT NULL,
	[FileSize] [int] NOT NULL,
	[Namespace] [varchar](256) NOT NULL,
	[Application] [varchar](20) NOT NULL,
	[Module] [varchar](40) NOT NULL,
	[ObjectType] [varchar](50) NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastWriteTime] [datetime] NOT NULL,
	[IsDirectory] [char](1) NOT NULL,
	[IsReadOnly] [char](1) NOT NULL,
	[FileContent] [varbinary](max) NULL,	
	[FileTextContent] [varchar](max) NULL,
	[AccountName] [varchar](128) NOT NULL,	
 CONSTRAINT [PK_TB_CustomMetadata] PRIMARY KEY CLUSTERED 
(
	[FileID] ASC
)) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_TB_CustomMetadata_CompleteFileName] ON [dbo].[TB_CustomMetadata](	[CompleteFileName] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_TB_CustomMetadata_FileName] ON [dbo].[TB_CustomMetadata](	[FileName] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_TB_CustomMetadata_Path] ON [dbo].[TB_CustomMetadata](	[PathName] ASC) ON [PRIMARY]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_PathName]  DEFAULT ('') FOR [PathName]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_FileName]  DEFAULT ('') FOR [FileName]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_CompleteFileName]  DEFAULT ('') FOR [CompleteFileName]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_FileType]  DEFAULT ('') FOR [FileType]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_FileSize]  DEFAULT ((0)) FOR [FileSize]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_Namespace]  DEFAULT ('') FOR [Namespace]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_Application]  DEFAULT ('') FOR [Application]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_Module]  DEFAULT ('') FOR [Module]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_ObjectType]  DEFAULT ('') FOR [ObjectType]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_LastWriteTime]  DEFAULT (getdate()) FOR [LastWriteTime]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_IsDirectory]  DEFAULT ('0') FOR [IsDirectory]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_IsReadOnly]  DEFAULT ('0') FOR [IsReadOnly]
ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_AccountName]  DEFAULT ('') FOR [AccountName]
ALTER TABLE [dbo].[TB_CustomMetadata]  WITH NOCHECK ADD  CONSTRAINT [FK_TB_CustomMetadata_TB_CustomMetadata] FOREIGN KEY([ParentID]) REFERENCES [dbo].[TB_CustomMetadata] ([FileID])
ALTER TABLE [dbo].[TB_CustomMetadata] NOCHECK CONSTRAINT [FK_TB_CustomMetadata_TB_CustomMetadata]
END
GO