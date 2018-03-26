SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_ismylock]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_ismylock]
GO

CREATE PROCEDURE [dbo].[sp_ismylock]
@TableName varchar(56), @LockKey varchar(512) ,@AuthenticationToken varchar(36), @Context varchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @IsMyLock int;	
	DECLARE @SelContext varchar(10);
	SET @IsMyLock = 0;
	BEGIN TRY
		SELECT @SelContext = Context FROM TB_Locks WHERE TableName = @TableName AND LockKey = @LockKey AND Context = @Context AND AuthenticationToken = @AuthenticationToken;
		if (@SelContext IS NOT NULL )
			SET @IsMyLock = 1;
	END TRY
	BEGIN CATCH
		SET @IsMyLock = 0;		
	END CATCH
	return @IsMyLock;
END
GO