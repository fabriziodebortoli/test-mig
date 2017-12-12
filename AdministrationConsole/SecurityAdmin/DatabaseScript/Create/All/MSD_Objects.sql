if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MSD_Objects] (
	[ObjectId] [int] IDENTITY (1, 1) NOT NULL ,
	[TypeId] [int] NOT NULL ,
	[ParentId] [int] NOT NULL ,
	[NameSpace] [varchar] (512) NOT NULL ,
	[Localize] [varchar] (64) NULL ,
	CONSTRAINT [PK_MSD_Objects] PRIMARY KEY  NONCLUSTERED 
	(
		[ObjectId]
	),
	CONSTRAINT [FK_MSD_Objects_ObjectTypes] FOREIGN KEY 
	(
		[TypeId]
	) REFERENCES [dbo].[MSD_ObjectTypes] (
		[TypeId]
	)
)

 CREATE  UNIQUE  INDEX [IX_MSD_Objects_NameSpace] ON [dbo].[MSD_Objects]([NameSpace], [TypeId])
 CREATE  INDEX [IX_MSD_Objects_TypeId] ON [dbo].[MSD_Objects]([TypeId])
 CREATE  INDEX [IX_MSD_Objects_ParentId] ON [dbo].[MSD_Objects]([ParentId])

END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TG_Objects]') and OBJECTPROPERTY(id, N'IsTrigger') = 1)
drop trigger [dbo].[TG_Objects]
GO

CREATE TRIGGER TG_Objects 
	ON MSD_Objects FOR INSERT, DELETE AS
		IF 
			EXISTS 
				( SELECT ObjectId 
				  FROM INSERTED
				  WHERE 
					INSERTED.ParentId <> 0
					AND
					NOT EXISTS ( 
						SELECT MSD_Objects.ObjectId 
						FROM  MSD_Objects 
						WHERE MSD_Objects.ObjectId = INSERTED.ParentId
						)
				)
			OR
			EXISTS	
				( SELECT ObjectId FROM DELETED
					WHERE EXISTS
						( 
							SELECT MSD_Objects.ParentId 
							FROM MSD_Objects
							WHERE MSD_Objects.ParentId = DELETED.ObjectId 
						)
				)
		BEGIN
			ROLLBACK TRANSACTION
		END
GO

