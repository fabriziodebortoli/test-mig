
if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'SendByFEPA') AND 
   not exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ElectronicInvoicing')
BEGIN
	ALTER TABLE [dbo].[MA_CustSupp] ADD ElectronicInvoicing [char] (1) NULL CONSTRAINT DF_CustSupp_Electronic_00 DEFAULT('');
END 
GO

if  exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'SendByFEPA') and 
	not exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ElectronicInvoicing')
BEGIN
	EXEC sp_RENAME 'MA_CustSupp.SendByFEPA', 'ElectronicInvoicing', 'COLUMN'
END
GO

if   exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'SendByFEPA') and 
	 exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ElectronicInvoicing')
BEGIN
	exec ('UPDATE [dbo].[MA_CustSupp] SET ElectronicInvoicing=SendByFEPA where [MA_CustSupp].[CustSuppType] = 3211264');
	ALTER TABLE [dbo].[MA_CustSupp] DROP CONSTRAINT DF_CustSupp_SendByFEPA_00;
	ALTER TABLE [dbo].[MA_CustSupp] DROP COLUMN SendByFEPA;
END
GO



if exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EORICodeForFEPA') AND 
   not exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EORICode')
BEGIN
	EXEC SP_RENAME 'MA_CustSupp.EORICodeForFEPA', 'EORICode', 'COLUMN'
END 
GO

if  not exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EORICodeForFEPA') and 
	not exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EORICode')
BEGIN
	ALTER TABLE [dbo].[MA_CustSupp] ADD [EORICode] [varchar] (17) NULL CONSTRAINT DF_CustSupp_EORICode_00 DEFAULT('');
END
GO

if exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EORICodeForFEPA') AND 
   exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'EORICode')
BEGIN
	EXEC ('UPDATE [dbo].[MA_CustSupp] SET EORICode=EORICodeForFEPA');
	ALTER TABLE [dbo].[MA_CustSupp] DROP CONSTRAINT DF_CustSupp_EORICodeFo_00;
	ALTER TABLE [dbo].[MA_CustSupp] DROP COLUMN EORICodeForFEPA;
END
GO

if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACode')
BEGIN
	ALTER TABLE [dbo].[MA_CustSupp] ADD [IPACode] [varchar] (6) NULL CONSTRAINT DF_CustSupp_IPACode_00 DEFAULT('');
END
GO

if exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACodeForFEPA') AND
	not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACode')

BEGIN
	EXEC SP_RENAME 'MA_CustSupp.IPACodeForFEPA', 'IPACode', 'COLUMN'
END


if exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACodeForFEPA') AND 
   exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSupp' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACode')
BEGIN
	EXEC('UPDATE [dbo].[MA_CustSupp] SET IPACode=IPACodeForFEPA');
	ALTER TABLE [dbo].[MA_CustSupp] DROP CONSTRAINT DF_CustSupp_IPACodeFor_00;
	ALTER TABLE [dbo].[MA_CustSupp] DROP COLUMN IPACodeForFEPA;
END
GO


if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACode')
BEGIN
	ALTER TABLE [dbo].[MA_CustSuppBranches] ADD [IPACode] [varchar] (6) NULL CONSTRAINT DF_CustSuppBranches_IPACode_00 DEFAULT('');
END
GO

if exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACodeForFEPA') AND
	not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACode')

BEGIN
	EXEC SP_RENAME 'MA_CustSuppBranches.IPACodeForFEPA', 'IPACode', 'COLUMN'
END


if exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACodeForFEPA') AND 
   exists ( select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_CustSuppBranches' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'IPACode')
BEGIN
	EXEC('UPDATE [dbo].[MA_CustSuppBranches] SET IPACode=IPACodeForFEPA');
	ALTER TABLE [dbo].[MA_CustSuppBranches] DROP CONSTRAINT DF_CustSuppBranches_IPACodeFor_00;
	ALTER TABLE [dbo].[MA_CustSuppBranches] DROP COLUMN IPACodeForFEPA;
END
GO