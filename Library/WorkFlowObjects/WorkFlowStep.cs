using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Microarea.Library.WorkFlowObjects
{
	/// <summary>
	/// WorkFlowStep.
	/// Single step for the workflow
	/// </summary>
	// ========================================================================
	public class WorkFlowStep
	{
		public static string WorkFlowStepTableName				= "MSD_WorkFlowStep";
		public const string TransitionIdColumnName				= "TransitionId";
		public const string WorkFlowIdColumnName				= "WorkFlowId";
		public const string CompanyIdColumnName					= "CompanyId";
		public const string UserIdColumnName					= "LoginId";
		public const string FromActivityIdColumnName			= "FromActivityId";
		public const string ToActivityIdColumnName				= "ToActivityId";
		public const string StateIdColumnName					= "StateId";
		public const string RoleIdColumnName                    = "RoleId";

		private int		companyId			= -1;
		private int     transitionId		= -1;
		private int		workFlowId			= -1;
		private int     loginId				= -1;
		private int     roleId              = -1;
		private int     fromActivityId		= -1;
		private int     toActivityId		= -1;

		//data read from MSD_WorkFlowActivity
		private string  fromActivityName			= string.Empty;
		private string  toActivityName				= string.Empty;

		//data read from MSD_WorkflowUser
		private string userOrRoleName				= string.Empty;

		//---------------------------------------------------------------------
		public int		TransitionId		{ get { return transitionId;	}} // primary key
		//---------------------------------------------------------------------
		public int		CompanyId			{ get { return companyId;		}} // primary key
		//---------------------------------------------------------------------
		public int		WorkFlowId			{ get { return workFlowId;		}} // primary key 
		//---------------------------------------------------------------------
		public int		LoginId				{ get { return loginId;			}}  
		//---------------------------------------------------------------------
		public int		RoleId				{ get { return roleId;			}}
		//---------------------------------------------------------------------
		public int      FromActivityId		{ get { return fromActivityId;	}}
		//---------------------------------------------------------------------
		public int		ToActivityId		{ get { return toActivityId;	}}
		

		//---------------------------------------------------------------------
		public WorkFlowStep()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		//---------------------------------------------------------------------
		public static string GetAllWorkFlowStep(int aCompanyId, int aWorkFlowId)
		{
			string queryText =  "SELECT * FROM " + WorkFlowStepTableName + " WHERE "; 
			queryText += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
			queryText +=  WorkFlowIdColumnName + " = " + aWorkFlowId.ToString();

			queryText +=  " ORDER BY " + TransitionIdColumnName;

			return queryText;
		}
	}
}
