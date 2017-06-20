if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Instances]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Instances] (
	[InstanceKey] [varchar] (50) NOT NULL,
	[Description] [varchar] (255) NULL CONSTRAINT DF_Instances_Description DEFAULT(''),
	[Disabled] [bit] NULL CONSTRAINT DF_Instances_Disabled DEFAULT(0),
	CONSTRAINT [PK_MP_Instances] PRIMARY KEY NONCLUSTERED 
	(
		[InstanceKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_ServerURLs]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_ServerURLs] (
	[InstanceKey] [varchar] (50) NOT NULL,
	[URLType] [int] NOT NULL,
	[URL] [varchar] (255) NULL CONSTRAINT DF_ServerURLs_URL DEFAULT(''),
	CONSTRAINT [PK_MP_ServerURLs] PRIMARY KEY NONCLUSTERED 
	(
		[InstanceKey],
		[URLType]
	),
	CONSTRAINT [FK_MP_ServerURLs_Instances] FOREIGN KEY 
	(
		[InstanceKey]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Subscriptions]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN	
CREATE TABLE [dbo].[MP_Subscriptions] (
	[SubscriptionKey] [varchar] (50) NOT NULL,
	[Description] [varchar] (255) NULL CONSTRAINT DF_Subscriptions_Description DEFAULT(''),
	[ActivationToken] [varchar](max) NULL CONSTRAINT DF_Subscriptions_ActivationToken DEFAULT(''),
	[PreferredLanguage] [varchar] (10) NULL CONSTRAINT DF_Subscriptions_PreferredLanguage DEFAULT(''),
	[ApplicationLanguage] [varchar] (10) NULL CONSTRAINT DF_Subscriptions_ApplicationLanguage DEFAULT(''),
	[MinDBSizeToWarn] [int] NULL CONSTRAINT DF_Subscriptions_MinDBSizeToWarn DEFAULT(2044723), 
	[InstanceKey] [varchar] (50) NOT NULL,
	CONSTRAINT [PK_MP_Subscriptions] PRIMARY KEY NONCLUSTERED 
	(
		[SubscriptionKey]
	),
	CONSTRAINT [FK_MP_Subscriptions_Instances] FOREIGN KEY 
	(
		[InstanceKey]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SubscriptionSlots]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SubscriptionSlots] (
	[SubscriptionKey] [varchar] (50) NOT NULL,
	[SlotsXml] [varchar] (max) NULL CONSTRAINT DF_SubscriptionSlots_SlotsXml DEFAULT(''),
	CONSTRAINT [PK_MP_SubscriptionSlots] PRIMARY KEY NONCLUSTERED 
	(
		[SubscriptionKey]
	),
	CONSTRAINT [FK_MP_Subscriptions_SubscriptionSlots] FOREIGN KEY 
	(
		[SubscriptionKey]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Companies]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Companies] (
	[CompanyId] [int] IDENTITY (1, 1) NOT NULL ,
	[Name] [varchar] (50) NOT NULL ,
	[Description] [varchar] (255) NULL CONSTRAINT DF_Companies_Description DEFAULT(''),
	[CompanyDBServer] [varchar] (255) NULL CONSTRAINT DF_Companies_DBServer DEFAULT(''),
	[CompanyDBName] [varchar] (255) NULL CONSTRAINT DF_Companies_DBName DEFAULT(''),
	[CompanyDBOwner] [varchar] (255) NULL CONSTRAINT DF_Companies_DBOwner DEFAULT(''),
	[CompanyDBPassword] [varchar] (255) NULL CONSTRAINT DF_Companies_DBPassword DEFAULT(''),
	[Disabled] [bit] NULL CONSTRAINT DF_Companies_Disabled DEFAULT(0),
	[DatabaseCulture] [varchar] (10) NULL CONSTRAINT DF_Companies_DatabaseCulture DEFAULT (''),
	[IsUnicode] [bit] NULL CONSTRAINT DF_Companies_IsUnicode DEFAULT(0),
	[PreferredLanguage] [varchar] (10) NULL CONSTRAINT DF_Companies_PreferredLanguage DEFAULT(''),
	[ApplicationLanguage] [varchar] (10) NULL CONSTRAINT DF_Companies_ApplicationLanguage DEFAULT(''),
	[Provider] [varchar] (15) NULL CONSTRAINT DF_Companies_Provider DEFAULT(''),
	[SubscriptionKey] [varchar] (50) NOT NULL,
	[UseDMS] [bit] NULL CONSTRAINT DF_Companies_UseDMS DEFAULT(0),
	[DMSDBServer] [varchar] (255) NULL CONSTRAINT DF_Companies_DMSDBServer DEFAULT(''),
	[DMSDBName] [varchar] (255) NULL CONSTRAINT DF_Companies_DMSDBName DEFAULT(''),
	[DMSDBOwner] [varchar] (255) NULL CONSTRAINT DF_Companies_DMSDBOwner DEFAULT(''),
	[DMSDBPassword] [varchar] (255) NULL CONSTRAINT DF_Companies_DMSDBPassword DEFAULT(''),
	CONSTRAINT [PK_MP_Companies] PRIMARY KEY NONCLUSTERED 
	(
		[CompanyId]
	),
	CONSTRAINT [FK_MP_Companies_Subscriptions] FOREIGN KEY 
	(
		[SubscriptionKey]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Accounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Accounts] (
	[AccountName] [varchar] (128) NOT NULL,
	[Password] [varchar] (128) NOT NULL,
	[FullName] [varchar] (255) NULL CONSTRAINT DF_Accounts_FullName DEFAULT(''),
	[Notes] [varchar] (500) NULL CONSTRAINT DF_Accounts_Notes DEFAULT(''),
	[Email] [varchar] (255) NULL CONSTRAINT DF_Accounts_Email DEFAULT(''),
	[LoginFailedCount] [int] NULL CONSTRAINT DF_Accounts_LoginFailedCount  DEFAULT (0), 
	[PasswordNeverExpires] [bit] NULL CONSTRAINT DF_Accounts_PasswordNeverExpires DEFAULT(0),
	[MustChangePassword] [bit] NULL CONSTRAINT DF_Accounts_MustChangePassword DEFAULT(0),
	[CannotChangePassword] [bit] NULL CONSTRAINT DF_Accounts_CannotChangePassword DEFAULT(0),
	[PasswordExpirationDate] [datetime] NULL CONSTRAINT DF_Accounts_PasswordExpirationDate DEFAULT (getdate()),
	[PasswordDuration] [int] NULL CONSTRAINT DF_Accounts_PasswordDuration DEFAULT(90),
	[Disabled] [bit] NULL CONSTRAINT DF_Accounts_Disabled DEFAULT (0),
	[Locked] [bit] NULL CONSTRAINT DF_Accounts_Locked DEFAULT (0),
	[ProvisioningAdmin] [bit] NULL CONSTRAINT DF_Accounts_ProvisioningAdmin DEFAULT (0),
	[WindowsAuthentication] [bit] NULL CONSTRAINT DF_Accounts_WindowsAuthentication DEFAULT (0),
	[PreferredLanguage] [varchar] (10) NULL CONSTRAINT DF_Accounts_PreferredLanguage DEFAULT (''),
	[ApplicationLanguage] [varchar] (10) NULL CONSTRAINT DF_Accounts_ApplicationLanguage DEFAULT (''),
	[ExpirationDate] [datetime] NULL CONSTRAINT DF_Accounts_ExpirationDate DEFAULT('17530101'),
	CONSTRAINT [PK_MP_Accounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountName]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_CompanyAccounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_CompanyAccounts] (
	[AccountName] [varchar] (128) NOT NULL,
	[CompanyId] [int] NOT NULL,
	[Admin] [bit] NULL CONSTRAINT DF_CompanyAccounts_Admin DEFAULT (0),
	CONSTRAINT [PK_MP_CompanyAccounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountName],
		[CompanyId]
	),
	CONSTRAINT [FK_MP_CompanyAccounts_Accounts] FOREIGN KEY 
	(
		[AccountName]
	) REFERENCES [dbo].[MP_Accounts] (
		[AccountName]
	),
	CONSTRAINT [FK_MP_CompanyAccounts_Companies] FOREIGN KEY 
	(
		[CompanyId]
	) REFERENCES [dbo].[MP_Companies] (
		[CompanyId]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SubscriptionAccounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SubscriptionAccounts] (
	[AccountName] [varchar] (128) NOT NULL ,
	[SubscriptionKey] [varchar] (50) NOT NULL,
	CONSTRAINT [PK_MP_SubscriptionAccounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountName],
		[SubscriptionKey]
	),
	CONSTRAINT [FK_MP_SubscriptionAccounts_Accounts] FOREIGN KEY 
	(
		[AccountName]
	) REFERENCES [dbo].[MP_Accounts] (
		[AccountName]
	),
	CONSTRAINT [FK_MP_SubscriptionAccounts_Subscriptions] FOREIGN KEY 
	(
		[SubscriptionKey]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SecurityTokens]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SecurityTokens] (
	[AccountName] [varchar] (128) NOT NULL,
	[TokenType] [int] NOT NULL,
	[Token] [uniqueidentifier] NULL CONSTRAINT DF_SecurityTokens_Token DEFAULT(0x00),
	[ExpirationDate] [datetime] NULL CONSTRAINT DF_SecurityTokens_ExpirationDate DEFAULT(getdate()),
	[Expired] [bit] NULL CONSTRAINT DF_SecurityTokens_Expired DEFAULT (0),
	CONSTRAINT [PK_MP_SecurityTokens] PRIMARY KEY NONCLUSTERED 
	(
		[AccountName],
		[TokenType]
	),
	CONSTRAINT [FK_MP_SecurityTokens_Accounts] FOREIGN KEY 
	(
		[AccountName]
	) REFERENCES [dbo].[MP_Accounts] (
		[AccountName]
	)
)
END
GO