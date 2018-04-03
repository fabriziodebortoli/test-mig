IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MP_InstanceTBFS]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MP_InstanceTBFS](
	[FileID] [int] IDENTITY(1,1) NOT NULL,
	[ParentID] [int] NULL,
	[PathName] [varchar](512) NOT NULL,
	[FileName] [varchar](256) NOT NULL,
	[FileType] [varchar](20) NOT NULL,
	[CompleteFileName] [varchar](780) NOT NULL,
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
 CONSTRAINT [PK_MP_InstanceTBFS] PRIMARY KEY NONCLUSTERED 
(
	[FileID] ASC
)) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE UNIQUE CLUSTERED INDEX [IX_MP_InstanceTBFS_1] ON [dbo].[MP_InstanceTBFS]([CompleteFileName] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_MP_InstanceTBFS_2] ON [dbo].[MP_InstanceTBFS]([FileType] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_MP_InstanceTBFS_3] ON [dbo].[MP_InstanceTBFS] ([FileName] ASC )ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_MP_InstanceTBFS_4] ON [dbo].[MP_InstanceTBFS]( [PathName] ASC ) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_MP_InstanceTBFS_5] ON [dbo].[MP_InstanceTBFS]( [ParentID] ASC ) ON [PRIMARY]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_InstanceKey]  DEFAULT ('') FOR [InstanceKey]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_PathName]  DEFAULT ('') FOR [PathName]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_FileName]  DEFAULT ('') FOR [FileName]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_FileType]  DEFAULT ('DIRECTORY') FOR [FileType]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_CompleteFileName]  DEFAULT ('') FOR [CompleteFileName]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_FileSize]  DEFAULT ((0)) FOR [FileSize]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_Namespace]  DEFAULT ('') FOR [Namespace]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_Application]  DEFAULT ('') FOR [Application]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_Module]  DEFAULT ('') FOR [Module]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_ObjectType]  DEFAULT ('') FOR [ObjectType]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_LastWriteTime]  DEFAULT (getdate()) FOR [LastWriteTime]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_IsDirectory]  DEFAULT ('0') FOR [IsDirectory]
ALTER TABLE [dbo].[MP_InstanceTBFS] ADD  CONSTRAINT [DF_InstanceTBFS_IsReadOnly]  DEFAULT ('0') FOR [IsReadOnly]

END

GO


