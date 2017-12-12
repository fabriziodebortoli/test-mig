using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Envelope.
	/// </summary>
	//=========================================================================
	public class Envelope : IEnvelope, IEnvelopeSerializable
	{
		public const string EnvelopeTag		= "Envelope";
		public const string HeaderTag		= "Header";
		public const string BodyTag			= "Body";
		public const string IdCodeTag		= "IdCode";
		public const string IdValueTag		= "IdValue";
		public const string CreationDateTag	= "CreationDate";
		public const string VersionTag		= "Version";

		public const string ValueAttr		= "value";

		private int		version				= 1;
		private string	idValue;
		private string	idCode;
		private string	creationDate;
		private string	content				= string.Empty;

		//---------------------------------------------------------------------------
		public Envelope(string idValue, string idCode)
		{
			this.idValue = idValue;
			this.idCode = idCode;
			creationDate = DateTime.UtcNow.ToString("s",CultureInfo.InvariantCulture);
		}

		//---------------------------------------------------------------------------
		public Envelope(string idCode)
		{
			this.idValue = Guid.NewGuid().ToString();
			this.idCode = idCode;
			creationDate = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
		}
		
		#region IEnvelope Members

		//---------------------------------------------------------------------------
		public virtual string IdValue
		{
			get
			{
				return this.idValue;
			}
		}

		//---------------------------------------------------------------------------
		public virtual string IdCode
		{
			get
			{
				return this.idCode;
			}
			set
			{
				this.idCode = value;
			}
		}

		//---------------------------------------------------------------------------
		public virtual string CreationDate
		{
			get
			{
				return this.creationDate;
			}
			set
			{
				this.creationDate = value;
			}
		}

		//---------------------------------------------------------------------------
		public virtual string Version
		{
			get
			{
				return version.ToString(CultureInfo.InvariantCulture);
			}
		}

		//---------------------------------------------------------------------------
		public virtual string Content
		{
			get
			{
				return this.content;
			}
			set
			{
				this.content = value;
			}
		}

		#endregion

		#region IEnvelopeSerializable Members

		//---------------------------------------------------------------------------
		public virtual string SerializeToString()
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement rootElem = xmlDoc.CreateElement(EnvelopeTag);

			XmlElement headerElem = xmlDoc.CreateElement(HeaderTag);

			XmlElement idVElem = xmlDoc.CreateElement(IdValueTag);
			idVElem.SetAttribute(ValueAttr, string.Empty, this.idValue);
			
			XmlElement idCElem = xmlDoc.CreateElement(IdCodeTag);
			idCElem.SetAttribute(ValueAttr, string.Empty, this.idCode);

			XmlElement creationDateElem = xmlDoc.CreateElement(CreationDateTag);
			creationDateElem.SetAttribute(ValueAttr, string.Empty, this.creationDate);

			XmlElement VersionElem = xmlDoc.CreateElement(VersionTag);
			VersionElem.SetAttribute(ValueAttr, string.Empty, version.ToString(CultureInfo.InvariantCulture));

			headerElem.AppendChild(idCElem);
			headerElem.AppendChild(idVElem);
			headerElem.AppendChild(creationDateElem);
			headerElem.AppendChild(VersionElem);

			XmlElement bodyElem = xmlDoc.CreateElement(BodyTag);
			bodyElem.InnerText = this.Content;

			rootElem.AppendChild(headerElem);
			rootElem.AppendChild(bodyElem);

			xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			xmlDoc.AppendChild(rootElem);

			return xmlDoc.OuterXml;
		}

		//---------------------------------------------------------------------------
		public bool SerializeToFile(string path)
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(path)) 
				{
					sw.Write(SerializeToString());
				}
				return true;
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Envelope.SerializeToFile:" + exc.Message);
				return false;
			}
		}

		#endregion

		//---------------------------------------------------------------------------
		private static Envelope ParseXmlInput(XmlDocument xmlDoc)
		{
			XmlAttribute a = (XmlAttribute) xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}/{2}/@{3}", EnvelopeTag, HeaderTag, IdValueTag, ValueAttr));
			if (a == null)
				return null;

			string idValue = a.Value;

			a = (XmlAttribute) xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}/{2}/@{3}", EnvelopeTag, HeaderTag, IdCodeTag, ValueAttr));
			if (a == null)
				return null;

			string idCode = a.Value;

			Envelope env = new Envelope(idValue, idCode);

			a = (XmlAttribute) xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}/{2}/@{3}", EnvelopeTag, HeaderTag, CreationDateTag, ValueAttr));
			if (a == null)
				return null;
			env.creationDate = a.Value;

			a = (XmlAttribute) xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}/{2}/@{3}", EnvelopeTag, HeaderTag, VersionTag, ValueAttr));
			if (a == null)
				return null;
			env.version = Int32.Parse(a.Value, CultureInfo.InvariantCulture);

			XmlNode b = xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", EnvelopeTag, BodyTag));
			if (b == null)
				return null;
			env.content = b.InnerText;

			return env;
		}

		//---------------------------------------------------------------------------
		public static IEnvelope ReadFromStream(Stream inputStream)
		{
			if (inputStream == null || inputStream.Length == 0)
				return null;

			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(inputStream);
			}
			catch(Exception exc)
			{
				Debug.WriteLine("Envelope.ReadFromStream:" + exc.ToString());
				return null;
			}

			return ParseXmlInput(xmlDoc);
		}

		//---------------------------------------------------------------------------
		public static IEnvelope ReadFromFile(string fileName)
		{
			if (fileName == null || fileName == string.Empty || !File.Exists(fileName))
				return null;

			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(fileName);
			}
			catch(Exception exc)
			{
				Debug.WriteLine("Envelope.ReadFromFile:" + exc.ToString());
				return null;
			}

			return ParseXmlInput(xmlDoc);
		}
	}

	//=========================================================================
	public class EnvelopeWithAttachs : Envelope, IEnvelopeWithAttach
	{
		private int version	 = 2;

		private IList attachments;

		//---------------------------------------------------------------------------
		public IList Attachments
		{
			get
			{
				return attachments;
			}
		}

		//---------------------------------------------------------------------------
		public EnvelopeWithAttachs(string idValue, string idCode)
			: base (idValue, idCode)
		{
			attachments = new ArrayList();
		}

		//---------------------------------------------------------------------------
		public EnvelopeWithAttachs(string idCode)
			: base (idCode)
		{
			attachments = new ArrayList();
		}

		//---------------------------------------------------------------------------
		public virtual void AddAttach(IEnvelopeAttach attach)
		{
			attachments.Add(attach);
		}

		//---------------------------------------------------------------------------
		public virtual void RemoveAttach(IEnvelopeAttach attach)
		{
			attachments.Remove(attach);
		}

		//---------------------------------------------------------------------------
		public virtual void AddAttach(string id, string content)
		{
			if (id == null || id.Length == 0 || content == null || content.Length == 0)
				return;

			IEnvelopeAttach aAttach = new EnvelopeAttach(id);
			aAttach.Content = content;
			
			AddAttach(aAttach);
		}

		//---------------------------------------------------------------------------
		public override string SerializeToString()
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement rootElem = xmlDoc.CreateElement(EnvelopeTag);

			XmlElement headerElem = xmlDoc.CreateElement(HeaderTag);

			XmlElement idVElem = xmlDoc.CreateElement(IdValueTag);
			idVElem.SetAttribute(ValueAttr, string.Empty, this.IdValue);
			
			XmlElement idCElem = xmlDoc.CreateElement(IdCodeTag);
			idCElem.SetAttribute(ValueAttr, string.Empty, this.IdCode);

			XmlElement creationDateElem = xmlDoc.CreateElement(CreationDateTag);
			creationDateElem.SetAttribute(ValueAttr, string.Empty, this.CreationDate);

			XmlElement VersionElem = xmlDoc.CreateElement(VersionTag);
			VersionElem.SetAttribute(ValueAttr, string.Empty, version.ToString(CultureInfo.InvariantCulture));

			headerElem.AppendChild(idCElem);
			headerElem.AppendChild(idVElem);
			headerElem.AppendChild(creationDateElem);
			headerElem.AppendChild(VersionElem);

			XmlElement bodyElem = xmlDoc.CreateElement(BodyTag);
			bodyElem.InnerText = this.Content;

			rootElem.AppendChild(headerElem);
			rootElem.AppendChild(bodyElem);

			XmlNode aNode = null;
			XmlAttribute aAttribute = null;
			foreach (IEnvelopeAttach attach in attachments)
			{
				aNode = xmlDoc.CreateNode(XmlNodeType.Element, EnvelopeAttach.AttachTag, "");
				
				aAttribute = xmlDoc.CreateAttribute(EnvelopeAttach.IdAttr);
				aAttribute.InnerText = attach.Id;

				aNode.Attributes.Append(aAttribute);
				aNode.InnerText = attach.Content;
				rootElem.AppendChild(aNode);
				
			}

			xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			xmlDoc.AppendChild(rootElem);

			return xmlDoc.OuterXml;
		}

		//---------------------------------------------------------------------------
		public static new IEnvelopeWithAttach ReadFromFile(string fileName)
		{
			if (fileName == null || fileName == string.Empty || !File.Exists(fileName))
				return null;

			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(fileName);
			}
			catch(Exception exc)
			{
				Debug.WriteLine("Envelope.ReadFromFile:" + exc.ToString());
				return null;
			}

			return ParseXmlInput(xmlDoc);
		}

		//---------------------------------------------------------------------------
		public static new IEnvelopeWithAttach ReadFromStream(Stream inputStream)
		{
			if (inputStream == null || inputStream.Length == 0)
				return null;

			XmlDocument xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.Load(inputStream);
			}
			catch(Exception exc)
			{
				Debug.WriteLine("Envelope.ReadFromStream:" + exc.ToString());
				return null;
			}

			return ParseXmlInput(xmlDoc);
		}

		//---------------------------------------------------------------------------
		private static EnvelopeWithAttachs ParseXmlInput(XmlDocument xmlDoc)
		{
			XmlAttribute a = (XmlAttribute) xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}/{2}/@{3}", EnvelopeTag, HeaderTag, IdValueTag, ValueAttr));
			if (a == null)
				return null;

			string idValue = a.Value;

			a = (XmlAttribute) xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}/{2}/@{3}", EnvelopeTag, HeaderTag, IdCodeTag, ValueAttr));
			if (a == null)
				return null;

			string idCode = a.Value;

			EnvelopeWithAttachs env = new EnvelopeWithAttachs(idValue, idCode);

			a = (XmlAttribute) xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}/{2}/@{3}", EnvelopeTag, HeaderTag, CreationDateTag, ValueAttr));
			if (a == null)
				return null;
			env.CreationDate = a.Value;

			a = (XmlAttribute) xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}/{2}/@{3}", EnvelopeTag, HeaderTag, VersionTag, ValueAttr));
			if (a == null)
				return null;
			env.version = Int32.Parse(a.Value, CultureInfo.InvariantCulture);

			XmlNode b = xmlDoc.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", EnvelopeTag, BodyTag));
			if (b == null)
				return null;

			env.Content = b.InnerText;

			XmlNodeList attachNodes = xmlDoc.SelectNodes(string.Format(CultureInfo.InvariantCulture, "/{0}/{1}", EnvelopeTag, EnvelopeAttach.AttachTag));
			foreach (XmlElement attach in attachNodes)
			{
				env.AddAttach(
					attach.GetAttribute(EnvelopeAttach.IdAttr),
					attach.InnerText
					);
			}

			return env;
		}
	}

	//=========================================================================
	public class EnvelopeAttach : IEnvelopeAttach
	{
		public const string AttachTag	= "Attach";
		public const string IdAttr		= "id";

		private string id = string.Empty;
		private string content = string.Empty;

		#region IEnvelopeAttach Members

		//---------------------------------------------------------------------------
		public string Id
		{
			get
			{
				return id;
			}
		}

		//---------------------------------------------------------------------------
		public string Content
		{
			get
			{
				return content;
			}
			set
			{
				content = value;
			}
		}

		#endregion

		//---------------------------------------------------------------------------
		public EnvelopeAttach(string id)
		{
			this.id = id;
		}

		//---------------------------------------------------------------------------
		public override string ToString()
		{
			if (id == null || id.Length == 0 || content == null || content.Length == 0)
				return String.Format("<{0} />", AttachTag);

			StringBuilder aBuilder = new StringBuilder();

			aBuilder.Append(
				String.Format(
				"<{0} {1}='{2}'>{3}</{0}>",
				AttachTag,
				IdAttr,
				id,
				content
				)
				);

			return aBuilder.ToString();
		}
	}

	//=========================================================================
	public interface IEnvelope
	{
		string IdValue {get;}
		string IdCode {get;}
		string CreationDate {get;set;}
		string Version {get;}
		string Content {get;set;}
	}

	//=========================================================================
	public interface IEnvelopeWithAttach : IEnvelope
	{
		IList Attachments {get;}

		//---------------------------------------------------------------------------
		void AddAttach(IEnvelopeAttach attach);

		//---------------------------------------------------------------------------
		void RemoveAttach(IEnvelopeAttach attach);
	}

	//=========================================================================
	public interface IEnvelopeAttach
	{
		string Id {get;}
		string Content {get;set;}
	}

	//=========================================================================
	public interface IEnvelopeSerializable
	{
		string SerializeToString();
		bool SerializeToFile(string path);
	}
}
