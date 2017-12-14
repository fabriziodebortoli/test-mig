using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Diagnostics;

using Microarea.Library.SMBaseHandler;
using Microarea.Library.WSLogger;
using Microarea.Library.SMHandlerDbInterface;
using Microarea.Library.SMHandlerOn;

namespace Microarea.Library.SalesModulesWriter
{
	//=========================================================================
	public class SMManager
	{
		public	static string			ErrorMessage		= null;
		private static WSCryptLogWriter writerLogger		= null;

		//---------------------------------------------------------------------
		public static bool IsPowerProducerCodAz(string codAz)
		{
            string MicroareaCodAz = "0110G081";
            string UsoInternoCodAz = "USOINTERNO";
            string UpperCodAz = codAz.ToUpper();

            return (String.Compare(UpperCodAz, MicroareaCodAz, true) == 0) || (String.Compare(UpperCodAz, UsoInternoCodAz, true) == 0);
		}

		//---------------------------------------------------------------------
		public static bool IsMicroarea(string codAz)
		{
			return (IsPowerProducerCodAz(codAz) || (String.Compare(codAz, GetMicroareaName(), true) == 0));
		}

		//---------------------------------------------------------------------
		public static string GetMicroareaName()
		{
			return "Microarea";
		}

		/// <summary>
		/// Esegue tutta la procedura di verifica e criptazione del file 
		/// </summary>
		/// <param name="path">path del file salvato in locale</param>
		/// <param name="originalFilename">nome originale del file</param>
		/// <param name="dest">path di destinazione del file</param>
		/// <param name="codiceAzienda">codice azienda del pai</param>
		/// <param name="product">nome prodotto</param>
		/// <param name="logger">sistema di log</param>
		/// <param name="needFullLicence">specifica se il prodotto è tale da necessitare di una licenza full</param>
		/// <param name="isRegisteredProduct">specifica se il prodotto è censito sul pai</param>
		//---------------------------------------------------------------------
		public static bool HandlerOn(
										string path, 
										string originalFilename, 
										out	string dest, 
										string codiceAzienda, 
										string product, 
										WSCryptLogWriter logger, 
										out bool needFullLicence, 
										bool isRegisteredProduct
									)
		{
			dest = null;
			writerLogger = logger;
			if (!SMVerified(path, originalFilename, codiceAzienda, product, out needFullLicence, isRegisteredProduct))
				return false;
			HandlerOn handlerOn = new HandlerOn(writerLogger, product); 
			bool ok = handlerOn.HandleOn(path, out dest);
			ErrorMessage = handlerOn.ErrorMessage;
			return ok;
		}

