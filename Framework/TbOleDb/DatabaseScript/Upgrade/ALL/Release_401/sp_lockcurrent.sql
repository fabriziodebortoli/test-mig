SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_lockcurrent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_lockcurrent]
GO

CREATE PROCEDURE [dbo].[sp_lockcurrent]
@TableName varchar(56), @LockKey varchar(512), @AuthenticationToken varchar(36), @AccountName varchar(128), @Context varchar(10), @ProcessName varchar(256), @ProcessGuid varchar(36) = '',
@LockerAccount varchar(128) output, @LockerProcess varchar(256) output, @LockerDate datetime output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @Locked int;
	DECLARE @LockerContext varchar(10);
	DECLARE @LockerAuthToken varchar(36);
	SET @Locked = 0;
	SET @LockerAccount = '';
	SET @LockerProcess = '';
	SET @LockerAuthToken = '';
	BEGIN TRY
		INSERT INTO TB_Locks (TableName, LockKey, AuthenticationToken, AccountName, Context, ProcessName, ProcessGuid) VALUES (@TableName, @LockKey, @AuthenticationToken, @AccountName, @Context, @ProcessName, @ProcessGuid)
		SET @Locked = 1;
	END TRY
	BEGIN CATCH
		if (ERROR_NUMBER() = 2627)
		BEGIN
		SELECT @LockerAuthToken = AuthenticationToken, @LockerAccount = AccountName, @LockerProcess = ProcessName, @LockerDate = LockDate, @LockerContext = Context FROM TB_Locks where TableName = @TableName and LockKey = @LockKey	
			if (@LockerAuthToken IS NOT NULL AND @LockerAuthToken = @AuthenticationToken AND @LockerContext IS NOT NULL AND @LockerContext = @Context)
				SET @Locked = 1; -- ismylock
			END
	END CATCH
	return @Locked;
END
GO