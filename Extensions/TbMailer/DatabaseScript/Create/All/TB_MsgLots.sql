if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TB_MsgLots]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[TB_MsgLots] (
	[LotID] [int] IDENTITY (1, 1) NOT NULL ,
    [Description] [varchar](2000) NOT NULL CONSTRAINT [DF_TB_MsgLots_Description] DEFAULT (''),
	[Status] [int] NOT NULL CONSTRAINT [DF_TB_MsgLots_Status] DEFAULT (0),
	[IdExt] [int] NOT NULL CONSTRAINT [DF_TB_MsgLots_IdExt] DEFAULT (0),
    [StatusExt] [int] NOT NULL CONSTRAINT [DF_TB_MsgLots_StatusExt] DEFAULT (0),
    [StatusDescriptionExt] [varchar](64) NOT NULL CONSTRAINT [DF_TB_MsgLots_StatusDescriptionExt] DEFAULT (''),
    [ErrorExt] [int] NOT NULL CONSTRAINT [DF_TB_MsgLots_ErrorExt] DEFAULT (0),
	[DeliveryType] [int] NOT NULL CONSTRAINT [DF_TB_MsgLots_DeliveryType] DEFAULT (0),
	[PrintType]	[int] NOT NULL CONSTRAINT [DF_TB_MsgLots_PrintType] DEFAULT (0),
	[TotalAmount] [float] NOT NULL CONSTRAINT [DF_TB_MsgLots_TotalAmount] DEFAULT (0),
	[PrintAmount] [float] NOT NULL CONSTRAINT [DF_TB_MsgLots_PrintAmount] DEFAULT (0),
	[PostageAmount] [float] NOT NULL CONSTRAINT [DF_TB_MsgLots_PostageAmount] DEFAULT (0),
 	[SendAfter] [datetime] CONSTRAINT [DF_TB_MsgLots_SendAfter] DEFAULT('17991231') ,
    [TotalPages] [int] NOT NULL CONSTRAINT [DF_TB_MsgLots_TotalPages] DEFAULT (0),
	[Fax] [varchar](16)	NOT NULL CONSTRAINT [DF_TB_MsgLots_Fax] DEFAULT (''),
    [Addressee] [varchar](128) NOT NULL CONSTRAINT [DF_TB_MsgLots_Addressee] DEFAULT (''),
    [Address] [varchar](128) NOT NULL CONSTRAINT [DF_TB_MsgLots_Address] DEFAULT (''),
    [Zip] [varchar](8) NOT NULL CONSTRAINT [DF_TB_MsgLots_Zip] DEFAULT (''),
    [City] [varchar](32) NOT NULL CONSTRAINT [DF_TB_MsgLots_City] DEFAULT (''),
    [County] [varchar](3) NOT NULL CONSTRAINT [DF_TB_MsgLots_County] DEFAULT (''),
    [Country] [varchar](64)	NOT NULL CONSTRAINT [DF_TB_MsgLots_Country] DEFAULT (''),
    [AddresseeNamespace] [varchar](256) COLLATE Latin1_General_CI_AS NOT NULL CONSTRAINT [DF_TB_MsgLots_AddresseeNamespace] DEFAULT (''),
    [AddresseePrimaryKey] [varchar](256) COLLATE Latin1_General_CI_AS NOT NULL CONSTRAINT [DF_TB_MsgLots_AddresseePrimaryKey] DEFAULT (''),
	[Incongruous] [char](1) NOT NULL CONSTRAINT [DF_TB_MsgLots_Incongruous] DEFAULT ('0'),
	CONSTRAINT [PK_TB_MsgLots] PRIMARY KEY NONCLUSTERED ([LotID]) ON [PRIMARY] 
) ON [PRIMARY]
END
GO

