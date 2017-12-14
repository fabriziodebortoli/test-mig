BEGIN

DECLARE @newtypeid as integer 

SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 11)
INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) Values (@newtypeid, 64, 'Edit Query')

END 

GO