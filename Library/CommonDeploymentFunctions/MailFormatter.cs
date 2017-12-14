using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Text;
using System.Net.Mail;
using System.Collections;

namespace Microarea.Library.CommonDeploymentFunctions
{
	//=========================================================================
	public enum Options {Result, Installation, Product, Date, Time, Details}
    //=========================================================================
    public enum MailFormat { Text, Html };
	/// <summary>
	/// 
	/// </summary>
	//=========================================================================
	[Serializable]
	public class MailBuilder
	{
		public  static string FileMailFormat	= "MailFormat.xml";

		private string to;
		private string from;
		private string subject;
		private MailFormat format;
		private MailPriority priority;
		private ArrayList optionsList;
		private bool defaultFrom;
		private bool defaultSubject;

		private string originalFrom;
		private string originalSubject;

		public string		To {get {return to;} set {to = value;}}
		public string		From		
		{
			get 
			{
				if (DefaultFrom || from == null || from.Length == 0)
					return OriginalFrom;
				else return from;
			}			
			set {from = value;}}
				
		public string		Subject		
		{
			get 
			{
				if (DefaultSubject || subject == null || subject.Length == 0)
					return OriginalSubject;
				else return subject;
			}			
			set {subject = value;}
		}

		public MailFormat	Format			{get {return format;}			set{format = value;}}
		public MailPriority Priority		{get {return priority;}			set{priority = value;}}
		public ArrayList	OptionsList		{get {return optionsList;}		set{optionsList = value;}}
		public string		OriginalFrom	{get {return originalFrom;}	}
		public string		OriginalSubject	{get {return originalSubject;}}
		public bool			DefaultFrom		{get {return defaultFrom;}		set{defaultFrom = value;}}
		public bool			DefaultSubject	{get {return defaultSubject;}	set{defaultSubject = value;}}

		//---------------------------------------------------------------------
		public MailBuilder(string originalFrom, string originalSubject)
		{
			this.originalFrom	 = originalFrom;
			this.originalSubject = originalSubject;
		}

		//---------------------------------------------------------------------
		public MailBuilder(XmlDocument doc, string originalFrom, string originalSubjetc)
		{
			string to		= String.Empty;
			string from		= String.Empty;
			string subject	= String.Empty;
			string folder	= String.Empty;
			string priority = String.Empty;
			string format	= String.Empty;
			bool   defaultFrom		= true;
			bool   defaultSubject	= true;
			this.originalFrom	 = originalFrom;
			this.originalSubject = originalSubjetc;

			XmlNode root = doc.SelectSingleNode("//" + XmlStrings.Element.Mail);
			
			XmlElement nTo = root.SelectSingleNode(XmlStrings.Element.To) as XmlElement;
			if (nTo != null)
				to = nTo.GetAttribute(XmlStrings.Attribute.Value);

			XmlElement nFrom = root.SelectSingleNode(XmlStrings.Element.From) as XmlElement;
			if (nFrom != null)
			{
				from = nFrom.GetAttribute(XmlStrings.Attribute.Value);
				string defaultFromS = nFrom.GetAttribute(XmlStrings.Attribute.Default);
				//se non c'è considero true
				defaultFrom = (String.Compare(bool.TrueString, defaultFromS, true, CultureInfo.InvariantCulture) == 0);
			}

			XmlElement nSubject = root.SelectSingleNode(XmlStrings.Element.Subject) as XmlElement;
			if (nSubject != null)
			{
				subject = nSubject.GetAttribute(XmlStrings.Attribute.Value);
				string defaultSubjectS = nFrom.GetAttribute(XmlStrings.Attribute.Default);
				//se non c'è considero true
				defaultSubject = (String.Compare(bool.TrueString, defaultSubjectS, true, CultureInfo.InvariantCulture) == 0);
			}

			XmlElement nFolder = root.SelectSingleNode(XmlStrings.Element.Folder) as XmlElement;
			if (nFolder != null)
				folder = nFolder.GetAttribute(XmlStrings.Attribute.Value);

			XmlElement nPriority = root.SelectSingleNode(XmlStrings.Element.Priority) as XmlElement;
			if (nPriority != null)
				priority = nPriority.GetAttribute(XmlStrings.Attribute.Value);

			XmlElement nFormat = root.SelectSingleNode(XmlStrings.Element.Format) as XmlElement;
			if (nFormat != null)
				format = nFormat.GetAttribute(XmlStrings.Attribute.Value);

			XmlNodeList nOptions = root.SelectNodes(XmlStrings.Element.OptionsList + "/node()/@" + XmlStrings.Attribute.Value);
			ArrayList list = null;
			if (nOptions != null)
			{
				list = new ArrayList();
				foreach (XmlAttribute opt in nOptions)
				{
					if (opt != null && opt.Value != null && opt.Value != string.Empty)
					{
						Options o = (Options)EnumParser.Parse(typeof(Options), opt.Value, true);
						if (!list.Contains(o))
							list.Add(o);
					}
				}
			}

			this.to			 = to;
			this.from		 = from;
			this.subject	 = subject;
			this.format		 = (MailFormat)EnumParser.Parse(typeof(MailFormat), format, true);
			this.priority	 = (MailPriority)EnumParser.Parse(typeof(MailPriority), priority, true);
			this.optionsList = list;
			this.defaultFrom = defaultFrom ;
			this.defaultSubject = defaultSubject;
		}

