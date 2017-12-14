using System;
using System.ComponentModel;
using System.Resources;

namespace Microarea.TaskBuilderNet.Core.Localization
{
	//=========================================================================
	[AttributeUsage(AttributeTargets.All)]
	public sealed class LocalizedCategoryAttribute : CategoryAttribute
	{
		private Type type;
		private ResourceHelper resourceHelper;

		//---------------------------------------------------------------------
		public LocalizedCategoryAttribute(string key, Type resourcesType)
			: base(key)
		{
			this.resourceHelper = new ResourceHelper();
			this.type = resourcesType;
		}

		//---------------------------------------------------------------------
		protected override string GetLocalizedString(string key)
		{
			ResourceManager rm = resourceHelper.GetResourceManager(type);
			return rm.GetString(key);
		}
	}

	//====================================================================
	/// <summary>
	///    Specifies a localized description for a property or event.
	/// </summary>
	/// <remarks>
	///    Unlike the DescriptionAttribute class, the Description property of
	///    LocalizedDescriptionAttribute returns a description that is set in a
	///    resource, which can be localized.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
	{
		private string descriptionResourceNameValue;
		private ResourceHelper resourceHelper;

		/// <summary>
		///    Initializes an instance of the LocalizedDescriptionAttribute class.
		/// </summary>
		/// <param name="descriptionResourceName">
		///    The name of the resource string to use as the description.
		/// </param>
		public LocalizedDescriptionAttribute(string descriptionResourceName, Type resourcesType)
		{
			descriptionResourceNameValue = descriptionResourceName;

			this.resourceHelper = new ResourceHelper();
			ResourceManager stringsResourceManager = resourceHelper.GetResourceManager(resourcesType);

			base.DescriptionValue = (stringsResourceManager != null) ? stringsResourceManager.GetString(descriptionResourceNameValue) : String.Empty;
		}
		//---------------------------------------------------------------------
		public string DescriptionResourceName { get { return descriptionResourceNameValue; } }

	}
}
