
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 
	DECLARE @referenceObjectTypeId as integer 
	SET @referenceObjectTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type =11)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.TBAuditing.TbAuditing.Users'
WHERE( NameSpace = 'Extensions.TBAuditing.TbAuditing.Utenti' AND 
TypeId = @referenceObjectTypeId
)
END
GO