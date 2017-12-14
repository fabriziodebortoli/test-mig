BEGIN
DECLARE @idob as integer 

if  not exists (select objectid from msd_objects where namespace = 'ERP.Sales.Services.DocumentMaintenance')
BEGIN	
	DECLARE @typeId as integer 
	set @typeId = (select typeid from msd_objectTypes where type = 5)

	SET @idob = @@IDENTITY
	INSERT INTO MSD_Objects 
		(TypeId, ParentId, NameSpace, Localize) 
	Values 
		(@typeId, 0, 'ERP.Sales.Services.DocumentMaintenance', null)

END
ELSE
BEGIN
	SET @idob = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.DocumentMaintenance')
END


DECLARE @oldObjectId as integer 
if exists(select objectid from msd_objects where objectid = @idob)
begin
	
	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.DocumentMaintenance' and typeid = @typeId)
	Begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.DocumentMaintenance' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		End
	END


	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.DeliveryNoteMaintenance' and typeid = @typeId)
	Begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.DeliveryNoteMaintenance' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId	
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		End
	END


	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.AccInvoiceMaintenance' and typeid = @typeId)
	begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.AccInvoiceMaintenance' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		End
	END


	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.CreditNoteMaintenance' and typeid = @typeId)
	begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.CreditNoteMaintenance' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		END
	End


	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.ReceiptMaintenance' and typeid = @typeId)
	begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.ReceiptMaintenance' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		End
	END
end
end