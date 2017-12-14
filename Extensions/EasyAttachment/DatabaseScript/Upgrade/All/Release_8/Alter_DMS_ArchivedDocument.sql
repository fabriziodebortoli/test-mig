--adding barcodetype to manage more than one type
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ArchivedDocument' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('BarcodeType'))
ALTER TABLE [dbo].[DMS_ArchivedDocument] 
ADD [BarcodeType] [varchar] (24) NULL CONSTRAINT DF_DMS_ArchivedDocument_BarcodeType DEFAULT(NULL)
GO
UPDATE [dbo].[DMS_ArchivedDocument] SET [dbo].[DMS_ArchivedDocument].[BarcodeType] = 'CODE39'
WHERE [dbo].[DMS_ArchivedDocument].[BarcodeType] IS NULL AND [dbo].[DMS_ArchivedDocument].[Barcode] IS NOT NULL and [dbo].[DMS_ArchivedDocument].[Barcode] <>''
GO
