BEGIN
	DECLARE @newtypeid as integer 

	if not exists (select * from MSD_ObjectTypes where Type =31)
begin
INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (31, 'TileManager')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
end

	if not exists (select * from MSD_ObjectTypes where Type =32)
begin
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (32, 'Tile')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')	
end
end
GO