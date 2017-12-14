BEGIN
	DECLARE @typeid as int
	SET @typeid = 0
	SELECT @typeid = TypeId FROM dbo.MSD_ObjectTypes WHERE Type = 15 

	DELETE FROM MSD_ObjectTypeGrants WHERE Typeid = @typeid AND GrantMask = 8
	INSERT INTO MSD_ObjectTypeGrants (Typeid, GrantMask, GrantName) Values (@typeid, 8, 'Delete Row')
	INSERT INTO MSD_ObjectTypeGrants (Typeid, GrantMask, GrantName) Values (@typeid, 16, 'Add Row')
	INSERT INTO MSD_ObjectTypeGrants (Typeid, GrantMask, GrantName) Values (@typeid, 32, 'Show Row View')
END
GO



