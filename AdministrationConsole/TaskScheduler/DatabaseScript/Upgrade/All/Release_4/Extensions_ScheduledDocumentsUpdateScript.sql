
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledTasks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'Extensions.XEngine.TbXmlTransfer.ImportCriteria'
WHERE( Command = 'Extensions.XEngine.TbXmlTransfer.CriteridiImportazione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'Extensions.XEngine.TbXmlTransfer.ImportExportParameters'
WHERE( Command = 'Extensions.XEngine.TbXmlTransfer.ParametridiImportExport' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'Extensions.XEngine.TbXmlTransfer.SiteParameters'
WHERE( Command = 'Extensions.XEngine.TbXmlTransfer.Parametridiconfigurazionedelsito' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'Extensions.XEngine.TbXmlTransfer.ExportCriteria'
WHERE( Command = 'Extensions.XEngine.TbXmlTransfer.CriteridiEsportazione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
GO