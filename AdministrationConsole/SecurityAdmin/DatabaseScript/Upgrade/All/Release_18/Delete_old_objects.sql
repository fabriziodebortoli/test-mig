DECLARE @objectId as int

set @objectId = (select objectid from msd_objects 
where 
	namespace = 'ERP.Accounting.Services.AccountingDefaults.AccountingDefaults.Default.TaxDefault.MA_TaxDefaults_RetailSalesToBeDistributed'
	AND
	typeid = 14)
print @objectId

delete from msd_objectgrants where objectid = @objectId
delete from msd_protectedobjects where objectid = @objectId
delete from msd_objects where objectid = @objectId

set @objectId = (select objectid from msd_objects 
where 
	namespace = 'ERP.Accounting.Services.AccountingDefaults.AccountingDefaults.Default.TaxDefault.MA_TaxDefaults_TaxCode'
	AND
	typeid  = 14)

delete from msd_objectgrants where objectid = @objectId
delete from msd_protectedobjects where objectid = @objectId
delete from msd_objects where objectid = @objectId