if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_Field]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_Field](
	[FieldName] [varchar](80) NOT NULL,
	[FieldDescription] [varchar](256) NULL CONSTRAINT DF_DMS_Field_Description DEFAULT(''),
	[ValueType] [varchar](50) NULL CONSTRAINT DF_DMS_Field_ValueType DEFAULT(''),
	[IsCategory] [bit]  NULL CONSTRAINT DF_DMS_Field_IsCategory DEFAULT((0))
 CONSTRAINT [PK_DMS_Field] PRIMARY KEY CLUSTERED 
(
	[FieldName] ASC
)) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_Collector]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_Collector](
	[CollectorID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](256) NULL CONSTRAINT DF_DMS_Collector_Name DEFAULT(''),
	[IsStandard] [bit] NULL CONSTRAINT DF_DMS_Collector_IsStandard DEFAULT((0)),
 CONSTRAINT [PK_DMS_Collector] PRIMARY KEY CLUSTERED 
(
	[CollectorID] ASC
)) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_Collection]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_Collection](
	[CollectionID] [int] IDENTITY(1,1) NOT NULL,
	[IsStandard] [bit] NULL CONSTRAINT DF_DMS_Collection_IsStandard DEFAULT((0)),
	[CollectorID] [int] NULL CONSTRAINT DF_DMS_Collection_CollectorID DEFAULT(0),
	[Name] [varchar](256) NULL CONSTRAINT DF_DMS_Collection_Name DEFAULT(''),
	[TemplateName] [varchar](128) NULL CONSTRAINT DF_DMS_Collection_TemplateName DEFAULT(''),
	[SosDocClass] [varchar](15) NULL CONSTRAINT DF_DMS_Collection_SosDocClass DEFAULT(('')),
	[Version] [int] NULL CONSTRAINT DF_DMS_Collection_Version DEFAULT(1),
	CONSTRAINT [PK_DMS_Collections] PRIMARY KEY CLUSTERED 
	(
		[CollectionID] ASC
	) ON [PRIMARY],
	CONSTRAINT [FK_DMS_Collection_DMS_Collector] FOREIGN KEY
    (
        [CollectorID]
    ) REFERENCES [dbo].[DMS_Collector] (
        [CollectorID]
    )
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_FieldProperties]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_FieldProperties](
	[FieldName] [varchar](80) NOT NULL,
	[XMLValues] [xml] NULL CONSTRAINT DF_DMS_FieldProperties_XMLValues DEFAULT(''),
	[FieldColor] [int] NULL CONSTRAINT DF_DMS_FieldProperties_FieldColor DEFAULT(-1),
	[Disabled] [bit] NULL CONSTRAINT DF_DMS_FieldProperties_Disabled DEFAULT((0)),
	CONSTRAINT [PK_DMS_FieldProperties] PRIMARY KEY CLUSTERED 
	(
		[FieldName] ASC
	),
    CONSTRAINT [FK_DMS_FieldProperties_DMS_Field] FOREIGN KEY
    (
        [FieldName]
    ) REFERENCES [dbo].[DMS_Field] (
        [FieldName]
    )
) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_CollectionsFields]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_CollectionsFields](
	[FieldName] [varchar](80) NOT NULL,
	[CollectionID] [int] NOT NULL,
	[ControlName] [varchar](256) NULL CONSTRAINT DF_DMS_CollectionsFields_ControlName DEFAULT(''),
	[OCRPosition] [varchar](50) NULL CONSTRAINT DF_DMS_CollectionsFields_OCRPosition DEFAULT(''),
	[PhysicalName] [varchar](256) NULL CONSTRAINT DF_DMS_CollectionsFields_PhysicalName DEFAULT(''),
	[FieldGroup] [int] NULL CONSTRAINT DF_DMS_CollectionsFields_Group DEFAULT('0'),
	[ShowAsDescription] [bit] NULL CONSTRAINT DF_DMS_CollectionsFields_ShowAsDescription DEFAULT((0)),
	[Disabled] [bit] NULL CONSTRAINT DF_DMS_CollectionsFields_Disabled DEFAULT((0)),
	[SosPosition] [int] NULL CONSTRAINT DF_DMS_CollectionsFields_SosPosition DEFAULT(-1),
	[HKLName] [varchar] (128) NULL CONSTRAINT DF_DMS_CollectionsFields_HKLName DEFAULT(''),
	[SosMandatory] [bit] NULL CONSTRAINT DF_DMS_CollectionsFields_SosMandatory DEFAULT((0)),
	[SosKeyCode] [varchar] (80) NULL CONSTRAINT DF_DMS_CollectionsFields_SosKeyCode DEFAULT(''),
	CONSTRAINT [PK_DMS_CollectionsFields] PRIMARY KEY CLUSTERED 
	(
		[FieldName] ASC,
		[CollectionID] ASC
	),
    CONSTRAINT [FK_DMS_CollectionsFields_DMS_Collection] FOREIGN KEY
    (
        [CollectionID]
    ) REFERENCES [dbo].[DMS_Collection] (
        [CollectionID]
    ),
	CONSTRAINT [FK_DMS_CollectionsFields_DMS_Field] FOREIGN KEY
    (
        [FieldName]
    ) REFERENCES [dbo].[DMS_Field] (
        [FieldName]
    )) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_ArchivedDocument]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_ArchivedDocument](
	[ArchivedDocID] [int] IDENTITY(1,1) NOT NULL,
	[Language] [varchar](10) NOT NULL,
	[Name] [varchar](256) NOT NULL,
	[Description][varchar](256) NULL CONSTRAINT DF_DMS_ArchivedDocument_Description DEFAULT '',
	[ExtensionType] [varchar](10) NOT NULL,
	[StorageType] [int] NULL CONSTRAINT DF_DMS_ArchivedDocument_StorageType DEFAULT(0),
	[Path] [varchar](512)  NOT NULL,
	[CreationTimeUtc] [datetime] NOT NULL,
	[LastWriteTimeUtc] [datetime] NOT NULL,
	[CRC] [bigint] NOT NULL,
	[Size] [bigint] NOT NULL,
	[TBCreatedID] [int] NULL CONSTRAINT DF_DMS_ArchivedDocument_TBCreatedID DEFAULT(0),
	[TBModifiedID] [int] NULL CONSTRAINT DF_DMS_ArchivedDocument_TBModifiedID DEFAULT(0),
	[TBCreated] [datetime] NULL CONSTRAINT DF_DMS_ArchivedDocument_TBCreated DEFAULT(getdate()),
	[TBModified] [datetime] NULL CONSTRAINT DF_DMS_ArchivedDocument_TBModified DEFAULT(getdate()),
	[IsWoormReport] [bit] NULL CONSTRAINT DF_DMS_ArchivedDocument_IsWoormReport DEFAULT((0)),
	[CollectionID] [int] NULL CONSTRAINT DF_DMS_ArchivedDocument_CollectionID DEFAULT(0),
	[ModifierID] [int] NULL CONSTRAINT DF_DMS_ArchivedDocument_ModifierID DEFAULT(0),
	[Barcode] [varchar] (48) NULL CONSTRAINT DF_DMS_ArchivedDocument_Barcode DEFAULT(''),
	[BarcodeType] [varchar] (24) NULL CONSTRAINT DF_DMS_ArchivedDocument_BarcodeType DEFAULT(NULL)
	CONSTRAINT [PK_DMS_ArchivedDocument] PRIMARY KEY CLUSTERED 
	(
		[ArchivedDocID] ASC
	),
    CONSTRAINT [FK_DMS_ArchivedDocument_DMS_Collection] FOREIGN KEY
    (
        [CollectionID]
    ) REFERENCES [dbo].[DMS_Collection] (
        [CollectionID]
    )) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocument1] ON [dbo].[DMS_ArchivedDocument] ([Name] ASC, [ExtensionType] ASC, [CreationTimeUtc] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocument2] ON [dbo].[DMS_ArchivedDocument] ([TBModified] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocument3] ON [dbo].[DMS_ArchivedDocument] ([TBCreated] ASC) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_ArchivedDocContent]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_ArchivedDocContent](
	[ArchivedDocID] [int] NOT NULL,
	[BinaryContent] [varbinary](max) NOT NULL,
	[ExtensionType] [varchar] (10) NULL CONSTRAINT DF_DMS_ArchivedDocContent_ExtensionType DEFAULT(''),	
	[OCRProcess] [bit] NULL CONSTRAINT DF_DMS_ArchivedDocContent_OCRProcess DEFAULT((0)),
	[StorageFile] [varchar](512) CONSTRAINT DF_DMS_ArchivedDocContent_StorageFile DEFAULT(('')),
	CONSTRAINT [PK_DMS_ArchivedDocContent] PRIMARY KEY CLUSTERED 
	(
		[ArchivedDocID] ASC
	),
    CONSTRAINT [FK_DMS_ArchivedDocContent_DMS_ArchivedDocument] FOREIGN KEY
    (
        [ArchivedDocID]
    ) REFERENCES [DMS_ArchivedDocument] (
        [ArchivedDocID]
    )) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocContent1] ON [dbo].[DMS_ArchivedDocContent] ([ExtensionType] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocContent2] ON [dbo].[DMS_ArchivedDocContent] ([OCRProcess] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_ArchivedDocContent3] ON [dbo].[DMS_ArchivedDocContent] ([StorageFile] ASC) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_ArchivedDocTextContent]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_ArchivedDocTextContent](
	[ArchivedDocID] [int] NOT NULL,
	[TextContent] [text] NOT NULL,
	CONSTRAINT [PK_DMS_ArchivedDocTextContent] PRIMARY KEY CLUSTERED 
	(
		[ArchivedDocID] ASC
	),
	CONSTRAINT [FK_DMS_ArchivedDocTextContent_DMS_ArchivedDocContent] FOREIGN KEY
	(
		[ArchivedDocID]
	) REFERENCES [DMS_ArchivedDocContent] (
		[ArchivedDocID]
	)) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_ErpDocument]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_ErpDocument](
	[ErpDocumentID] [int] IDENTITY(1,1) NOT NULL,
	[DocNamespace] [varchar](256) NULL CONSTRAINT DF_DMS_ErpDocument_DocNamespace DEFAULT(''),	
	[TBGuid] [uniqueidentifier] NULL CONSTRAINT DF_DMS_ErpDocument_TBGuid DEFAULT ('00000000-0000-0000-0000-000000000000'),
	[PrimaryKeyValue] [varchar](256) NULL CONSTRAINT DF_DMS_ErpDocument_PrimaryKeyValue DEFAULT(''),
	[DescriptionValue] [varchar](256) NULL CONSTRAINT DF_DMS_ErpDocument_DescriptionValue DEFAULT(''),
	CONSTRAINT [PK_DMS_ErpDocument] PRIMARY KEY CLUSTERED 
	(
		[ErpDocumentID] ASC
	)) ON [PRIMARY] 
