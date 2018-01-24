//****** Object:  Table [dbo].[TB_CustomMetadata]    Script Date: 25/09/2017 15:06:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [IX_TB_CustomMetadata_CompleteFileName]    Script Date: 25/09/2017 15:06:57 ******/
CREATE NONCLUSTERED INDEX [IX_TB_CustomMetadata_CompleteFileName] ON [dbo].[TB_CustomMetadata]
(
	[CompleteFileName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [IX_TB_CustomMetadata_FileName]    Script Date: 25/09/2017 15:06:57 ******/
CREATE NONCLUSTERED INDEX [IX_TB_CustomMetadata_FileName] ON [dbo].[TB_CustomMetadata]
(
	[FileName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [IX_TB_CustomMetadata_Path]    Script Date: 25/09/2017 15:06:57 ******/
CREATE NONCLUSTERED INDEX [IX_TB_CustomMetadata_Path] ON [dbo].[TB_CustomMetadata]
(
	[PathName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_PathName]  DEFAULT ('') FOR [PathName]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_FileName]  DEFAULT ('') FOR [FileName]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_CompleteFileName]  DEFAULT ('') FOR [CompleteFileName]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_FileType]  DEFAULT ('') FOR [FileType]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_FileSize]  DEFAULT ((0)) FOR [FileSize]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_Namespace]  DEFAULT ('') FOR [Namespace]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_Application]  DEFAULT ('') FOR [Application]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_Module]  DEFAULT ('') FOR [Module]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_ObjectType]  DEFAULT ('') FOR [ObjectType]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_LastWriteTime]  DEFAULT (getdate()) FOR [LastWriteTime]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_IsDirectory]  DEFAULT ('0') FOR [IsDirectory]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_IsReadOnly]  DEFAULT ('0') FOR [IsReadOnly]
GO

ALTER TABLE [dbo].[TB_CustomMetadata] ADD  CONSTRAINT [DF_TB_CustomMetadata_AccountName]  DEFAULT ('') FOR [AccountName]
GO

ALTER TABLE [dbo].[TB_CustomMetadata]  WITH NOCHECK ADD  CONSTRAINT [FK_TB_CustomMetadata_TB_CustomMetadata] FOREIGN KEY([ParentID])
REFERENCES [dbo].[TB_CustomMetadata] ([FileID])
GO

ALTER TABLE [dbo].[TB_CustomMetadata] NOCHECK CONSTRAINT [FK_TB_CustomMetadata_TB_CustomMetadata]
GO



