if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('ContractProjectFromOrders'))
	EXEC('insert into MA_EI_ITParameters (ParameterId, TaxJournal, ContractProjectFromOrders, ContractProjectFromJobs,  SetItemCode, ItemCodeType, Link)
	select ParameterId, TaxJournal, ContractProjectFromOrders, ContractProjectFromJobs, SendItemCode, ItemCodeType, LinkFEPA from MA_FEPAParameters');
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('AttachReport'))
	EXEC('insert into MA_EIParameters (ParameterId, AttachReport, DocumentPath)
	select ParameterId,  AttachReport, DocumentPath from MA_FEPAParameters');
GO

if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_FEPAParame_TaxJournal_00')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP CONSTRAINT DF_FEPAParame_TaxJournal_00
GO
if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('TaxJournal'))
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP COLUMN [TaxJournal]
GO
	
if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_FEPAParame_ContractPr_00')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP CONSTRAINT DF_FEPAParame_ContractPr_00
GO
if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('ContractProjectFromOrders'))
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP COLUMN [ContractProjectFromOrders]
GO
	
if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_FEPAParame_ContractPr_01')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP CONSTRAINT DF_FEPAParame_ContractPr_01
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('ContractProjectFromJobs'))
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP COLUMN [ContractProjectFromJobs]
GO

if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_FEPAParame_SendItemCo_00')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP CONSTRAINT DF_FEPAParame_SendItemCo_00
GO
if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('SendItemCode'))
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP COLUMN [SendItemCode]
GO

if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_FEPAParame_ItemCodeTy_00')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP CONSTRAINT DF_FEPAParame_ItemCodeTy_00
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('ItemCodeType'))
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP COLUMN [ItemCodeType]
GO
	
if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_FEPAParame_LinkFEPA_00')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP CONSTRAINT DF_FEPAParame_LinkFEPA_00
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('LinkFEPA'))
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP COLUMN [LinkFEPA]
GO

if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_FEPAParame_AttachRepo_00')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP CONSTRAINT DF_FEPAParame_AttachRepo_00
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('AttachReport'))
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP COLUMN [AttachReport]
GO

if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_FEPAParame_DocumentPa_00')
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP CONSTRAINT DF_FEPAParame_DocumentPa_00
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('AttachReport'))
	ALTER TABLE [dbo].[MA_FEPAParameters] 
	DROP COLUMN [AttachReport]
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('UseInternalTranscoding'))
	ALTER TABLE [dbo].[MA_FEPAParameters] ADD [UseInternalTranscoding]	char	(1) NULL CONSTRAINT DF_FEPAParame_UseInterna_00 DEFAULT('0')
GO
	