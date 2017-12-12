
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) 
	DECLARE @tableTypeId as integer 
	SET @tableTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 10)
	DECLARE @viewTypeId as integer 
	SET @viewTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 13)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.XEngine.Dbl.XE_KeyExtension'
WHERE( NameSpace = 'Extensions.XEngine.XEngineDbl.XE_KeyExtension' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.TbAuditing.Dbl.AUDIT_Namespaces'
WHERE( NameSpace = 'Extensions.TbAuditing.TbAuditingDbl.AUDIT_Namespaces' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.TbAuditing.Dbl.AUDIT_Tables'
WHERE( NameSpace = 'Extensions.TbAuditing.TbAuditingDbl.AUDIT_Tables' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.XEngine.Dbl.XE_LostAndFound'
WHERE( NameSpace = 'Extensions.XEngine.XEngineDbl.XE_LostAndFound' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.XEngine.Dbl.XE_TransferParams'
WHERE( NameSpace = 'Extensions.XEngine.XEngineDbl.XE_TransferParams' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.XEngine.Dbl.XE_SiteParams'
WHERE( NameSpace = 'Extensions.XEngine.XEngineDbl.XE_SiteParams' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
GO