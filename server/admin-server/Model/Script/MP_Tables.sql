if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Instances]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Instances] (
	[InstanceId] [int] IDENTITY (1, 1) NOT NULL ,
	[Name] [varchar] (50) NOT NULL ,
	[Customer] [varchar] (12) NULL CONSTRAINT DF_Instances_Customer DEFAULT(''),
	[Disabled] [bit] NULL CONSTRAINT DF_Instances_Disabled DEFAULT(0),
	CONSTRAINT [PK_MP_Instances] PRIMARY KEY NONCLUSTERED 
	(
		[InstanceId]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_ServerURLs]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_ServerURLs] (
	[InstanceId] [int] NOT NULL,
	[URLType] [int] NOT NULL,
	[URL] [varchar] (255) NULL CONSTRAINT DF_ServerURLs_URL DEFAULT(''),
	CONSTRAINT [PK_MP_ServerURLs] PRIMARY KEY NONCLUSTERED 
	(
		[InstanceId],
		[URLType]
	),
	CONSTRAINT [FK_MP_ServerURLs_Instances] FOREIGN KEY 
	(
		[InstanceId]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceId]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Subscriptions]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Subscriptions] (
	[SubscriptionId] [int] IDENTITY (1, 1) NOT NULL ,
	[Name] [varchar] (50) NOT NULL ,
	[ActivationToken] [varchar](max) NULL CONSTRAINT DF_Subscriptions_ActivationToken DEFAULT(''),
	[PurchaseId] [varchar] (50) NULL CONSTRAINT DF_Subscriptions_PurchaseId DEFAULT(''),
	[InstanceId] [int] NOT NULL,
	CONSTRAINT [PK_MP_Subscriptions] PRIMARY KEY NONCLUSTERED 
	(
		[SubscriptionId]
	),
	CONSTRAINT [FK_MP_Subscriptions_Instances] FOREIGN KEY 
	(
		[InstanceId]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceId]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SubscriptionSlots]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SubscriptionSlots] (
	[SubscriptionId] [int] NOT NULL,
	[SlotsXml] [varchar] (max) NULL CONSTRAINT DF_SubscriptionSlots_SlotsXml DEFAULT(''),
	CONSTRAINT [PK_MP_SubscriptionSlots] PRIMARY KEY NONCLUSTERED 
	(
		[SubscriptionId]
	),
	CONSTRAINT [FK_MP_Subscriptions_SubscriptionSlots] FOREIGN KEY 
	(
		[SubscriptionId]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionId]
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
	[SubscriptionId] [int] NOT NULL,
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
		[SubscriptionId]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionId]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Accounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Accounts] (
	[AccountName] [varchar] (255) NOT NULL,
	[Password] [varchar] (128) NOT NULL,
	[FullName] [varchar] (255) NULL CONSTRAINT DF_Accounts_FullName DEFAULT(''),
	[LoginFailedCount] [int] NULL CONSTRAINT DF_Accounts_LoginFailedCount  DEFAULT (0), 
	[Notes] [varchar] (500) NULL CONSTRAINT DF_Accounts_Notes DEFAULT(''),
	[Email] [varchar] (255) NULL CONSTRAINT DF_Accounts_Email DEFAULT(''),
	[PasswordNeverExpires] [bit] NULL CONSTRAINT DF_Accounts_PasswordNeverExpires DEFAULT(0),
	[MustChangePassword] [bit] NULL CONSTRAINT DF_Accounts_MustChangePassword DEFAULT(0),
	[CannotChangePassword] [bit] NULL CONSTRAINT DF_Accounts_CannotChangePassword DEFAULT(0),
	[PasswordExpirationDateCannotChange] [bit] NULL CONSTRAINT DF_Accounts_PasswordExpirationDateCannotChange DEFAULT(0),
	[PasswordExpirationDate] [datetime] NULL CONSTRAINT DF_Accounts_PasswordExpirationDate DEFAULT (getdate()),
	[Disabled] [bit] NULL CONSTRAINT DF_Accounts_Disabled DEFAULT (0),
	[Locked] [bit] NULL CONSTRAINT DF_Accounts_Locked DEFAULT (0),
	[ProvisioningAdmin] [bit] NULL CONSTRAINT DF_Accounts_ProvisioningAdmin DEFAULT (0),
	[WindowsAuthentication] [bit] NULL CONSTRAINT DF_Accounts_WindowsAuthentication DEFAULT (0),
	[PreferredLanguage] [varchar] (10) NULL CONSTRAINT DF_Accounts_PreferredLanguage DEFAULT (''),
	[ApplicationLanguage] [varchar] (10) NULL CONSTRAINT DF_Accounts_ApplicationLanguage DEFAULT (''),
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
	[AccountName] [varchar] (255) NOT NULL ,
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

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_InstanceAccounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_InstanceAccounts] (
	[AccountName] [varchar] (255) NOT NULL ,
	[InstanceId] [int] NOT NULL,
	CONSTRAINT [PK_MP_InstanceAccounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountName],
		[InstanceId]
	),
	CONSTRAINT [FK_MP_InstanceAccounts_Accounts] FOREIGN KEY 
	(
		[AccountName]
	) REFERENCES [dbo].[MP_Accounts] (
		[AccountName]
	),
	CONSTRAINT [FK_MP_InstanceAccounts_Instances] FOREIGN KEY 
	(
		[InstanceId]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceId]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SubscriptionAccounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SubscriptionAccounts] (
	[AccountName] [varchar] (255) NOT NULL ,
	[SubscriptionId] [int] NOT NULL,
	CONSTRAINT [PK_MP_SubscriptionAccounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountName],
		[SubscriptionId]
	),
	CONSTRAINT [FK_MP_SubscriptionAccounts_Accounts] FOREIGN KEY 
	(
		[AccountName]
	) REFERENCES [dbo].[MP_Accounts] (
		[AccountName]
	),
	CONSTRAINT [FK_MP_SubscriptionAccounts_Subscriptions] FOREIGN KEY 
	(
		[SubscriptionId]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionId]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SecurityTokens]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SecurityTokens] (
	[AccountName] [varchar] (255) NOT NULL ,
	[TokenType] [int] NOT NULL,
	[Token] [uniqueidentifier] NULL CONSTRAINT DF_SecurityTokens_Token DEFAULT(0x00),
	[CreationDate] [datetime] NULL CONSTRAINT DF_SecurityTokens_CreationDate DEFAULT(getdate()),
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