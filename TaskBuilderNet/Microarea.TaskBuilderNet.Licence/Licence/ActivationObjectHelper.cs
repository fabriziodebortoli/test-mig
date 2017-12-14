using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using Reader = Microarea.TaskBuilderNet.Licence.SalesModulesReader;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	//=========================================================================
	public class ActivationObjectHelper
	{
		
		//---------------------------------------------------------------------
		public ActivationObjectHelper()
		{
			
		}

		//---------------------------------------------------------------------
		public static int CalculateCalEnt(IList<SerialNumberInfo> list, CalTypeEnum caltype)
		{
			int c = 0 ;
			foreach (SerialNumberInfo s in list)
			{
				SerialNumber ss = new SerialNumber(s.GetSerialWOSeparator(), caltype);
				if (ss.Edition == Edition.Enterprise)
					if (ss.ConcurrentCAL != null && ss.ConcurrentCAL.Length > 0)
						c += int.Parse(ss.ConcurrentCAL);
			}
			return c;
		}

		//--------------------------------------------------------------------- 
		public static Edition GetEditionFromString(string edition)
		{
			if (String.Compare(edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Standard;
			if (String.Compare(edition, NameSolverStrings.ProfessionalEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Professional;
			if (String.Compare(edition, NameSolverStrings.EnterpriseEdition, true, CultureInfo.InvariantCulture) == 0) return Edition.Enterprise;
            if (String.Compare(edition, "ALL", true, CultureInfo.InvariantCulture) == 0) return Edition.ALL;
			return Edition.Undefined;
		}

		//---------------------------------------------------------------------
		public string GetArticleStringByPath(string path, string productname)
		{ 

			if (path == string.Empty || path == null)
				return string.Empty;
			
				XmlDocument doc;
				bool ok = Reader.SMManager.HandlerOff(path,  productname, out doc);
				if (!ok)
				{
					return string.Empty;
				}
				return doc.OuterXml;
			
			
//			byte[] buffer = GetFileAsBytes(path);
//			return GetArticleString(buffer);
		}

//		//---------------------------------------------------------------------
//		public bool SaveArticleValue(string sourceFileFullName, out string destFileFullName)
//		{ 
//			destFileFullName = string.Empty;
//
//			if (sourceFileFullName == null || sourceFileFullName == string.Empty)
//			{
//				Debug.Fail("Errore in ActivationObjectHelper.SaveArticleValue: SalesModule non specificato.");
//				return false;
//			}
//
//			if (!File.Exists(sourceFileFullName))
//			{
//				Debug.Fail("Errore in ActivationObjectHelper.SaveArticleValue: il SalesModule " + sourceFileFullName + " non esiste.");
//				return false;
//			}
//
//			try
//			{
//				if (HasOtherExtension(sourceFileFullName))
//				{
//					Debug.Fail("Errore nel salvataggio del SalesModule " + sourceFileFullName + Environment.NewLine + "Il file è già salvato.");
//					return false;
//				}
//
//				destFileFullName = sourceFileFullName;
//				destFileFullName = GetOtherName(sourceFileFullName);
//				string content = GetArticleValueFromPath(sourceFileFullName);
//				return SaveFileFromString(content, destFileFullName);
//			}
//			catch (Exception exc)
//			{
//				Debug.Fail("Errore nel salvataggio del SalesModule " + sourceFileFullName + Environment.NewLine + exc.Message);
//				return false;
//			}
//		}

		//---------------------------------------------------------------------
		public static string GetOtherName(string sourceFileFullName)
		{
			return Path.ChangeExtension(sourceFileFullName, NameSolverStrings.CsmExtension);
		}

		//---------------------------------------------------------------------
		public static bool HasOtherExtension(string aValue)
		{ 
			if (aValue == null || aValue == string.Empty)
				return false;

			string extension = Path.GetExtension(aValue);

			if (extension == null || extension == string.Empty)
				return false;

			return IsCsm(extension);
		}

		//---------------------------------------------------------------------
		public static bool IsCsm(string extension)
		{
			return string.Compare(extension, NameSolverStrings.CsmExtension, true, CultureInfo.InvariantCulture) == 0;
		}

		//---------------------------------------------------------------------
		public static bool IsPowerProducer(string internalCode)
		{
			bool ok = false;
			foreach (string s in UserInfo.InternalCodes)
			{
				ok = ok || String.Compare(internalCode, s, true, CultureInfo.InvariantCulture) == 0;
			}
			return ok;
		}
		
		/*//---------------------------------------------------------------------
		public static bool UseEBackendSide(string storageName, string storageRelease)
		{
			// UseEncryptionBackendSide
			if (string.Compare(storageRelease, "1.0", true, CultureInfo.InvariantCulture) == 0 &&
				string.Compare(storageName, "MagoNet-Pro", true, CultureInfo.InvariantCulture) == 0 &&
				Functions.IsTestDeployment())	// decidiamo di tenere compatibilità solo in test
				return false;
			return true;
			// NOTE se decidiamo di togliere compatibilità con sorgente di MagoNet-Pro 1.0,
			//		allora questa non va pubblicata su server di deployment e occorre mettere
			//		un blocco a WebUpdater perché impedisca e notifichi che non può installare
			//		la versione 1.0 di MagoNetPro
		}*/

//		//---------------------------------------------------------------------
//		private static string GetArticleString(byte[] buffer)
//		{ 
//			if (buffer == null || buffer.Length == 0 )
//				return string.Empty;
//			string inner = BasePathFinder.GetInnerPath(webPath);
//			string outer = BasePathFinder.GetOuterPath(webPath);
//			return BasePathFinder.GetArticle(buffer, inner, outer);
//		}
//		//---------------------------------------------------------------------
//		private static string GetArticleValueFromPath(string path)
//		{ 
//			if (path == string.Empty || path == null)
//				return string.Empty;
//			byte[] buffer = GetFileAsBytes(path);
//			
//			return GetArticleValue(buffer);
//		}
//
//		//---------------------------------------------------------------------
//		private static string GetArticleValue(byte[] buffer)
//		{ 
//			if (buffer == null || buffer.Length == 0 )
//				return string.Empty;
//
//			string inner = BasePathFinder.GetInnerPath(webPath);
//			string outer = BasePathFinder.GetOuterPath(webPath);
//			return BasePathFinder.SetArticle(buffer, inner, outer);
//		}

		//---------------------------------------------------------------------
		private static bool SaveFileFromString(string aValue, string destPath)
		{ 
			if (aValue == string.Empty || aValue == null)
				return false;
			try
			{
				byte[] buffer = Convert.FromBase64String(aValue);
				FileStream fs = new FileStream(destPath, FileMode.Create);
				BinaryWriter sw = new BinaryWriter(fs);
				sw.Write(buffer, 0, buffer.Length);
				fs.Close();
				sw.Close();
			}
			catch (Exception exc)
			{
				Debug.Fail("Errore nel salvataggio del SalesModule: " + destPath + Environment.NewLine + exc.Message);
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		private static byte[] GetFileAsBytes(string filePath)
		{
			Stream stream = null;
			if (!File.Exists(filePath))
				return null;
			stream = File.OpenRead(filePath);
			long len = stream.Length;
			byte[] buffer = new byte[stream.Length];
			stream.Read(buffer, 0, (int)len);
			stream.Close();
			return buffer;
		}
		
		//---------------------------------------------------------------------
		public static int GetActivationVersionFromSolution(string path)
		{
			if (path == null || path.Length == 0 || !File.Exists(path))
				return 0;
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(path);
				XmlNode n = doc.DocumentElement.SelectSingleNode("@" + WceStrings.Attribute.ActivationVersion);
				if (n != null)
					return int.Parse(n.Value, CultureInfo.InvariantCulture);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.ToString());
			}
			return 0;
		}

	}
}
