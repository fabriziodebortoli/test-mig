BEGIN
	DECLARE @newtypeid as integer 

INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (36, 'PropertyGrid')
	SET @newtypeid = @@IDENTITY
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (37, 'TilePanelTab')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
		
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (38, 'TilePanel')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
end
GO