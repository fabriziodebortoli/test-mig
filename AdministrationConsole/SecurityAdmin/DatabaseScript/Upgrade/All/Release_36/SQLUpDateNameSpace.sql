if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) 
	DECLARE @documentTypeId as integer 
	SET @documentTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 5)
	DECLARE @batchTypeId as integer 
	SET @batchTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 7)
	DECLARE  @reportTypeId as integer 
	SET @reportTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 4)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_IT.Documents.DeclarationIntentJournal'
WHERE( NameSpace = 'ERP.AccountingDMS.AddOnsAccounting.ArchiveDeclarationIntentJournal' AND 
TypeId = @batchTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.FixedAssetsJournal'
WHERE( NameSpace = 'ERP.AccountingDMS.AddOnsAccounting.ArchiveFixedAssetsJournal' AND 
TypeId = @batchTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.AccountingSettings'
WHERE( NameSpace = 'ERP.Accounting.Services.AccountingSettings' AND 
TypeId = @documentTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Core.Services.CoreSettings'
WHERE( NameSpace = 'ERP.Core.Services.MastersAddOnFly' AND 
TypeId = @documentTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Services.PyblsRcvblsSettings'
WHERE( NameSpace = 'ERP.AP_AR.Services.PymtScheduleSettings' AND 
TypeId = @documentTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Services.IntrastatSettings'
WHERE( NameSpace = 'ERP.Intrastat.Services.IntrastatSettings' AND 
TypeId = @documentTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Documents.GenerateDetail'
WHERE( NameSpace = 'ERP.CostAccounting.Documents.Shared.GenerateDetail' AND 
TypeId = @documentTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Components.AccountingTemplates'
WHERE( NameSpace = 'ERP.IdsMng.Documents.AccountingTemplates' AND 
TypeId = @documentTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.BOMPosting'
WHERE( NameSpace = 'ERP.Manufacturing.Documents.SwitchRunBOMPostingVersion' AND 
TypeId = @batchTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.BillOfMaterialsList'
WHERE( NameSpace = 'ERP.BillOfMaterials.BOMList' AND 
TypeId = @batchTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.BatchDocuments.ItemsNavigation'
WHERE( NameSpace = 'ERP.Items.Documents.ItemsNavigation' AND 
TypeId = @batchTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.WMS.BatchDocuments.GenStorageUnit'
WHERE( NameSpace = 'ERP.WMS.Documents.GenStorageUnit' AND 
TypeId = @batchTypeId
)
END


BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.Documents.IssuedDeclarationOfIntent'
WHERE( NameSpace = 'ERP.SpecialTax.Services.IssuedDeclarationOfIntent' AND 
TypeId = @documentTypeId
)
END

BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.Documents.ReceivedDeclarationOfIntent'
WHERE( NameSpace = 'ERP.SpecialTax.Services.ReceivedDeclarationOfIntent' AND 
TypeId = @documentTypeId
)
END

GO