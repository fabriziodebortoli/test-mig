IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[MA_EI_ITDocAdditionalData]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MA_EI_ITDocAdditionalData] (
    [DocID] [int] NOT NULL,
    [DocSubID] [int] NOT NULL,
    [Line] [int] NOT NULL,
	[SubLine] [int] NOT NULL,
    [FieldName] [varchar] (256) NOT NULL,
    [FieldValue] [varchar] (256) NULL CONSTRAINT DF_EIITDocAdd_FieldValue_00 DEFAULT (''),
    CONSTRAINT [PK_EI_ITDocAdditionalData] PRIMARY KEY NONCLUSTERED
    (
        [DocID],
        [DocSubID],
        [Line],
		[SubLine],
        [FieldName]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO
