if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_CompanyRolesLogins]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_CompanyRolesLogins] (
	[CompanyId] [int] NOT NULL ,
	[RoleId] [int] NOT NULL ,
	[LoginId] [int] NOT NULL ,
	CONSTRAINT [PK_MSD_CompanyRolesLogins] PRIMARY KEY  NONCLUSTERED 
	(
		[CompanyId],
		[RoleId],
		[LoginId]
	),
	CONSTRAINT [FK_MSD_CompanyRolesLogins_CompanyRoles] FOREIGN KEY 
	(
		[CompanyId],
		[RoleId]
	) REFERENCES [dbo].[MSD_CompanyRoles] (
		[CompanyId],
		[RoleId]
	),
	CONSTRAINT [FK_MSD_CompanyRolesLogins_Logins] FOREIGN KEY 
	(
		[LoginId]
	) REFERENCES [dbo].[MSD_Logins] (
		[LoginId]
	)
)
END

GO


