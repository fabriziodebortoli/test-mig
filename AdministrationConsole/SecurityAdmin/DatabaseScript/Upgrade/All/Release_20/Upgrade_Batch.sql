BEGIN

DECLARE @idob as integer 

if  not exists (select objectid from msd_objects where namespace = 'ERP.Sales.Services.DocumentDeleting')
BEGIN	
	DECLARE @typeId as integer 
	set @typeId = (select typeid from msd_objectTypes where type = 7)

	SET @idob = @@IDENTITY
	INSERT INTO MSD_Objects 
		(TypeId, ParentId, NameSpace, Localize) 
	Values 
		(@typeId, 0, 'ERP.Sales.Services.DocumentDeleting', null)

END
ELSE
BEGIN
	SET @idob = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.DocumentDeleting')
END

DECLARE @oldObjectId as integer 

if exists(select objectid from msd_objects where objectid = @idob)
begin
	
	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.NCReceiptMaintenance' and typeid = @typeId)
	Begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.NCReceiptMaintenance' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		END
	END

	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.InvoicesDeleting' and typeid = @typeId)
	begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.InvoicesDeleting' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		end
	end

	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.DeliveryNotesDeleting' and typeid = @typeId)
	begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.DeliveryNotesDeleting' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		END
	END

	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.AccInvoicesDeleting' and typeid = @typeId)
	begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.AccInvoicesDeleting' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		END
	END

	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.CreditNotesDeleting' and typeid = @typeId)
	begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.CreditNotesDeleting' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		END
	END

	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.ReceiptsDeleting' and typeid = @typeId)
	begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.ReceiptsDeleting' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		END
	END

	if  exists(select objectid from msd_objects where namespace = 'ERP.Sales.Services.NCReceiptsDeleting' and typeid = @typeId)
	Begin
		set @oldObjectId = (select objectid from msd_objects where namespace = 'ERP.Sales.Services.NCReceiptsDeleting' and typeid = @typeId)
		if  exists(select objectid from msd_protectedobjects where objectid = @oldObjectId)
		Begin
			update msd_protectedobjects set objectid = @idob where objectid = @oldObjectId
			update msd_objectgrants set objectid = @idob where objectid = @oldObjectId
		END
	END
END
END