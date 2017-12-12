/*elimino grant di BROWSE sulle child window/rowview*/
DELETE FROM [MSD_ObjectTypeGrants]
WHERE ([grantMask] = 16 OR [grantMask] = 1024)
AND [typeid] IN
 (SELECT [typeid] FROM [MSD_ObjectTypes] WHERE [type] = 14 OR [type] = 6)

GO

BEGIN
	
DECLARE @newtypeinserted as integer
SET @newtypeinserted = 0

SELECT @newtypeinserted = [type] FROM [MSD_ObjectTypes] WHERE [type] = 30

IF @newtypeinserted = 0 
BEGIN

DECLARE @newtypeid as integer 

SET @newtypeid = 0

SELECT @newtypeid = [typeid] FROM [MSD_ObjectTypes] WHERE [type] = 14

INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 8, 'Delete Row')
INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 16, 'Add Row')

INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (30, 'Tabber')
	SET @newtypeid = @@IDENTITY
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (31, 'TileManager')
	SET @newtypeid = @@IDENTITY
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (32, 'Tile')
	SET @newtypeid = @@IDENTITY
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (33, 'Toolbar')
	SET @newtypeid = @@IDENTITY
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (34, 'ToolbarButton')
	SET @newtypeid = @@IDENTITY
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')

INSERT INTO MSD_ObjectTypes (Type, TypeName) Values (35, 'EmbeddedView')
	SET @newtypeid = @@IDENTITY
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1, 'Execute')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 2, 'Edit')
	INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 4, 'New')
END
END
GO

