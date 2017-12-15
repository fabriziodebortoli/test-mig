DELETE MSD_ObjectTypeGrants 
WHERE (GrantMask = 4 OR GrantMask = 8) AND TypeId IN (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 16)
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

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_InsertObject]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	drop procedure [dbo].[MSD_InsertObject]
GO

CREATE  PROCEDURE MSD_InsertObject
	(
		@par_companyid integer, 
		@par_userid integer, 
		@par_objectNamespace varchar(512), 
		@par_objectType int,
		@par_parentNamespace varchar(512),
		@par_parentType int,
		@par_localize varchar(64)
	) AS 

	DECLARE @v_parentid INTEGER
	DECLARE @v_typeid INTEGER
	
BEGIN TRANSACTION

	SET @v_parentid = 0
	SET @v_typeid = 0

	SELECT @v_typeid = MSD_ObjectTypes.typeid
		FROM MSD_ObjectTypes 
		WHERE MSD_ObjectTypes.Type = @par_objectType

	IF @v_typeid = 0  
		BEGIN
			ROLLBACK TRANSACTION
			return 1
		END

	IF NOT EXISTS 
		( 
			SELECT MSD_Objects.objectid
			FROM MSD_Objects  
			WHERE 
				MSD_Objects.NameSpace = @par_objectNamespace AND
				MSD_Objects.TypeId = @v_typeid
		) 
		BEGIN
	
			IF @par_parentNamespace <> '' 
				BEGIN
					SELECT @v_parentid = MSD_Objects.objectid
						FROM MSD_Objects INNER JOIN MSD_ObjectTypes 
						ON MSD_ObjectTypes.TypeId = MSD_Objects.TypeId
						WHERE 
							MSD_Objects.NameSpace = @par_parentNamespace AND 
							MSD_ObjectTypes.Type = @par_parentType

					IF @v_parentid = 0  
					BEGIN
						ROLLBACK TRANSACTION
						return 2
					END
				END
			
			INSERT MSD_Objects 
				(namespace, typeid, parentid, localize)
			VALUES
				(@par_objectNamespace, @v_typeid, @v_parentid, @par_localize)

			IF @@error <> 0
				BEGIN
					ROLLBACK TRANSACTION
					RETURN (3)
				END
		END
		else begin
			UPDATE MSD_Objects 
			SET
				localize = @par_localize,
				typeid = @v_typeid
			WHERE namespace = @par_objectNamespace

			IF @@error <> 0
				BEGIN
					ROLLBACK TRANSACTION
					RETURN 4
				END

		end

COMMIT TRANSACTION
RETURN (0)

/* END PROCEDURE */
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO