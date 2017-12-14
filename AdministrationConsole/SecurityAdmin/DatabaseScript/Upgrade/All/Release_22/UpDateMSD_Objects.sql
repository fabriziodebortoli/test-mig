BEGIN

DECLARE @newtypeid		as integer 
DECLARE @companyid		as integer
DECLARE @newObjectID		as integer
DECLARE @oldObjectID		as integer
DECLARE @grant			as integer
DECLARE @loginID		as integer
DECLARE @roleID			as integer

/* Prendo l'ID del Tipo */
SET @newtypeid = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 4)

/* ERP.AP_AR.CustAccStatementsBySalespers  ERP.AP_AR_Plus.CustAccStatementsBySalespers */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.CustAccStatementsBySalespers')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Plus.CustAccStatementsBySalespers' where namespace = 'ERP.AP_AR.CustAccStatementsBySalespers' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.CustAccStatementsBySalespers')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.CustAccStatementsBySalespers')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end 


/* ERP.AP_AR.CustPymtScheduleByName ERP.AP_AR_Analysis.CustPymtScheduleByName */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.CustPymtScheduleByName')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.CustPymtScheduleByName' where namespace = 'ERP.AP_AR.CustPymtScheduleByName' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.CustPymtScheduleByName')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.CustPymtScheduleByName')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end 


/* ERP.AP_AR.ShortMediumTermReceivables ERP.AP_AR_Analysis.ShortMediumTermReceivables */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.ShortMediumTermReceivables')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermReceivables' where namespace = 'ERP.AP_AR.ShortMediumTermReceivables' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.ShortMediumTermReceivables')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.ShortMediumTermReceivables')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end 


/* ERP.AP_AR.ShortMediumTermRcvblsByCust ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust' where namespace = 'ERP.AP_AR.ShortMediumTermRcvblsByCust' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.ShortMediumTermRcvblsByCust')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.ShortMediumTermRcvblsByCust')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end 


/* ERP.AP_AR.CashFlowReceivables ERP.AP_AR_Analysis.CashFlowReceivables */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.CashFlowReceivables')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlowReceivables' where namespace = 'ERP.AP_AR.CashFlowReceivables' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.CashFlowReceivables')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.CashFlowReceivables')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end 



/* ERP.AP_AR.CashFlow ERP.AP_AR_Analysis.CashFlow */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.CashFlow')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlow' where namespace = 'ERP.AP_AR.CashFlow' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.CashFlow')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.CashFlow')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end 


/* ERP.AP_AR.SuppPymtScheduleByName ERP.AP_AR_Analysis.SuppPymtScheduleByName */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.SuppPymtScheduleByName')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.SuppPymtScheduleByName' where namespace = 'ERP.AP_AR.SuppPymtScheduleByName' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.SuppPymtScheduleByName')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.SuppPymtScheduleByName')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end 


/* ERP.AP_AR.ShortMediumTermPayables ERP.AP_AR_Analysis.ShortMediumTermPayables  */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.ShortMediumTermPayables')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPayables' where namespace = 'ERP.AP_AR.ShortMediumTermPayables' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.ShortMediumTermPayables')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.ShortMediumTermPayables')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end


/* ERP.AP_AR.ShortMediumTermPyblsBySupp ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp' where namespace = 'ERP.AP_AR.ShortMediumTermPyblsBySupp' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.ShortMediumTermPyblsBySupp')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.ShortMediumTermPyblsBySupp')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end



/* ERP.AP_AR.CashFlowPayables ERP.AP_AR_Analysis.CashFlowPayables */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.CashFlowPayables')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR_Analysis.CashFlowPayables' where namespace = 'ERP.AP_AR.CashFlowPayables' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Analysis.CashFlowPayables')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.CashFlowPayables')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end



/* ERP.AP_AR_Plus.VouchersToBePresented ERP.AP_AR.VouchersToBePresented */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersToBePresented')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.VouchersToBePresented' where namespace = 'ERP.AP_AR_Plus.VouchersToBePresented' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersToBePresented')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.VouchersToBePresented')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end


/* ERP.AP_AR_Plus.VouchersToBePresentedByCust ERP.AP_AR.VouchersToBePresentedByCust */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersToBePresentedByCust')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.VouchersToBePresentedByCust' where namespace = 'ERP.AP_AR_Plus.VouchersToBePresentedByCust' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersToBePresentedByCust')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.VouchersToBePresentedByCust')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end


