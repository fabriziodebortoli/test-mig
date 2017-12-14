if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SOSConfiguration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
Update [DMS_SOSConfiguration]
  SET DocClasses .modify('insert <ERPSOSDocumentType erpDocNS="Document.ERP.Accounting.Documents.SaleDocJE" erpDocType="Registrazione emessi" /> into (/SOSConfigurationState/DocumentClasses/DocClassesList/DocClass[@code=("FATTEMESSE")]/ERPDocNamespaces)[1]')
  WHERE DocClasses IS NOT NULL
END
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_SOSConfiguration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
Update [DMS_SOSConfiguration]
  SET DocClasses .modify('insert <ERPSOSDocumentType erpDocNS="Document.ERP.Accounting.Documents.PurchaseDocJE" erpDocType="Registrazione ricevuti" /> into (/SOSConfigurationState/DocumentClasses/DocClassesList/DocClass[@code=("FATTRICEVUTE")]/ERPDocNamespaces)[1]')
  WHERE DocClasses IS NOT NULL
END
GO