		//---------------------------------------------------------------------
		public static MailBuilder GetDefaultMailBuilder(string to)
		{
			if (to == null || to.Length == 0)
				return null;
			MailBuilder mb = new MailBuilder("", "");
			mb.To = to;
			ArrayList optionslist = new ArrayList();
			Array list = Enum.GetValues(typeof(Options));
			for (int i = 0; i < list.Length; i++) 
				optionslist.Add(((Options)list.GetValue(i)));
			mb.OptionsList = optionslist;
			return mb;
		}

		//---------------------------------------------------------------------
		public string GetXmlString()
		{
			XmlDocument doc = GetXml();
			if (doc == null)
				return String.Empty;
			return doc.OuterXml;
		}

		//TODO verifica non null
		//---------------------------------------------------------------------
		public XmlDocument GetXml()
		{
			XmlDocument doc = new XmlDocument();
			doc.CreateXmlDeclaration(Strings.Consts.XmlDeclarationVersion, Strings.Consts.XmlDeclarationEncoding, null);
			XmlNode root = doc.CreateElement(XmlStrings.Element.Mail);

			XmlElement nTo = doc.CreateElement(XmlStrings.Element.To);
			nTo.SetAttribute(XmlStrings.Attribute.Value, to);
			root.AppendChild(nTo);

			XmlElement nFrom = doc.CreateElement(XmlStrings.Element.From);
			if (defaultFrom || from == null || from.Length == 0)
				nFrom.SetAttribute(XmlStrings.Attribute.Default, bool.TrueString);
			else
			{
				nFrom.SetAttribute(XmlStrings.Attribute.Value, from);
				nFrom.SetAttribute(XmlStrings.Attribute.Default, bool.FalseString);
			}
			root.AppendChild(nFrom);

			XmlElement nSubject = doc.CreateElement(XmlStrings.Element.Subject);
			if (defaultSubject || subject == null || subject.Length == 0)
				nSubject.SetAttribute(XmlStrings.Attribute.Default, bool.TrueString);
			else
			{
				nSubject.SetAttribute(XmlStrings.Attribute.Value, subject);
				nSubject.SetAttribute(XmlStrings.Attribute.Default, bool.FalseString);
			}
			root.AppendChild(nSubject);

			XmlElement nPriority = doc.CreateElement(XmlStrings.Element.Priority);
			nPriority.SetAttribute(XmlStrings.Attribute.Value, priority.ToString());
			root.AppendChild(nPriority);

			XmlElement nFormat = doc.CreateElement(XmlStrings.Element.Format);
			nFormat.SetAttribute(XmlStrings.Attribute.Value, format.ToString());
			root.AppendChild(nFormat);

			if (optionsList != null)
			{
				XmlElement nOptions = doc.CreateElement(XmlStrings.Element.OptionsList);
				foreach (Options opt in optionsList)
				{
					XmlElement nOpt = doc.CreateElement(XmlStrings.Element.Option);
					nOpt.SetAttribute(XmlStrings.Attribute.Value, opt.ToString());
					nOptions.AppendChild(nOpt);
				}
				root.AppendChild(nOptions);
			}
			doc.AppendChild(root);
			return doc;
		}

//		//---------------------------------------------------------------------
//		public static Folder GetMailFolder(XmlDocument doc)
//		{
//			XmlNode n = doc.SelectSingleNode(
//												"//" + 
//												XmlStrings.Element.Mail + 
//												"/" + 
//												XmlStrings.Element.Folder + 
//												"/@" + 
//												XmlStrings.Attribute.Value
//											);
//			if (n != null && n.Value != null && n.Value != String.Empty)
//				try
//				{
//					return (Folder)EnumParser.Parse(typeof(Folder), n.Value, true);
//				}
//				catch (ArgumentException)
//				{
//					Debug.Fail("MailFormatter.GetMailFolder:Errore nel parsing dell'enumerativo Folder, stringa non riconosciuta: " + n.Value);
//				}
//			return Folder.None;
//		}

