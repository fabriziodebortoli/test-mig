if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SOSEnvelope]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_SOSEnvelope](
	[EnvelopeID] [int] IDENTITY(1,1) NOT NULL,
	[DispatchDate] [datetime] NULL CONSTRAINT DF_DMS_SOSEnvelope_DispatchDate DEFAULT(getdate()),
	[CollectionID] [int] NOT NULL,
	[DispatchStatus] [int] NULL CONSTRAINT DF_DMS_SOSEnvelope_DispatchStatus DEFAULT (0),
	[DispatchCode] [varchar](512) NULL CONSTRAINT DF_DMS_SOSEnvelope_DispatchCode DEFAULT (''),
	[SynchronizedDate] [datetime] NULL CONSTRAINT DF_DMS_SOSEnvelope_SynchronizedDate DEFAULT (getdate()),
	[LoginId] [int] NULL CONSTRAINT DF_DMS_SOSEnvelope_LoginId DEFAULT (0),
	 CONSTRAINT [PK_DMS_SOSEnvelope] PRIMARY KEY CLUSTERED 
	(
		[EnvelopeID] ASC
	),
	CONSTRAINT [FK_DMS_SOSEnvelope_DMS_Collection] FOREIGN KEY([CollectionID])
	REFERENCES [dbo].[DMS_Collection] ([CollectionID]),

	) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSEnvelope1] ON [dbo].[DMS_SOSEnvelope] ([CollectionID] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSEnvelope2] ON [dbo].[DMS_SOSEnvelope] ([DispatchStatus] ASC) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SOSDocument]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_SOSDocument](
	[AttachmentID] [int] NOT NULL,
	[FileName] [varchar](256) NULL CONSTRAINT DF_DMS_SOSDocument_FileName DEFAULT (''),
	[PdfABinary] [varbinary](max) NULL CONSTRAINT DF_DMS_SOSDocument_PdfABinary DEFAULT (0x00),
	[Size] [bigint] NULL CONSTRAINT DF_DMS_SOSDocument_Size DEFAULT (0),
	[DescriptionKeys] [varchar](max) NOT NULL,
	[HashCode] [varchar](128) NULL CONSTRAINT DF_DMS_SOSDocument_HashCode DEFAULT (''),
	[DocumentStatus] [int] NULL CONSTRAINT DF_DMS_SOSDocument_DocumentStatus DEFAULT (0),
	[AbsoluteCode] [varchar](50) NULL CONSTRAINT DF_DMS_SOSDocument_AbsoluteCode DEFAULT (''),
	[LotID] [varchar](50) NULL CONSTRAINT DF_DMS_SOSDocument_LotID DEFAULT (''),
	[ArchivedDate] [datetime] NULL,
	[RegistrationDate] [datetime] NULL,
	[EnvelopeID] [int] NULL CONSTRAINT DF_DMS_SOSDocument_EnvelopeID DEFAULT (-1),
	CONSTRAINT [PK_DMS_SOSDocument] PRIMARY KEY CLUSTERED 
	(
		[AttachmentID] ASC
	),
	CONSTRAINT [FK_DMS_SOSDocument_DMS_Attachment] FOREIGN KEY ([AttachmentID])	REFERENCES [dbo].[DMS_Attachment] ([AttachmentID]),
	CONSTRAINT [FK_DMS_SOSDocument_DMS_SOSEnvelope] FOREIGN KEY([EnvelopeID]) REFERENCES [dbo].[DMS_SOSEnvelope] ([EnvelopeID])
	) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument2] ON [dbo].[DMS_SOSDocument] ([ArchivedDate] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument3] ON [dbo].[DMS_SOSDocument] ([EnvelopeID] ASC) ON [PRIMARY]
ALTER TABLE [dbo].[DMS_SOSDocument] NOCHECK CONSTRAINT [FK_DMS_SOSDocument_DMS_SOSEnvelope]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SOSConfiguration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_SOSConfiguration](
	[SubjectCode] [varchar](20) NOT NULL,
	[AncestorCode] [varchar](20) NOT NULL,
	[KeeperCode] [varchar](20) NOT NULL,
	[MySOSUser] [varchar](50) NOT NULL,
	[MySOSPassword] [varchar](40) NOT NULL,
	[MySOSUrl] [varchar](128) NOT NULL,
	[DocClasses] [xml] NULL,
	[SOSWebServiceUrl] [varchar](128) NOT NULL,
	CONSTRAINT [PK_DMS_SOSConfiguration] PRIMARY KEY CLUSTERED 
	(
		[SubjectCode] ASC
	)) ON [PRIMARY]
END
GO