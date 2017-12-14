if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_SaleDoc' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('FEPAStatus'))
BEGIN
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112640 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474240')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112840 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474241')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112841 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474242')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112842 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474243')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112843 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474244')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112844 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474245')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112845 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474246')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112846 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474247')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112847 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474248')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112848 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474249')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112849 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474250')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112850 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474251')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112851 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474252')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112852 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474253')
EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[FEPAStatus] = 32112853 WHERE [dbo].[MA_SaleDoc].[FEPAStatus] = 30474254')
END
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_FEPAParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('DocStatusCheckError'))
BEGIN
EXEC ('UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[DocStatusCheckError] = 10682368 WHERE [dbo].[MA_FEPAParameters].[DocStatusCheckError] = 30801920')
EXEC ('UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[DocStatusCheckError] = 10682369 WHERE [dbo].[MA_FEPAParameters].[DocStatusCheckError] = 30801921')
EXEC ('UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[DocStatusCheckError] = 10682370 WHERE [dbo].[MA_FEPAParameters].[DocStatusCheckError] = 30801922')
END
GO

BEGIN TRY
	if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_SaleDoc' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('FEPAStatus'))
	EXEC ('UPDATE [dbo].[MA_SaleDoc] SET [dbo].[MA_SaleDoc].[EIStatus] = [dbo].[MA_SaleDoc].[FEPAStatus] WHERE [dbo].[MA_SaleDoc].[FEPAStatus] != 32112640')

	if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_SaleDoc_FEPAStatus_00')
	ALTER TABLE [dbo].[MA_SaleDoc] 
	DROP CONSTRAINT DF_SaleDoc_FEPAStatus_00

	if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_SaleDoc' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('FEPAStatus'))
	ALTER TABLE [dbo].[MA_SaleDoc] 	DROP COLUMN [FEPAStatus]

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_SaleDoc' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('FEPALetter770'))
	EXEC('Insert Into MA_EI_ITDocAdditionalData ( DocID, DocSubID, Line, SubLine, FieldName, FieldValue) 
	Select SaleDocId, 0, 0, 0, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiGeneraliDocumento.DatiRitenuta.CausalePagamento'', 
	CASE WHEN FEPALetter770 = ''A''	THEN ''32440320''
	     WHEN FEPALetter770 = ''B''	THEN ''32440321''
		 WHEN FEPALetter770 = ''C''	THEN ''32440322''
		 WHEN FEPALetter770 = ''D''	THEN ''32440323''
		 WHEN FEPALetter770 = ''E''	THEN ''32440324''
		 WHEN FEPALetter770 = ''G''	THEN ''32440325''
		 WHEN FEPALetter770 = ''H''	THEN ''32440326''
		 WHEN FEPALetter770 = ''I''	THEN ''32440327''
		 WHEN FEPALetter770 = ''L''	THEN ''32440328''
		 WHEN FEPALetter770 = ''M''	THEN ''32440329''
		 WHEN FEPALetter770 = ''N''	THEN ''32440330''
		 WHEN FEPALetter770 = ''O''	THEN ''32440331''
		 WHEN FEPALetter770 = ''P''	THEN ''32440332''
		 WHEN FEPALetter770 = ''Q''	THEN ''32440333''
		 WHEN FEPALetter770 = ''R''	THEN ''32440334''
		 WHEN FEPALetter770 = ''S''	THEN ''32440335''
		 WHEN FEPALetter770 = ''T''	THEN ''32440336''
		 WHEN FEPALetter770 = ''U''	THEN ''32440337''
		 WHEN FEPALetter770 = ''V''	THEN ''32440338''
		 WHEN FEPALetter770 = ''W''	THEN ''32440339''
		 WHEN FEPALetter770 = ''X''	THEN ''32440340''
		 WHEN FEPALetter770 = ''Y''	THEN ''32440341''
		 WHEN FEPALetter770 = ''Z''	THEN ''32440342''
		 WHEN FEPALetter770 = ''L1''	THEN ''32440343''
		 WHEN FEPALetter770 = ''M1''	THEN ''32440344''
		 WHEN FEPALetter770 = ''O1''	THEN ''32440345''
		 WHEN FEPALetter770 = ''V1''	THEN ''32440346''
		END AS [FieldValue]
	From MA_SaleDoc where FEPALetter770 = ''A'' OR  FEPALetter770 = ''B'' OR FEPALetter770 = ''C'' OR FEPALetter770 = ''D'' OR FEPALetter770 = ''E'' 
		OR FEPALetter770 = ''G'' OR  FEPALetter770 = ''H'' OR FEPALetter770 = ''I'' OR FEPALetter770 = ''L'' OR FEPALetter770 = ''M''
		OR FEPALetter770 = ''N'' OR  FEPALetter770 = ''O'' OR FEPALetter770 = ''P'' OR FEPALetter770 = ''Q'' OR FEPALetter770 = ''R'' 
		OR FEPALetter770 = ''S'' OR  FEPALetter770 = ''T'' OR FEPALetter770 = ''U'' OR FEPALetter770 = ''V'' OR FEPALetter770 = ''W'' 
		OR FEPALetter770 = ''X'' OR  FEPALetter770 = ''Y'' OR FEPALetter770 = ''Z'' OR FEPALetter770 = ''L1'' OR FEPALetter770 = ''M1''
		OR FEPALetter770 = ''O1'' OR  FEPALetter770 = ''V1''')

	if exists (select dbo.sysobjects.name from dbo.sysobjects 
	where dbo.sysobjects.name = 'DF_SaleDoc_FEPALetter_00')
	ALTER TABLE [dbo].[MA_SaleDoc] 
	DROP CONSTRAINT DF_SaleDoc_FEPALetter_00

	if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
	where dbo.sysobjects.name = 'MA_SaleDoc' and dbo.sysobjects.id = dbo.syscolumns.id 
	and dbo.syscolumns.name IN ('FEPALetter770'))
	ALTER TABLE [dbo].[MA_SaleDoc] 
	DROP COLUMN [FEPALetter770]

END TRY
BEGIN CATCH
END CATCH;
GO
