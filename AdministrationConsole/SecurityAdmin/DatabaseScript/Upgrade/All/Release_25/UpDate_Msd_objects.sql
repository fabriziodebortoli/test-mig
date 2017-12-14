BEGIN

DECLARE @newtypeid		as integer 


/* Prendo l'ID del Tipo */
SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 5)

if not exists (select * from msd_Objects where NameSpace = 'ERP.Items.Components.BDItemsMultiSelectionForImport')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Items.Components.BDItemsMultiSelectionForImport' where namespace = 'ERP.Items.Components.BDChooseItemsBookForImport' and typeid = @newtypeid
end

end
go