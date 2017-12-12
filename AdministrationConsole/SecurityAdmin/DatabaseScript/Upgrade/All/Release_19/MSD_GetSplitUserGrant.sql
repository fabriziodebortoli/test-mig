SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_GetSplitUserGrant]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_GetSplitUserGrant]
GO


CREATE PROCEDURE MSD_GetSplitUserGrant
	(
		@par_companyid int, @par_userid int, @par_objectid int, 
		
		@parout_thereis_usrgrant integer OUTPUT, 
			@parout_usr_grant integer OUTPUT, 
			@parout_usr_inheritmask integer OUTPUT, 
			
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
/*BEGIN*/

/*Init OUTPUT parameters*/
SET @parout_thereis_rolegrant = 0
SET @parout_role_grant = 0  
SET @parout_role_inheritmask = 0  

SET @parout_thereis_usrgrant = 0
SET @parout_usr_inheritmask = 0
SET @parout_usr_grant = 0

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

/*--- grant specifici dell'utente sull'oggetto ---*/
SELECT 
		@parout_usr_grant = Grants, 
		@parout_usr_inheritmask = InheritMask, 
		@parout_thereis_usrgrant = 1
	FROM MSD_ObjectGrants
	WHERE CompanyId =  @par_companyid AND ObjectId = @par_objectid AND
			LoginId = @par_userid

/*--- grant specifici dei ruoli a cui appartiene l'utente sull'oggetto ---*/
/*righe di grant dei ruoli a cui appartiene l'utente quindi cerco sui ruoli*/
/*e basta in join */
DECLARE cr_righe CURSOR LOCAL FAST_FORWARD FOR 

	SELECT Grants, InheritMask
		FROM MSD_ObjectGrants
		WHERE CompanyId =  @par_companyid AND ObjectId = @par_objectid AND
			RoleId IN 
			( 
				SELECT RoleId FROM MSD_CompanyRolesLogins
				WHERE CompanyId = @par_companyid AND 
				      LoginId = @par_userid
			)

OPEN cr_righe
FETCH cr_righe INTO @role_grant, @role_inheritmask
WHILE @@FETCH_STATUS = 0
BEGIN
	IF @parout_thereis_rolegrant = 0
	BEGIN
		SET @parout_role_grant = @role_grant
		SET @parout_role_inheritmask = @role_inheritmask
	END
	SET @parout_thereis_rolegrant = 1

	SET @parout_role_grant = @parout_role_grant | @role_grant 
	SET @parout_role_inheritmask = @parout_role_inheritmask | @role_inheritmask 

	FETCH cr_righe INTO @role_grant, @role_inheritmask
END

CLOSE cr_righe
DEALLOCATE cr_righe

/*Se un ruolo concede un grant, elimino una eventuale ereditarieta proveniente da un altro ruolo*/
IF @parout_thereis_rolegrant = 1 
BEGIN
	SET @i = 0
	SET @nbit = 1
	WHILE @i < 11
	BEGIN
		IF (@nbit & @parout_role_grant) <> 0
		BEGIN
			SET @parout_role_inheritmask = @parout_role_inheritmask & ~@nbit
		END

		SET @nbit = @nbit * 2
		SET @i = @i + 1
	END
END

/*---- Ricorsione sul parent: mi basta il totale che inserisco nei parametri relativi al PARENT ----*/
IF @parentid <> 0
BEGIN
	DECLARE	@thereis_parent_usrgrant integer
	DECLARE	@usr_parent_grant integer
	DECLARE @usr_parent_inheritmask integer

	DECLARE @thereis_parent_rolegrant integer
	DECLARE	@role_parent_grant integer
	DECLARE @role_parent_inheritmask integer

	DECLARE @thereis_parent_parentgrant integer
	DECLARE	@parent_parent_grant integer
	DECLARE @parent_parent_inheritmask integer

	DECLARE @parent_isprotected integer
	DECLARE @parent_exist integer

	DECLARE @parent_object_type integer

	EXEC MSD_GetSplitUserGrant @par_companyid, @par_userid, @parentid, 
			@thereis_parent_usrgrant OUTPUT, 
				@usr_parent_grant OUTPUT, 
				@usr_parent_inheritmask OUTPUT, 
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
	SET @subtotal_grant = @parout_role_grant
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
				IF (@parout_protected_object = 1) AND (@parout_thereis_usrgrant = 0)
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

IF @parout_thereis_usrgrant = 1 AND @thereis_subtotalgrant = 1
BEGIN
	SET @parout_thereis_totalgrant = 1 /* grant somma di usr + (role + parent) */
	SET @parout_total_inheritmask = @parout_usr_inheritmask
	SET @parout_total_grant = @parout_usr_grant

	SET @i = 0
	SET @nbit = 1
	WHILE @i < 11
	BEGIN
		IF (@nbit & @parout_total_inheritmask) <> 0
		BEGIN
			SET @parout_total_inheritmask = @parout_total_inheritmask & ~@nbit
			SET @parout_total_grant = @parout_total_grant & ~@nbit
			SET @parout_total_grant = @parout_total_grant | (@subtotal_grant & @nbit)
		END
		SET @nbit = @nbit * 2
		SET @i = @i + 1
	END
END

IF @parout_thereis_usrgrant = 1 AND @thereis_subtotalgrant = 0
BEGIN
	SET @parout_thereis_totalgrant = 1 /* grant somma di usr + (role + parent) */
	SET @parout_total_inheritmask = @parout_usr_inheritmask
	SET @parout_total_grant = @parout_usr_grant & ~@parout_usr_inheritmask
END

IF @parout_thereis_usrgrant = 0 AND @thereis_subtotalgrant = 1
BEGIN
	SET @parout_thereis_totalgrant = 1 /* grant somma di usr + (role + parent) */
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
	/*Non possono overraidare i permessi del padre */
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
IF @parout_thereis_totalgrant = 0 and @parout_protected_object = 1
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

