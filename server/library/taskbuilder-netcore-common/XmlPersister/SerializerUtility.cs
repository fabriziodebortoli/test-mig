using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Microarea.Common.XmlPersister
{
	/// <summary>
	/// CLasse che si occupa di serializzare e deserializzare a strighe gli oggetti
	/// </summary>
	//=========================================================================
	public class SerializerUtility
	{
		/// <summary>
		/// Trasforma un qualunque oggetto in un'unica stringa XML.
		/// </summary>
		/// <param name="obj">l'oggetto da serializzare</param>
		/// <returns>la rappresentazione XML dell'oggetto passato</returns>
		//---------------------------------------------------------------------
		public static string SerializeToString(object obj)
		{
			string xml = string.Empty;

			XmlNode n = SerializeToXmlNode(obj);
			return (n == null) ? string.Empty : n.OuterXml;
			
		}

		/// <summary>
		/// Trasforma un qualunque oggetto in un nodo XML.
		/// </summary>
		/// <remarks>
		/// Il metodo usa XmlSerializer. Solo i data member pubblici
		/// dell'oggetto sono rappresentati nella stringa XML
		/// </remarks>
		/// <param name="obj">l'oggetto da serializzare</param>
		/// <returns>la rappresentazione XML dell'oggetto passato</returns>
		//---------------------------------------------------------------------
		public static XmlNode SerializeToXmlNode(object obj)
		{
			MemoryStream ms = null;
			try
			{   //Lara
				XmlSerializer serializer = new XmlSerializer(obj.GetType());
				ms = new MemoryStream();
				serializer.Serialize(ms, obj);
				ms.Seek(0,0);
				XmlDocument namesDoc = new XmlDocument();
				namesDoc.Load(ms);
				return namesDoc.DocumentElement;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				throw ex;
			}
			finally
			{
				if (ms != null)
					ms.Dispose();
			}
		}

		/// <summary>
		/// Deserializza un oggetto in formato xml
		/// </summary>
		/// <param name="xmlString">la stringa contenente l'xml che descrive l'oggetto</param>
		/// <param name="systemType">il tipo dell'oggetto</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static object DeserializeFromString(string xmlString, Type systemType)
		{
			StringReader sr = null;

			try
			{
				XmlSerializer serializer = new XmlSerializer(systemType);
				sr = new StringReader(xmlString);
				object obj = serializer.Deserialize(sr);
				return obj;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				return null;
			}
			finally
			{
				if (sr != null)
					sr.Dispose();
			}
		}

		/// <summary>
		/// Deserializza un oggetto in formato xml
		/// </summary>
		/// <param name="node">il nodo contenente l'xml che descrive l'oggetto</param>
		/// <param name="systemType">il tipo dell'oggetto</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static object DeserializeFromXmlNode(XmlNode node, Type systemType)
		{
			return DeserializeFromString(node.OuterXml, systemType);
		}
	}
}
