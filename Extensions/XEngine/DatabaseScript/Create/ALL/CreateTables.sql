if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_KeyExtension]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[XE_KeyExtension] (
	[KeyFieldName] [varchar] (32) COLLATE Latin1_General_CI_AS NOT NULL ,
	[KeyTableName] [varchar] (32) COLLATE Latin1_General_CI_AS NOT NULL ,
	[KeyExtension] [smallint] NULL ,
	[DocumentNamespace] [varchar] (256) COLLATE Latin1_General_CI_AS NULL CONSTRAINT DF_KeyExt_DocNamespace_00 DEFAULT(''), 
	CONSTRAINT [PK_XMLKeyExtension] PRIMARY KEY  NONCLUSTERED 
	(
		[KeyFieldName],
		[KeyTableName]
	)  
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_LostAndFound]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[XE_LostAndFound] (
	[OriginalKey] [varchar] (256) NOT NULL ,
	[KeyTableName] [varchar] (32) NOT NULL ,
	[DocumentNamespace] [varchar] (256) COLLATE Latin1_General_CI_AS NULL CONSTRAINT DF_LostFound_DocNamespace_00 DEFAULT(''),
	[UniversalKey] [varchar] (256) COLLATE Latin1_General_CI_AS NULL CONSTRAINT DF_LostFound_UKey_00 DEFAULT(''),
	[CreationDate] [datetime] NULL, 
	CONSTRAINT [PK_XLostAndFound] PRIMARY KEY  NONCLUSTERED 
	(
		[OriginalKey],
		[KeyTableName]
	)
)
END 
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_Parameters]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[XE_Parameters] 
(
	[IdParam] [smallint] NOT NULL,
	[DomainName] [varchar](64) COLLATE Latin1_General_CI_AS NULL CONSTRAINT [DF_XEParams_DomName_00]  DEFAULT (''),
	[SiteName] [varchar](64) COLLATE Latin1_General_CI_AS NULL CONSTRAINT [DF_XEParams_SiteName_00]  DEFAULT (''),
	[SiteCode] [varchar](4) COLLATE Latin1_General_CI_AS NULL CONSTRAINT [DF_XEParams_SiteCode_00]  DEFAULT (''),
	[EncodingType] [char](1) NULL CONSTRAINT [DF_XEParams_EncType_00]  DEFAULT ('1'),
	[ImportPath] [varchar](256) COLLATE Latin1_General_CI_AS NULL CONSTRAINT [DF_XEParams_ImpPath_00]  DEFAULT (''),
	[ExportPath] [varchar](256) COLLATE Latin1_General_CI_AS NULL CONSTRAINT [DF_XEParams_ExpPath_00]  DEFAULT (''),
	[MaxDoc] [smallint] NULL CONSTRAINT [DF_XEParams_MaxDoc_00] DEFAULT (10),
	[MaxKByte] [smallint] NULL CONSTRAINT [DF_XEParams_MaxKByte_00] DEFAULT (100),
	[UseEnvClassExt] [char](1) NULL CONSTRAINT [DF_XEParams_ClassExt_00]  DEFAULT ('0'),
	[EnvPaddingNum] [smallint] NULL CONSTRAINT [DF_XEParams_EnvPaddNum_00] DEFAULT (4),
	[UseAttribute] [char](1) NULL CONSTRAINT [DF_XEParams_UseAttr_00]  DEFAULT ('0'),
	[UseEnumAsNum] [char](1) NULL CONSTRAINT [DF_XEParams_UseEnums_00]  DEFAULT ('1')
 CONSTRAINT [PK_XE_Parameters] PRIMARY KEY NONCLUSTERED 
(
	[IdParam]
)
) ON [PRIMARY]
END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_Parameters]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	INSERT INTO [dbo].[XE_Parameters]([IdParam], [DomainName], [SiteName], [SiteCode])
	VALUES(1, 'DEFAULT_DOMAIN', 'DEFAULT_SITE', 'DESI')
END
GO
