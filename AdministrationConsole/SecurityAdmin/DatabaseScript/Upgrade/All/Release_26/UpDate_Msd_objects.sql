BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.BillOfMaterialsPlus.Dbl.MA_ItemsTechnicalData')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.QualityInspection.Dbl.MA_ItemsTechnicalData' where namespace = 'ERP.BillOfMaterialsPlus.Dbl.MA_ItemsTechnicalData' and typeid = 8
end

end
go

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.BillOfMaterialsPlus.Dbl.MA_ItemsTechDataDefinition')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.QualityInspection.Dbl.MA_ItemsTechDataDefinition' where namespace = 'ERP.BillOfMaterialsPlus.Dbl.MA_ItemsTechDataDefinition' and typeid = 8
end

end
go

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.BillOfMaterialsPlus.Dbl.TechnicalDataSheet')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.QualityInspection.TechnicalDataSheet' where namespace = 'ERP.BillOfMaterialsPlus.Dbl.TechnicalDataSheet' and typeid = 2
end

end
go

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.BillOfMaterialsPlus.Documents.TechnicalDataDefinition')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.QualityInspection.Documents.TechnicalDataDefinition' where namespace = 'ERP.BillOfMaterialsPlus.Documents.TechnicalDataDefinition' and typeid = 3
end

end
go

