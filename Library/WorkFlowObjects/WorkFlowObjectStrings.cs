using System;
using System.Resources;
using System.Reflection;

namespace Microarea.Library.WorkFlowObjects
{
	/// <summary>
	/// Summary description for WorkFlowObjectStrings.
	/// </summary>
	// ========================================================================
	public class WorkFlowObjectStrings
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowObjects.Strings", Assembly.GetExecutingAssembly());

		public static string EmptyConnectionStringMsg					{ get { return rm.GetString("EmptyConnectionStringMsg"); }}
		public static string InvalidWorkFlowConstruction				{ get { return rm.GetString("InvalidWorkFlowConstruction"); }}
		public static string InvalidWorkFlowActivityConstruction		{ get { return rm.GetString("InvalidWorkFlowActivityConstruction"); }}
		public static string WorkFlowDeleteFailedMsgFmt					{ get { return rm.GetString("WorkFlowDeleteFailedMsgFmt"); }}
		public static string WorkFlowActivityDeleteFailedMsgFmt			{ get { return rm.GetString("WorkFlowActivityDeleteFailedMsgFmt"); }}
		public static string WorkFlowStateDeleteFailedMsgFmt            { get { return rm.GetString("WorkFlowStateDeleteFailedMsgFmt"); }}
		public static string InvalidWorkFlowTemplateConstruction		{ get { return rm.GetString("InvalidWorkFlowTemplateConstruction"); }}
		public static string InvalidWorkFlowStateConstruction			{ get { return rm.GetString("InvalidWorkFlowStateConstruction"); }}
		public static string WorkFlowTemplateFailedMsgFmt				{ get { return rm.GetString("WorkFlowTemplateFailedMsgFmt"); }}
		public static string WorkFlowTemplateActivityFailedMsgFmt		{ get { return rm.GetString("WorkFlowTemplateActivityFailedMsgFmt"); }}
		public static string WorkFlowTemplateStateFailedMsgFmt			{ get { return rm.GetString("WorkFlowTemplateStateFailedMsgFmt"); }}
		public static string WorkFlowTemplateUpdateFailedMsgFmt			{ get { return rm.GetString("WorkFlowTemplateUpdateFailedMsgFmt"); }}
		public static string WorkFlowTemplateActivityUpdateFailedMsgFmt { get { return rm.GetString("WorkFlowTemplateActivityUpdateFailedMsgFmt"); }}
		public static string WorkFlowTemplateStateUpdateFailedMsgFmt	{ get { return rm.GetString("WorkFlowTemplateStateUpdateFailedMsgFmt"); }}
		public static string WorkFlowTemplateDeleteFailedMsgFmt			{ get { return rm.GetString("WorkFlowTemplateDeleteFailedMsgFmt"); }}
		public static string WorkFlowTemplateStateDeleteFailedMsgFmt	{ get { return rm.GetString("WorkFlowTemplateStateDeleteFailedMsgFmt"); }}
		public static string WorkFlowTemplateActivityDeleteFailedMsgFmt { get { return rm.GetString("WorkFlowTemplateActivityDeleteFailedMsgFmt"); }}
		public static string WorkFlowTemplateGenericExceptionMsg		{ get { return rm.GetString("WorkFlowTemplateGenericExceptionMsg"); }}
		public static string InvalidSqlConnectionErrMsg                 { get { return rm.GetString("InvalidSqlConnectionErrMsg"); }}
		public static string WorkFlowFailedMsgFmt                       { get { return rm.GetString("WorkFlowFailedMsgFmt"); }}
		public static string WorkFlowActivityFailedMsgFmt               { get { return rm.GetString("WorkFlowActivityFailedMsgFmt"); }}
		public static string WorkFlowStateFailedMsgFmt				    { get { return rm.GetString("WorkFlowStateFailedMsgFmt"); }}
		public static string WorkFlowActivityUpdateFailedMsgFmt         { get { return rm.GetString("WorkFlowActivityUpdateFailedMsgFmt"); }}
		public static string WorkFlowStateUpdateFailedMsgFmt			{ get { return rm.GetString("WorkFlowStateUpdateFailedMsgFmt"); }}
		public static string WorkFlowUpdateFailedMsgFmt                 { get { return rm.GetString("WorkFlowUpdateFailedMsgFmt"); }}
		public static string WorkFlowGenericExceptionMsg                { get { return rm.GetString("WorkFlowGenericExceptionMsg"); }}
		public static string InvalidWorkFlowUserConstruction			{ get { return rm.GetString("InvalidWorkFlowUserConstruction"); }}
		public static string WorkFlowUserFailedMsgFmt                   { get { return rm.GetString("WorkFlowUserFailedMsgFmt"); }}
		public static string WorkFlowUserDeleteFailedMsgFmt             { get { return rm.GetString("WorkFlowUserDeleteFailedMsgFmt"); }}
		public static string WorkFlowUserUpdateFailedMsgFmt             { get { return rm.GetString("WorkFlowUserUpdateFailedMsgFmt"); }}
	}
}
