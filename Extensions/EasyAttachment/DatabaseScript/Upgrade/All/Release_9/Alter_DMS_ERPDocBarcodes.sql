--adding barcodetype to manage more than one type
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ErpDocBarcodes' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('BarcodeType'))
ALTER TABLE [dbo].[DMS_ErpDocBarcodes] 
ADD [BarcodeType] [varchar] (24) NULL CONSTRAINT DF_DMS_ErpDocBarcodes_BarcodeType DEFAULT(NULL)
GO
UPDATE [dbo].[DMS_ErpDocBarcodes] SET [dbo].[DMS_ErpDocBarcodes].[BarcodeType] = COALESCE((Select   
CASE DMS_Settings.Settings.value('(/SettingState/Options/BarcodeDetectionOptions/BarcodeType)[1]', 'varchar(50)')
	When 'BC_CODE39' then 'CODE39'
	When 'BC_CODE128' then 'CODE128'
	When 'BC_EAN128' then 'EAN128'
END
from DMS_Settings where SettingType = 2), 'CODE39')
WHERE [dbo].[DMS_ErpDocBarcodes].[BarcodeType] IS NULL AND [dbo].[DMS_ErpDocBarcodes].[Barcode] IS NOT NULL and [dbo].[DMS_ErpDocBarcodes].[Barcode] <>''
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ErpDocBarcodes' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('Name'))
ALTER TABLE [dbo].[DMS_ErpDocBarcodes] 
ADD [Name] [varchar] (256) NULL CONSTRAINT DF_DMS_ErpDocBarcodes_Name DEFAULT(NULL)
GO

UPDATE [dbo].[DMS_ErpDocBarcodes] SET [dbo].[DMS_ErpDocBarcodes].[Name] = '' WHERE [Name] IS NULL
GO

