if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ArchivedDocument' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('Barcode'))
ALTER TABLE [dbo].[DMS_ArchivedDocument] 
ADD [Barcode] [varchar] (48) NULL CONSTRAINT DF_DMS_ArchivedDocument_Barcode DEFAULT('')
GO

UPDATE [dbo].[DMS_ArchivedDocument] SET [dbo].[DMS_ArchivedDocument].[Barcode] = '' 
WHERE [dbo].[DMS_ArchivedDocument].[Barcode] IS NULL
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_DocumentLinks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
DROP TABLE [dbo].[DMS_DocumentLinks]
END
GO