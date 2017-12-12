SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_GetUserGrant]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_GetUserGrant]
GO


IF EXISTS (SELECT name FROM sysobjects WHERE NAME = 'MSD_GetUserGrant' and type = 'P')
	drop PROCEDURE MSD_GetUserGrant
GO
CREATE  PROCEDURE MSD_GetUserGrant
	(
		@par_companyid integer, 
		@par_userid integer, 
		@par_objectNameSpace varchar(512), 
		@par_objectType int,
		
		@parout_object_grant integer OUTPUT, 
		@parout_object_inheritmask integer OUTPUT
		
	) AS 

DECLARE		@v_thereis_objectgrant integer 
DECLARE		@v_protected_object integer 

/*Declare local variables */
DECLARE @v_thereis_usrgrant integer  
DECLARE @v_usr_grant integer  
DECLARE @v_usr_inheritmask integer  
			
DECLARE @v_thereis_rolegrant integer 
DECLARE @v_role_grant integer  
DECLARE @v_role_inheritmask integer 
			
/* DECLARE @v_thereis_parentgrant integer */
DECLARE @v_parent_grant integer  
DECLARE @v_parent_inheritmask integer 

DECLARE @thereis_subtotalgrant integer
DECLARE @subtotal_grant integer
DECLARE @subtotal_inheritmask integer

DECLARE @role_grant integer
DECLARE @role_inheritmask integer

DECLARE @v_parentns varchar(512)
DECLARE @v_parentid integer
DECLARE @v_objectid integer
DECLARE @v_objecttypeid integer
DECLARE @v_objecttype integer

DECLARE @i integer
DECLARE @nbit integer

/*BEGIN*/
SET @parout_object_inheritmask = 0
SET @parout_object_grant = 0

/*Init locals*/
SET @v_thereis_objectgrant = 0
SET @v_thereis_rolegrant = 0
SET @v_role_grant = 0  
SET @v_role_inheritmask = 0  

SET @v_thereis_usrgrant = 0
SET @v_usr_inheritmask = 0
SET @v_usr_grant = 0

/* SET @v_thereis_parentgrant = 0 */
SET @v_parent_inheritmask = 0
SET @v_parent_grant = 0

SET @thereis_subtotalgrant =0
SET @subtotal_grant =0
SET @subtotal_inheritmask =0

SET @role_grant =0
SET @role_inheritmask =0

SET @v_protected_object = 0

SET @v_parentid = 0
SET @v_parentns = ''
SET @v_objectid = 0
SET @v_objecttype = 0
SET @v_objecttypeid = 0

/*START*/

SELECT @v_objectid = ObjectId, 
	@v_parentid = ParentId, 
	@v_objecttypeid = MSD_Objects.TypeId
	FROM MSD_Objects 
	INNER JOIN MSD_ObjectTypes 
	ON MSD_ObjectTypes.TypeId = MSD_Objects.TypeId
	WHERE NameSpace = @par_objectNameSpace AND 
		MSD_ObjectTypes.Type = @par_objectType 

SELECT @v_protected_object = 1
	FROM MSD_ProtectedObjects
	WHERE MSD_ProtectedObjects.CompanyId = @par_companyid AND
		 MSD_ProtectedObjects.ObjectId = @v_objectid 

SET @v_objecttype = @par_objectType

/*--- grant specifici dell'utente sull'oggetto ---*/
SELECT 
		@v_usr_grant = Grants, 
		@v_usr_inheritmask = InheritMask, 
		@v_thereis_usrgrant = 1
	FROM MSD_ObjectGrants
	WHERE CompanyId =  @par_companyid AND
		ObjectId = @v_objectid AND
		LoginId = @par_userid

/*--- grant specifici dei ruoli a cui appartiene l'utente sull'oggetto ---*/
/*righe di grant dei ruoli a cui appartiene l'utente quindi cerco sui ruoli*/
/*e basta in join */
DECLARE cr_righe CURSOR LOCAL FAST_FORWARD FOR 
	SELECT Grants, InheritMask
		FROM MSD_ObjectGrants
		WHERE CompanyId =  @par_companyid AND ObjectId = @v_objectid AND
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
	IF @v_thereis_rolegrant = 0
	BEGIN
		SET @v_role_grant = @role_grant
		SET @v_role_inheritmask = @role_inheritmask
	END
	SET @v_thereis_rolegrant = 1

	SET @v_role_grant = @v_role_grant | @role_grant 
	SET @v_role_inheritmask = @v_role_inheritmask | @role_inheritmask 

	FETCH cr_righe INTO @role_grant, @role_inheritmask
END

CLOSE cr_righe
DEALLOCATE cr_righe