		//---------------------------------------------------------------------
		public static string GetOptionMessage(Options opt, UpdaterEventArgs e, string branded)
		{
			switch (opt)
			{
				case Options.Date:
					if (e.Time.Date.ToShortDateString().Length != 0)
						return String.Format(Strings.Strings.Date, e.Time.Date.ToString("D"));//Data in formato esteso
					break;
//				case Options.Details:
//					if (e.Details != null && e.Details.ToString().Length != 0)
//						return String.Format(Strings.Strings.Details, e.Details);
//					break;
				case Options.Time:
					return String.Format(Strings.Strings.Hour, e.Time.ToLongTimeString());
				case Options.Installation:
					if (e.Installation.Length != 0)
						return String.Format(Strings.Strings.Installation, e.Installation);  
					break;
				case Options.Product:
					if (branded != null && branded.Length != 0)
						return String.Format(Strings.Strings.Product, branded);
					else if (e.Product != null && e.Product.Length != 0)
						return String.Format(Strings.Strings.Product, e.Product);
					break;
				case Options.Result:
					if (e.Message != null && e.Message.Length != 0)
						return String.Format(Strings.Strings.Result, e.Message);
					break;
				default:
					return String.Empty;
			}
			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static string WriteHtmlMail(MailBuilder mb, UpdaterEventArgs e, string branded)
		{
			StringBuilder MailMessage = new StringBuilder();
			string header = @"<html>
						<head>
						<title>" + mb.OriginalSubject + @"</title>
						<meta http-equiv=""Content-Type"" content=""text/html; charset=iso-8859-1"">
						<style>
						h3 {font-family: Verdana,Arial,Helvetia,sans-serif; color: #000080; font-size: 14px;}
						p  {font-family: Verdana,Arial,Helvetia,sans-serif; font-size: 10px; color: #000080; text-align: justify}
						td {font-family: Verdana,Arial,Helvetia,sans-serif; font-size: 10px; color: #000080;}
						li {font-family: Verdana,Arial,Helvetia,sans-serif; font-size: 10px; color: #000080; text-align: justify}
						body {background: #FFFFFF; font-family: Verdana,Arial,Helvetia,sans-serif; font-size: 10px; color: #000080; text-align: justify}
						.ReportHeader {font-family: Verdana,Arial,Helvetia,sans-serif; font-weight:bold; font-size: 10px; color: #FFFFFF; background: #3366FF; border-top:1px solid #003399; border-left:1px solid #003399; border-bottom:1px solid #003399; border-right:1px solid #003399;}   
						.borderBottom {background: #F0F0F0; border-top:0px solid #003399;border-left:0px solid #003399; border-bottom:1px solid #003399; border-right:0px solid #003399;}										
						.border1 {background: #F0F0F0; border-top:1px solid #003399; border-left:1px solid #003399; border-bottom:1px solid #003399; border-right:1px solid #003399;}
						</style>
						</head>
						<body>";
			MailMessage.Append(header);
			MailMessage.Append("<table class='border1' border='0' width='600' cellspacing='0' cellpadding='5'>");
			int counter = 0;
			foreach (Options opt in mb.OptionsList)
			{	
				string message = MailBuilder.GetOptionMessage(opt, e, branded);
				if (message.Length == 0) continue;
				counter++;
				MailMessage.Append("<tr><td class='BorderBottom'><b>" + counter.ToString() + "</b></td>");
				MailMessage.Append("<td class='BorderBottom'>" + message + "</td></tr>");
			}
			MailMessage.Append("</table><br /><br />");
			string footer = String.Empty;
			MailMessage.Append("<table border='0' width='600' cellspacing='0' cellpadding='0'>");
			MailMessage.Append("<tr><td colspan='2'>" + footer + "</td></tr>");
			MailMessage.Append("</table>");
			MailMessage.Append("</body></html>");

			return MailMessage.ToString();
		}

		//---------------------------------------------------------------------
		public static string WriteTextMail(MailBuilder mb, UpdaterEventArgs e, string branded)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Options opt in mb.OptionsList)
			{
				sb.Append(MailBuilder.GetOptionMessage(opt, e, branded));
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}
	}

	//=========================================================================
	public class EnumParser
	{
		/// <summary>
		/// Parsa una stringa e la trasforma in enumerativo, 
		/// in caso di errore ritorna un valore di default dell'enumerativo.
		/// </summary>
		//---------------------------------------------------------------------
		public static Enum Parse(Type enumType, string aValue, bool ignoreCase)
		{
			if (aValue == null || aValue.Length == 0) 
				return SetDefault(enumType);
			object enumParsed;
			try
			{
				enumParsed = Enum.Parse(enumType, aValue, ignoreCase);
			}
			catch (ArgumentException)
			{
				enumParsed = null;
			}
			if (enumParsed == null) 
				return SetDefault(enumType);
			if (enumParsed is Enum)
				return enumParsed as Enum;
			return SetDefault(enumType);
		}

		//---------------------------------------------------------------------
		public static Enum SetDefault(Type enumType)
		{
			Debug.WriteLine("EnumParser.SetDefault: Si è cercato di parsare ad enumerativo una stringa non valida, verrà settato il default.");
			if (enumType == typeof(MailPriority))
				return MailPriority.Normal;
			if (enumType == typeof(MailFormat))
				return MailFormat.Text;
			if (enumType == typeof(Options))
				return Options.Result;
			Debug.Fail("EnumParser.SetDefault: Si è cercato di parsare ad enumerativo una stringa non valida, il tipo di enumerativo però è sconosciuto, verrà settato il primo valore.");
			return Enum.GetValues(enumType).GetValue(0) as Enum;
		}
	}

	//=========================================================================
	public class XmlStrings
	{
		//=========================================================================
		public class Attribute
		{
			public static string Value		= "value";
			public static string Default	= "default";

		}

		//=========================================================================
		public class Element
		{
			public static string Mail		= "Mail";
			public static string Format		= "Format";
			public static string Priority	= "Priority";
			public static string Folder		= "Folder";
			public static string Subject	= "Subject";
			public static string From		= "From";
			public static string To			= "To";
			public static string Option		= "Option";
			public static string OptionsList = "OptionsList";

		}
	}
}
