using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.NameSolver
{
    //=========================================================================
    public sealed class HelpInfo
	{
		public string Server	= string.Empty;
		public string Language	= string.Empty;
		public string Edition	= string.Empty;
	}

	/// <summary>
	/// Summary description for ApplicationConfigInfo.
	/// </summary>
	//=========================================================================
	public sealed class ApplicationConfigInfo : IApplicationConfigInfo
	{
		#region Private members
		private string		appConfigFile	= string.Empty;
		private string		appType			= string.Empty;
		private string		name			= string.Empty;
		private string		icon			= string.Empty;
		private string		welcomeBmp		= string.Empty;
		private string		dbSignature		= string.Empty;
		private string		version			= string.Empty;
		private string		uuid			= string.Empty;
		private bool		visible			= true;
		private string		helpModule		= string.Empty;
		
		private XmlDocument	appConfigDocument = null;
		#endregion

		#region Properties
		public string		Name		{ get { return name;		} }
		public string       Type        { get { return appType; } set { appType = value; } }
		public string		Icon		{ get { return icon;		} }
		public string		WelcomeBmp	{ get { return welcomeBmp;	} }
		public string		DbSignature	{ get { return dbSignature; } set { dbSignature = value; } }
		public string		Version		{ get { return version;		} set { version = value; } }
		public string		Uuid		{ get { return uuid;		} }
		public bool			Visible		{ get { return visible;		} }
		public string		HelpModule	{ get { return helpModule;	} }
		#endregion

		//------------------------------------------------------------------------------
		public ApplicationConfigInfo(string name, string configFile)
		{
			this.name = name;
			appConfigFile = configFile;
			dbSignature = NameSolverXmlStrings.Default;
		}

		//------------------------------------------------------------------------------
		public bool Parse()
		{
			using (Stream stream = File.OpenRead(appConfigFile))
				return FromStream(stream);
		}

		//------------------------------------------------------------------------------
		public bool FromStream(Stream stream)
		{
			try
			{
                //Lara
				appConfigDocument = new XmlDocument();
				appConfigDocument.Load(stream);

				XmlElement root = appConfigDocument.DocumentElement;
				if (root == null)
				{
					Debug.Fail("Sintassi del file " + appConfigFile);
					return false;
				}

				XmlElement elementNode = null;

				// tipo
				XmlNodeList elementNodeList = root.GetElementsByTagName(ApplicationConfigXML.Element.Type);
				if (elementNodeList != null && elementNodeList.Count > 0)
				{
					elementNode = (XmlElement)elementNodeList[0];
					appType = elementNode.InnerText;
				}

				// icona
				elementNodeList = root.GetElementsByTagName(ApplicationConfigXML.Element.Icon);
				if (elementNodeList != null && elementNodeList.Count > 0)
				{
					elementNode = (XmlElement)elementNodeList[0];
					icon = elementNode.InnerText;
				}

				// welcome bitmap
				elementNodeList = root.GetElementsByTagName(ApplicationConfigXML.Element.WelcomeBmp);
				if (elementNodeList != null && elementNodeList.Count > 0)
				{
					elementNode = (XmlElement)elementNodeList[0];
					welcomeBmp = elementNode.InnerText;
				}

				// Signature di database
				elementNodeList = root.GetElementsByTagName(ApplicationConfigXML.Element.DbSignature);
				if (elementNodeList != null && elementNodeList.Count > 0)
				{
					elementNode = (XmlElement)elementNodeList[0];
					dbSignature = elementNode.InnerText;
				}

				if (dbSignature == null || dbSignature.Length == 0)
					dbSignature = name;

				// Versione di applicazione
				elementNodeList = root.GetElementsByTagName(ApplicationConfigXML.Element.Version);
				if (elementNodeList != null && elementNodeList.Count > 0)
				{
					elementNode = (XmlElement)elementNodeList[0];
					version = elementNode.InnerText;
				}

				if (version == null || version.Length == 0)
					version = "1.0.0";

				// GUID
				elementNodeList = root.GetElementsByTagName(ApplicationConfigXML.Element.Uuid);
				if (elementNodeList != null && elementNodeList.Count > 0)
				{
					elementNode = (XmlElement)elementNodeList[0];
					uuid = elementNode.InnerText;
				}

				//Visible
				elementNodeList = root.GetElementsByTagName(ApplicationConfigXML.Element.Visible);
				if (elementNodeList != null && elementNodeList.Count > 0)
				{
					elementNode = (XmlElement)elementNodeList[0];
					string strVisible = elementNode.InnerText;
					if (strVisible == "0")
						visible = false;
				}

				elementNodeList = root.GetElementsByTagName(ApplicationConfigXML.Element.HelpModule);
				if (elementNodeList != null && elementNodeList.Count > 0)
				{
					elementNode = (XmlElement)elementNodeList[0];
					helpModule = elementNode.InnerText;
				}
			}
			catch (XmlException e)
			{
				Debug.Fail(e.Message);
				return false;
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
				return false;
			}

			return true;
		}

		//------------------------------------------------------------------------------
		public bool Save()
		{
			try
			{
				// Si assume che il nodo esista sempre.
				XmlNode node = appConfigDocument.DocumentElement.SelectSingleNode(ApplicationConfigXML.Element.Version);
				node.InnerText = version;

				// Si assume che il file esista sempre.
				FileInfo appConfigFileInfo = new FileInfo(appConfigFile);
				if ((appConfigFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					appConfigFileInfo.Attributes -= FileAttributes.ReadOnly;

				appConfigDocument.Save(File.OpenWrite(appConfigFile));
				// Nota:
				// Il file è in SourceSafe, ma non ripristino l'attributo ReadOnly per non perdere le
				// modifiche alla prossima sync in attesa di futura eventuale interazione con VSS.
				// Nel caso ci si dimentichi di aggiornare il file in SourceSafe, SourceSafe, trovando
				// in locale un file non protetto, avvisa l'utente.
			}
			catch (Exception exc)
			{
				Debug.Fail("Errore in ApplicationConfigInfo.Save() durante il salvataggio di " + appConfigFile + "\n" + exc.Message);
				return false;
			}

			return true;
		}
	}
}