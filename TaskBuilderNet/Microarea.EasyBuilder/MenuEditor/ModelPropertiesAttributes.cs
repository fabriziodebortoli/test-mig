using System;
using System.Xml;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	/// <remarks/>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal sealed class AffectsAppearanceAttribute : System.Attribute
	{
		private bool affectsAppearance = false;

		//---------------------------------------------------------------------
		/// <remarks/>
		public AffectsAppearanceAttribute(bool aAffectsAppearanceFlag)
		{
			affectsAppearance = aAffectsAppearanceFlag;
		}
		//---------------------------------------------------------------------
		/// <remarks/>
		public bool AffectsAppearance { get { return affectsAppearance; } }
	}

	//====================================================================
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal sealed class MenuItemXmlNodeTypeAttribute : Attribute
	{
		private XmlNodeType xmlNodeType = XmlNodeType.None;

		//---------------------------------------------------------------------
		/// <remarks/>
		public XmlNodeType XmlNodeType
		{
			get
			{
				return xmlNodeType;
			}
		}

		//---------------------------------------------------------------------
		/// <remarks/>
		public MenuItemXmlNodeTypeAttribute(XmlNodeType aXmlNodeType)
		{
			xmlNodeType = aXmlNodeType;
		}
	}

	//====================================================================
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	internal sealed class MenuItemXmlNodeNameAttribute : Attribute
	{
		private string xmlNodeName = string.Empty;

		//---------------------------------------------------------------------
		/// <remarks/>
		public string XmlNodeName
		{
			get
			{
				return xmlNodeName;
			}
		}

		//---------------------------------------------------------------------
		public MenuItemXmlNodeNameAttribute(string aXmlNodeName)
		{
			xmlNodeName = aXmlNodeName;
		}
	}
}
