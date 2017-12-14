 BEGIN
delete grantRow 
  FROM [MSD_ObjectTypeGrants]grantRow 
  join MSD_ObjectTypes on MSD_ObjectTypes.TypeId = grantRow.TypeId
  where MSD_ObjectTypes.Type =34 

  	DECLARE @newtypeid as integer 
		SET @newtypeid = (select typeid from MSD_ObjectTypes where Type = 34)
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
end
GO