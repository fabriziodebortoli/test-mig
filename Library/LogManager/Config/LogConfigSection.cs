using System.Configuration;
using Microarea.Library.LogManager.Loggers;

namespace Microarea.Library.LogManager.Config
{
	//=========================================================================
	class LogConfigSection : ConfigurationSection
	{
		//---------------------------------------------------------------------
		[ConfigurationProperty(BaseLogger.LoggersTag, IsRequired = false)]
		public LoggerConfigElementCollection Loggers
		{
			get
			{
				return this[BaseLogger.LoggersTag] as LoggerConfigElementCollection;
			}
		}
	}
}
