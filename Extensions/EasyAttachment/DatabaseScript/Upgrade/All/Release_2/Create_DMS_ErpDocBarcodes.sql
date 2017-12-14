if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_ErpDocBarcodes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DMS_ErpDocBarcodes](
	[Barcode] [varchar](48) NOT NULL,	
	[Notes] [varchar](128) NULL CONSTRAINT DF_DMS_ErpDocBarcodes_Notes DEFAULT(''),
	[ErpDocumentID] [int] NOT NULL,
	CONSTRAINT [PK_DMS_ErpDocBarcodes] PRIMARY KEY CLUSTERED 
	(
		[Barcode] ASC,
		[ErpDocumentID] ASC
	),
    CONSTRAINT [FK_DMS_ErpDocBarcodes_DMS_ErpDocument] FOREIGN KEY
    (
        [ErpDocumentID]
    ) REFERENCES [dbo].[DMS_ErpDocument] (
        [ErpDocumentID]
    )) ON [PRIMARY]
END
GO