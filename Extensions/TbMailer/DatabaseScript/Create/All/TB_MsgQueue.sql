if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TB_MsgQueue]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[TB_MsgQueue] (
	[MsgID] [int] IDENTITY (1, 1) NOT NULL ,
	[LotId]	[int] NULL,
	[Fax] [varchar](16) NOT NULL CONSTRAINT [DF_TB_MsgQueue_Fax] DEFAULT (''),
    [Addressee] [varchar](128) NOT NULL CONSTRAINT [DF_TB_MsgQueue_Addressee] DEFAULT (''),
    [Address] [varchar](128) NOT NULL CONSTRAINT [DF_TB_MsgQueue_Address] DEFAULT (''),
    [Zip] [varchar](8) NOT NULL CONSTRAINT [DF_TB_MsgQueue_Zip] DEFAULT (''),
    [City] [varchar](32) NOT NULL CONSTRAINT [DF_TB_MsgQueue_City] DEFAULT (''),
    [County] [varchar](3) NOT NULL CONSTRAINT [DF_TB_MsgQueue_County] DEFAULT (''),
    [Country] [varchar](64) NOT NULL CONSTRAINT [DF_TB_MsgQueue_Country] DEFAULT (''),
    [Subject] [varchar](2000) NOT NULL CONSTRAINT [DF_TB_MsgQueue_Subject] DEFAULT (''),
	[DocNamespace] [varchar](256) COLLATE Latin1_General_CI_AS NOT NULL CONSTRAINT [DF_TB_MsgQueue_DocNamespace] DEFAULT (''),
    [DocPrimaryKey] [varchar](256) COLLATE Latin1_General_CI_AS NOT NULL CONSTRAINT [DF_TB_MsgQueue_DocPrimaryKey] DEFAULT (''),
    [AddresseeNamespace] [varchar](256) COLLATE Latin1_General_CI_AS NOT NULL CONSTRAINT [DF_TB_MsgQueue_AddresseeNamespace] DEFAULT (''),
    [AddresseePrimaryKey] [varchar](256) COLLATE Latin1_General_CI_AS NOT NULL CONSTRAINT [DF_TB_MsgQueue_AddresseePrimaryKey] DEFAULT (''),
    [DocFileName] [varchar](256) COLLATE Latin1_General_CI_AS NOT NULL CONSTRAINT [DF_TB_MsgQueue_DocFileName] DEFAULT (''),
    [DocPages] [int] NOT NULL CONSTRAINT [DF_TB_MsgQueue_DocPages] DEFAULT (0),
    [DocSize] [int] NOT NULL CONSTRAINT [DF_TB_MsgQueue_DocSize] DEFAULT (0),
	[DeliveryType] [int] NOT NULL CONSTRAINT [DF_TB_MsgQueue_DeliveryType] DEFAULT (0),
	[PrintType] [int] NOT NULL CONSTRAINT [DF_TB_MsgQueue_PrintType] DEFAULT (0),
    [DocImage] [image] NULL,
	CONSTRAINT [PK_TB_MsgQueue] PRIMARY KEY NONCLUSTERED ([MsgID]) ON [PRIMARY]
) ON [PRIMARY]
END
GO