		/// <summary>
		/// Verifica la sintassi e la grammatica
		/// </summary>
		/// <param name="doc">xmldocumentsu cui è caricato il salesmodule</param>
		/// <param name="path">path del salesmodules da caricare</param>
		//---------------------------------------------------------------------
		private static bool VerifyXml(ref XmlDocument doc, string path, string codAz, out string shortName)
		{
			shortName = null;
			doc = new XmlDocument();
			try
			{
				doc.Load(path);
			}
			catch (Exception exc)
			{
				ErrorMessage = MessagesCode.NotConsistentXml;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.NotConsistentXml, String.Format("Not valid XML in {0}. Exception message: {1}", path, exc.Message));
				return false;
			}
			//cerco solo il tag salesmodule che è l'unico comune a tutti, 
			//per esempio Application non esiste nei moduli SampleData...
			XmlNode smNode = doc.SelectSingleNode("SalesModule");
			if (smNode == null)
			{
				ErrorMessage = MessagesCode.WrongSyntax;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.WrongSyntax, String.Format("Syntax error in {0}.", path));
				return false;
			}
			//Verifica Includes
			XmlNodeList includeList = smNode.SelectNodes("Includes/Include/@name");
			if (includeList != null && includeList.Count > 0)
			{
				foreach (XmlNode include in includeList)
					if (include != null && include.Value != null && !IsMicroarea(codAz))
					{
						string name = include.Value.ToLower();
						if (
							name.StartsWith("erp-std.")				|| 
							name.StartsWith("erp-ent.")				|| 
							name.StartsWith("erp-pro.")				|| 
							name.StartsWith("erp.language")			||
							name.StartsWith("educationalpackage")
							)

						{
							ErrorMessage = MessagesCode.IncludesInvalid;
							writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.IncludesInvalid, String.Format("Syntax error in {0}. Invalid value of tag \"Includes\".", path));
							return false;
						}
				
					}
			}
			//obbligatorietà del tag shortNames
			bool shortNamePresent	= false;
			bool shortNameValid		= false;
			bool shortNamePrivate	= false;
			XmlNodeList nList = smNode.SelectNodes("ShortNames/ShortName/@name");
			if (nList != null && nList.Count > 0)
				foreach (XmlNode node in nList)
					if (node != null && node.Value != null)
					{
						shortNameValid = shortNameValid || node.Value.Length == 4;
						shortNamePrivate = shortNamePrivate || (String.Compare(node.Value, "DVLP", true) == 0 && !IsPowerProducerCodAz(codAz));
						shortNamePresent = true;
						//siccome nella generazione dei serial number viene capitalizzato:
						node.Value = node.Value.ToUpper();
						shortName = node.Value;
					}
			bool hasserialattribute = VerifyHasSerialAttribute(doc);
			if (!shortNamePresent && hasserialattribute)
			{
				shortName = null;
				ErrorMessage = MessagesCode.ShortNamesMissing;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.ShortNamesMissing, String.Format("Syntax error in {0}. Missing tag \"ShortNames\".", path));
				return false;
			}
			else if (!shortNameValid && hasserialattribute)
			{
				shortName = null;
				ErrorMessage = MessagesCode.ShortNameInvalid;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.ShortNameInvalid, String.Format("Syntax error in {0}. The value of tag \"ShortNames\" is not 4 chars.", path));
				return false;
			}
			if (shortNamePrivate)
			{
				shortName = null;
				ErrorMessage = MessagesCode.ShortNamePrivate;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.ShortNamePrivate, String.Format("The value of tag \"ShortNames\" is not a valid value, please change it.", path));
				return false;
			}
		
			return true;
		}


        //---------------------------------------------------------------------
        public static bool EasyHandlerOn(
                                        string path,
                                        string originalFilename,
                                        out string dest,
                                        WSCryptLogWriter logger,
            string  prodname
                                    )
        {
            dest = null;
            writerLogger = logger;
            EasySMVerified(path, originalFilename);
            HandlerOn handlerOn = new HandlerOn(writerLogger, "valorestudio");
            bool ok = handlerOn.HandleOn(path, out dest);
            ErrorMessage = handlerOn.ErrorMessage;
            return ok;
        }

        //---------------------------------------------------------------------
        private static bool EasySMVerified(string path, string originalFilename)
        {
            ErrorMessage = null;

            //il file esiste
            if (!File.Exists(path))
            {
                ErrorMessage = MessagesCode.GenericServerError;
                writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.SMVerified", MessagesCode.GenericServerError, String.Format("The path of the file to verify not exist: {0}.", path));
                return false;
            }
            XmlDocument doc = null;
            //l'xml è corretto
            string shortName;
            bool ok = EasyVerifyXml(ref doc, path, out shortName);
            if (!ok)
                return false;


            //imprimo il nome del file
            ok = WriteFileName(doc, originalFilename);
            if (!ok)
                return false;

            //salvo tutte le modifiche
            ok = SaveFile(doc, path);
            return ok;
        }

        //---------------------------------------------------------------------
        private static bool EasyVerifyXml(ref XmlDocument doc, string path, out string shortName)
        {
            shortName = null;
            doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch (Exception exc)
            {
                ErrorMessage = MessagesCode.NotConsistentXml;
                writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.NotConsistentXml, String.Format("Not valid XML in {0}. Exception message: {1}", path, exc.Message));
                return false;
            }
            //cerco solo il tag salesmodule che è l'unico comune a tutti, 
            //per esempio Application non esiste nei moduli SampleData...
            XmlNode smNode = doc.SelectSingleNode("SalesModule");
            if (smNode == null)
            {
                ErrorMessage = MessagesCode.WrongSyntax;
                writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.WrongSyntax, String.Format("Syntax error in {0}.", path));
                return false;
            }

            //obbligatorietà del tag shortNames
            bool shortNamePresent = false;
            bool shortNameValid = false;
            bool shortNamePrivate = false;
            XmlNodeList nList = smNode.SelectNodes("ShortNames/ShortName/@name");
            if (nList != null && nList.Count > 0)
                foreach (XmlNode node in nList)
                    if (node != null && node.Value != null)
                    {
                        shortNameValid = shortNameValid || node.Value.Length == 4;
                        shortNamePrivate = shortNamePrivate || (String.Compare(node.Value, "DVLP", true) == 0);
                        shortNamePresent = true;
                        //siccome nella generazione dei serial number viene capitalizzato:
                        node.Value = node.Value.ToUpper();
                        shortName = node.Value;
                    }
            bool hasserialattribute = VerifyHasSerialAttribute(doc);
            if (!shortNamePresent && hasserialattribute)
            {
                shortName = null;
                ErrorMessage = MessagesCode.ShortNamesMissing;
                writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.ShortNamesMissing, String.Format("Syntax error in {0}. Missing tag \"ShortNames\".", path));
                return false;
            }
            else if (!shortNameValid && hasserialattribute)
            {
                shortName = null;
                ErrorMessage = MessagesCode.ShortNameInvalid;
                writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.ShortNameInvalid, String.Format("Syntax error in {0}. The value of tag \"ShortNames\" is not 4 chars.", path));
                return false;
            }
            if (shortNamePrivate)
            {
                shortName = null;
                ErrorMessage = MessagesCode.ShortNamePrivate;
                writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyXml", MessagesCode.ShortNamePrivate, String.Format("The value of tag \"ShortNames\" is not a valid value, please change it.", path));
                return false;
            }

            return true;
        }



        /// <summary>
        /// torna il valore di hasserial
        /// </summary>
        //---------------------------------------------------------------------
        private static bool VerifyHasSerialAttribute(XmlDocument doc)
		{
			XmlNode hasserialNode = doc.SelectSingleNode("SalesModule/@hasserial");
			if (hasserialNode == null)
				return true;
			string hasserialValue = hasserialNode.Value;
			return (string.Compare(hasserialValue, bool.FalseString, true) != 0);

		}

	
		/// <summary>
		///Verifica i contenuti se accettabili in base alle licenze
		/// </summary>
		/// <param name="doc">xmldocumentsu cui è caricato il salesmodule</param>
		/// <param name="codAz">codice azienda sul pai</param>
		/// <param name="productSignature">nome prodotto</param>
		/// <param name="moduleName">nome del modulo</param>
		/// <param name="needFullLicence">specifica se necessita di licenza full(contiene solo tb e non mago)</param>
		/// <param name="isFullLicence">specifica se il produttore ha licenza full</param>
		/// <param name="isembedded">specifica se il produttore ha licenza per embedding</param>
		/// <param name="isRegisteredProduct">specifica se il prodotto è censito sul pai</param>
		//---------------------------------------------------------------------
		private static bool VerifyContents(
			XmlDocument doc, 
			string codAz, 
			string productSignature, 
			string moduleName, 
			out bool needFullLicence, 
			bool isFullLicence,
			bool isembedded,
			bool isRegisteredProduct,
			string shortName,
			out bool isServerEmbedded
			)
		{
			isServerEmbedded = false;
			needFullLicence = false;
			if(!VerifyHasSerialAttribute(doc))
			{
				ErrorMessage = MessagesCode.InvalidHasSerial;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManagerVerifyHasSerialAttribute", MessagesCode.InvalidHasSerial, String.Format("The module {0}-{1}-{2} has hasserial attribute set to false.", codAz, productSignature, moduleName));
				return false;
			}
			bool tb = false;
			bool mago = false;
			bool myApp = false;
			ArrayList magolist = new ArrayList();
			XmlNodeList appList = doc.SelectNodes("SalesModule/Application");
			if (appList == null)
				return true; // caso di sampledata
			foreach (XmlNode n in appList)
			{
				string container = ((XmlElement)n).GetAttribute("container");
				
				bool verifyTb = true;
				ArrayList toverify = null;
				if( (String.Compare(container, "TaskBuilder", true) == 0) || (String.Compare(container, "TaskBuilder.Net", true) == 0))
				{
					
					verifyTb = VerifyTB(n as XmlElement, out toverify);
					tb = true;
					if (verifyTb) continue;
				}
				if (!verifyTb || (String.Compare(container, "TaskBuilderApplications", true) == 0) || (String.Compare(container, "Applications", true) == 0))
				{
					string name = ((XmlElement)n).GetAttribute("name");
					if (
						(String.Compare(name, "MagoNet", true) == 0)	|| 
						(String.Compare(name, "ERP", true) == 0)		||
						!verifyTb//devo controllare anche i namespace di tb vietati
						)
					{
						mago = true;
						//accetta solo se embedded
						if (!isembedded)
						{
							ErrorMessage = MessagesCode.NotAuthorizedContents;
							writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyContents", MessagesCode.NotAuthorizedContents, String.Format("The module {0}-{1}-{2} contains a namespace 'ERP', but Embedded licence is missing", codAz, productSignature, moduleName));
							return false;
						}
						magolist = (!verifyTb && toverify.Count >0) ? new ArrayList(toverify) : FillMagoList(n);
						ArrayList dbMagoList = SMHandlerDbInterface.SMHandlerDbInterface.GetContainedPermitted(productSignature, codAz, shortName);
						isServerEmbedded = (dbMagoList != null && dbMagoList.Count > 0);
						foreach (string sSM in magolist)
						{
							string tocompare = String.Concat(name, ".", sSM);
							bool found = false;
							foreach (string sDB in dbMagoList)
							{
								if (String.Compare(tocompare, sDB, true) == 0)
								{
									found = true;
									break;
								}
							}
							if (!found)
							{
								writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyContents", MessagesCode.NotAuthorizedContents, String.Format("The module {0}-{1}-{2} contains namespace of ERP not allowed({3})", codAz, productSignature, moduleName, tocompare));
								ErrorMessage = MessagesCode.NotAuthorizedContents;
								return false;
							}
						}
					}
					else
						myApp = true;
				}
			}
			if	(!tb && mago)
			{
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyContents", MessagesCode.NotAuthorizedContents, String.Format("The module {0}-{1}-{2} contains namespace of ERP but not of TB", codAz, productSignature, moduleName));
				ErrorMessage = MessagesCode.NotAuthorizedContents;
				return false;
			}
			if (mago && !myApp)
			{
				ErrorMessage = MessagesCode.IncompleteContent;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyContents", MessagesCode.IncompleteContent, String.Format("The module {0}-{1}-{2} contains namespace of PRODUCER allowed, but it can not contains only these.", codAz, productSignature, moduleName));
				return false;
			}

			needFullLicence = (tb && !mago);

			if (isRegisteredProduct && needFullLicence)
			{
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyContents", MessagesCode.NotAuthorizedContents, String.Format("Il modulo {0}-{1}-{2} contiene solo TB, ma non è standAlone, perchè è registrato sul db", codAz, productSignature, moduleName));
				ErrorMessage = MessagesCode.NotAuthorizedContents;
				return false;
			}
			
			
			return true;
		}
		//---------------------------------------------------------------------
		private static bool VerifyTB(XmlElement node, out ArrayList toVerify)// container, string application, string module)
		{
			toVerify = new ArrayList();
			
			XmlDocument doc = new XmlDocument();
			string resourceName = "Microarea.Library.SalesModulesWriter.MicroareaNamespaces.xml";
			try
			{
				Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
				doc.Load(s);
			}
			catch (Exception exc)
			{
				ErrorMessage = exc.Message;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.LoadMicroareaNamespaces", "Impossibile caricare la lista di moduli permessi", exc.Message);
				return false;
			}
			string container = node.GetAttribute("container");
			string application = node.GetAttribute("name");
			bool ok = true;
			XmlNode n  = doc.SelectSingleNode("//Application[@container='"+ container +"' and @name='"+application+"']");
			if (n == null) return false;
			foreach (XmlElement el in node.ChildNodes)
			{
				string module = el.GetAttribute("name");
				XmlNode nn  = n.SelectSingleNode("node()[@name='"+ module +"']");
				if (nn != null) continue;
				else 
				{
					ok = false;
					toVerify.Add(module);
				}
			}
			return ok;

		}
		//---------------------------------------------------------------------
		private static ArrayList FillMagoList(XmlNode node)
		{
			
			XmlNodeList list = node.SelectNodes("node()/@name");
			ArrayList nameList = new ArrayList();
			foreach (XmlNode n in list)
			{
				nameList.Add(n.Value);
			}
			return nameList;
		}

