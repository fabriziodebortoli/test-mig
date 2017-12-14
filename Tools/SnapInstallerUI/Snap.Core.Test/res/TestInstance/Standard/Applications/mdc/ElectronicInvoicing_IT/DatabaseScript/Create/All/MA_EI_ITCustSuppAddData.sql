IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[MA_EI_ITCustSuppAddData]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MA_EI_ITCustSuppAddData] (
    [CustSupp] [varchar] (12) NOT NULL,
    [CustSuppType] [int] NOT NULL DEFAULT(3211264)
    CONSTRAINT [PK_EI_ITCustSuppAddData] PRIMARY KEY NONCLUSTERED
    (
        [CustSupp],
        [CustSuppType]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[MA_EI_ITCustSuppAddDataDet]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MA_EI_ITCustSuppAddDataDet] (
    [CustSupp] [varchar] (12) NOT NULL,
    [CustSuppType] [int] NOT NULL DEFAULT(3211264),
    [FieldName] [varchar] (256) NOT NULL,
	[FieldMessage] [int] DEFAULT 11599873,
    CONSTRAINT [PK_EI_ITCustSuppAddDataDet] PRIMARY KEY NONCLUSTERED
    (
        [CustSupp],
        [CustSuppType],
        [FieldName]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO