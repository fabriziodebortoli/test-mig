BEGIN

DECLARE @newtypeid as integer 

SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 5)
INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 1024, 'Extended Browse')

END 

GO