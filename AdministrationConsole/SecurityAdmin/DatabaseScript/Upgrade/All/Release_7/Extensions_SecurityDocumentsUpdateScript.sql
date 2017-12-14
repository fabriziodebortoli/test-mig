
if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) 
	DECLARE @documentTypeId as integer 
	SET @documentTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 5)
	DECLARE @batchTypeId as integer 
	SET @batchTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 7)
	DECLARE  @finderTypeId as integer 
	SET @finderTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 21)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.XEngine.TbXmlTransfer.ImportCriteria'
WHERE( NameSpace = 'Extensions.XEngine.TbXmlTransfer.CriteridiImportazione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.XEngine.TbXmlTransfer.ImportExportParameters'
WHERE( NameSpace = 'Extensions.XEngine.TbXmlTransfer.ParametridiImportExport' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.XEngine.TbXmlTransfer.SiteParameters'
WHERE( NameSpace = 'Extensions.XEngine.TbXmlTransfer.Parametridiconfigurazionedelsito' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'Extensions.XEngine.TbXmlTransfer.ExportCriteria'
WHERE( NameSpace = 'Extensions.XEngine.TbXmlTransfer.CriteridiEsportazione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
GO