SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_iscurrentlocked]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_iscurrentlocked]
GO

CREATE PROCEDURE [dbo].[sp_iscurrentlocked]
@TableName varchar(56), @LockKey varchar(512), @Context varchar(10)
AS
BEGIN
	DECLARE @IsCurrentLocked int;
	SET @IsCurrentLocked = 0;
	DECLARE @SelContext varchar(10);
	BEGIN TRY
		SET @SelContext = (SELECT TOP 1 Context FROM TB_Locks WHERE TableName = @TableName AND LockKey = @LockKey);
		if ( @SelContext IS NOT NULL AND @SelContext <> @Context)
			SET @IsCurrentLocked = 1;
	END TRY
	BEGIN CATCH
		SET @IsCurrentLocked = 0;		
	END CATCH
	return @IsCurrentLocked;
END
GO