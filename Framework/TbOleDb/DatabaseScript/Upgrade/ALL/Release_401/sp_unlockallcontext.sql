SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_unlockallcontext]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_unlockallcontext]
GO

CREATE PROCEDURE [dbo].[sp_unlockallcontext]
@Context varchar(10),  @AuthenticationToken varchar(36)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	BEGIN TRY
		DELETE FROM TB_Locks WHERE Context = @Context AND AuthenticationToken = @AuthenticationToken;
		return  @@ROWCOUNT;
	END TRY
	BEGIN CATCH
		return 0;		
	END CATCH
END
GO