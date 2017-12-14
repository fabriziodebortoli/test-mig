/****** Object:  Table [DS_ActionsLog] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_ActionsLog]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_ActionsLog](
	[LogId] [int] IDENTITY(1,1) NOT NULL,
	[ProviderName] [varchar](20) NOT NULL,
	[DocNamespace] [varchar](256) NOT NULL,
	[DocTBGuid] [uniqueidentifier] NOT NULL,
	[ActionType] [int] NOT NULL,
	[ActionData] [varchar](1999) NOT NULL,
	[SynchDirection] [int] NOT NULL,
	[SynchXMLData] [text] NOT NULL,
	[SynchStatus] [int] NOT NULL,
	[SynchMessage] [varchar](1024) NOT NULL
 CONSTRAINT [PK_DS_ActionsLog] PRIMARY KEY NONCLUSTERED 
(
	[LogId] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [DS_SynchronizationInfo] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_SynchronizationInfo]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_SynchronizationInfo](
	[DocTBGuid] [uniqueidentifier] NOT NULL,
	[ProviderName] [varchar](20) NOT NULL,
	[DocNamespace] [varchar](256) NOT NULL,
	[SynchStatus] [int] NOT NULL, 
	[SynchDate] [datetime]  NULL CONSTRAINT DF_SynchInfo_SynchDate DEFAULT('17991231') ,
	[SynchDirection] [int] NULL CONSTRAINT DF_SynchInfo_SynchDirection DEFAULT(31522816), 
	[LastAction] [int]  NULL CONSTRAINT DF_SynchInfo_LastAction DEFAULT(31588352),
	[WorkerID] [int] NULL CONSTRAINT DF_SynchInfo_WorkerID DEFAULT(0),
	[StartSynchDate] [datetime]  NULL CONSTRAINT DF_SynchInfo_StartSynchDate DEFAULT('17991231') ,
	[HasDirtyTbModified] [char](1) NOT NULL CONSTRAINT DF_SynchInfo_HasDirtyTbModified DEFAULT('0') ,
 CONSTRAINT [PK_DS_SynchronizationInfo] PRIMARY KEY NONCLUSTERED 
(
	[DocTBGuid],[ProviderName]  ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [DS_Providers] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_Providers]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_Providers](
	[Name] [varchar](20) NOT NULL,
	[Description] [varchar](256) NOT NULL,
	[Disabled] [char](1) NULL CONSTRAINT DF_Providers_Disabled DEFAULT ('0'),
	[ProviderUrl] [varchar](128) NULL CONSTRAINT DF_Providers_Url DEFAULT(''),
	[ProviderUser] [varchar](50) NULL CONSTRAINT DF_Providers_User DEFAULT(''),
	[ProviderPassword] [varchar](40) NULL CONSTRAINT DF_Providers_Password DEFAULT(''),
	[ProviderParameters] [varchar](512) NULL CONSTRAINT DF_Providers_Parameters DEFAULT(''),
	[IsEAProvider] [char](1) NULL CONSTRAINT DF_Providers_IsEAProvider DEFAULT ('0'),
	[IAFModules] [varchar](256) NULL CONSTRAINT DF_Providers_IAFModules DEFAULT(''),
	[TBGuid] [uniqueidentifier] NULL CONSTRAINT DF_Providers_TBGuid DEFAULT(0x00),
	[SkipCrtValidation] [char](1) NULL CONSTRAINT DF_Providers_SkipCrtValidation DEFAULT ('0')
 CONSTRAINT [PK_DS_Providers] PRIMARY KEY NONCLUSTERED 
(
	[Name] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [DS_Transcoding] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_Transcoding]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_Transcoding](
	[ProviderName] [varchar](20) NOT NULL,
	[ERPTableName] [varchar](50) NOT NULL,
	[ERPKey] [varchar](256) NOT NULL,
	[DocTBGuid] [uniqueidentifier] NULL CONSTRAINT DS_Transcoding_DocTBGuid DEFAULT(0x00),
	[EntityName] [varchar](128) NOT NULL,
	[EntityID] [varchar](256) NOT NULL,
 CONSTRAINT [PK_DS_Transcoding] PRIMARY KEY CLUSTERED 
(
	[ProviderName] ASC,
	[ERPTableName] ASC,
	[ERPKey] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [DS_ActionsQueue] ******/
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_ActionsQueue]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_ActionsQueue](
	[LogId] [int] IDENTITY(1,1) NOT NULL,
	[ProviderName] [varchar](20) NOT NULL,
	[ActionName] [varchar](256) NOT NULL,
	[SynchXMLData] [text] NULL CONSTRAINT DF_ActionsQueue_XMLData DEFAULT(''),
	[SynchStatus] [int] NOT NULL,
	[SynchDirection] [int] NOT NULL,
	[SynchFilters] [varchar](1999) NULL CONSTRAINT DF_ActionsQueue_Filters DEFAULT('')
 CONSTRAINT [PK_DS_ActionsQueue] PRIMARY KEY NONCLUSTERED 
(
	[LogId] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [DS_SynchroFilter] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_SynchroFilter]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_SynchroFilter](
	[DocNamespace] [varchar](256) NOT NULL,
	[ProviderName] [varchar](20) NOT NULL,
	[SynchroFilter] [varchar](1024) NULL CONSTRAINT DF_SynchroFilter_Filter DEFAULT('')
 CONSTRAINT [PK_DS_SynchroFilter] PRIMARY KEY CLUSTERED 
(
	[DocNamespace] ASC,
	[ProviderName] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [DS_AttachmentSynchroInfo] ******/
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_AttachmentSynchroInfo]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_AttachmentSynchroInfo] (
	[ProviderName] [varchar](20) NOT NULL,
	[DocTBGuid] [uniqueidentifier] NOT NULL,
	[AttachmentID] [int] NOT NULL,
	[DocNamespace] [varchar](256) NULL CONSTRAINT DF_AttSynch_DocNamespace DEFAULT(''),
	[SynchStatus] [int] NULL CONSTRAINT DF_AttSynch_SynchStatus DEFAULT(31457280),
	[SynchDate] [datetime] NULL CONSTRAINT DF_AttSynch_SynchDate DEFAULT('17991231'),
	[SynchDirection] [int] NULL CONSTRAINT DF_AttSynch_SynchDirection DEFAULT(31522816),
	[LastAction] [int] NULL CONSTRAINT DF_AttSynch_LastAction DEFAULT(31588352),
	[WorkerID] [int] NULL CONSTRAINT DF_AttSynch_WorkerID DEFAULT(0),
 CONSTRAINT [PK_DS_AttachmentSynchroInfo] PRIMARY KEY NONCLUSTERED 
(
	[ProviderName] ASC,
	[DocTBGuid] ASC,
	[AttachmentID] ASC	
)) ON [PRIMARY]
END
GO

