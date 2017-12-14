BEGIN

update MSD_ObjectGrants

set

grants = grants | ((grants & 16) * 64),

InheritMask = (InheritMask & (~ 1024)) | (((InheritMask & (~ 1024)) & 16) * 64)

where objectid in 

(select MSD_Objects .objectid 

from MSD_Objects, MSD_ObjectTypes

where MSD_Objects.typeid = MSD_ObjectTypes.typeid and

MSD_ObjectTypes.type = 5)



END 

GO