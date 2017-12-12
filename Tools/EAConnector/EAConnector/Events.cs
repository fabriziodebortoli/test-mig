using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using PAT.Workflow.Runtime;
using System.Activities;

namespace EAConnector.Events
{
	//in questo namespace vengono definiti gli eventi resi disponibili dal connettore

	//<summary>
	// Definition of event when raised on MyApplicationObject update
	// Category             --> è un campo che descrive la categoria dell'evento;
	//                          è un campo semplicemente descrittivo
	// Description          --> Descrizione dell'evento;
	// WorkflowObject       --> GUID univoco dell'evento
	// ApplicationProvider  --> tipo della connessione a cui è associato l'evento


	/********************NewAttachmentById*******************/
	[Category("Attachments")]
	[Description("New Attachment")]
	[WorkflowObject("123B495A-0F7D-41A6-A696-B99AC94576DF")]
	[ApplicationProvider(typeof(EAConnector.EAConnection))]
	public class NewAttachment: ApplicationProviderEvent
	{
		/// <summary>
		/// definisco gli argument in ingresso
		/// </summary>
		/// <param name="builder"></param>
		protected override void InitializeWorkflow(/*ref*/ ActivityBuilder builder)
		{
			var attachmentId		= new DynamicActivityProperty { Name = "AttachmentId", Type = typeof(InArgument<int>) };
			var requesterUserName	= new DynamicActivityProperty { Name = "RequesterUserName", Type = typeof(InArgument<string>) };
			var approvalUserName	= new DynamicActivityProperty { Name = "ApprovalUserName", Type = typeof(InArgument<string>) };
			var comments			= new DynamicActivityProperty { Name = "Comments", Type = typeof(InArgument<string>) };

			builder.Properties.Add(attachmentId);
			builder.Properties.Add(requesterUserName);
			builder.Properties.Add(approvalUserName);
			builder.Properties.Add(comments);
		}
	}

	/*************Add user in BB Studio***********/
	[Category("Attachments")]
	[Description("Add User")]
	[WorkflowObject("789B495A-0F7D-41A6-A696-B99AC94576DF")]
	[ApplicationProvider(typeof(EAConnector.EAConnection))]
	public class AddUser: ApplicationProviderEvent
	{
		/// <summary>
		/// definisco gli argument in ingresso
		/// </summary>
		/// <param name="builder"></param>
		protected override void InitializeWorkflow(/*ref*/ ActivityBuilder builder)
		{
			var userName	= new DynamicActivityProperty { Name = "Username", Type = typeof(InArgument<string>) };
			var firstName	= new DynamicActivityProperty { Name = "FirstName", Type = typeof(InArgument<string>) };
			var lastName	= new DynamicActivityProperty { Name = "LastName", Type = typeof(InArgument<string>) };
			var companyName = new DynamicActivityProperty { Name = "CompanyName", Type = typeof(InArgument<string>) };
			var password	= new DynamicActivityProperty { Name = "Password", Type = typeof(InArgument<string>) };
			builder.Properties.Add(userName);
			builder.Properties.Add(firstName);
			builder.Properties.Add(lastName);
			builder.Properties.Add(companyName);
			builder.Properties.Add(password);
		}
	}

	/*************Delete user in BB Studio***********/
	[Category("Attachments")]
	[Description("Delete User")]
	[WorkflowObject("012B495A-0F7D-41A6-A696-B99AC94576DF")]
	[ApplicationProvider(typeof(EAConnector.EAConnection))]
	public class DeleteUser: ApplicationProviderEvent
	{
		//definisco gli argument in ingresso; 
		//in questo caso i parametri in ingresso saranno:
		//username  nvarchar 25
		protected override void InitializeWorkflow(/*ref*/ ActivityBuilder builder)
		{
			var username = new DynamicActivityProperty { Name = "Username", Type = typeof(InArgument<string>) };
			builder.Properties.Add(username);
		}
	}

	/*************Processo di Test per il BB Settings***********/
	[Category("Test")]
	[Description("Test Process")]
	[WorkflowObject("234B495A-0F7D-41A6-A696-B99AC94576DF")]
	[ApplicationProvider(typeof(EAConnector.EAConnection))]
	public class TestProcess : ApplicationProviderEvent
	{
		/// <summary>
		/// definisco gli argument in ingresso
		/// </summary>
		/// <param name="builder"></param>
		protected override void InitializeWorkflow(/*ref*/ ActivityBuilder builder)
		{
			var requesterUserName = new DynamicActivityProperty { Name = "RequesterUserName", Type = typeof(InArgument<string>) };
			builder.Properties.Add(requesterUserName);
		}
	}

	/*************Processo di Test***********/
	[Category("FormInstance")]
	[Description("Delete FormInstance")]
	[WorkflowObject("345B495A-0F7D-41A6-A696-B99AC94576DF")]
	[ApplicationProvider(typeof(EAConnector.EAConnection))]
	public class DeleteFormInstance : ApplicationProviderEvent
	{
		/// <summary>
		/// definisco gli argument in ingresso
		/// </summary>
		/// <param name="builder"></param>
		protected override void InitializeWorkflow(/*ref*/ ActivityBuilder builder)
		{
			var requesterUserName	= new DynamicActivityProperty { Name = "RequesterUserName", Type = typeof(InArgument<string>) };
			var formInstanceId		= new DynamicActivityProperty { Name = "FormInstanceId", Type = typeof(InArgument<int>) };
			builder.Properties.Add(requesterUserName);
			builder.Properties.Add(formInstanceId);
		}
	}
}
