IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[MA_EIPaymentType]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MA_EIPaymentType] (
    [PaymentType] [int] NOT NULL,
    [EICode] [varchar] (8) NOT NULL,
    CONSTRAINT [PK_EIPaymentType] PRIMARY KEY NONCLUSTERED
    (
        [PaymentType],
        [EICode]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO
