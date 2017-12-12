using System;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	/// <summary>
	/// Attributo per identificare i campi aggiunti alle varie classi dalla customizzazione
	/// </summary>
	//=============================================================================
	[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
	public class ExcludeFromIntellisenseAttribute : System.Attribute
	{
		//-----------------------------------------------------------------------------
		public ExcludeFromIntellisenseAttribute()
		{ }
	}

	/// <summary>
	/// Attributo per identificare i campi aggiunti alle varie classi dalla customizzazione
	/// </summary>
	//=============================================================================
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class PreserveFieldAttribute : System.Attribute
	{
		//-----------------------------------------------------------------------------
		public PreserveFieldAttribute()
		{
		}
	}
	
	/// <summary>
	/// Attributo per indicare che il valore di una property è un reference ad un oggetto dell'object model
	/// </summary>
	//=============================================================================
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class DocumentNamespaceAttribute : System.Attribute
	{
		private string documentNamespace;

		//-----------------------------------------------------------------------------
		public string DocumentNamespace
		{
			get { return documentNamespace; }
		}
		//-----------------------------------------------------------------------------
		public DocumentNamespaceAttribute(string documentNamespace)
		{
			this.documentNamespace = documentNamespace;
		}

	}

	/// <summary>
	/// Attributo per indicare che il valore di una property è un reference ad un oggetto dell'object model
	/// </summary>
	//=============================================================================
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public sealed class ObjectReferenceAttribute : System.Attribute
	{
		//-----------------------------------------------------------------------------
		public ObjectReferenceAttribute()
		{
		}
	}

	/// <summary>
	/// </summary>
	//=============================================================================
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
	public class ContentDescriptionAttribute : System.Attribute
	{
		private string contentType;
		private string nameSpace;
		private string mainClass;

		//-----------------------------------------------------------------------------
		public string ContentType { get { return contentType; }}
		public string Namespace { get { return nameSpace; } set { nameSpace = value; } }
		public string MainClass { get { return mainClass; } set { mainClass = value; }  }

		//-----------------------------------------------------------------------------
		public ContentDescriptionAttribute(string contentType, string nameSpace, string mainClass)
		{
			this.contentType = contentType;
			this.nameSpace = nameSpace;
			this.mainClass = mainClass;
		}

		//--------------------------------------------------------------------------------
		public override bool Equals (object obj)
		{
			ContentDescriptionAttribute attribute = obj as ContentDescriptionAttribute;

			return attribute != null && attribute.ContentType == ContentType && attribute.Namespace == Namespace && attribute.MainClass == MainClass;
		}

		//--------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	///<summary>
	/// Describes the licence to be checked to load an assembly
	/// </summary>
	//=========================================================================
	[AttributeUsageAttribute(AttributeTargets.Assembly)]
	public class OwnerEasyBuilderAppAttribute : Attribute
	{
		public string Application { get; set; }
		public string Module { get; set; }

		//--------------------------------------------------------------------------------
		public OwnerEasyBuilderAppAttribute(string application, string module)
		{
			this.Application = application;
			this.Module = module;
		}

		//--------------------------------------------------------------------------------
		public static bool GetModuleInfo(System.Reflection.Assembly asm, out string app, out string mod)
		{
			app = mod = "";
			object[] attrs = asm.GetCustomAttributes(typeof(OwnerEasyBuilderAppAttribute), true);

			if (attrs == null || attrs.Length == 0)
				return false;

			OwnerEasyBuilderAppAttribute attr = (OwnerEasyBuilderAppAttribute)attrs[0];
			app = attr.Application;
			mod = attr.Module;
			return true;
		}
	}
}
