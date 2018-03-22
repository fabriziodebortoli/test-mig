using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
	//=========================================================================
	public class Strings
	{
		// parameters
		public static readonly string ApplicationName = "applicationName";
		public static readonly string ApplicationType = "applicationType";
		public static readonly string DefaultPair = "defaultPair";
		public static readonly string DefaultReq = "defaultReq";
		public static readonly string ModuleName = "moduleName";
		public static readonly string Namespace = "ns";		
		public static readonly string User = "user";
	
		// messages
		public static readonly string MissingApplicationType = "Missing parameter applicationType";
		public static readonly string MissingApplicationName = "Missing parameter applicationName";
		public static readonly string MissingModuleName = "Missing parameter moduleName";
		public static readonly string ErrorCreatingObject = "Error Creating Object";
		public static readonly string ErrorDeletingObject = "Error Deleting Object";
		public static readonly string ObjectSuccessfullyCreated = "Successfully Created";
		public static readonly string ObjectSuccessfullyDeleted = "Successfully Deleted";
		public static readonly string ApplicationAlreadyExists = "Application already exists!";
		public static readonly string Separator = ";";

	}


	//========================================================================
	public sealed class EasyStudioPreferencesXML
	{
		private EasyStudioPreferencesXML()
		{ }
		public sealed class Element
		{
			private Element()
			{ }
			public const string Root = "Root";
			public const string ESPreferences = "EasyStudioPreferences";
			public const string DefaultContext = "DefaultContext";
			public const string CurrentContext = "CurrentContext";
		}
		public sealed class Attribute
		{
			private Attribute()
			{ }
			public const string DefaultApplication = "DefaultContextApplication";
			public const string DefaultModule = "DefaultContextModule";
			public const string CurrentApplication = "CurrentApplication";
			public const string CurrentModule = "CurrentModule";
			//public const string UtcDate = "utcDate";
		}
	}

}
