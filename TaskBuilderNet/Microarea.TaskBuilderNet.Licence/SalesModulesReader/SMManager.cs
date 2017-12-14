using System;
using System.IO;
using System.Xml;

namespace Microarea.TaskBuilderNet.Licence.SalesModulesReader
{
	//=========================================================================
	public static class SMManager
	{
		public static string ErrorMessage = null;
		
		//decripta 
		//---------------------------------------------------------------------
		public static bool HandlerOff(string path, string productname, out XmlDocument doc)
		{
			doc = null;
			if (path == String.Empty || !File.Exists(path))
				return false;
			try
			{
				HandlerOff reader = new HandlerOff(productname);
				string val = reader.GetArticleStringByPath(path);
				doc = new XmlDocument();
				doc.LoadXml(val);
			}
			catch (Exception exc)
			{
				ErrorMessage = "SalesModule reading failed. " + exc.Message;
				return false;
			}
			return true;
		}

	}
}
