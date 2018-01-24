using Microarea.Common.NameSolver;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Microarea.Common.XmlPersister
{
	/// <summary>
	/// La classe State è una classe abstract che rappresenta la base class
	/// per tutte rappresentazioni di stato di oggetti applicativi, che lo
	/// dovranno includere come private member.
	/// <newpara>
	/// Gli oggetti derivati dalla classe ottengono intrinsecamente la capacità
	/// di serializzarsi e deserializzarsi su/da file XML o stringa XML. Tale 
	/// caratteristica permette di persistere lo stato degli oggetti su file in
	/// formato XML senza che il programmatore debba preoccuparsi di descrivere
	/// ogni volta la struttura XML.
	/// </newpara>
	/// <para>
	/// Un'istanza di una classe derivata da State può anche essere utilizzata
	/// per rappresentare una fotografia istantanea di uno stato, magari per
	/// comunicarlo ad altri oggetti deputati alla visualizzazione dello stato.
	/// </para>
	/// </summary>
	/// <remarks>
	/// Tutti i suoi membri o proprietà che si desidera potere persistere
	/// serializzandoli su file XML o stringa XMl devono essere public r/w,
	/// perché la serializzare in formato XML è ottenuta utilizzando XmlSerializer.
	/// <para>
	/// Si raccomanda di includere l'oggetto come private member, così
	/// da potere tutelare dati sensibili dello stato).
	/// </para>
	/// <para>
	/// La classe permette di serializzare e deserializzarsi, oltre che da file,
	/// anche da stringa XML, in modo che copie by value possano essere passate
	/// anche via socket come stringhe di testo.
	/// </para>
	/// </remarks>
	[Serializable]
	public abstract class State
	{
		// TODO - ora che la classe è divenuta parte di una libreria
		//		è meglio farle fare il throw delle eccezioni.

		/// <summary>
		/// attributo versione dell'oggetto descrittivo di uno stato
		/// </summary>
		[UrlAttribute]
		public string ver;
		// segue lista public di data members
		
		/// <summary>
		/// Restituisce una copia dello stato, eventualmente per comunicarlo
		/// ad altri oggetti deputati alla visualizzazione dello stato evitando
		/// di passare l'oggetto stesso.
		/// La ragione per cui tale metodo è implementato è che se per comunicare
		/// uno stato si passasse il reference all'oggetto stesso il ricevente
		/// avrebbe una copia by reference di cui potrebbe modificare i data member
		/// che per essere persistibili sono accessibili come public. utilizzando
		/// invece una MemberwiseClone dell'oggetto si ottiene di passare una copia
		/// come se l'istanza fosse una struct (e quindi by value).
		/// Nota: attenzione che i membri reference type non sono duplicati e
		/// restano pertanto accessibili. Si sconsiglia pertanto di utilizzare
		/// membri reference type in tali casi, o perlomeno di proteggerli
		/// dichiarandoli internal anziché public.
		/// </summary>
		/// <returns>Una copia dell'oggetto</returns>
		//---------------------------------------------------------------------
		public State GetShallowCopy()
		{
			return (State)this.MemberwiseClone();
		}

		/// <summary>
		/// Salva su file una descrizione XML dell'oggetto. Il programmatore
		/// non deve preoccuparsi di descrivere la struttura XML.
		/// Solo le proprietà pubbliche sono rese persistenti su file.
		/// </summary>
		/// <param name="filePath">file in cui scrivere lo stato</param>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		//---------------------------------------------------------------------
		public bool SaveToXml(string filePath)
		{
			if (filePath == null || filePath == string.Empty)
				return false;
			TextWriter writer = null;
			try
			{
				string fileName = Path.GetFileName(filePath);
				string dirPath = filePath.Substring(0, filePath.Length - fileName.Length);
				if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(dirPath))
                    PathFinder.PathFinderInstance.FileSystemManager.CreateFolder(dirPath, false);
				
				XmlSerializer serializer = new XmlSerializer(this.GetType());
				writer = new StreamWriter(File.OpenWrite(filePath));
				serializer.Serialize(writer, this);
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("State.SaveToXml: " + ex.Message);
				return false;
			}
			finally
			{
				if (writer != null)
					writer.Dispose();
			}
		}

		/// <summary>
		/// Restituisce un'istanza della classe reperendone la descrizione da file XML.
		/// </summary>
		/// <param name="fileName">file da cui scrivere lo stato</param>
		/// <param name="systemType">il System.Type dell'oggetto da leggere</param>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		//---------------------------------------------------------------------
		public static State GetFromXml(string fileName, System.Type systemType)
		{
			string message;
			return GetFromXml(fileName, systemType, out message);
		}

		/// <summary>
		/// Restituisce un'istanza della classe reperendone la descrizione da file XML.
		/// </summary>
		/// <param name="fileName">file da cui scrivere lo stato</param>
		/// <param name="systemType">il System.Type dell'oggetto da leggere</param>
		/// <param name="message">messaggio di errore</param>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		//---------------------------------------------------------------------
		public static State GetFromXml(string fileName, System.Type systemType, out string message)
		{
			message = String.Empty;
			FileStream fs = null;
			try
			{
				// A FileStream is needed to read the XML document.
				if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(fileName))
				{
					message = string.Concat(message, ("GetFromXml-File does not exist: " + fileName), Environment.NewLine);
					return null;
				}

				XmlSerializer serializer = new XmlSerializer(systemType);
				
				// If the XML document has been altered with unknown 
				// nodes or attributes, handle them with the 
				// UnknownNode and UnknownAttribute events.

                //TODO RSWEB non esiste
				//serializer..UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				//serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				
				fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				if (fs == null || fs.Length == 0)
				{
					message = string.Concat(message, ("GetFromXml-File stream empty: "+ fileName), Environment.NewLine);
				}
				State state = (State) serializer.Deserialize(fs);
				if (state == null)
				{
					message = string.Concat(message, ("GetFromXml-state null: "+ fileName), Environment.NewLine);
				}
				return state;
			}
			catch (Exception exc)
			{
				message = string.Concat(message, ("GetFromXml-Exception: "+ exc.ToString()), Environment.NewLine);
				Debug.WriteLine("State.GetFromXml: " + exc.Message);
				return null;
			}
			finally
			{
				if (fs != null)
					fs.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private static void serializer_UnknownNode(object sender/*, XmlNodeEventArgs e TODO RSWEb non esiste*/)
		{
			//Debug.WriteLine("Unknown Node:" + e.Name + "\t" + e.Text);
		}

		//---------------------------------------------------------------------
		private static void serializer_UnknownAttribute(object sender/*, XmlAttributeEventArgs e TODO RSWEB non esiste*/)
		{
			//if (e != null)
			//{
			//	System.Xml.XmlAttribute attr = e.Attr;
			//	Debug.WriteLine("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
			//}
		}

		/// <summary>
		/// Restituisce come variabile out una stringa XML descrivente l'oggetto,
		/// comprensiva della xml declaration.
		/// </summary>
		/// <param name="xmlStateString">stringa XML descrivente l'oggetto</param>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		//---------------------------------------------------------------------
		public bool GetXmlString(out string xmlStateString)
		{
			return GetXmlString(out xmlStateString, true);
		}

		/// <summary>
		/// Restituisce come variabile out una stringa XML descrivente l'oggetto.
		/// La signature del metodo è tale così da permetterne l'invocazione
		/// via socket, in tale caso occorre specificare di non includere
		/// la Xml declaration nell'intestazione della stringa XML.
		/// </summary>
		/// <param name="xmlStateString">stringa XML descrivente l'oggetto</param>
		/// <param name="includeXmlDeclaration">booleano che indica se includere la Xml declaration</param>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		//---------------------------------------------------------------------
		public bool GetXmlString(out string xmlStateString, bool includeXmlDeclaration)
		{
			// OPTIMIZE - Al momento sfrutto un DOM e ne leggo InnerXml
			//		ma si potrebbe comporre uno StringBuilder aggiungendovi
			//		i pezzi di xml letti nel MemoryStream da un XmlReader
			//		o da un TextReader
			xmlStateString = string.Empty;
			XmlDocument stateDoc = GetXmlDom();
			if (stateDoc != null)
			{
				if (!includeXmlDeclaration)
					RemoveXmlDeclaration(ref stateDoc);
				xmlStateString = stateDoc.InnerXml;
				return true;
			}
			return false;
		}

		//---------------------------------------------------------------------
		private XmlDocument GetXmlDom()
		{
			MemoryStream ms = null;
			try
			{   //Lara
				XmlSerializer serializer = new XmlSerializer(this.GetType());
				ms = new MemoryStream();
				serializer.Serialize(ms, this);
				ms.Seek(0,0);
				XmlDocument stateDoc = new XmlDocument();
				stateDoc.Load(ms);
				return stateDoc;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("State.GetXmlDom: " + ex.Message);
				return null;
			}
			finally
			{
				if (ms != null)
					ms.Dispose();
			}
		}


		//---------------------------------------------------------------------
		private void RemoveXmlDeclaration(ref XmlDocument dom)
		{
			if (dom == null)
				return;
			XmlNode firstNode = dom.FirstChild;
			if (firstNode == null)
				return;
			if (firstNode.NodeType != XmlNodeType.XmlDeclaration)
				return;
			dom.RemoveChild(firstNode);
		}

		/// <summary>
		/// Restituisce un'istanza della classe reperendone la descrizione da stringa XML.
		/// </summary>
		/// <param name="xmlString">stringa XML descrivente l'oggetto</param>
		/// <param name="systemType">il System.Type dell'oggetto da leggere</param>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		//---------------------------------------------------------------------
		public static State GetFromXmlString(string xmlString, System.Type systemType)
		{
            if (string.IsNullOrEmpty(xmlString)) return null;
            StringReader sr = null;
			try
			{
				XmlSerializer serializer = new XmlSerializer(systemType);
				
				// If the XML document has been altered with unknown 
				// nodes or attributes, handle them with the 
				// UnknownNode and UnknownAttribute events.

                //TODO rsweb non esiste
				//serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
				//serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
				
				sr = new StringReader(xmlString);
				State state = (State) serializer.Deserialize(sr);

				return state;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("State.GetFromXmlString: " + ex.Message);
				return null;
			}
			finally
			{
				if (sr != null)
					sr.Dispose();
			}
		}
	}
}
