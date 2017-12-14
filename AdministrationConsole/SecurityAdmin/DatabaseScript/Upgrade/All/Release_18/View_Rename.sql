DECLARE @nameSpace varchar(512)
DECLARE @objectType as int

DECLARE @oldNameSpaceLenght as int
DECLARE @endString varchar(512)
DECLARE @newNameSpace varchar(512)


SET @nameSpace =''
SET @newNameSpace =''
SET @objectType = 0

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.Invoice.Document.Invoice%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 45, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.Invoice.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.CorrectionInvoice.Document.Invoice%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 55, @oldNameSpaceLenght)
	
	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.CorrectionInvoice.SalesDocument.SaleDoc' + @endString
	
	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.InvoiceForAdvance.Document.Invoice%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 55, @oldNameSpaceLenght)
	
	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.InvoiceForAdvance.SalesDocument.SaleDoc' + @endString
	
	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor


-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.DeliveryNotes.Document.DeliveryNote%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 57, @oldNameSpaceLenght)
	
	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.DeliveryNotes.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.PickingRequests.Document.DeliveryNote%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 59, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.PickingRequests.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor


-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.CreditNote.Document.CreditNote%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 51, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.CreditNote.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.AccInvoice.Document.AccInvoice%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 51, @oldNameSpaceLenght)
	
	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.AccInvoice.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.CorrectionAccInvoice.Document.AccInvoice%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 61, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.CorrectionAccInvoice.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.Non-CollectedReceipt.Document.Non-CollectedReceipt%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 71, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.Non-CollectedReceipt.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.Receipt.Document.Receipt%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 45, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.Receipt.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.Paragon.Document.Receipt%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 45, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.Paragon.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.CorrectionParagon.Document.Receipt%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 55, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.CorrectionParagon.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.CorrectionReceipt.Document.Receipt%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 55, @oldNameSpaceLenght)
	
	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.CorrectionReceipt.SalesDocument.Receipt' + @endString
	
	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.ReturnsFromCustomer.Document.ReturnsFromCustomer%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 76, @oldNameSpaceLenght)
	
	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.ReturnsFromCustomer.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor


-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Sales.Documents.ReturnToSupplier.Document.Return%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 67, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Sales.Documents.ReturnToSupplier.SalesDocument.SaleDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor


-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Purchases.Documents.BillOfLading.Document.BillOfLading%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Purchases.Documents.BillOfLading.Document.BillOfLading
	set @endString = SUBSTRING(@nameSpace, 60, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Purchases.Documents.BillOfLading.PurchaseDocument.PurchaseDoc' + @endString

	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Purchases.Documents.Invoice.Document.Invoice%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 50, @oldNameSpaceLenght)
	
	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Purchases.Documents.Invoice.PurchaseDocument.PurchaseDoc' + @endString
	
	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Purchases.Documents.PurchaseCorrectionInvoice.Document.Invoice%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 68, @oldNameSpaceLenght)
	
	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Purchases.Documents.PurchaseCorrectionInvoice.PurchaseDocument.PurchaseDoc' + @endString
	
	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor

-----------------------------------------------------------------------------------------

DECLARE oldNamespace_Cursor CURSOR FOR 
select NameSpace, typeid from msd_objects where namespace like 'ERP.Purchases.Documents.CreditNotes.Document.CreditNotes%' 


-- apro il cursore
open oldNamespace_Cursor

--associo il valore della colonna NameSpace al parametro 
FETCH oldNamespace_Cursor INTO @nameSpace, @objectType

WHILE @@FETCH_STATUS = 0
BEGIN
	-- conto la lunghezza totale del vecchio namespace
	set @oldNameSpaceLenght = LEN(@nameSpace)

	-- estraggo tutto il pezzo finale del Namespace togliendo ERP.Sales.Documents.Invoice.Document.Invoice
	set @endString = SUBSTRING(@nameSpace, 58, @oldNameSpaceLenght)

	-- concateno il nuovo namespace 
	SET @newNameSpace = 'ERP.Purchases.Documents.CreditNotes.PurchaseDocument.PurchaseDoc' + @endString
	
	if not exists (select * from msd_Objects where NameSpace =@newNameSpace  and typeid = @objectType)
	begin
		UPDATE MSD_OBJECTS set NameSpace =@newNameSpace where namespace = @nameSpace  and typeid = @objectType
	end 

	SET @endString = ''
	SET @newNameSpace = ''
	SET @oldNameSpaceLenght = 0

	-- equivale a una move next
	FETCH oldNamespace_Cursor INTO @nameSpace, @objectType
END

CLOSE oldNamespace_Cursor
DEALLOCATE oldNamespace_Cursor