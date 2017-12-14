BEGIN

DECLARE @newtypeid		as integer 


/* Prendo l'ID del Tipo */
SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 5)

if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.Services.XGateParameters')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.Services.XGateParameters' where namespace = 'ERP.XGate.Services.XGateParameters' and typeid = @newtypeid
end


if not exists (select * from msd_Objects where NameSpace = 'ERP.Company.Services.XGateParametersForDocType')
begin
	UPDATE MSD_OBJECTS set NameSpace ='ERP.Company.Services.XGateParametersForDocType' where namespace = 'ERP.XGate.Services.XGateParametersForDocType' and typeid = @newtypeid
end


end
go