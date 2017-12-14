using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class EBLink : IEBLink
	{
		private string application;
		private string module;
	
		/// <remarks/>
		//-----------------------------------------------------------------------------
		public static EBLink Parse(string path)
		{
			using (StreamReader input = new StreamReader(path))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(EBLink));
				return serializer.Deserialize(input) as EBLink;
			}
		}
		//-----------------------------------------------------------------------------
		internal void Save(string path)
		{
			using (StreamWriter sw = new StreamWriter(path))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(EBLink));
				serializer.Serialize(sw, this);
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		//-----------------------------------------------------------------------------
		public string Application
		{
			get
			{
				return this.application;
			}
			set
			{
				this.application = value;
			}
		}
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
		//-----------------------------------------------------------------------------
		public string Module
		{
			get
			{
				return this.module;
			}
			set
			{
				this.module = value;
			}
		}

		//-----------------------------------------------------------------------------
		public static IList<IEBLink> ParseFolder(string path)
		{
			string[] files = Directory.GetFiles(path, "*" + NameSolverStrings.EbLinkExtension);
			IList<IEBLink> ar = new List<IEBLink>();

			for (int i = 0; i < files.Length; i++)
				ar.Add(Parse(files[i]));

			return ar;
		}
	}
}
