BEGIN

UPDATE MSD_ObjectGrants
SET grants = grants | 64
WHERE
(grants & 1) = 1 and
MSD_ObjectGrants.objectid IN 
	(
		SELECT MSD_Objects.objectid 
		FROM MSD_ObjectTypes, MSD_Objects
		WHERE 
			MSD_Objects.typeid = MSD_ObjectTypes.typeid 
			AND MSD_ObjectTypes.type = 11
 	)


END 

GO