SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_unlockcurrent]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_unlockcurrent]
GO

CREATE PROCEDURE [dbo].[sp_unlockcurrent]
@TableName varchar(56), @LockKey varchar(512), @AuthenticationToken varchar(36), @Context varchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	BEGIN TRY
		DELETE FROM TB_Locks WHERE TableName = @TableName AND LockKey = @LockKey AND AuthenticationToken = @AuthenticationToken AND Context = @Context; 		
		return @@ROWCOUNT;
	END TRY
	BEGIN CATCH
		return 0;		
	END CATCH
END
GO