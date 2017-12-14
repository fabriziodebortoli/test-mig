
/*Elimina il grant 'Delete' dall'oggetto Scheda*/
DELETE MSD_ObjectTypeGrants
WHERE GrantMask = 8 AND TypeId IN (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 8) 

GO 

/*Elimina le righe di grants di tipo 'Delete' dagli oggetti di tipo Scheda */
UPDATE MSD_ObjectGrants
SET 
MSD_ObjectGrants.grants = (grants & ~(8)),
MSD_ObjectGrants.inheritmask = (inheritmask & ~(8))
WHERE 
MSD_ObjectGrants.ObjectId 
IN
(select MSD_ObjectGrants.ObjectId from MSD_Objects, MSD_ObjectGrants, MSD_ObjectTypes
where 
MSD_ObjectGrants.ObjectId = MSD_Objects.ObjectId
and 
MSD_Objects.TypeId = MSD_ObjectTypes.TypeId 
and
MSD_ObjectTypes.Type = 8
)

GO

/*corregge il nome dell'oggetto*/
UPDATE MSD_ObjectTypes 
SET TypeName = 'Finder Documents'
WHERE Type = 21

GO

/*Elimina il grant 'Edit' dall'oggetto 'Finder Documents' */
DELETE MSD_ObjectTypeGrants
WHERE GrantMask = 2 AND TypeId IN (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 21) 

GO 

/*Elimina le righe di grants di tipo 'Edit' dagli oggetti di tipo 'Finder Documents' */
UPDATE MSD_ObjectGrants
SET 
MSD_ObjectGrants.grants = (grants & ~(2)),
MSD_ObjectGrants.inheritmask = (inheritmask & ~(2))
WHERE 
MSD_ObjectGrants.ObjectId 
IN
(select MSD_Objects.ObjectId from MSD_Objects, MSD_ObjectTypes
where 
MSD_Objects.TypeId = MSD_ObjectTypes.TypeId 
and
MSD_ObjectTypes.Type = 21
)

GO
/*Le colonne delle griglie ricevono il nuovo grant 'New' */
BEGIN
	DECLARE @column_typeid as integer 
	DECLARE @a as integer 

	SET @column_typeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 16)
	
	SET @a = (SELECT count(*) FROM MSD_ObjectTypeGrants WHERE TypeId = @column_typeid AND GrantMask = 4)
	
	IF @a = 0 
	BEGIN
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) VALUES (@column_typeid, 4, 'New')
	END
END 

GO

/*Le colonne delle griglie ricevono per il nuovo grant 'New' gli stessi permessi che hanno per il grant 'Edit' */
update MSD_ObjectGrants
set 
MSD_ObjectGrants.grants = (grants & ~(4)) | ((grants & 2) * 2),
MSD_ObjectGrants.inheritmask = (inheritmask & ~(4)) | ((inheritmask & 2) * 2)
where 
MSD_ObjectGrants.ObjectId 
in 
(select MSD_Objects.ObjectId from MSD_Objects, MSD_ObjectTypes
where 
MSD_Objects.TypeId = MSD_ObjectTypes.TypeId 
and
MSD_ObjectTypes.Type = 16
)

GO


