BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.CostAccounting.Documents.CostAccEntriesFromAccounting')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.CostAccounting.Documents.CostAccEntriesFromAccounting' where namespace = 'erp.costaccounting.documents.CostAccEntriesGeneration' and typeid = 5
end

end
go
