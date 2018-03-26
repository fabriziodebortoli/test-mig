SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_getlockinfo]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_getlockinfo]
GO

CREATE PROCEDURE [dbo].[sp_getlockinfo]
@TableName varchar(56), @LockKey varchar(512),
@LockerAccount varchar(128) output, @LockerProcess varchar(256) output, @LockerDate datetime output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @Locked bit;
	SET @Locked = 0;
	SET @LockerAccount = '';
	SET @LockerProcess = '';
	BEGIN TRY
		SELECT @LockerAccount = AccountName, @LockerProcess = ProcessName, @LockerDate = LockDate FROM TB_Locks where TableName = @TableName and LockKey = @LockKey	
		return @@ROWCOUNT;
	END TRY
	BEGIN CATCH
		return 0
	END CATCH
END
GO