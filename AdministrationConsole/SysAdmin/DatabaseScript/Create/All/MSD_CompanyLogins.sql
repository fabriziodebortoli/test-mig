if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CompanyLogins]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_CompanyLogins] (
	[CompanyId] [int] NOT NULL ,
	[LoginId] [int] NOT NULL ,
	[DBUser] [varchar] (50) NOT NULL  DEFAULT (''),
	[DBPassword] [varchar] (128) NOT NULL  DEFAULT (''),
	[DBWindowsAuthentication] [bit] NOT NULL  DEFAULT (0),
	[Admin] [bit] NOT NULL  DEFAULT (0),
	[LastModifyGrants] [datetime] NULL ,
	[Disabled] [bit] NOT NULL  DEFAULT (0),
	[EBDeveloper] [bit] NOT NULL  DEFAULT (0),
	[TB_Modified] [datetime] NULL,
	[OtherData] [varchar] (128) NULL CONSTRAINT DF_CompanyLogins_OtherData DEFAULT (''),
	[ssoid] [varchar] (40) NULL CONSTRAINT DF_CompanyLogins_ssoid DEFAULT (''),
	[InfinityData] [varchar] (256) NULL CONSTRAINT DF_CompanyLogins_InfinityData DEFAULT (''),
	[MMMENUID] [varchar](32) NULL CONSTRAINT DF_CompanyLogins_MMMENUID DEFAULT ('')
	CONSTRAINT [PK_MSD_CompanyLogins] PRIMARY KEY  NONCLUSTERED 
	(
		[CompanyId],
		[LoginId]
	),
	CONSTRAINT [FK_MSD_CompanyLogins_Logins] FOREIGN KEY 
	(
		[LoginId]
	) REFERENCES [dbo].[MSD_Logins] (
		[LoginId]
	)
)
END
GO