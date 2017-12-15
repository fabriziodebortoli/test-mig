DECLARE 
V_COLUMN1_EXIST INT;
V_COLUMN2_EXIST INT;

BEGIN
	SELECT COUNT(*) INTO V_COLUMN1_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPP' AND COLUMN_NAME = 'SENDBYFEPA';

	SELECT COUNT(*) INTO V_COLUMN2_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPPCUSTOMEROPTIONS' AND COLUMN_NAME = 'PUBLICAUTHORITY';

	IF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPPCUSTOMEROPTIONS ADD COLUMN PUBLICAUTHORITY char (1) NULL DEFAULT (''0'')';
		EXECUTE IMMEDIATE 'UPDATE MA_CUSTSUPPCUSTOMEROPTIONS t set t.PUBLICAUTHORITY = (
				SELECT SENDBYFEPA
				FROM MA_CUSTSUPP
				WHERE t.CUSTOMER = MA_CUSTSUPP.CUSTSUPP
				AND MA_CUSTSUPP.CUSTSUPPTYPE = 3211264
				)';
	ELSIF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST > 0  THEN
		EXECUTE IMMEDIATE 'UPDATE MA_CUSTSUPPCUSTOMEROPTIONS t set t.PUBLICAUTHORITY = (
				SELECT SENDBYFEPA
				FROM MA_CUSTSUPP
				WHERE t.CUSTOMER = MA_CUSTSUPP.CUSTSUPP
				AND MA_CUSTSUPP.CUSTSUPPTYPE = 3211264
				)';
	ELSIF V_COLUMN1_EXIST = 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPPCUSTOMEROPTIONS ADD COLUMN PUBLICAUTHORITY char (1) NULL DEFAULT (''0'')';
	END IF;

	SELECT COUNT(*) INTO V_COLUMN1_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPP' AND COLUMN_NAME = 'SENDBYFEPA';

	SELECT COUNT(*) INTO V_COLUMN2_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPP' AND COLUMN_NAME = 'ELECTRONICINVOICING';

	IF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP ADD COLUMN ELECTRONICINVOICING char (1) NULL DEFAULT (''0'')';
		EXECUTE IMMEDIATE 'UPDATE MA_CUSTSUPP SET ELECTRONICINVOICING=SENDBYFEPA WHERE MA_CUSTSUPP.CUSTSUPPTYPE = 3211264';
	    EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP DROP COLUMN SENDBYFEPA';
	ELSIF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST > 0  THEN
		EXECUTE IMMEDIATE 'UPDATE MA_CUSTSUPP SET ELECTRONICINVOICING=SENDBYFEPA WHERE MA_CUSTSUPP.CUSTSUPPTYPE = 3211264';
	    EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP DROP COLUMN SENDBYFEPA';
	ELSIF V_COLUMN1_EXIST = 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP ADD COLUMN ELECTRONICINVOICING char (1) NULL DEFAULT (''0'')';
	END IF;

	SELECT COUNT(*) INTO V_COLUMN1_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPP' AND COLUMN_NAME = 'EORICODEFORFEPA';

	SELECT COUNT(*) INTO V_COLUMN2_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPP' AND COLUMN_NAME = 'EORICODE';

	IF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP RENAME COLUMN EORICODEFORFEPA TO EORICODE';
	ELSIF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST > 0  THEN
		EXECUTE IMMEDIATE 'UPDATE MA_CUSTSUPP SET EORICODE=EORICODEFORFEPA';
	    EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP DROP COLUMN EORICODEFORFEPA';
	ELSIF V_COLUMN1_EXIST = 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP ADD COLUMN EORICODE varchar2 (17) NULL DEFAULT ('''')';
	END IF;

	SELECT COUNT(*) INTO V_COLUMN1_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPP' AND COLUMN_NAME = 'IPACODEFORFEPA';

	SELECT COUNT(*) INTO V_COLUMN2_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPP' AND COLUMN_NAME = 'IPACODE';

	IF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP RENAME COLUMN IPACODEFORFEPA TO IPACODE';
	ELSIF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST > 0  THEN
		EXECUTE IMMEDIATE 'UPDATE MA_CUSTSUPP SET IPACODE=IPACODEFORFEPA';
	    EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP DROP COLUMN IPACODEFORFEPA';
	ELSIF V_COLUMN1_EXIST = 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPP ADD COLUMN IPACODE varchar2 (6) NULL DEFAULT ('''')';
	END IF;
	

	SELECT COUNT(*) INTO V_COLUMN1_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPPBRANCHES' AND COLUMN_NAME = 'IPACODEFORFEPA';

	SELECT COUNT(*) INTO V_COLUMN2_EXIST 
	FROM USER_TAB_COLUMNS
	WHERE TABLE_NAME = 'MA_CUSTSUPPBRANCHES' AND COLUMN_NAME = 'IPACODE';

	IF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPPBRANCHES RENAME COLUMN IPACODEFORFEPA TO IPACODE';
	ELSIF V_COLUMN1_EXIST > 0 AND V_COLUMN2_EXIST > 0  THEN
		EXECUTE IMMEDIATE 'UPDATE MA_CUSTSUPPBRANCHES SET IPACODE=IPACODEFORFEPA';
	    EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPPBRANCHES DROP COLUMN IPACODEFORFEPA';
	ELSIF V_COLUMN1_EXIST = 0 AND V_COLUMN2_EXIST = 0 THEN
		EXECUTE IMMEDIATE 'ALTER TABLE MA_CUSTSUPPBRANCHES ADD COLUMN IPACODE varchar2 (6) NULL DEFAULT ('''')';
	END IF;
	
END;
GO