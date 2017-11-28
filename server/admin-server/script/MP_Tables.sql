 if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Instances]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Instances] (
	[InstanceKey] [varchar] (50) NOT NULL,
	[Description] [varchar] (255) NULL CONSTRAINT DF_Instances_Description DEFAULT(''),
	[Disabled] [bit] NULL CONSTRAINT DF_Instances_Disabled DEFAULT(0),
	[Origin] [varchar] (20) NULL CONSTRAINT DF_Instances_Origin DEFAULT(''),
	[Tags] [varchar] (255) NULL CONSTRAINT DF_Instances_Tags DEFAULT(''),
	[UnderMaintenance] [bit] NULL CONSTRAINT DF_Instances_UnderMaintenance DEFAULT(0),
	[PendingDate] [datetime] NULL CONSTRAINT DF_Instances_PendingDate DEFAULT (DateAdd (day , 10 , getdate() )),
	[VerificationCode] [varchar] (50) NULL CONSTRAINT DF_Instances_VerificationCode DEFAULT (''),
	[Ticks] [int] NULL CONSTRAINT DF_Instances_Ticks DEFAULT (0),
	[SecurityValue] [varchar] (250) NULL CONSTRAINT DF_Instances_SecurityValue DEFAULT(''),
	CONSTRAINT [PK_MP_Instances] PRIMARY KEY NONCLUSTERED 
	(
		[InstanceKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_InstanceAccounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].MP_InstanceAccounts (
	[AccountName] [varchar] (128) NOT NULL ,
	[InstanceKey] [varchar] (50) NOT NULL,
	[Ticks] [int] NULL CONSTRAINT DF_InstanceAccounts_Ticks DEFAULT (0),
	CONSTRAINT [PK_MP_InstanceAccounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountName],
		[InstanceKey]
	),
	CONSTRAINT [FK_MP_InstanceAccounts_Accounts] FOREIGN KEY 
	(
		[AccountName]
	) REFERENCES [dbo].[MP_Accounts] (
		[AccountName]
	),
	CONSTRAINT [FK_MP_InstanceAccounts_Instances] FOREIGN KEY 
	(
		[InstanceKey]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_InstanceTBFS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_InstanceTBFS](
	[FileID] [int] IDENTITY(1,1) NOT NULL,
	[ParentID] [int] NULL,
	[InstanceKey] [varchar](50) NOT NULL CONSTRAINT [DF_InstanceTBFS_InstanceKey] DEFAULT (''),
	[PathName] [varchar](512) NOT NULL CONSTRAINT [DF_InstanceTBFS_PathName] DEFAULT (''),
	[FileName] [varchar](256) NOT NULL CONSTRAINT [DF_InstanceTBFS_FileName] DEFAULT (''),
	[FileType] [varchar](20) NOT NULL CONSTRAINT [DF_InstanceTBFS_FileType] DEFAULT ('DIRECTORY'),
	[CompleteFileName] [varchar](780) NOT NULL CONSTRAINT [DF_InstanceTBFS_CompleteFileName] DEFAULT (''),
	[FileSize] [int] NOT NULL CONSTRAINT [DF_InstanceTBFS_FileSize] DEFAULT (0),
	[Namespace] [varchar](256) NOT NULL CONSTRAINT [DF_InstanceTBFS_Namespace] DEFAULT (''),
	[Application] [varchar](20) NOT NULL CONSTRAINT [DF_InstanceTBFS_Application] DEFAULT (''),
	[Module] [varchar](40) NOT NULL CONSTRAINT [DF_InstanceTBFS_Module] DEFAULT (''),
	[ObjectType] [varchar](50) NOT NULL CONSTRAINT [DF_InstanceTBFS_ObjectType] DEFAULT (''),
	[CreationTime] [datetime] NOT NULL CONSTRAINT [DF_InstanceTBFS_CreationTime] DEFAULT (getdate()),
	[LastWriteTime] [datetime] NOT NULL CONSTRAINT [DF_InstanceTBFS_LastWriteTime] DEFAULT (getdate()),
	[IsDirectory] [char](1) NOT NULL CONSTRAINT [DF_InstanceTBFS_IsDirectory] DEFAULT ('0'),
	[IsReadOnly] [char](1) NOT NULL CONSTRAINT [DF_InstanceTBFS_IsReadOnly] DEFAULT ('0'),
	[FileContent] [varbinary](max) NULL,
	[FileTextContent] [varchar](max) NULL,
	CONSTRAINT [PK_MP_InstanceTBFS] PRIMARY KEY NONCLUSTERED 
	(
		[FileID]
	),
	CONSTRAINT [FK_MP_InstanceTBFS_Instances] FOREIGN KEY 
	(
		[InstanceKey]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceKey]
	)
) 

CREATE INDEX [IX_MP_InstanceTBFS_1] ON [dbo].[MP_InstanceTBFS] ([CompleteFileName], [InstanceKey]) ON [PRIMARY]
CREATE INDEX [IX_MP_InstanceTBFS_2] ON [dbo].[MP_InstanceTBFS] ([FileType]) ON [PRIMARY]
CREATE INDEX [IX_MP_InstanceTBFS_3] ON [dbo].[MP_InstanceTBFS] ([FileName]) ON [PRIMARY]
CREATE INDEX [IX_MP_InstanceTBFS_4] ON [dbo].[MP_InstanceTBFS] ([PathName]) ON [PRIMARY]

END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_InstanceTBFS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
	ALTER TABLE [dbo].[MP_InstanceTBFS] WITH NOCHECK 
	ADD CONSTRAINT [FK_InstanceTBFS_InstanceTBFS] FOREIGN KEY 
	(
		[ParentID]
	) REFERENCES [dbo].[MP_InstanceTBFS] (
	[FileID]
	)
END
GO
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_InstanceTBFS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
	ALTER TABLE [dbo].[MP_InstanceTBFS] NOCHECK CONSTRAINT [FK_InstanceTBFS_InstanceTBFS]
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
	[Language] [varchar] (10) NULL CONSTRAINT DF_Subscriptions_Language DEFAULT(''),
	[RegionalSettings] [varchar] (10) NULL CONSTRAINT DF_Subscriptions_RegionalSettings DEFAULT(''),
	[MinDBSizeToWarn] [int] NULL CONSTRAINT DF_Subscriptions_MinDBSizeToWarn DEFAULT(2044723), 
	[UnderMaintenance] [bit] NULL CONSTRAINT DF_Subscriptions_UnderMaintenance DEFAULT(0),
	[Ticks] [int] NULL CONSTRAINT DF_Subscriptions_Ticks DEFAULT (0),
	CONSTRAINT [PK_MP_Subscriptions] PRIMARY KEY NONCLUSTERED 
	(
		[SubscriptionKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[[MP_SubscriptionInstances]]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN	
CREATE TABLE [dbo].[MP_SubscriptionInstances] (
	[SubscriptionKey] [varchar] (50) NOT NULL,
	[InstanceKey] [varchar] (50) NOT NULL,
	[Ticks] [int] NULL CONSTRAINT DF_SubscriptionInstances_Ticks DEFAULT (0),
	CONSTRAINT [PK_MP_SubscriptionInstances] PRIMARY KEY NONCLUSTERED 
	(
		[SubscriptionKey],
		[InstanceKey]
	),
	CONSTRAINT [FK_MP_SubscriptionInstances_Subscriptions] FOREIGN KEY 
	(
		[SubscriptionKey]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionKey]
	),
	CONSTRAINT [FK_MP_SubscriptionInstances_Instances] FOREIGN KEY 
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

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SubscriptionDatabases]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SubscriptionDatabases] (
	[InstanceKey] [varchar] (50) NOT NULL,
	[SubscriptionKey] [varchar] (50) NOT NULL,
	[Name] [varchar] (50) NOT NULL ,
	[Description] [varchar] (255) NULL CONSTRAINT DF_Databases_Description DEFAULT(''),
	[DBServer] [varchar] (255) NULL CONSTRAINT DF_Databases_DBServer DEFAULT(''),
	[DBName] [varchar] (255) NULL CONSTRAINT DF_Databases_DBName DEFAULT(''),
	[DBOwner] [varchar] (255) NULL CONSTRAINT DF_Databases_DBOwner DEFAULT(''),
	[DBPassword] [varchar] (255) NULL CONSTRAINT DF_Databases_DBPassword DEFAULT(''),
	[Disabled] [bit] NULL CONSTRAINT DF_Databases_Disabled DEFAULT(0),
	[DatabaseCulture] [varchar] (10) NULL CONSTRAINT DF_Databases_DatabaseCulture DEFAULT (''),
	[IsUnicode] [bit] NULL CONSTRAINT DF_Databases_IsUnicode DEFAULT(0),
	[Provider] [varchar] (15) NULL CONSTRAINT DF_Databases_Provider DEFAULT(''),
	[UseDMS] [bit] NULL CONSTRAINT DF_Databases_UseDMS DEFAULT(0),
	[DMSDBServer] [varchar] (255) NULL CONSTRAINT DF_Databases_DMSDBServer DEFAULT(''),
	[DMSDBName] [varchar] (255) NULL CONSTRAINT DF_Databases_DMSDBName DEFAULT(''),
	[DMSDBOwner] [varchar] (255) NULL CONSTRAINT DF_Databases_DMSDBOwner DEFAULT(''),
	[DMSDBPassword] [varchar] (255) NULL CONSTRAINT DF_Databases_DMSDBPassword DEFAULT(''),
	[Test] [bit] NULL CONSTRAINT DF_Databases_Test DEFAULT(0),
	[UnderMaintenance] [bit] NULL CONSTRAINT DF_Databases_UnderMaintenance DEFAULT(0),
	CONSTRAINT [PK_MP_SubscriptionDatabases] PRIMARY KEY NONCLUSTERED 
	(
		[InstanceKey],
		[SubscriptionKey],
		[Name]
	),
	CONSTRAINT [FK_MP_SubscriptionDatabases_Subscriptions] FOREIGN KEY 
	(
		[SubscriptionKey]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionKey]
	),
		CONSTRAINT [FK_MP_SubscriptionDatabases_Instances] FOREIGN KEY 
	(
		[InstanceKey]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SubscriptionExternalSources]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SubscriptionExternalSources] (
	[InstanceKey] [varchar] (50) NOT NULL,
	[SubscriptionKey] [varchar] (50) NOT NULL,
	[Source] [varchar] (50) NOT NULL ,
	[Description] [varchar] (255) NULL CONSTRAINT DF_ExternalSources_Description DEFAULT(''),
	[Provider] [varchar] (15) NULL CONSTRAINT DF_ExternalSources_Provider DEFAULT(''),
	[Server] [varchar] (255) NULL CONSTRAINT DF_ExternalSources_Server DEFAULT(''),
	[Database] [varchar] (255) NULL CONSTRAINT DF_ExternalSources_Database DEFAULT(''),
	[User] [varchar] (255) NULL CONSTRAINT DF_ExternalSources_User DEFAULT(''),
	[Password] [varchar] (255) NULL CONSTRAINT DF_ExternalSources_Password DEFAULT(''),
	[Disabled] [bit] NULL CONSTRAINT DF_ExternalSources_Disabled DEFAULT(0),
	[UnderMaintenance] [bit] NULL CONSTRAINT DF_ExternalSources_UnderMaintenance DEFAULT(0),
	[AdditionalInfo] [varchar] (255) NULL CONSTRAINT DF_ExternalSources_AdditionalInfo DEFAULT(''),
	CONSTRAINT [PK_MP_SubscriptionExternalSources] PRIMARY KEY NONCLUSTERED 
	(
		[InstanceKey],
		[SubscriptionKey],
		[Source]
	),
	CONSTRAINT [FK_MP_SubscriptionExternalSources_Subscriptions] FOREIGN KEY 
	(
		[SubscriptionKey]
	) REFERENCES [dbo].[MP_Subscriptions] (
		[SubscriptionKey]
	),
		CONSTRAINT [FK_MP_SubscriptionExternalSources_Instances] FOREIGN KEY 
	(
		[InstanceKey]
	) REFERENCES [dbo].[MP_Instances] (
		[InstanceKey]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Accounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Accounts] (
	[AccountName] [varchar] (128) NOT NULL,
	[Password] [varchar] (128) NOT NULL,
	[Salt] [varbinary] (128) NULL,
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
	[WindowsAuthentication] [bit] NULL CONSTRAINT DF_Accounts_WindowsAuthentication DEFAULT (0),
	[Language] [varchar] (10) NULL CONSTRAINT DF_Accounts_Language DEFAULT (''),
	[RegionalSettings] [varchar] (10) NULL CONSTRAINT DF_Accounts_RegionalSettings DEFAULT (''),
	[Ticks] [int] NULL CONSTRAINT DF_Accounts_Ticks DEFAULT (0),
	[ExpirationDate] [datetime] NULL CONSTRAINT DF_Accounts_ExpirationDate DEFAULT('17530101'),
	[ParentAccount] [varchar] (128) NULL CONSTRAINT DF_Accounts_ParentAccount DEFAULT (''),
	[Confirmed] [bit] NULL CONSTRAINT DF_Accounts_Confirmed DEFAULT (0),
	CONSTRAINT [PK_MP_Accounts] PRIMARY KEY NONCLUSTERED 
	(
		[AccountName]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_SubscriptionAccounts]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_SubscriptionAccounts] (
	[AccountName] [varchar] (128) NOT NULL ,
	[SubscriptionKey] [varchar] (50) NOT NULL,
	[Ticks] [int] NULL CONSTRAINT DF_SubscriptionAccounts_Ticks DEFAULT (0),
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

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_Roles]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_Roles] (
	[RoleName] [varchar] (50) NOT NULL,
	[Description] [varchar] (100) NULL CONSTRAINT DF_Roles_Description DEFAULT(''),
	[ParentRoleName] [varchar] (50) NULL CONSTRAINT DF_Roles_ParentRoleName DEFAULT(''),
	[Disabled] [bit] NULL CONSTRAINT DF_Roles_Disabled DEFAULT (0),
	CONSTRAINT [PK_MP_Roles] PRIMARY KEY NONCLUSTERED 
	(
		[RoleName]
	)
)
END
GO

