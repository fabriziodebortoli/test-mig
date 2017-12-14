
/****** Object:  Table [DS_ValidationInfo] ******/
if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DS_ValidationInfo]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[DS_ValidationInfo] (
	[ProviderName] [varchar](20) NOT NULL,
	[DocTBGuid] [uniqueidentifier] NOT NULL,
	[ActionName] [varchar](255) NULL CONSTRAINT DF_ValidInfo_Action_Name DEFAULT(''),
	[DocNamespace] [varchar](255) NULL CONSTRAINT DF_ValidInfo_Namespace DEFAULT(''),
	[FKError] [char](1) NULL CONSTRAINT DF_ValidInfo_FK_FKErr DEFAULT('0'),
	[XSDError] [char](1) NULL CONSTRAINT DF_ValidInfo_FK_XSDErr DEFAULT('0'),
	[UsedForFilter] [char](1) NULL CONSTRAINT DF_ValidInfo_FK_Filter DEFAULT('0'),
	[MessageError] [text] NULL CONSTRAINT DF_ValidInfo_FK_MsgError DEFAULT(''),
	[ValidationDate] [datetime] NULL CONSTRAINT DF_ValidInfo_Valid_Date DEFAULT('17991231'),
 CONSTRAINT [PK_DS_ValidationInfo] PRIMARY KEY NONCLUSTERED 
(
	[ProviderName] ASC,
	[DocTBGuid] ASC
)) ON [PRIMARY]
END
GO
