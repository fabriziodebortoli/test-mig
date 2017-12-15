BEGIN
	INSERT INTO MA_EI_ITPARAMETERS (PARAMETERID, TAXJOURNAL, CONTRACTPROJECTFROMORDERS, CONTRACTPROJECTFROMJOBS, SETITEMCODE, ITEMCODETYPE, LINK)
	SELECT PARAMETERID, TAXJOURNAL, CONTRACTPROJECTFROMORDERS, CONTRACTPROJECTFROMJOBS, SENDITEMCODE, ITEMCODETYPE, LINKFEPA from MA_FEPAPARAMETERS;
	
	INSERT INTO MA_EIPARAMETERS (PARAMETERID, ATTACHREPORT, DOCUMENTPATH) SELECT PARAMETERID,ATTACHREPORT, DOCUMENTPATH from MA_FEPAPARAMETERS;
	EXECUTE IMMEDIATE 'ALTER TABLE MA_FEPAPARAMETERS ADD USEINTERNALTRANSCODING CHAR(1) DEFAULT (''0'')';

	EXECUTE IMMEDIATE 'DROP COLUMN TAXJOURNAL';
	EXECUTE IMMEDIATE 'DROP COLUMN CONTRACTPROJECTFROMORDERS';
	EXECUTE IMMEDIATE 'DROP COLUMN CONTRACTPROJECTFROMJOBS';
	EXECUTE IMMEDIATE 'DROP COLUMN DOCSTATUSCHECKERROR';
	EXECUTE IMMEDIATE 'DROP COLUMN SENDITEMCODE';
	EXECUTE IMMEDIATE 'DROP COLUMN ITEMCODETYPE';
	EXECUTE IMMEDIATE 'DROP COLUMN LINKFEPA';
	
	EXCEPTION
	WHEN OTHERS THEN NULL;
END;