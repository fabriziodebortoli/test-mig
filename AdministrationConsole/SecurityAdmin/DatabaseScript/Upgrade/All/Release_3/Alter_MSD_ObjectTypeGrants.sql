if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ObjectTypeGrants]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	DECLARE @newtypeid as integer 
	SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 11)
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 256, 'Export')
	
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (23, 'Documento Word')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
		
	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (24, 'Foglio Excel')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (25, 'Template Documento Word')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')

	INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (26, 'Template Foglio Excel')
		SET @newtypeid = @@IDENTITY
		INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
END
GO