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

	SELECT @v_typeid = MSD_ObjectTypes.TypeId
		FROM MSD_ObjectTypes 
		WHERE MSD_ObjectTypes.Type = @par_objectType

	IF @v_typeid = 0  
		BEGIN
			ROLLBACK TRANSACTION
			return 1
		END

	IF @par_parentNamespace <> '' 
		BEGIN
			SELECT @v_parentid = MSD_Objects.ObjectId
				FROM MSD_Objects INNER JOIN MSD_ObjectTypes 
				ON MSD_ObjectTypes.TypeId = MSD_Objects.TypeId
				WHERE 
					MSD_Objects.NameSpace = @par_parentNamespace AND 
					MSD_ObjectTypes.Type = @par_parentType

			IF @v_parentid = 0  
			BEGIN
				ROLLBACK TRANSACTION
				RETURN 2
			END
		END 
	ELSE 
		BEGIN
			IF @par_objectType IN 
										(
											6,  /* finestra secondaria */
											8,  /* scheda */
											14, /* finestra dettaglio di riga */
											15, /* griglia */
											16, /* colonna di griglia */
											17,  /* controllo */
											30, 31, 32, 33, 34, 35, /*Tabber, TileManager, Tile, Toolbar, ToolbarButton, EmbeddedView*/
											36, /*PropertyGrid */
											37,
											38
										)
				BEGIN
					ROLLBACK TRANSACTION
					RETURN 5
				END
	END

	IF NOT EXISTS 
		( 
			SELECT MSD_Objects.ObjectId
			FROM MSD_Objects  
			WHERE 
				MSD_Objects.NameSpace = @par_objectNamespace AND
				MSD_Objects.TypeId = @v_typeid
		) 
		BEGIN
			
			INSERT MSD_Objects 
				(NameSpace, TypeId, ParentId, Localize)
			VALUES
				(@par_objectNamespace, @v_typeid, @v_parentid, @par_localize)

			IF @@error <> 0
				BEGIN
					ROLLBACK TRANSACTION
					RETURN 3
				END
		END ELSE BEGIN

			UPDATE MSD_Objects 
			SET
				ParentId = @v_parentid,
				localize = @par_localize
			WHERE namespace = @par_objectNamespace AND
				MSD_Objects.TypeId = @v_typeid

			IF @@error <> 0
				BEGIN
					ROLLBACK TRANSACTION
					RETURN 4
				END
		END

COMMIT TRANSACTION
RETURN (0)

/* END PROCEDURE */
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

