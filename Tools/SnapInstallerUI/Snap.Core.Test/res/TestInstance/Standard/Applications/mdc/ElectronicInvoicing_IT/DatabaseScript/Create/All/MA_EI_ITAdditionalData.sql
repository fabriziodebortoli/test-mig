IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[MA_EI_ITAdditionalData]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MA_EI_ITAdditionalData] (
    [FieldName] [varchar] (256) NOT NULL,
    [XMLSection] [int] NULL CONSTRAINT DF_EIITAdditi_XMLSection_00 DEFAULT (31981568),
    [Multiple] [char] (1) NULL CONSTRAINT DF_EIITAdditi_Multiple_00 DEFAULT ('0'),
    [Mandatory] [char] (1) NULL CONSTRAINT DF_EIITAdditi_Mandatory_00 DEFAULT ('0'),
    [DataType] [varchar] (256) NULL CONSTRAINT DF_EIITAdditi_DataType_00 DEFAULT (''),
    [MinLength] [smallint] NULL CONSTRAINT DF_EIITAdditi_MinLength_00 DEFAULT (0),
    [MaxLength] [smallint] NULL CONSTRAINT DF_EIITAdditi_MaxLength_00 DEFAULT (0),
    [UpperCase] [char] (1) NULL CONSTRAINT DF_EIITAdditi_UpperCase_00 DEFAULT ('0'),
    [MinValue] [smallint] NULL CONSTRAINT DF_EIITAdditi_MinValue_00 DEFAULT (0),
    [MaxValue] [smallint] NULL CONSTRAINT DF_EIITAdditi_MaxValue_00 DEFAULT (0),
    [Disabled] [char] (1) NULL CONSTRAINT DF_EIITAdditi_Disabled_00 DEFAULT ('0'),
    [FromVersion] [varchar] (8) NULL CONSTRAINT DF_EIITAdditi_DisabledFr_00 DEFAULT (''),
	[ToVersion] [varchar] (8) NULL CONSTRAINT DF_EIITAdditi_DisabledTo_00 DEFAULT (''),
	[ViewType] [varchar] (32) NULL CONSTRAINT DF_EIITAdditi_ViewType_00 DEFAULT (''),
	[NodeNumber] [varchar] (16) NULL CONSTRAINT DF_EIITAdditi_NodeNumb_00 DEFAULT (''),
    CONSTRAINT [PK_EI_ITAdditionalData] PRIMARY KEY NONCLUSTERED
    (
        [FieldName]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO