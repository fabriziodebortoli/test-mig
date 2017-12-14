
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) 
	DECLARE @documentTypeId as integer 
	SET @documentTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 5)
	DECLARE @batchTypeId as integer 
	SET @batchTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 7)
	DECLARE  @finderTypeId as integer 
	SET @finderTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 21)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Framework.TbWoormEngine.TbWoormEngine.TbWoormAskParameters'
WHERE( NameSpace = 'Framework.TbWoormEngine.TbWoormEngine.TbWoormRichiestaParametri' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
GO