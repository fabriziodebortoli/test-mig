BEGIN
	DECLARE @newtypeid as integer 

	if not exists (select * from MSD_ObjectTypes where Type =30)
begin
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (30, 'Tabber')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
end

	if not exists (select * from MSD_ObjectTypes where Type =33)
begin
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (33, 'Toolbar')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
end

	if not exists (select * from MSD_ObjectTypes where Type =34)
begin
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (34, 'ToolbarButton')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
end

	if not exists (select * from MSD_ObjectTypes where Type =35)
begin
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (35, 'EmbeddedView')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
end
end
GO