/****** Object:  Table [DS_ValidationInfo] ******/
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_ValidationInfo]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_ValidationInfo] (
	[ProviderName] [varchar](20) NOT NULL,
	[DocTBGuid] [uniqueidentifier] NOT NULL,
	[ActionName] [varchar](255) NULL CONSTRAINT DF_ValidInfo_Action_Name DEFAULT(''),
	[DocNamespace] [varchar](255) NULL CONSTRAINT DF_ValidInfo_Namespace DEFAULT(''),
	[FKError] [char](1) NULL CONSTRAINT DF_ValidInfo_FK_FKErr DEFAULT('0'),
	[XSDError] [char](1) NULL CONSTRAINT DF_ValidInfo_FK_XSDErr DEFAULT('0'),
	[UsedForFilter] [char](1) NULL CONSTRAINT DF_ValidInfo_FK_Filter DEFAULT('0'),
	[MessageError] [text] NULL CONSTRAINT DF_ValidInfo_FK_MsgError DEFAULT(''),
	[ValidationDate] [datetime] NULL CONSTRAINT DF_ValidInfo_Valid_Date DEFAULT('17991231'),
 CONSTRAINT [PK_DS_ValidationInfo] PRIMARY KEY NONCLUSTERED 
(
	[ProviderName] ASC,
	[DocTBGuid] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [DS_ValidationFKtoFix] ******/
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_ValidationFKtoFix]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_ValidationFKtoFix] (
	[FKToFixID] [int] IDENTITY(1,1) NOT NULL,
	[ProviderName] [varchar](20)  NULL CONSTRAINT DF_ValidFKtoFix_ProvName DEFAULT(''),
	[DocNamespace] [varchar](255)  NULL CONSTRAINT DF_ValidFKtoFix_DocName DEFAULT(''),
	[TableName] [varchar](255) NULL CONSTRAINT DF_ValidFKtoFix_Table DEFAULT(''),
	[FieldName] [varchar](255) NULL CONSTRAINT DF_ValidFKtoFix_Field DEFAULT(''),
	[ValueToFix] [varchar](255) NULL CONSTRAINT DF_ValidFKtoFix_Value DEFAULT(''),
	[RelatedErrors] [int]  NULL CONSTRAINT DF_ValidFKtoFix_RelErr DEFAULT(0),
	[ValidationDate] [datetime] NULL CONSTRAINT DF_ValidFKtoFix_ValDate DEFAULT('17991231'),
 CONSTRAINT [PK_DS_ValidationFKtoFix] PRIMARY KEY NONCLUSTERED 
(
	[FKToFixID] ASC
)) ON [PRIMARY]
END
GO



