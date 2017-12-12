if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ObjectGrants]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_ObjectGrants] (
	[CompanyId] [int] NOT NULL ,
	[ObjectId] [int] NOT NULL ,
	[LoginId] [int] NOT NULL DEFAULT (0),
	[RoleId] [int] NOT NULL DEFAULT (0),
	[Grants] [int] NOT NULL DEFAULT (0),
	[InheritMask] [int] NOT NULL DEFAULT (0),
	CONSTRAINT [PK_MSD_ObjectGrants] PRIMARY KEY  NONCLUSTERED 
	(
		[CompanyId],
		[ObjectId],
		[LoginId],
		[RoleId]
	),
	CONSTRAINT [FK_MSD_ObjectGrants_ProtectedObjects] FOREIGN KEY 
	(
		[CompanyId],
		[ObjectId]
	) REFERENCES [dbo].[MSD_ProtectedObjects] (
		[CompanyId],
		[ObjectId]
	)
)
END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TG_ObjectGrants]') and OBJECTPROPERTY(id, N'IsTrigger') = 1)
drop trigger [dbo].[TG_ObjectGrants]
GO

/* non copre tutti i casi: lo vedremo pi avanti*/
CREATE TRIGGER TG_ObjectGrants 
	ON MSD_ObjectGrants FOR INSERT, UPDATE AS
		IF 
			EXISTS 	
				( SELECT LoginId, RoleId 
				  FROM INSERTED
				  WHERE (LoginId = 0 AND RoleId = 0) 
					OR 
					(LoginId  <> 0 AND RoleId  <> 0) 
				)
		BEGIN
			ROLLBACK TRANSACTION
		END
GO
