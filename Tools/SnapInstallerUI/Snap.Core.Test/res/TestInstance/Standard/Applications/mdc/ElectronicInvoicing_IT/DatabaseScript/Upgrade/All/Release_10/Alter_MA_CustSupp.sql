IF EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
    dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
    AND dbo.syscolumns.name = 'IPACode')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
   ALTER COLUMN [IPACode] [varchar] (7)
END
GO

IF EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
    dbo.sysobjects.name = 'MA_CustSuppBranches' AND dbo.sysobjects.id = dbo.syscolumns.id
    AND dbo.syscolumns.name = 'IPACode')
BEGIN
ALTER TABLE [dbo].[MA_CustSuppBranches]
   ALTER COLUMN [IPACode] [varchar] (7)
END
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'PermanentBranchCode')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
	ADD [PermanentBranchCode] [varchar] (8) NULL CONSTRAINT DF_CustSupp_PermBrCode_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[PermanentBranchCode] = '' WHERE [dbo].[MA_CustSupp].[PermanentBranchCode] IS NULL
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'FDISOCountryCode')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
	ADD [FDISOCountryCode] [varchar] (2) NULL CONSTRAINT DF_CustSupp_FDISOCCo_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDISOCountryCode] = '' WHERE [dbo].[MA_CustSupp].[FDISOCountryCode] IS NULL
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'FDFiscalCode')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
	ADD [FDFiscalCode] [varchar] (16) NULL CONSTRAINT DF_CustSupp_FDFisCode_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDFiscalCode] = '' WHERE [dbo].[MA_CustSupp].[FDFiscalCode] IS NULL
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'FDNaturalPerson')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
	ADD [FDNaturalPerson] [char] (1) NULL CONSTRAINT DF_CustSupp_FDNatPerson_00 DEFAULT ('0')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDNaturalPerson] = '0' WHERE [dbo].[MA_CustSupp].[FDNaturalPerson] IS NULL
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'FDCompanyName')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
	ADD [FDCompanyName] [varchar] (80) NULL CONSTRAINT DF_CustSupp_FDCompN_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDCompanyName] = '' WHERE [dbo].[MA_CustSupp].[FDCompanyName] IS NULL
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'FDName')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
	ADD [FDName] [varchar] (60) NULL CONSTRAINT DF_CustSupp_FDName_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDName] = '' WHERE [dbo].[MA_CustSupp].[FDName] IS NULL
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'FDLastName')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
	ADD [FDLastName] [varchar] (60) NULL CONSTRAINT DF_CustSupp_FDLastName_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDLastName] = '' WHERE [dbo].[MA_CustSupp].[FDLastName] IS NULL
GO


if NOT EXISTS (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FiscalRegime')
BEGIN
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FiscalRegime] [varchar] (8) NULL CONSTRAINT DF_CustSupp_FiscalReg_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FiscalRegime] = '' WHERE [dbo].[MA_CustSupp].[FiscalRegime] IS NULL
GO

if NOT EXISTS (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDFiscalCodeID')
BEGIN
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDFiscalCodeID] [varchar] (28) NULL CONSTRAINT DF_CustSupp_FDFiscalID_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDFiscalCodeID] = '' WHERE [dbo].[MA_CustSupp].[FDFiscalCodeID] IS NULL
GO

if NOT EXISTS (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDEORICode')
BEGIN
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDEORICode] [varchar] (17) NULL CONSTRAINT DF_CustSupp_FDEORICode_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDEORICode] = '' WHERE [dbo].[MA_CustSupp].[FDEORICode] IS NULL
GO

if NOT EXISTS (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDTitleCode')
BEGIN
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDTitleCode] [varchar] (8) NULL CONSTRAINT DF_CustSupp_FDTitleCode_00 DEFAULT ('')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDTitleCode] = '' WHERE [dbo].[MA_CustSupp].[FDTitleCode] IS NULL
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_CustSupp' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'SendByCertifiedEmail')
BEGIN
ALTER TABLE [dbo].[MA_CustSupp]
	ADD [SendByCertifiedEmail] [char] (1) NULL CONSTRAINT DF_CustSupp_SendByPEC_00 DEFAULT ('0')
END
GO

UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[SendByCertifiedEmail] = '0' WHERE [dbo].[MA_CustSupp].[SendByCertifiedEmail] IS NULL
GO



