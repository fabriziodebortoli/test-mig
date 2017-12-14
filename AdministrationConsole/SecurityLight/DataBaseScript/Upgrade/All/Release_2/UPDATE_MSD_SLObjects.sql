begin

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.dunnedcustomer')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.dunnedcustomer' where namespace = 'ERP.AP_AR_Plus.dunnedcustomer' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.CustAccStatementsBySalespers')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.CustAccStatementsBySalespers' where namespace = 'ERP.AP_AR_Plus.CustAccStatementsBySalespers' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.ShortMediumTermPayables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.ShortMediumTermPayables' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermPayables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.ShortMediumTermPyblsBySupp')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.ShortMediumTermPyblsBySupp' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermPyblsBySupp' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.ShortMediumTermRcvblsByCust')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.ShortMediumTermRcvblsByCust' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermRcvblsByCust' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.ShortMediumTermReceivables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.ShortMediumTermReceivables' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermReceivables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.CashFlow')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.CashFlow' where namespace = 'ERP.AP_AR_Plus.CashFlow' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.CashFlowPayables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.CashFlowPayables' where namespace = 'ERP.AP_AR_Plus.CashFlowPayables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.CashFlowReceivebles')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.CashFlowReceivebles' where namespace = 'ERP.AP_AR_Plus.CashFlowReceivebles' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.CustPymtScheduleByName')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.CustPymtScheduleByName' where namespace = 'ERP.AP_AR_Plus.CustPymtScheduleByName' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.SuppPymtScheduleByName')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.SuppPymtScheduleByName' where namespace = 'ERP.AP_AR_Plus.SuppPymtScheduleByName' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.RcvblsOverdueByCust')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.RcvblsOverdueByCust' where namespace = 'ERP.AP_AR_Plus.RcvblsOverdueByCust' and type = 3
end
end

GO