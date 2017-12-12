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

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_SiteParams]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
DECLARE @oldtblcount as integer;
DECLARE @newtblcount as integer;
SELECT @oldtblcount = COUNT(*) FROM [dbo].[XE_SiteParams];
SELECT @newtblcount = COUNT(*) FROM [dbo].[XE_Parameters];

if @newtblcount = 0
BEGIN
if @oldtblcount = 1
INSERT INTO [dbo].[XE_Parameters]([IdParam],[DomainName],[SiteName],[SiteCode],[EncodingType],[ImportPath],[ExportPath])
SELECT [XE_SiteParams].[IdParam],
           [XE_SiteParams].[DomainName],
           [XE_SiteParams].[SiteName],
           [XE_SiteParams].[SiteCode],
           [XE_SiteParams].[EncodingType],
           [XE_SiteParams].[ImportPath],
           [XE_SiteParams].[ExportPath]
FROM [dbo].[XE_SiteParams]
WHERE [XE_SiteParams].[IdParam] = 1
ELSE
INSERT INTO [dbo].[XE_Parameters]([IdParam], [DomainName], [SiteName], [SiteCode]) VALUES(1, 'DEFAULT_DOMAIN', 'DEFAULT_SITE', 'DESI')
END
END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_TransferParams]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
DECLARE @oldtblcount as integer;
SELECT @oldtblcount = COUNT(*) FROM [dbo].[XE_TransferParams];
if @oldtblcount = 1
UPDATE [dbo].[XE_Parameters]
SET [XE_Parameters].[MaxDoc] = [XE_TransferParams].[MaxDoc],
[XE_Parameters].[MaxKByte] = [XE_TransferParams].[MaxKByte],
[XE_Parameters].[UseEnvClassExt] = [XE_TransferParams].[UseEnvClassExt],
[XE_Parameters].[EnvPaddingNum] = [XE_TransferParams].[EnvPaddingNum],
[XE_Parameters].[UseAttribute] = [XE_TransferParams].[UseAttribute],
[XE_Parameters].[UseEnumAsNum] = [XE_TransferParams].[UseEnumAsNum]
FROM [dbo].[XE_Parameters]
INNER JOIN [XE_TransferParams] ON [XE_Parameters].[IdParam] = [XE_TransferParams].[IdParam]
END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_TransferParams]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
DROP TABLE [XE_TransferParams]
END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_SiteParams]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
DROP TABLE [XE_SiteParams]
END
GO