/* ERP.AP_AR_Plus.VouchersSlip ERP.AP_AR.VouchersSlip */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersSlip')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.VouchersSlip' where namespace = 'ERP.AP_AR_Plus.VouchersSlip' and typeid = 2
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersSlip')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.VouchersSlip')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end


/* ERP.AP_AR_Plus.PresentedVouchers ERP.AP_AR.PresentedVouchers */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR.PresentedVouchers')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.PresentedVouchers' where namespace = 'ERP.AP_AR_Plus.PresentedVouchers' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.PresentedVouchers')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.PresentedVouchers')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end


/* ERP.AP_AR_Plus.VouchersSlips ERP.AP_AR.VouchersSlip */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersSlip')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.VouchersSlip' where namespace = 'ERP.AP_AR_Plus.VouchersSlip' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersSlip')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.VouchersSlip')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end

/* ERP.AP_AR_Plus.VouchersSlips ERP.AP_AR.VouchersSlips */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersSlips')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.VouchersSlips' where namespace = 'ERP.AP_AR_Plus.VouchersSlips' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.VouchersSlips')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.VouchersSlips')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end

/* ERP.AP_AR_Plus.Voucher ERP.AP_AR.Voucher */ 

/* Controllo se esiste giá nel DB il nuovo Namespace */
if not exists (select * from msd_Objects where NameSpace = 'ERP.AP_AR.Voucher')
begin
	/* Non esiste quindi sostituisco i namespace e finisce li*/
	UPDATE MSD_OBJECTS set NameSpace ='ERP.AP_AR.Voucher' where namespace = 'ERP.AP_AR_Plus.Voucher' and typeid = @newtypeid
end
else
begin
	/* Esiste quindi devo prendermi l'ID del nuovo namespace */
	SET @newObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR.Voucher')
	/* Devo prendermi l'ID del vecchio namespace */
	SET @oldObjectID = (select objectId from msd_Objects where NameSpace = 'ERP.AP_AR_Plus.Voucher')
	
	/* Controllo se era inserito nella MSD_ProtectedObjects l'oggetto vecchio */
	if (exists (select * from msd_protectedObjects where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @companyId = 0
		DECLARE my_Cursor CURSOR FOR 
		select companyId from msd_protectedobjects where objectid = @oldObjectID
		-- apro il cursore
		open my_Cursor
		--associo il valore della colonna companyId
		FETCH my_Cursor INTO @companyId
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from msd_protectedobjects where companyid = @companyId and objectid = @newObjectID))
			BEGIN
				INSERT INTO msd_protectedobjects (companyId, objectid, disabled) values (@companyId, @newObjectID, '0')
			END
			
			-- equivale a una move next
			FETCH my_Cursor INTO @companyId
		END
		
		CLOSE my_Cursor
		DEALLOCATE my_Cursor	
	end
	
	/* Controllo se era inserito nella MSD_ObjectGrants l'oggetto vecchio */
	if (exists (select * from MSD_ObjectGrants where objectid = @oldObjectID))
	begin 
		/* devo looppare su tutti i record perché posso avere piú company */
		SET @grant = 0
		SET @loginID = 0;
		SET @roleID = 0;
		DECLARE my_CursorGrant CURSOR FOR 
		select grants, roleid, companyId, loginid from MSD_ObjectGrants where objectid = @oldObjectID
		-- apro il cursore
		open my_CursorGrant
		--associo il valore della colonna companyId
		FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		
		WHILE @@FETCH_STATUS = 0
		BEGIN
			if (not exists(select * from MSD_ObjectGrants where companyid = @companyId and objectid = @newObjectID and roleid = @roleid and loginid = @loginid))
			BEGIN
				INSERT INTO MSD_ObjectGrants (companyId, objectid, loginid, roleid, grants, inheritmask) values 
											(@companyId, @newObjectID,  @loginid, @roleid, @grant, 0)
			END
			
			-- equivale a una move next
			FETCH my_CursorGrant INTO @grant,  @roleid, @companyid, @loginid
		END
		
		CLOSE my_CursorGrant
		DEALLOCATE my_CursorGrant	
	end
end
end 

go
