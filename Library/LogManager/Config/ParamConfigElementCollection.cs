using System.Configuration;
using Microarea.Library.LogManager.Loggers;

namespace Microarea.Library.LogManager.Config
{
	//=========================================================================
	[ConfigurationCollection(
		typeof(ParamConfigElement),
		AddItemName = BaseLogger.ParamTag,
		CollectionType = ConfigurationElementCollectionType.BasicMap
		)]
	class ParamConfigElementCollection : ConfigurationElementCollection
	{
		private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

		//---------------------------------------------------------------------
		protected override ConfigurationElement CreateNewElement()
		{
			return new ParamConfigElement();
		}

		//---------------------------------------------------------------------
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ParamConfigElement)element).Name;
		}

		//---------------------------------------------------------------------
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}

		//---------------------------------------------------------------------
		protected override string ElementName
		{
			get
			{
				return BaseLogger.ParamTag;
			}
		}

		//---------------------------------------------------------------------
		protected override ConfigurationPropertyCollection Properties
		{
			get
			{
				return _properties;
			}
		}
	}
}