if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_AccountRoles]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MP_AccountRoles] (
	[RoleName] [varchar] (50) NOT NULL,
	[AccountName] [varchar] (128) NOT NULL,
	[EntityKey] [varchar] (50) NOT NULL,
	[Level] [varchar] (50) NOT NULL CONSTRAINT DF_AccountRoles_Level DEFAULT (''),
	[Ticks] [int] NULL CONSTRAINT DF_AccountRoles_Ticks DEFAULT (0),
	CONSTRAINT [PK_MP_AccountRoles] PRIMARY KEY NONCLUSTERED 
	(
		[RoleName],
		[AccountName],
		[EntityKey]
	),
	CONSTRAINT [FK_MP_AccountRoles_Accounts] FOREIGN KEY 
	(
		[AccountName]
	) REFERENCES [dbo].[MP_Accounts] (
		[AccountName]
	),
	CONSTRAINT [FK_MP_AccountRoles_Roles] FOREIGN KEY 
	(
		[RoleName]
	) REFERENCES [dbo].[MP_Roles] (
		[RoleName]
	)
)
END
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MP_DeleteAccount]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MP_DeleteAccount]
GO

CREATE PROCEDURE MP_DeleteAccount(@par_account varchar(128))
AS
	BEGIN TRANSACTION
		DELETE MP_AccountRoles WHERE AccountName = @par_account
		DELETE MP_SecurityTokens WHERE AccountName = @par_account
		DELETE MP_SubscriptionAccounts WHERE AccountName = @par_account
		DELETE MP_Accounts WHERE AccountName = @par_account

		IF @@error <> 0 
        BEGIN
			ROLLBACK TRANSACTION
			RETURN(1)
        END

COMMIT TRANSACTION
RETURN (0)
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO