
if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddData' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddData where CustSuppType = 3211264 and CustSupp = '*')
	EXEC ('Insert Into MA_EI_ITCustSuppAddData (CustSupp, CustSuppType) values (''*'', 3211264)')


if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.IdDocumento')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.IdDocumento'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.Data')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.Data'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.CodiceCommessaConvenzione')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.CodiceCommessaConvenzione'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.CodiceCUP')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.CodiceCUP'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.CodiceCIG')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiOrdineAcquisto.CodiceCIG'', 11599873)')



if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.IdDocumento')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.IdDocumento'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.Data')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.Data'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.CodiceCommessaConvenzione')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.CodiceCommessaConvenzione'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.CodiceCUP')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.CodiceCUP'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.CodiceCIG')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiFattureCollegate.CodiceCIG'', 11599873)')


if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiDDT.NumeroDDT')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiDDT.NumeroDDT'', 11599873)')

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MA_EI_ITCustSuppAddDataDet' and dbo.sysobjects.id = dbo.syscolumns.id ) and 
	not exists (select * from MA_EI_ITCustSuppAddDataDet where CustSuppType = 3211264 and CustSupp = '*' and FieldName = 'FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiDDT.DataDDT')
	EXEC ('Insert Into MA_EI_ITCustSuppAddDataDet (CustSupp, CustSuppType, FieldName, FieldMessage) values (''*'', 3211264, ''FatturaElettronica.FatturaElettronicaBody.DatiGenerali.DatiDDT.DataDDT'', 11599873)')
