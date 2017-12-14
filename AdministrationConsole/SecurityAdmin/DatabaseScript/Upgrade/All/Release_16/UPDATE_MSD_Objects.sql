BEGIN

DECLARE @newtypeid as integer 

SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 4)

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.dunnedcustomer')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.dunnedcustomer' where namespace = 'ERP.AP_AR_Plus.dunnedcustomer' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Plus.CustAccStatementsBySalespers')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Plus.CustAccStatementsBySalespers' where namespace = 'ERP.AP_AR.CustAccStatementsBySalespers' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPayables')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPayables' where (namespace = 'ERP.AP_AR.ShortMediumTermPayables')  and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp' 
	where (namespace = 'ERP.AP_AR.ShortMediumTermPyblsBySupp')  and typeid = @newtypeid
end


if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust' 
	where (namespace = 'ERP.AP_AR.ShortMediumTermRcvblsByCust') and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermReceivables')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermReceivables' 
	where (namespace = 'ERP.AP_AR.ShortMediumTermReceivables' ) and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.CashFlow')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlow' 
	where (namespace = 'ERP.AP_AR.CashFlow' ) and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.CashFlowPayables')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlowPayables' 
	where (namespace = 'ERP.AP_AR.CashFlowPayables' ) and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.CashFlowReceivebles')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlowReceivebles' 
	where (namespace = 'ERP.AP_AR_Plus.CashFlowReceivebles' ) and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.CustPymtScheduleByName')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.CustPymtScheduleByName' 
	where (namespace = 'ERP.AP_AR_Plus.CustPymtScheduleByName' ) and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.SuppPymtScheduleByName')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.SuppPymtScheduleByName' 
	where (namespace = 'ERP.AP_AR_Plus.SuppPymtScheduleByName') and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR_Analysis.RcvblsOverdueByCust')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.RcvblsOverdueByCust' 
	where (namespace = 'ERP.AP_AR_Plus.RcvblsOverdueByCust' ) and typeid = @newtypeid
end
END

GO