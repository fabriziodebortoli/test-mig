using System.Configuration;
using Microarea.Library.LogManager.Loggers;

namespace Microarea.Library.LogManager.Config
{
	//=========================================================================
	[ConfigurationCollection(
		typeof(LoggerConfigElement),
		AddItemName = BaseLogger.LoggerTag,
		CollectionType = ConfigurationElementCollectionType.BasicMap
		)]
	class LoggerConfigElementCollection : ConfigurationElementCollection
	{
		private static readonly ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

		//---------------------------------------------------------------------
		protected override ConfigurationElement CreateNewElement()
		{
			return new LoggerConfigElement();
		}

		//---------------------------------------------------------------------
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((LoggerConfigElement)element).Name;
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
				return BaseLogger.LoggerTag;
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
