if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACode')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [IPACode] [varchar] (7) NULL CONSTRAINT DF_CustSupp_IPACode_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[IPACode] = '' WHERE [dbo].[MA_CustSupp].[IPACode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EORICode')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [EORICode] [varchar] (17) NULL CONSTRAINT DF_CustSupp_EORICodeo_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[EORICode] = '' WHERE [dbo].[MA_CustSupp].[EORICode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACode')
	ALTER TABLE [dbo].[MA_CustSuppBranches] 
	ADD [IPACode] [varchar] (7) NULL CONSTRAINT DF_CustSuppBranches_IPACode_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_CustSuppBranches] SET [dbo].[MA_CustSuppBranches].[IPACode] = '' WHERE [dbo].[MA_CustSuppBranches].[IPACode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'AdministrationReference')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [AdministrationReference] [varchar] (20) NULL CONSTRAINT DF_CustSupp_AdminRefer_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[AdministrationReference] = '' WHERE [dbo].[MA_CustSupp].[AdministrationReference] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'AdministrationReference')
	ALTER TABLE [dbo].[MA_CustSuppBranches] 
	ADD [AdministrationReference] [varchar] (20) NULL CONSTRAINT DF_CustSuppBranches_AdminRefer_00 DEFAULT('')
GO

	UPDATE [dbo].[MA_CustSuppBranches] SET [dbo].[MA_CustSuppBranches].[AdministrationReference] = '' WHERE [dbo].[MA_CustSuppBranches].[AdministrationReference] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ImmediateLikeAccompanying')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [ImmediateLikeAccompanying] [char] (1) NULL CONSTRAINT DF_CustSupp_ImmLikeAcc_00 DEFAULT('0')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[ImmediateLikeAccompanying] = '0' WHERE [dbo].[MA_CustSupp].[ImmediateLikeAccompanying] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ElectronicInvoicing')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [ElectronicInvoicing] [varchar] (1) NULL CONSTRAINT DF_CustSupp_Electronic_00 DEFAULT('0')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[ElectronicInvoicing] = '0' WHERE [dbo].[MA_CustSupp].[ElectronicInvoicing] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'PermanentBranchCode')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [PermanentBranchCode] [varchar] (8) NULL CONSTRAINT DF_CustSupp_PermBrCode_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[PermanentBranchCode] = '' WHERE [dbo].[MA_CustSupp].[PermanentBranchCode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDISOCountryCode')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDISOCountryCode] [varchar] (2) NULL CONSTRAINT DF_CustSupp_FDISOCCo_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDISOCountryCode] = '' WHERE [dbo].[MA_CustSupp].[FDISOCountryCode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDFiscalCode')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDFiscalCode] [varchar] (16) NULL CONSTRAINT DF_CustSupp_FDFisCode_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDFiscalCode] = '' WHERE [dbo].[MA_CustSupp].[FDFiscalCode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDNaturalPerson')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDNaturalPerson] [char] (1) NULL CONSTRAINT DF_CustSupp_FDNatPerson_00 DEFAULT ('0')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDNaturalPerson] = '0' WHERE [dbo].[MA_CustSupp].[FDNaturalPerson] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDCompanyName')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDCompanyName] [varchar] (80) NULL CONSTRAINT DF_CustSupp_FDCompN_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDCompanyName] = '' WHERE [dbo].[MA_CustSupp].[FDCompanyName] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDName')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDName] [varchar] (60) NULL CONSTRAINT DF_CustSupp_FDName_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDName] = '' WHERE [dbo].[MA_CustSupp].[FDName] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDLastName')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDLastName] [varchar] (60) NULL CONSTRAINT DF_CustSupp_FDLastName_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDLastName] = '' WHERE [dbo].[MA_CustSupp].[FDLastName] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FiscalRegime')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FiscalRegime] [varchar] (8) NULL CONSTRAINT DF_CustSupp_FiscalReg_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FiscalRegime] = '' WHERE [dbo].[MA_CustSupp].[FiscalRegime] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDFiscalCodeID')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDFiscalCodeID] [varchar] (28) NULL CONSTRAINT DF_CustSupp_FDFiscalID_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDFiscalCodeID] = '' WHERE [dbo].[MA_CustSupp].[FDFiscalCodeID] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDEORICode')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDEORICode] [varchar] (17) NULL CONSTRAINT DF_CustSupp_FDEORICode_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDEORICode] = '' WHERE [dbo].[MA_CustSupp].[FDEORICode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'FDTitleCode')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [FDTitleCode] [varchar] (8) NULL CONSTRAINT DF_CustSupp_FDTitleCode_00 DEFAULT ('')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[FDTitleCode] = '' WHERE [dbo].[MA_CustSupp].[FDTitleCode] IS NULL
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'SendByCertifiedEmail')
	ALTER TABLE [dbo].[MA_CustSupp] 
	ADD [SendByCertifiedEmail] [char] (1) NULL CONSTRAINT DF_CustSupp_SendByPEC_00 DEFAULT ('0')
GO

	UPDATE [dbo].[MA_CustSupp] SET [dbo].[MA_CustSupp].[SendByCertifiedEmail] = '0' WHERE [dbo].[MA_CustSupp].[SendByCertifiedEmail] IS NULL
GO