CREATE NONCLUSTERED INDEX [IX_DMS_ErpDocument1] ON [dbo].[DMS_ErpDocument]([DocNamespace] ASC,[PrimaryKeyValue] ASC) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_ErpDocBarcodes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_ErpDocBarcodes](
	[Barcode] [varchar](48) NOT NULL,	
	[BarcodeType] [varchar] (24) NULL CONSTRAINT DF_DMS_ErpDocBarcodes_BarcodeType DEFAULT(NULL),
	[Notes] [varchar](128) NULL CONSTRAINT DF_DMS_ErpDocBarcodes_Notes DEFAULT(''),
	[Name] [varchar](256) NOT NULL,
	[ErpDocumentID] [int] NOT NULL,
    

	CONSTRAINT [PK_DMS_ErpDocBarcodes] PRIMARY KEY CLUSTERED 
	(
		[Barcode] ASC,
		[ErpDocumentID] ASC
	),
    CONSTRAINT [FK_DMS_ErpDocBarcodes_DMS_ErpDocument] FOREIGN KEY
    (
        [ErpDocumentID]
    ) REFERENCES [dbo].[DMS_ErpDocument] (
        [ErpDocumentID]
    )) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_Attachment]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_Attachment](
	[AttachmentID] [int] IDENTITY(1,1) NOT NULL,	
	[ErpDocumentID] [int] NOT NULL,
	[CollectionID] [int] NOT NULL,
	[ArchivedDocID] [int] NOT NULL,
	[TBCreatedID] [int] NULL CONSTRAINT DF_DMS_Attachment_TBCreatedID DEFAULT(0),
	[TBModifiedID] [int] NULL CONSTRAINT DF_DMS_Attachment_TBModifiedID DEFAULT(0),
	[TBCreated] [datetime] NULL CONSTRAINT DF_DMS_Attachment_TBCreated DEFAULT(getdate()),
	[TBModified] [datetime] NULL CONSTRAINT DF_DMS_Attachment_TBModified DEFAULT(getdate()),
	[IsMainDoc] [bit] NULL CONSTRAINT DF_DMS_Attachment_IsMainDoc DEFAULT((0)),
	[Description] [varchar](512) NULL CONSTRAINT DF_DMS_Attachment_Description DEFAULT(''),
	[AbsoluteCode] [varchar](50) NULL CONSTRAINT DF_DMS_Attachment_AbsoluteCode DEFAULT (''),
	[LotID] [varchar](50) NULL CONSTRAINT DF_DMS_Attachment_LotID DEFAULT (''),
	[RegistrationDate] [datetime] NULL,
	[IsForMail] [bit] NULL CONSTRAINT DF_DMS_Attachment_IsForMail DEFAULT((0))	
	CONSTRAINT [PK_DMS_Attachment] PRIMARY KEY CLUSTERED 
	(
		[AttachmentID] ASC
	),
	CONSTRAINT [FK_DMS_Attachment_DMS_ArchivedDocument] FOREIGN KEY
    (
        [ArchivedDocID]
    ) REFERENCES [dbo].[DMS_ArchivedDocument] (
        [ArchivedDocID]
    ),
	CONSTRAINT [FK_DMS_Attachment_DMS_ErpDocument] FOREIGN KEY
    (
        [ErpDocumentID]
    ) REFERENCES [dbo].[DMS_ErpDocument] (
        [ErpDocumentID]
    ),
	CONSTRAINT [FK_DMS_Attachment_DMS_Collection] FOREIGN KEY
    (
        [CollectionID]
    ) REFERENCES [dbo].[DMS_Collection] (
        [CollectionID]
    )) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_DMS_Attachment1] ON [dbo].[DMS_Attachment] ([ErpDocumentID] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_Attachment2] ON [dbo].[DMS_Attachment] ([TBCreated] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_Attachment3] ON [dbo].[DMS_Attachment] ([TBModified] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_Attachment4] ON [dbo].[DMS_Attachment] ([CollectionID] ASC) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SearchFieldIndexes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_SearchFieldIndexes](
	[SearchIndexID] [int] IDENTITY(1,1) NOT NULL,
	[FieldName] [varchar](80) NOT NULL,
	[FieldValue] [varchar](1024) NOT NULL,
	[FormattedValue] [varchar](1024) NULL,
	CONSTRAINT [PK_DMS_SearchFieldIndexes] PRIMARY KEY NONCLUSTERED 
	(
		[SearchIndexID] ASC
	)) ON [PRIMARY]