/*Se un ruolo concede un grant, elimino una eventuale ereditarietà proveniente da un altro ruolo*/
IF @v_thereis_rolegrant = 1 
BEGIN
	SET @i = 0
	SET @nbit = 1
	WHILE @i < 11
	BEGIN
		IF (@nbit & @v_role_grant) <> 0
		BEGIN
			SET @v_role_inheritmask = @v_role_inheritmask & ~@nbit
		END
		SET @nbit = @nbit * 2
		SET @i = @i + 1
	END
END

/*---- Ricorsione sul parent: mi basta il totale che inserisco nei parametri relativi al PARENT ----*/
/* non esistono casi
IF @v_parentns <> 0 AND NOT (@v_objecttype IN 
										(
											6,  /* finestra secondaria */
											8,  /* scheda */
											14, /* finestra dettaglio di riga */
											15, /* griglia */
											16, /* colonna di griglia */
											17,  /* controllo */
											TODO
										))
BEGIN
	DECLARE	@parent_grant integer
	DECLARE @parent_inheritmask integer
	DECLARE @parent_isprotected integer
	
	EXEC MSD_GetUserGrant @par_companyid, @par_userid, @v_parentns, 
			@v_thereis_parent_grant OUTPUT,
			@parent_grant OUTPUT, 
			@parent_inheritmask OUTPUT,
			@parent_isprotected OUTPUT
END
*/

/*----  Ho le tre componenti: calcolo il totale ----*/
/* Sommo i grant del parent a quelli dei ruoli dell'utente */
/*
IF @v_thereis_parentgrant = 1 AND @v_thereis_rolegrant = 1
BEGIN
	SET @thereis_subtotalgrant = 1 
	SET @subtotal_inheritmask = @v_role_inheritmask
	SET @subtotal_grant = @v_role_grant

	SET @i = 0
	SET @nbit = 1
	WHILE @i < 11
	BEGIN
		IF (@nbit & @subtotal_inheritmask) <> 0
		BEGIN
			SET @subtotal_grant = @subtotal_grant & ~@nbit
			SET @subtotal_grant = @subtotal_grant | (@v_parent_grant & @nbit)
		END
		SET @nbit = @nbit * 2
		SET @i = @i + 1
	END
END
*/

IF /* @v_thereis_parentgrant = 0 AND */ @v_thereis_rolegrant = 1
BEGIN
	SET @thereis_subtotalgrant = 1
	SET @subtotal_inheritmask = @v_role_inheritmask
	SET @subtotal_grant = @v_role_grant
END

/*
IF @v_thereis_parent_grant = 1 AND @v_thereis_rolegrant = 0
BEGIN
	SET @thereis_subtotalgrant = 1
	SET @subtotal_inheritmask = @parent_inheritmask
	SET @subtotal_grant = @parent_grant
END
*/

/*---- Sommo il subtotale precedente (role spec.+totale dal parent) allo spec. dell'utente ----*/
IF @v_thereis_usrgrant = 1 AND @thereis_subtotalgrant = 1
BEGIN
	SET @v_thereis_objectgrant = 1 /* grant somma di usr + (role + parent) */
	SET @parout_object_inheritmask = @v_usr_inheritmask
	SET @parout_object_grant = @v_usr_grant

	SET @i = 0
	SET @nbit = 1
	WHILE @i < 11
	BEGIN
		IF (@nbit & @parout_object_inheritmask) <> 0
		BEGIN
			SET @parout_object_grant = @parout_object_grant & ~@nbit
			SET @parout_object_grant = @parout_object_grant | (@subtotal_grant & @nbit)
		END
		SET @nbit = @nbit * 2
		SET @i = @i + 1
	END
END

IF @v_thereis_usrgrant = 1 AND @thereis_subtotalgrant = 0
BEGIN
	SET @v_thereis_objectgrant = 1 /* grant somma di usr + (role + parent) */
	SET @parout_object_inheritmask = @v_usr_inheritmask
	SET @parout_object_grant = @v_usr_grant & ~@v_usr_inheritmask
END

IF @v_thereis_usrgrant = 0 AND @thereis_subtotalgrant = 1
BEGIN
	SET @v_thereis_objectgrant = 1 /* grant somma di usr + (role + parent) */
	SET @parout_object_inheritmask = @subtotal_inheritmask
	SET @parout_object_grant = @subtotal_grant
END

/*---- ----*/
IF @v_protected_object = 1 AND @v_thereis_objectgrant = 0
BEGIN
	SET @parout_object_grant = @parout_object_grant | 0x20000000;
END

IF @v_protected_object = 0
BEGIN
	SET @parout_object_grant = @parout_object_grant | 0x40000000;
END

RETURN (0)

/* END PROCEDURE */
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

