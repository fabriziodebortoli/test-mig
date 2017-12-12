SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_GetSplitRoleGrant]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_GetSplitRoleGrant]
GO


CREATE PROCEDURE MSD_GetSplitRoleGrant
	(
		@par_companyid int, @par_roleid int, @par_objectid int, 
		
		@parout_thereis_rolegrant integer OUTPUT,
			@parout_role_grant integer OUTPUT, 
			@parout_role_inheritmask integer OUTPUT,
			
		@parout_thereis_parentgrant integer OUTPUT,
			@parout_parent_grant integer OUTPUT, 
			@parout_parent_inheritmask integer OUTPUT,
			
		@parout_thereis_totalgrant integer OUTPUT,
			@parout_total_grant integer OUTPUT, 
			@parout_total_inheritmask integer OUTPUT,

		@parout_protected_object integer OUTPUT,
		@parout_existparent_object integer OUTPUT
	) AS 

/*Declare local variables */
DECLARE @thereis_subtotalgrant integer
DECLARE @subtotal_grant integer
DECLARE @subtotal_inheritmask integer

DECLARE @role_grant integer
DECLARE @role_inheritmask integer

DECLARE @parentid int

DECLARE @i integer
DECLARE @nbit integer

DECLARE @object_type integer

/*Init OUTPUT parameters*/
SET @parout_thereis_rolegrant = 0
SET @parout_role_grant = 0  
SET @parout_role_inheritmask = 0  

SET @parout_thereis_parentgrant = 0
SET @parout_parent_inheritmask = 0
SET @parout_parent_grant = 0

SET @parout_thereis_totalgrant = 0
SET @parout_total_inheritmask = 0
SET @parout_total_grant = 0

/*Init locals variables*/
SET @thereis_subtotalgrant =0
SET @subtotal_grant =0
SET @subtotal_inheritmask =0

SET @role_grant =0
SET @role_inheritmask =0

/*START*/

SET @parout_protected_object = 0
SELECT @parout_protected_object = 1 
	FROM MSD_ProtectedObjects
	WHERE CompanyId = @par_companyid AND ObjectId = @par_objectid

/**/
SET @object_type = 0
SET @parentid = 0

SELECT 
	@parentid = MSD_Objects.ParentId, 
	@object_type = MSD_ObjectTypes.Type
FROM MSD_Objects, MSD_ObjectTypes
WHERE 
	MSD_Objects.ObjectId = @par_objectid AND
	MSD_Objects.TypeId = MSD_ObjectTypes.TypeId AND
	MSD_ObjectTypes.Type <> 10 /* tipo "vincolo" */

IF @parentid != 0
SET @parout_existparent_object = 1
ELSE
SET @parout_existparent_object = 0
/**/

/*--- grant specifici del Ruolo sull'oggetto ---*/
SELECT 
		@parout_Role_grant = Grants, 
		@parout_Role_inheritmask = InheritMask, 
		@parout_thereis_Rolegrant = 1
	FROM MSD_ObjectGrants
	WHERE CompanyId =  @par_companyid AND ObjectId = @par_objectid AND
			RoleId = @par_roleid

/*---- Ricorsione sul parent: mi basta il totale che inserisco nei parametri relativi al PARENT ----*/
IF @parentid <> 0
BEGIN
	
	DECLARE @thereis_parent_rolegrant integer
	DECLARE	@role_parent_grant integer
	DECLARE @role_parent_inheritmask integer

	DECLARE @thereis_parent_parentgrant integer
	DECLARE	@parent_parent_grant integer
	DECLARE @parent_parent_inheritmask integer
	DECLARE @parent_isprotected integer
	DECLARE @parent_exist integer

	DECLARE @parent_object_type integer

	EXEC MSD_GetSplitRoleGrant @par_companyid, @par_roleid, @parentid, 
			@thereis_parent_rolegrant OUTPUT,
				@role_parent_grant OUTPUT, 
				@role_parent_inheritmask OUTPUT,
			@thereis_parent_parentgrant OUTPUT,
				@parent_parent_grant OUTPUT, 
				@parent_parent_inheritmask OUTPUT,
			@parout_thereis_parentgrant OUTPUT,
				@parout_parent_grant OUTPUT, 
				@parout_parent_inheritmask OUTPUT,
				@parent_isprotected OUTPUT,
				@parent_exist OUTPUT

	SET @parent_object_type = 0
		
	SELECT 
		@parent_object_type = MSD_ObjectTypes.Type
	FROM MSD_Objects, MSD_ObjectTypes
	WHERE 
		MSD_Objects.ObjectId = @parentid AND
		MSD_Objects.TypeId = MSD_ObjectTypes.TypeId 

	if @parent_object_type = 7 OR @parent_object_type = 21 /* 7:Batch, 21:FinderDoc*/
	BEGIN
		/* 2: "Edit", 4: "New", 8: "Delete"*/
		SET @parout_parent_grant = @parout_parent_grant | (2|4)
		SET @parout_parent_inheritmask = @parout_parent_inheritmask | (2|4)
	END

	if @object_type = 15 /* 15: Bodyedit*/
	BEGIN
		/* 8: "Delete Row", 16: "Add/Insert Row", 32: "Show RowView" */
		SET @parout_parent_grant = @parout_parent_grant | (8|16|32)
		SET @parout_parent_inheritmask = @parout_parent_inheritmask | (8|16|32)
	END
