BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.Dbl.MA_CalendarsExcludedDay')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.Dbl.MA_CalendarsExcludedDay' where namespace = 'ERP.Routing.Dbl.MA_CalendarsExcludedDay' and typeid = 8
end

end
go

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.Dbl.MA_CalendarsShift')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.Dbl.MA_CalendarsShift' where namespace = 'ERP.Routing.Dbl.MA_CalendarsShift' and typeid = 8
end

end
go

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.Dbl.MA_Calendars')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.Dbl.MA_Calendars' where namespace = 'ERP.Routing.Dbl.MA_Calendars' and typeid =8
end

end
go

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.Dbl.MA_BreakdownReasons')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.Dbl.MA_BreakdownReasons' where namespace = 'ERP.Routing.Dbl.MA_BreakdownReasons' and typeid = 8
end

end

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.Documents.Calendars')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.Documents.Calendars' where namespace = 'ERP.Routing.Documents.Calendars' and typeid = 3
end

end

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.Documents.BreakdownReasons')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.Documents.BreakdownReasons' where namespace = 'ERP.Routing.Documents.BreakdownReasons' and typeid = 3
end

end


BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.CalendarsSheet')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.CalendarsSheet' where namespace = 'ERP.Routing.CalendarsSheet' and typeid = 2
end

end

BEGIN
if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.BreakdownReasonsList')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.BreakdownReasonsList' where namespace = 'ERP.Routing.BreakdownReasonsList' and typeid = 2
end

end

go