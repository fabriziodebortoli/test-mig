--adding SosKeyCode for SosConnector functionalty
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_CollectionsFields' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('SosKeyCode'))
ALTER TABLE [dbo].[DMS_CollectionsFields] 
ADD [SosKeyCode] [varchar] (80) NULL CONSTRAINT DF_DMS_CollectionsFields_SosKeyCode DEFAULT('')
GO

UPDATE [dbo].[DMS_CollectionsFields] SET [dbo].[DMS_CollectionsFields].[SosKeyCode] = [dbo].[DMS_CollectionsFields].[FieldName]
WHERE [dbo].[DMS_CollectionsFields].[SosKeyCode] IS NULL
GO