CREATE CLUSTERED INDEX [IX_DMS_SearchFieldIndex] ON [dbo].[DMS_SearchFieldIndexes](	[FieldName] ASC) ON [PRIMARY]
END
GO
 
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_AttachmentSearchIndexes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_AttachmentSearchIndexes](
	[AttachmentID] [int] NOT NULL,
	[SearchIndexID] [int] NOT NULL,
	CONSTRAINT [FK_DMS_AttachmentSearchIndexes_DMS_AttachmentSearchIndexes] PRIMARY KEY CLUSTERED 
	(
		[AttachmentID] ASC,
		[SearchIndexID] ASC
	),
	CONSTRAINT [FK_DMS_AttachmentSearchIndexes_DMS_Attachment] FOREIGN KEY
    (
        [AttachmentID]
    ) REFERENCES [dbo].[DMS_Attachment] (
        [AttachmentID]
    ),
	CONSTRAINT [FK_DMS_AttachmentSearchIndexes_DMS_SearchFieldIndexes] FOREIGN KEY
    (
		[SearchIndexID]
    ) REFERENCES [dbo].[DMS_SearchFieldIndexes] (
        [SearchIndexID]
    )) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_ArchivedDocSearchIndexes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_ArchivedDocSearchIndexes](
	[ArchivedDocID] [int] NOT NULL,
	[SearchIndexID] [int] NOT NULL,
	CONSTRAINT [PK_DMS_ArchivedDocSearchIndexes] PRIMARY KEY CLUSTERED 
	(
		[ArchivedDocID] ASC,
		[SearchIndexID] ASC
	),
	CONSTRAINT [FK_DMS_ArchivedDocSearchIndexes_DMS_ArchivedDocument] FOREIGN KEY
    (
        [ArchivedDocID]
    ) REFERENCES [dbo].[DMS_ArchivedDocument] (
        [ArchivedDocID]
    ),
	CONSTRAINT [FK_DMS_ArchivedDocSearchIndexes_DMS_SearchFieldIndexes] FOREIGN KEY
    (
		[SearchIndexID]
    ) REFERENCES [dbo].[DMS_SearchFieldIndexes] (
        [SearchIndexID]
    )
	) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_IndexesSynchronization]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_IndexesSynchronization](
	[SynchID] [int] IDENTITY(1,1) NOT NULL,
	[ErpDocumentID] [int] NOT NULL,
	[ChangedFields] [xml] NOT NULL
	CONSTRAINT [PK_DMS_IndexesSynchronization] PRIMARY KEY CLUSTERED 
	(
		[SynchID] ASC
	),
	CONSTRAINT [FK_DMS_IndexesSynchronization_DMS_ErpDocument] FOREIGN KEY
    (
        [ErpDocumentID]
    ) REFERENCES [dbo].[DMS_ErpDocument] (
        [ErpDocumentID]
    )) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_Settings]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_Settings](
	[WorkerID] [int] NOT NULL,
	[SettingType] [int] NOT NULL,
	[Settings] [xml] NOT NULL,
	 CONSTRAINT [PK_DMS_Settings] PRIMARY KEY CLUSTERED 
	(
		[WorkerID] ASC,
		[SettingType] ASC
	)) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_TextExtensions]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_TextExtensions](
	[ExtensionID] [int] IDENTITY(1,1) NOT NULL,
	[ExtensionType] [varchar](10) NOT NULL,
 CONSTRAINT [PK_DMS_TextExtensions] PRIMARY KEY CLUSTERED 
(
	[ExtensionID] ASC
)) ON [PRIMARY]

INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.asp')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.aspx')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.atom')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.bat')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.cls')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.cpp')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.cs')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.csproj')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.css')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.doc')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.docx')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.dotx')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.frm')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.h')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.hrc')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.ini')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.java')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.jsp')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.odm')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.odt')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.odf')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.php')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.py')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.pyc')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.pyo')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.rb')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.rc')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.rtf')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.sh')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.tpl')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.txt')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.vb')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.vbp')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.vbproj')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.vbs')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.vcproj')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.vip')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.wri')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.xls')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.xlsx')
END
GO

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
	[SendingType] [int] NULL CONSTRAINT [DF_DMS_SOSEnvelope_SendingType]  DEFAULT (0),
	[CreationDate] [datetime] NULL CONSTRAINT DF_DMS_SOSEnvelope_CreationDate DEFAULT (getdate()),
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
	[TaxJournal] [varchar](8) NULL CONSTRAINT DF_DMS_SOSDocument_TaxJournal DEFAULT(''),
	[DocumentType] [varchar](50) NULL CONSTRAINT DF_DMS_SOSDocument_DocumentType DEFAULT(''),
	[FiscalYear] [varchar](4) NULL CONSTRAINT DF_DMS_SOSDocument_FiscalYear  DEFAULT (''),
	[SendingType] [int] NULL CONSTRAINT [DF_DMS_SOSDocument_SendingType]  DEFAULT ((0))
	CONSTRAINT [PK_DMS_SOSDocument] PRIMARY KEY CLUSTERED 
	(
		[AttachmentID] ASC
	),
	CONSTRAINT [FK_DMS_SOSDocument_DMS_Attachment] FOREIGN KEY ([AttachmentID])	REFERENCES [dbo].[DMS_Attachment] ([AttachmentID]),
	CONSTRAINT [FK_DMS_SOSDocument_DMS_SOSEnvelope] FOREIGN KEY([EnvelopeID]) REFERENCES [dbo].[DMS_SOSEnvelope] ([EnvelopeID])
	) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument2] ON [dbo].[DMS_SOSDocument] ([ArchivedDate] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument3] ON [dbo].[DMS_SOSDocument] ([EnvelopeID] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument4] ON [dbo].[DMS_SOSDocument] ([TaxJournal] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument5] ON [dbo].[DMS_SOSDocument] ([DocumentType] ASC) ON [PRIMARY]
CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument6] ON [dbo].[DMS_SOSDocument] ([TaxJournal] ASC, [DocumentType] ASC) ON [PRIMARY]
ALTER TABLE [dbo].[DMS_SOSDocument] NOCHECK CONSTRAINT [FK_DMS_SOSDocument_DMS_SOSEnvelope]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SOSConfiguration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_SOSConfiguration](
	[ParamID] [int] NOT NULL,
	[SubjectCode] [varchar](20) NOT NULL,
	[AncestorCode] [varchar](20) NOT NULL,
	[KeeperCode] [varchar](20) NOT NULL,
	[MySOSUser] [varchar](50) NOT NULL,
	[MySOSPassword] [varchar](40) NOT NULL,
	[DocClasses] [xml] NULL,
	[SOSWebServiceUrl] [varchar](128) NOT NULL,
	[ChunkDimension] [int] NULL CONSTRAINT DF_DMS_SOSConfig_ChunkDim DEFAULT(20),
	[EnvelopeDimension] [int] NULL CONSTRAINT DF_DMS_SOSConfig_EnvelopeDim DEFAULT(600),
	[FTPSend] [bit] NULL CONSTRAINT DF_DMS_SOSConfig_FTPSend DEFAULT(0),
	[FTPSharedFolder] [varchar](512) NULL CONSTRAINT DF_DMS_SOSConfig_FTPSharedFolder DEFAULT(''),
	[FTPUpdateDayOfWeek] [int] NULL CONSTRAINT DF_DMS_SOSConfig_FTPUpdateDayOfWeek DEFAULT(7)
	CONSTRAINT [PK_DMS_SOSConfiguration] PRIMARY KEY CLUSTERED 
	(
		[ParamID] ASC
	)) ON [PRIMARY]
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_WFAttachments]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_WFAttachments](
	[AttachmentID] [int] NOT NULL,
	[ApprovalStatus] [int] NOT NULL CONSTRAINT DF_DMS_WFAttachments_ApprovedStatus DEFAULT (0),	
	[WorkerID] [int] NOT NULL,
	[RequestDate] [datetime] NOT NULL CONSTRAINT DF_DMS_WFAttachments_ApprovedDate DEFAULT (getdate()),
	[ApproverID] [int] NULL,
	[ApprovalDate] [datetime] NULL,
	[RequestComments] [varchar] (1024) NULL,
	[ApprovalComments] [varchar] (1024) NULL,
	CONSTRAINT [PK_DMS_WFAttachments] PRIMARY KEY CLUSTERED 
	(
		[AttachmentID] ASC,
		[WorkerID] ASC
	)
	) ON [PRIMARY]
END
GO
