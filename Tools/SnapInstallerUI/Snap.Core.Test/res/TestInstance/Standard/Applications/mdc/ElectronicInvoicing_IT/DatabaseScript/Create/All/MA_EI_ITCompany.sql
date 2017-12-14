IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[MA_EI_ITCompany]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[MA_EI_ITCompany] (
    [CompanyId] [int] NOT NULL,
    [SenderISOCountryCode] [varchar] (2) NULL CONSTRAINT DF_EIITCompany_SenderISOC_00 DEFAULT (''),
    [SenderFiscalCodeID] [varchar] (28) NULL CONSTRAINT DF_EIITCompany_SenderFisc_00 DEFAULT (''),
    [LastSentNo] [varchar] (5) NULL CONSTRAINT DF_EIITCompany_LastSentNo_00 DEFAULT ('1'),
    [FiscalRegime] [varchar] (8) NULL CONSTRAINT DF_EIITCompany_FiscalRegi_00 DEFAULT (''),
    [PermanentBranchCode] [varchar] (8) NULL CONSTRAINT DF_EIITCompany_PermanentB_00 DEFAULT (''),
	[IssuerSubject]	[int] NULL CONSTRAINT DF_EIITCompany_IssuerSubj_00 DEFAULT (32047104),
    [FDISOCountryCode] [varchar] (2) NULL CONSTRAINT DF_EIITCompany_FDISOCount_00 DEFAULT (''),
    [FDFiscalCodeID] [varchar] (28) NULL CONSTRAINT DF_EIITCompany_FDFiscalCo_00 DEFAULT (''),
    [FDFiscalCode] [varchar] (16) NULL CONSTRAINT DF_EIITCompany_FDFiscalCo_01 DEFAULT (''),
    [FDNaturalPerson] [char] (1) NULL CONSTRAINT DF_EIITCompany_FDNaturalP_00 DEFAULT ('0'),
    [FDCompanyName] [varchar] (80) NULL CONSTRAINT DF_EIITCompany_FDCompanyN_00 DEFAULT (''),
    [FDName] [varchar] (60) NULL CONSTRAINT DF_EIITCompany_FDName_00 DEFAULT (''),
    [FDLastName] [varchar] (60) NULL CONSTRAINT DF_EIITCompany_FDLastName_00 DEFAULT (''),
    [FDTitleCode] [varchar] (8) NULL CONSTRAINT DF_EIITCompany_FDTitleCod_00 DEFAULT (''),
    [FDEORICode] [varchar] (17) NULL CONSTRAINT DF_EIITCompany_FDEORICode_00 DEFAULT (''),
    [TIISOCountryCode] [varchar] (2) NULL CONSTRAINT DF_EIITCompany_TIISOCount_00 DEFAULT (''),
    [TIFiscalCodeID] [varchar] (28) NULL CONSTRAINT DF_EIITCompany_TIFiscalCo_00 DEFAULT (''),
    [TIFiscalCode] [varchar] (16) NULL CONSTRAINT DF_EIITCompany_TIFiscalCo_01 DEFAULT (''),
    [TINaturalPerson] [char] (1) NULL CONSTRAINT DF_EIITCompany_TINaturalP_00 DEFAULT ('0'),
    [TICompanyName] [varchar] (80) NULL CONSTRAINT DF_EIITCompany_TICompanyN_00 DEFAULT (''),
    [TIName] [varchar] (60) NULL CONSTRAINT DF_EIITCompany_TIName_00 DEFAULT (''),
    [TILastName] [varchar] (60) NULL CONSTRAINT DF_EIITCompany_TILastName_00 DEFAULT (''),
    [TITitleCode] [varchar] (8) NULL CONSTRAINT DF_EIITCompany_TITitleCod_00 DEFAULT (''),
    [TIEORICode] [varchar] (17) NULL CONSTRAINT DF_EIITCompany_TIEORICode_00 DEFAULT (''),
	[SenderTelephone] [varchar] (20) NULL CONSTRAINT DF_EIITCompany_SenderTelephone_00 DEFAULT(''),
	[Email] [varchar] (64) NULL CONSTRAINT DF_EIITCompany_Email_00 DEFAULT(''),
    CONSTRAINT [PK_EI_ITCompany] PRIMARY KEY NONCLUSTERED
    (
        [CompanyId]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO

