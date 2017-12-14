/****** Object:  Table [DS_ValidationFKtoFix] ******/
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_ValidationFKtoFix]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_ValidationFKtoFix] (
	[FKToFixID] [int] IDENTITY(1,1) NOT NULL,
	[ProviderName] [varchar](20)  NULL CONSTRAINT DF_ValidFKtoFix_ProvName DEFAULT(''),
	[DocNamespace] [varchar](255)  NULL CONSTRAINT DF_ValidFKtoFix_DocName DEFAULT(''),
	[TableName] [varchar](255) NULL CONSTRAINT DF_ValidFKtoFix_Table DEFAULT(''),
	[FieldName] [varchar](255) NULL CONSTRAINT DF_ValidFKtoFix_Field DEFAULT(''),
	[ValueToFix] [varchar](255) NULL CONSTRAINT DF_ValidFKtoFix_Value DEFAULT(''),
	[RelatedErrors] [int]  NULL CONSTRAINT DF_ValidFKtoFix_RelErr DEFAULT(0),
	[ValidationDate] [datetime] NULL CONSTRAINT DF_ValidFKtoFix_ValDate DEFAULT('17991231'),
 CONSTRAINT [PK_DS_ValidationFKtoFix] PRIMARY KEY NONCLUSTERED 
(
	[FKToFixID] ASC
)) ON [PRIMARY]
END
GO