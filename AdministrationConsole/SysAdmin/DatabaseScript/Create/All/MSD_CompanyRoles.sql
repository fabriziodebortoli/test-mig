if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CompanyRoles]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_CompanyRoles] (
	[CompanyId] [int] NOT NULL ,
	[RoleId] [int] IDENTITY (1, 1) NOT NULL ,
	[Role] [varchar] (50) NOT NULL ,
	[Description] [varchar] (255) NULL ,
	[Disabled] [bit] NOT NULL  DEFAULT (0),
	[LastModifyGrants] [datetime] NOT NULL DEFAULT (getdate()),
	[Email] [varchar] (255) NOT NULL DEFAULT (''),
	[ReadOnly] [bit] NOT NULL  DEFAULT (0),
	CONSTRAINT [PK_MSD_CompanyRoles] PRIMARY KEY  NONCLUSTERED 
	(
		[CompanyId],
		[RoleId]
	),
	CONSTRAINT [IX_MSD_CompanyRoles] UNIQUE  NONCLUSTERED 
	(
		[CompanyId],
		[Role]
	),
	CONSTRAINT [FK_MSD_CompanyRoles_Company] FOREIGN KEY 
	(
		[CompanyId]
	) REFERENCES [dbo].[MSD_Companies] (
		[CompanyId]
	)
)
END

GO