//		/// <summary>
//		/// Carica i contenuti accettabili dal file xml che conmtiene i namespace accettabili per tb
//		/// </summary>
//		/// <param name="codAz">codica azienda del pai</param>
//		/// <param name="productSignature">nome del prodotto</param>
//		//---------------------------------------------------------------------
//		private static bool LoadMicroareaNamespaces(string codAz, string productSignature)
//		{
//			MicroareaNamespaces = new ArrayList();
//			XmlDocument doc = new XmlDocument();
//			string resourceName = "Microarea.Library.SalesModulesWriter.MicroareaNamespaces.xml";
//			try
//			{
//				Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
//				doc.Load(s);
//			}
//			catch (Exception exc)
//			{
//				ErrorMessage = exc.Message;
//				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.LoadMicroareaNamespaces", "Impossibile caricare la lista di moduli permessi", exc.Message);
//				return false;
//			}
//			XmlNodeList appList = doc.SelectNodes("//Application");
//			string application, name, module;
//			bool ismago;
//
//			if (appList == null)
//				return true; // caso di sampledata
//
//			foreach (XmlElement app in appList)
//			{
//				ArrayList microareaModules = new ArrayList();
//				application = app.GetAttribute("container");
//				name = app.GetAttribute("name");
//				ismago = (String.Compare(app.GetAttribute("ismago"), bool.TrueString, true) == 0);
//				XmlNodeList mlist = app.SelectNodes("Module");
//				XmlNodeList flist = app.SelectNodes("Functionality");
//				foreach (XmlElement mod in mlist)
//				{
//					module = mod.GetAttribute("name");
//					if (module != String.Empty)
//						microareaModules.Add(module);
//				}
//				foreach (XmlElement mod in flist)
//				{
//					module = mod.GetAttribute("name");
//					if (module != String.Empty)
//						microareaModules.Add(module);
//				}
//				MicroareaNamespace mn = new MicroareaNamespace(name, microareaModules, ismago? MicroareaNamespace.ModuleTypology.Mago : MicroareaNamespace.ModuleTypology.Tb);
//				MicroareaNamespaces.Add(mn);
//			}
//			ArrayList list = SMHandlerDbInterface.SMHandlerDbInterface.GetContainedPermitted(productSignature, codAz);
//			foreach (string s in list)
//			{
//				string[] nsPart = s.Split('.');
//				ArrayList modules = new ArrayList();
//				modules.Add(nsPart[1]);
//				MicroareaNamespace mn = new MicroareaNamespace(nsPart[0], modules, MicroareaNamespace.ModuleTypology.Mago);
//				MicroareaNamespaces.Add(mn);
//			}
//			return true;
//		}

		/// <summary>
		/// verifica ed eventualmente sostituisce il produttore con il codice rivenditore del pai, mette il produittore originale in un altro tag
		/// </summary>
		/// <param name="doc">xmlDocument sul quale è caricato il salesmodule</param>
		/// <param name="producerCode">codice azienda del pai</param>
		/// <param name="modified">indica se il documento è stato modificato</param>
		//---------------------------------------------------------------------
		private static bool VerifyProducer(XmlDocument doc, string producerCode, out bool modified)
		{
			modified = false;
			XmlElement smNode = doc.SelectSingleNode("SalesModule") as XmlElement;
			if (smNode == null)
			{
				ErrorMessage = MessagesCode.WrongSyntax;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyProducer", MessagesCode.WrongSyntax, "Syntax error, the tag 'SalesModule' is missing");
				return false;
			}
			string attributeName = "producer";
			string producer = smNode.GetAttribute(attributeName);
			if (producer == null || producer.Length <= 0)
			{
				attributeName = "internalcode";
				producer = smNode.GetAttribute(attributeName);
			}
//			if (IsMicroareaCodAz(producerCode))
//				producerCode = GetMicroareaName();
			if (producerCode != producer)
			{
				smNode.SetAttribute(attributeName, producerCode);
				smNode.SetAttribute("privatecode", producer);
				modified = true;
			}
			return true;
		}
		
		/// <summary>
		/// Verifica completa del sdalesModule
		/// </summary>
		/// <param name="path">path del file salvato in locale</param>
		/// <param name="originalFilename">nome originale del file</param>
		/// <param name="codAz">codice azienda del pai</param>
		/// <param name="productSignature">nome prodotto</param>
		/// <param name="needFullLicence">specifica se necessita di licenza full</param>
		/// <param name="isRegisteredProduct">specifica se il prodotto è censito sul db</param>
		//---------------------------------------------------------------------
		private static bool SMVerified(string path,string originalFilename, string codAz, string productSignature, out bool needFullLicence, bool isRegisteredProduct)
		{
			ErrorMessage = null;
			needFullLicence = true;
			//il file esiste
			if (!File.Exists(path))
			{
				ErrorMessage = MessagesCode.GenericServerError;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.SMVerified", MessagesCode.GenericServerError, String.Format("The path of the file to verify not exist: {0}.", path));
				return false;
			}
			XmlDocument doc = null;
			//l'xml è corretto
			string shortName;
			bool ok = VerifyXml(ref doc, path, codAz, out shortName);
			if (!ok)
				return false;
			
			bool isFullLicence		= false;
			bool isembedded			= false;
			bool modified;
			//il produttore è verificato
			ok = VerifyProducer(doc, codAz, out modified);
			if (!ok)
				return false;

			//solo se il produttore non è microarea allora verifico
			//1. licenza del produttore
			//2. namespaces contenuti
			bool isServerEmbedded = false;
			if (!IsMicroarea(codAz))
			{
				ok = VerifyProductOnLicence(codAz, productSignature, isRegisteredProduct, out isFullLicence, out isembedded);
				if (!ok)
					return false;
				ok = VerifyContents(doc, codAz, productSignature, Path.GetFileNameWithoutExtension(path), out needFullLicence, isFullLicence, isembedded, isRegisteredProduct, shortName, out isServerEmbedded);
				if (!ok)
					return false;
			}
			//imprimo il nome del file
			ok = WriteFileName(doc, originalFilename);
			if (!ok)
				return false;	
			if (IsMicroarea(codAz) || (isembedded && isServerEmbedded))//per adesso solo microarea o se è il server di un embedded
			{
				ok = WriteProductID(doc, codAz, productSignature, originalFilename);
				if (!ok)
					return false;	 
			}
			//salvo tutte le modifiche
			ok = SaveFile(doc, path);
			return ok;
		}

		/// <summary>
		/// Ricava le licenze del produttore e verifica che siano compatibili col prodotto
		/// </summary>
		/// <param name="codAz">codice azienda pai</param>
		/// <param name="productSignature">nome del prodotto</param>
		/// <param name="isRegisteredProduct">specifica se il prodotto è censito sul pai</param>
		/// <param name="isFullLicence">specifica seil produttore ha licenza full</param>
		/// <param name="isembedded">specifica se il produttore ha licenza per embedding</param>
		//---------------------------------------------------------------------
		private static bool VerifyProductOnLicence(string codAz, string productSignature, bool isRegisteredProduct, out bool isFullLicence, out bool isembedded)
		{
			bool error			= false;
			isFullLicence		= false;
			isembedded			= false;

			isFullLicence		= SMHandlerDbInterface.SMHandlerDbInterface.IsFull(codAz, out error);
			if (error)
			{
				ErrorMessage		= MessagesCode.ErrorContactingDb;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyProductOnLicence", MessagesCode.ErrorContactingDb, String.Format("Error during call to method {0}.", "IsFull"));
				return false;
			}
			isembedded			= SMHandlerDbInterface.SMHandlerDbInterface.IsEmbedded(codAz, out error);
			if (error)
			{
				ErrorMessage		= MessagesCode.ErrorContactingDb;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyProductOnLicence", MessagesCode.ErrorContactingDb, String.Format("Error during call to method {0}.", "IsEmbedded"));
				return false;
			}
			//per fare verticali o embedding necessita di licenza embedding, o full per fare standAlone
			if (!isembedded && !isFullLicence)
			{
				ErrorMessage = MessagesCode.NotAuthorizedCdS;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyProductOnLicence", MessagesCode.NotAuthorizedCdS, String.Format("The producer {0} is not registered, he does not have embedding or full licence.", codAz));
				return false;
			}

			//il nome del prodotto non è censito sul DB, quindi vuol dire che è uno StanAlone e deve esserlo
			if (!isRegisteredProduct && !isFullLicence)
			{
				ErrorMessage = MessagesCode.ProductNotRegisteredAndNoLicence;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.VerifyContents", MessagesCode.ProductNotRegisteredAndNoLicence, String.Format("The product {0} is not registered and the producer {1} does not have full licence.", productSignature, codAz));
				return false;
			}
			return true;

		}
		
		/// <summary>
		/// Salva il salesModule
		/// </summary>
		/// <param name="doc">xmldocument da salvare</param>
		/// <param name="path">percorso dove salvare il file</param>
		//---------------------------------------------------------------------
		private static bool SaveFile(XmlDocument doc, string path)
		{
			try
			{
				doc.Save(path);
			}
			catch (Exception exc)
			{
				ErrorMessage = MessagesCode.GenericServerError;
				Debug.Fail(exc.Message);
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.SaveFile", MessagesCode.GenericServerError, String.Format("Impossible save {0}. Exception message: {1}", path, exc.Message));
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		private static bool WriteProductID(XmlDocument doc, string codAz, string productSignature, string originalFilename)
		{
			XmlElement smNode = doc.SelectSingleNode("SalesModule") as XmlElement;
			if (smNode == null)
			{
				ErrorMessage =  MessagesCode.GenericServerError;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.WriteProductID",  MessagesCode.GenericServerError, String.Format("Syntax error in {0} , the tag 'SalesModule' is missing", originalFilename));
				return false;
			}
			try
			{
				//se trovo già l'attributo lo lascio com'è.(??)
				
				string existingProdID = smNode.GetAttribute("prodid") ;
				if (existingProdID != null && existingProdID.Length > 0 && IsPowerProducerCodAz(codAz))
					return true;
					

				string dbProdID = SMHandlerDbInterface.SMHandlerDbInterface.GetProdID(codAz, productSignature) ;
				smNode.SetAttribute("prodid", dbProdID);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				ErrorMessage = MessagesCode.GenericServerError;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.WriteFileName", MessagesCode.GenericServerError, String.Format("It is impossible to set file tag in {0}. Exception message: {1}", originalFilename, exc.Message));
				return false;
			}
			return true;
		}
		
		/// <summary>
		/// aggiunge un tag col nome del file, per evitare che venga modificato dopo la criptazione
		/// </summary>
		/// <param name="doc">xmldocument da modificare</param>
		/// <param name="originalFilename">nome originale del file</param>
		//---------------------------------------------------------------------
		private static bool WriteFileName(XmlDocument doc, string originalFilename)
		{
			
			XmlElement smNode = doc.SelectSingleNode("SalesModule") as XmlElement;
			if (smNode == null)
			{
				ErrorMessage =  MessagesCode.GenericServerError;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.WriteFileName",  MessagesCode.GenericServerError, String.Format("Syntax error in {0} , the tag 'SalesModule' is missing", originalFilename));
				return false;
			}
			try
			{
				//se trovo già dei nodi 'File'  li elimino
				XmlNodeList list = smNode.SelectNodes("File") ;
				if (list != null || list.Count > 0)
				{
					foreach (XmlNode n in list)
						smNode.RemoveChild(n);
				}
				
				string filename = Path.GetFileNameWithoutExtension(originalFilename);
				XmlElement filenode = doc.CreateElement("File");
				filenode.SetAttribute("name", filename);
				if (smNode.ChildNodes != null && smNode.ChildNodes.Count > 0)
					smNode.InsertBefore(filenode, smNode.ChildNodes[0]);
				else
					smNode.AppendChild(filenode);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				ErrorMessage = MessagesCode.GenericServerError;
				writerLogger.WriteAndSendError("SalesModulesWriter.SMManager.WriteFileName", MessagesCode.GenericServerError, String.Format("It is impossible to set file tag in {0}. Exception message: {1}", originalFilename, exc.Message));
				return false;
			}
			return true;
		}
		
	}

	/// <summary>
	/// Elenca i namespace di microarea che possono essere inclusi nei SalesModule degli altri produttori
	/// </summary>
	//=========================================================================
	public class MicroareaNamespace
	{
		public	enum			ModuleTypology		{Tb, Mago, None }
		public	string			Application;
		public	ArrayList		MicroareaModules;
		private ModuleTypology	ModuleType			= ModuleTypology.None;

		public	bool			IsMagoModule	{get {return ModuleType == ModuleTypology.Mago;}}
		//---------------------------------------------------------------------
		public MicroareaNamespace(string application, ArrayList microareaModules, ModuleTypology moduleType)
		{
			Application			= application;
			MicroareaModules	= microareaModules;
			ModuleType			= moduleType;
		}
	}
}