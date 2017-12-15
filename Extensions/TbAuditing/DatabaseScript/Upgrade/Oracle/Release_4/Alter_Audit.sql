DECLARE CURSOR cur_tables IS    
SELECT USER_TAB_COLUMNS.TABLE_NAME TABLE_NAME, USER_TAB_COLUMNS.COLUMN_NAME COLUMN_NAME 
	FROM USER_TAB_COLUMNS
	WHERE USER_TAB_COLUMNS.TABLE_NAME LIKE 'AU_%' AND 
	USER_TAB_COLUMNS.COLUMN_NAME = 'AU_ID' AND USER_TAB_COLUMNS.DATA_TYPE = 'NUMBER' AND USER_TAB_COLUMNS.DATA_PRECISION = 6;
	
	BEGIN
	  FOR t IN cur_tables
	  LOOP
		EXECUTE IMMEDIATE('ALTER TABLE ' || t.TABLE_NAME || ' MODIFY ' || t.COLUMN_NAME || ' NUMBER(10)');
		  COMMIT;
	  END LOOP;
	END;
GO