END

/*----  Ho le tre componenti: calcolo il totale ----*/
/* Sommo i grant del parent a quelli dei ruoli dell'utente */
IF @parout_thereis_parentgrant = 1 AND @parout_thereis_rolegrant = 1
BEGIN
	SET @thereis_subtotalgrant = 1 /* grant somma di role + parent */
	SET @subtotal_inheritmask = @parout_role_inheritmask
	SET @subtotal_grant = @parout_role_grant

	SET @i = 0
	SET @nbit = 1
	WHILE @i < 11
	BEGIN
		IF (@nbit & @parout_role_inheritmask) <> 0 
		BEGIN
			IF ((@nbit & @parout_parent_grant) <> 0) AND ((@nbit & @parout_parent_inheritmask) <> 0)
				BEGIN
					/*elimino la condizione "proibita"*/
					SET @subtotal_grant = @subtotal_grant  | @nbit
					SET @subtotal_inheritmask = @subtotal_inheritmask & ~@nbit
				END
			ELSE 
				BEGIN
					SET @subtotal_inheritmask = @subtotal_inheritmask & ~@nbit
					SET @subtotal_grant = @subtotal_grant & ~@nbit
					SET @subtotal_grant = @subtotal_grant | (@parout_parent_grant & @nbit)
				END
		END

		SET @nbit = @nbit * 2
		SET @i = @i + 1
	END
END

IF @parout_thereis_parentgrant = 0 AND @parout_thereis_rolegrant = 1
BEGIN
	SET @thereis_subtotalgrant = 1
	SET @subtotal_inheritmask = @parout_role_inheritmask
	SET @subtotal_grant = @parout_role_grant  & ~@parout_role_inheritmask
END

IF @parout_thereis_parentgrant = 1 AND @parout_thereis_rolegrant = 0
BEGIN
	SET @thereis_subtotalgrant = 1
	SET @subtotal_inheritmask = @parout_parent_inheritmask
	SET @subtotal_grant = @parout_parent_grant

	SET @i = 0
	SET @nbit = 1
	WHILE @i < 11
	BEGIN
		IF (@nbit & @subtotal_inheritmask) <> 0 
		BEGIN
			IF ((@nbit & @subtotal_grant) <> 0) 
			BEGIN
				/*elimino la condizione "proibita"*/
				SET @subtotal_inheritmask = @subtotal_inheritmask & ~@nbit
				/*i grant introdotti da un oggetto sono di default negati */
				IF (@parout_protected_object = 1) AND (@parout_thereis_rolegrant = 0)
				BEGIN
					SET @subtotal_grant = @subtotal_grant & ~@nbit
				END
			END
		END

		SET @nbit = @nbit * 2
		SET @i = @i + 1
	END
END

/*---- Sommo il subtotale precedente (role spec.+totale dal parent) allo spec. dell'utente ----*/

IF  @thereis_subtotalgrant = 1
BEGIN
	SET @parout_thereis_totalgrant = 1 /* grant somma di role + parent */
	SET @parout_total_inheritmask = @subtotal_inheritmask
	SET @parout_total_grant = @subtotal_grant
END

SELECT 	@i = type
	FROM MSD_Objects INNER JOIN MSD_ObjectTypes
	ON MSD_Objects.TypeId = MSD_ObjectTypes.TypeId
WHERE ObjectId = @par_objectid 

IF @parout_thereis_parentgrant = 1 AND 
	@object_type IN 
		(
			6,  /* finestra secondaria */
			8,  /* scheda */
			14, /* finestra dettaglio di riga */
			15, /* griglia */
			16, /* colonna di griglia */
			17  /* controllo */
		)
BEGIN
	SET @parout_total_grant = (@parout_total_grant & @parout_parent_grant)
END

IF @object_type = 15 /* griglia */
BEGIN
	/*I permessi di aggiunta riga o cancellazione riga dipendono dall'edit/new */
	IF (((8|16) & @parout_total_grant) <> 0) 
		BEGIN
			IF (((2|4) & @parout_total_grant) = 0)
			BEGIN
				SET @parout_total_grant = (@parout_total_grant & ~(8|16))
			END
			ELSE 
			BEGIN
				IF (((2|4) & @parout_total_grant) <> (2|4))
				BEGIN
					SET @i = @parout_total_grant & (8|16)
					SET @parout_total_grant = (@parout_total_grant & ~(@i))
					SET @parout_total_inheritmask = @parout_total_inheritmask | @i
				END
			END
		
		END
END

/*---- ----*/
IF /*@parout_thereis_totalgrant = 0 and*/ @parout_protected_object = 1
BEGIN
	SET @parout_thereis_totalgrant = 1
END

RETURN (0)
/* END PROCEDURE */

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

