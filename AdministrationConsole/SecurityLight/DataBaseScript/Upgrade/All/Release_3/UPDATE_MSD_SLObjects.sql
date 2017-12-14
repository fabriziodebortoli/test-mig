BEGIN

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR.dunnedcustomer')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR.dunnedcustomer' where namespace = 'ERP.AP_AR_Plus.dunnedcustomer' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Plus.CustAccStatementsBySalespers')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Plus.CustAccStatementsBySalespers' where namespace = 'ERP.AP_AR.CustAccStatementsBySalespers' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPayables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPayables' where namespace = 'ERP.AP_AR.ShortMediumTermPayables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPayables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPayables' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermPayables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp' where namespace = 'ERP.AP_AR.ShortMediumTermPyblsBySupp' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermPyblsBySupp' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust' where namespace = 'ERP.AP_AR.ShortMediumTermRcvblsByCust' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermRcvblsByCust' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermReceivables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermReceivables' where namespace = 'ERP.AP_AR.ShortMediumTermReceivables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermReceivables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermReceivables' where namespace = 'ERP.AP_AR_Plus.ShortMediumTermReceivables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.CashFlow')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlow' where namespace = 'ERP.AP_AR.CashFlow' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.CashFlow')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlow' where namespace = 'ERP.AP_AR_Plus.CashFlow' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.CashFlowPayables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlowPayables' where namespace = 'ERP.AP_AR.CashFlowPayables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.CashFlowPayables')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlowPayables' where namespace = 'ERP.AP_AR_Plus.CashFlowPayables' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.CashFlowReceivebles')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlowReceivebles' where namespace = 'ERP.AP_AR.CashFlowReceivebles' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.CashFlowReceivebles')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlowReceivebles' where namespace = 'ERP.AP_AR_Plus.CashFlowReceivebles' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.CustPymtScheduleByName')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.CustPymtScheduleByName' where namespace = 'ERP.AP_AR.CustPymtScheduleByName' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.CustPymtScheduleByName')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.CustPymtScheduleByName' where namespace = 'ERP.AP_AR_Plus.CustPymtScheduleByName' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.SuppPymtScheduleByName')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.SuppPymtScheduleByName' where namespace = 'ERP.AP_AR.SuppPymtScheduleByName' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.SuppPymtScheduleByName')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.SuppPymtScheduleByName' where namespace = 'ERP.AP_AR_Plus.SuppPymtScheduleByName' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.RcvblsOverdueByCust')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.RcvblsOverdueByCust' where namespace = 'ERP.AP_AR.RcvblsOverdueByCust' and type = 3
end

if not exists (select * from MSD_SLOBJECTS where NameSpace ='ERP.AP_AR_Analysis.RcvblsOverdueByCust')
begin
UPDATE MSD_SLOBJECTS set NameSpace ='ERP.AP_AR_Analysis.RcvblsOverdueByCust' where namespace = 'ERP.AP_AR_Plus.RcvblsOverdueByCust' and type = 3
end

END

GO