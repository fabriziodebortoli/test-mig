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

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Subscriptions]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Subscriptions] (
	[SubscriptionId] [int] IDENTITY (1, 1) NOT NULL ,
	[Name] [varchar] (50) NOT NULL ,
	[ActivationKey] [varchar](max) NULL CONSTRAINT DF_Subscriptions_ActivationKey DEFAULT(''),
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
	[AccountId] [int] IDENTITY (1, 1) NOT NULL ,
	[AccountName] [varchar] (255) NOT NULL ,
	[Password] [varchar] (128) NOT NULL ,
	[FullName] [varchar] (255) NULL CONSTRAINT DF_Accounts_FullName DEFAULT(''),
	[Notes] [varchar] (500) NULL CONSTRAINT DF_Accounts_Notes DEFAULT(''),
	[Email] [varchar] (255) NULL CONSTRAINT DF_Accounts_Email DEFAULT(''),
	[PasswordNeverExpired] [bit] NULL CONSTRAINT DF_Accounts_PasswordNeverExpired DEFAULT(0),
	[MustChangePassword] [bit] NULL CONSTRAINT DF_Accounts_MustChangePassword DEFAULT(0),
	[CannotChangePassword] [bit] NULL CONSTRAINT DF_Accounts_CannotChangePassword DEFAULT(0),
	[ExpiredDateCannotChange] [bit] NULL CONSTRAINT DF_Accounts_ExpiredDateCannotChange DEFAULT(0),
	[ExpiredDatePassword] [datetime] NULL CONSTRAINT DF_Accounts_ExpiredDatePassword DEFAULT (getdate()),
	[Disabled] [bit] NULL CONSTRAINT DF_Accounts_Disabled DEFAULT (0),
	[Locked] [bit] NULL CONSTRAINT DF_Accounts_Locked DEFAULT (0),
	[ProvisioningAdmin] [bit] NULL CONSTRAINT DF_Accounts_ProvisioningAdmin DEFAULT (0),
	[WindowsAuthentication] [bit] NOT NULL  DEFAULT (0),
	[PreferredLanguage] [varchar] (10) NULL CONSTRAINT DF_Accounts_PreferredLanguage DEFAULT (''),
	[ApplicationLanguage] [varchar] (10) NULL CONSTRAINT DF_Accounts_ApplicationLanguage DEFAULT (''),
	CONSTRAINT [PK_MP_Accounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountId]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_CompanyAccounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_CompanyAccounts] (
	[AccountId] [int] NOT NULL ,
	[CompanyId] [int] NOT NULL,
	[Admin] [bit] NULL CONSTRAINT DF_CompanyAccounts_Admin DEFAULT (0),
	CONSTRAINT [PK_MP_CompanyAccounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountId],
		[CompanyId]
	),
	CONSTRAINT [FK_MP_CompanyAccounts_Accounts] FOREIGN KEY 
	(
		[AccountId]
	) REFERENCES [dbo].[MP_Accounts] (
		[AccountId]
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