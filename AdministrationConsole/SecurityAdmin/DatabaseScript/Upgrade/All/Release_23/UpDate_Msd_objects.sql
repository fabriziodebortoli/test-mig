BEGIN

DECLARE @newtypeid		as integer 
DECLARE @companyid		as integer
DECLARE @newObjectID	as integer
DECLARE @oldObjectID	as integer
DECLARE @grant			as integer
DECLARE @loginID		as integer
DECLARE @roleID			as integer

/* Prendo l'ID del Tipo */
SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 4)

if not exists (select * from msd_Objects where NameSpace = 'ERP.BillOfMaterialsPlus.DrawingsListPlus')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.BillOfMaterialsPlus.DrawingsListPlus' where namespace = 'ERP.ManufacturingPlus.DrawingsListPlus' and typeid = @newtypeid
end


if not exists (select * from msd_Objects where NameSpace = 'ERP.BillOfMaterialsPlus.DrawingsSheetPlus')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.BillOfMaterialsPlus.DrawingsSheetPlus' where namespace = 'ERP.ManufacturingPlus.DrawingsSheetPlus' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace = 'ERP.BillOfMaterialsPlus.TechnicalDataSheet')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.BillOfMaterialsPlus.TechnicalDataSheet' where namespace = 'ERP.ManufacturingPlus.TechnicalDataSheet' and typeid = @newtypeid
end


SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 5)
if not exists (select * from msd_Objects where NameSpace = 'ERP.BillOfMaterialsPlus.Documents.TechnicalDataDefinition')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.BillOfMaterialsPlus.Documents.TechnicalDataDefinition' where namespace = 'ERP.ManufacturingPlus.Documents.TechnicalDataDefinition' and typeid = @newtypeid
end
end
go

