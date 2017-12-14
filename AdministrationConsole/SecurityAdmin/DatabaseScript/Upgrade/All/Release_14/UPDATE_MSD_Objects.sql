BEGIN

DECLARE @newtypeid as integer 

SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 4)

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.dunnedcustomer')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.dunnedcustomer' where namespace = 'ERP.AP_AR_Plus.dunnedcustomer' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.CustAccStatementsBySalespers')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.CustAccStatementsBySalespers' where namespace = 'ERP.AP_AR_Plus.CustAccStatementsBySalespers' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.ShortMediumTermPayables')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.ShortMediumTermPayables' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermPayables' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.ShortMediumTermPyblsBySupp')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.ShortMediumTermPyblsBySupp' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermPyblsBySupp' and typeid = @newtypeid
end


if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.ShortMediumTermRcvblsByCust')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.ShortMediumTermRcvblsByCust' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermRcvblsByCust' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.ShortMediumTermReceivables')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.ShortMediumTermReceivables' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermReceivables' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.CashFlow')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.CashFlow' where namespace = 'ERP.AP_AR_Plus.CashFlow' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.CashFlowPayables')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.CashFlowPayables' where namespace = 'ERP.AP_AR_Plus.CashFlowPayables' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.CashFlowReceivebles')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.CashFlowReceivebles' where namespace = 'ERP.AP_AR_Plus.CashFlowReceivebles' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.CustPymtScheduleByName')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.CustPymtScheduleByName' where namespace = 'ERP.AP_AR_Plus.CustPymtScheduleByName' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.SuppPymtScheduleByName')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.SuppPymtScheduleByName' where namespace = 'ERP.AP_AR_Plus.SuppPymtScheduleByName' and typeid = @newtypeid
end

if not exists (select * from msd_Objects where NameSpace ='ERP.AP_AR.RcvblsOverdueByCust')
begin
UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.RcvblsOverdueByCust' where namespace = 'ERP.AP_AR_Plus.RcvblsOverdueByCust' and typeid = @newtypeid
end

END

GO