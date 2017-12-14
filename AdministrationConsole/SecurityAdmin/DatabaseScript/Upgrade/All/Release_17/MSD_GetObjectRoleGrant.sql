SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_GetExecutionRoleGrant]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_GetExecutionRoleGrant]
GO


if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_GetObjectRoleGrant]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[MSD_GetObjectRoleGrant]
GO

CREATE PROCEDURE MSD_GetObjectRoleGrant
	(
		@par_companyid int, 
		@par_roleid int, 
		@par_objectNameSpace varchar (512),	
		@par_objectType int,
		@parout_isprotected int OUTPUT,
		@parout_grant int OUTPUT
	) AS 

	DECLARE @thereis_parentgrant integer
	DECLARE	@parent_grant integer
	DECLARE @parent_inheritmask integer
	
	DECLARE @thereis_rolegrant integer
	DECLARE	@role_grant integer
	DECLARE @role_inheritmask integer
	
	DECLARE @v_thereis_grant integer
	DECLARE	@v_inheritmask integer	
	DECLARE @v_parent_exist integer
	
	DECLARE @v_objectid integer

	SET @parout_grant = 0
	SET @parout_isprotected = 0

	SET @v_objectid = 0

	SELECT @v_objectid = ObjectId 
	FROM MSD_Objects INNER JOIN MSD_ObjectTypes 
    ON MSD_ObjectTypes.TypeId = MSD_Objects.TypeId
	WHERE NameSpace = @par_objectNameSpace AND 
		MSD_ObjectTypes.Type = @par_objectType

	EXEC MSD_GetSplitRoleGrant @par_companyid, @par_roleid, @v_objectid, 
			@thereis_rolegrant OUTPUT,
				@role_grant OUTPUT, 
				@role_inheritmask OUTPUT,				
			@thereis_parentgrant OUTPUT,
				@parent_grant OUTPUT, 
				@parent_inheritmask OUTPUT,				
			@v_thereis_grant OUTPUT,
				@parout_grant OUTPUT, 
				@v_inheritmask OUTPUT,				
			@parout_isprotected OUTPUT,
			@v_parent_exist OUTPUT
				
/* END PROCEDURE */

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

