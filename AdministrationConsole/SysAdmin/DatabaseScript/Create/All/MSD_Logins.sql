if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Logins]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_Logins] (
	[LoginId] [int] IDENTITY (1, 1) NOT NULL ,
	[Login] [varchar] (50) NOT NULL ,
	[Password] [varchar] (128) NOT NULL ,
	[Description] [varchar] (255) NULL ,
	[PasswordNeverExpired] [bit] NOT NULL  DEFAULT (0),
	[UserMustChangePassword] [bit] NOT NULL  DEFAULT (0),
	[UserCannotChangePassword] [bit] NOT NULL  DEFAULT (0),
	[ExpiredDateCannotChange] [bit] NOT NULL  DEFAULT (0),
	[LastModifyGrants] [datetime] NOT NULL  DEFAULT (getdate()),
	[ExpiredDatePassword] [datetime] NOT NULL  DEFAULT (getdate()),
	[Disabled] [bit] NOT NULL  DEFAULT (0),
	[Email] [varchar] (255) NOT NULL DEFAULT (''),
	[WindowsAuthentication] [bit] NOT NULL  DEFAULT (0),
	[PreferredLanguage] [varchar] (10) NOT NULL DEFAULT (''),
	[AlternativeLanguage] [varchar] (10) NOT NULL  DEFAULT (''),
	[ApplicationLanguage] [varchar] (10) NOT NULL DEFAULT (''),
	[SmartClientAccess] [bit] NOT NULL DEFAULT (1),
	[WebAccess] [bit] NOT NULL DEFAULT (1),
	[ConcurrentAccess] [bit] NOT NULL DEFAULT (0),
	[PrivateAreaWebSiteAccess] [bit] NOT NULL DEFAULT (0),
	[Locked] [bit] NOT NULL DEFAULT (0),
	[LoginFailedCount] [int] NOT NULL DEFAULT (0), 
	CONSTRAINT [PK_MSD_Logins] PRIMARY KEY  NONCLUSTERED 
	(
		[LoginId]
	),
	CONSTRAINT [IX_MSD_Logins] UNIQUE  NONCLUSTERED 
	(
		[Login]
	)
)
END
GO