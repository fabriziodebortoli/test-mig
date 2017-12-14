IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[MA_EICoding]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MA_EICoding] (
    [EIType] [int] NOT NULL DEFAULT(32112640),
    [EICode] [varchar] (8) NOT NULL,
    [Description] [varchar] (128) NULL CONSTRAINT DF_EICoding_Descriptio_00 DEFAULT (''),
    [Disabled] [char] (1) NULL CONSTRAINT DF_EICoding_Disabled_00 DEFAULT ('0'),
    CONSTRAINT [PK_EICoding] PRIMARY KEY NONCLUSTERED
    (
        [EIType],
        [EICode]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO
