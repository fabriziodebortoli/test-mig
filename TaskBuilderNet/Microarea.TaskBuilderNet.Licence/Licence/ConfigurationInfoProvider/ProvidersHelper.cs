using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider
{
	/// <summary>
	/// Summary description for ProvidersHelper.
	/// </summary>
	//public class ProvidersHelper
	//{

	//	//---------------------------------------------------------------------
	//	public static XmlDocument LoadArticleFromCsm(string fileName)
	//	{
	//		try
	//		{
	//			XmlDocument doc = new XmlDocument();

	//			ActivationObjectHelper aoh	= new ActivationObjectHelper();
	//			string articleString		= aoh.GetArticleStringByPath(fileName);
	//			if (articleString == null || articleString == String.Empty)
	//				return null;
	//			doc.LoadXml(articleString);

	//			return doc;
	//		}
	//		catch (Exception exc)
	//		{
	//			Debug.WriteLine("IMProvider.LoadArticleFromCsm - Error: " + exc.Message);
	//			string error = String.Format(CultureInfo.InvariantCulture, LicenceStrings.ErrorLoading, fileName);
	//			string message = String.Concat(error, " ", LicenceStrings.ExceptionMessage, exc.Message);
	//			//TODO log
	//			return null;
	//		}
	//	}
	//}
}
