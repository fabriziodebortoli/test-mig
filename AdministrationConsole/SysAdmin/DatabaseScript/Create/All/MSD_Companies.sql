if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Companies]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_Companies] (
	[CompanyId] [int] IDENTITY (1, 1) NOT NULL ,
	[Company] [varchar] (50) NOT NULL ,
	[Description] [varchar] (255) NULL ,
	[ProviderId] [int] NOT NULL ,
	[CompanyDBServer] [varchar] (255) NULL ,
	[CompanyDBName] [varchar] (255) NULL ,
	[CompanyDBOwner] [int] NOT NULL  DEFAULT (0),
	[PreferredLanguage] [varchar] (10) NOT NULL  DEFAULT (''),
	[AlternativeLanguage] [varchar] (10) NOT NULL DEFAULT (''),
	[ApplicationLanguage] [varchar] (10) NOT NULL DEFAULT (''),
	[DBDefaultUser] [varchar] (50) NULL ,
	[DBDefaultPassword] [varchar] (128) NULL ,
	[CompanyDBWindowsAuthentication] [bit] NOT NULL DEFAULT (0),
	[UseTransaction] [bit] NOT NULL  DEFAULT (1),
	[UseSecurity] [bit] NOT NULL  DEFAULT (0),
	[UseAuditing] [bit] NOT NULL  DEFAULT (0),
	[UseKeyedUpdate] [bit] NOT NULL DEFAULT (1),
	[UseUnicode] [bit] NOT NULL DEFAULT (0),
	[Disabled] [bit] NOT NULL DEFAULT (0),
	[Updating] [bit] NOT NULL DEFAULT (0),
	[IsValid] [bit] NOT NULL DEFAULT (1),
	[DatabaseCulture] [int] NOT NULL DEFAULT (0),
	[SupportColumnCollation] [bit] NOT NULL DEFAULT (0),
	[IsSecurityLightMigrated] [bit] NOT NULL DEFAULT (0),
	[Port] [int] NOT NULL DEFAULT (0),
	[UseDBSlave] [bit] NOT NULL CONSTRAINT DF_Companies_UseDBSlave DEFAULT (0),
	[UseDataSynchro] [bit] NOT NULL CONSTRAINT DF_Companies_UseDataSynchro DEFAULT (0),
	[UseRowSecurity] [bit] NOT NULL CONSTRAINT DF_Companies_UseRowSecurity DEFAULT (0),
	CONSTRAINT [PK_MSD_Companies] PRIMARY KEY NONCLUSTERED 
	(
		[CompanyId]
	),
	CONSTRAINT [IX_MSD_Companies] UNIQUE  NONCLUSTERED 
	(
		[Company]
	),
	CONSTRAINT [FK_MSD_Companies_Logins] FOREIGN KEY 
	(
		[CompanyDBOwner]
	) REFERENCES [dbo].[MSD_Logins] (
		[LoginId]
	),
	CONSTRAINT [FK_MSD_Companies_Providers] FOREIGN KEY 
	(
		[ProviderId]
	) REFERENCES [dbo].[MSD_Providers] (
		[ProviderId]
	)
)
END
GO