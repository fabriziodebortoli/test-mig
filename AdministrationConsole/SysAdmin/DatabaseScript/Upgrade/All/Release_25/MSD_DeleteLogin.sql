SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_DeleteLogin]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_DeleteLogin]
GO

CREATE PROCEDURE MSD_DeleteLogin
	(
		@par_loginid int
	) AS
BEGIN TRANSACTION
	IF exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ObjectGrants]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
		DELETE MSD_ObjectGrants WHERE LoginId = @par_loginid
	DELETE MSD_CompanyRolesLogins WHERE LoginId = @par_loginid
	DELETE MSD_SlaveLogins WHERE LoginId = @par_loginid
	DELETE MSD_CompanyLogins WHERE LoginId = @par_loginid
	DELETE MSD_Logins WHERE LoginId = @par_loginid
	IF @@error <> 0 
        BEGIN
			ROLLBACK TRANSACTION
			RETURN(1)
        END
        
COMMIT TRANSACTION
RETURN (0)
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

