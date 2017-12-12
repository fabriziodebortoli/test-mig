using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace Microarea.TaskBuilderNet.UI.DocumentMerge
{
	//=========================================================================
	public class Odt
	{
		private static string OoExe   = string.Empty;		// path to soffice.exe
		private static string SaveDir = string.Empty;		// default directory, where documents are saved before printing

		private string			templateFile;
		private XmlDocument		doc;
		private OdtDocFields	inputs;

		public OdtDocFields Inputs { get { return inputs; } }

		//---------------------------------------------------------------------
		public Odt(string odtFile)
		{
			Load(odtFile);
		}

		//---------------------------------------------------------------------
		public void Load(string odtTemplateFile)
		{
			this.templateFile = odtTemplateFile;

			// Read content.xml
			using (ZipInputStream zis = new ZipInputStream(File.OpenRead(templateFile)))
			{
				ZipEntry ze;
				while ((ze = zis.GetNextEntry()) != null)
				{
					if (ze.Name == "content.xml")
					{
						StreamReader sr = new StreamReader(zis, Encoding.UTF8);
						string text = sr.ReadToEnd();
						doc = new XmlDocument();
						doc.LoadXml(text);
						break;
					}
				}
			}

			// Add all input fields in collection
			inputs = new OdtDocFields(this);
			PopulateInputs(doc.DocumentElement);
		}

		//---------------------------------------------------------------------
		private void PopulateInputs(XmlNode node)
		{
			if (node.Name == "text:text-input")
				inputs.AddNode(node);

			foreach (XmlNode child in node.ChildNodes)
			{
				PopulateInputs(child);
			}
		}

		//---------------------------------------------------------------------
		public void Save(string fileName)
		{
			int count;
			byte[] buf = new byte[4096];
			DateTime now = DateTime.Now;

			using (ZipInputStream zis = new ZipInputStream(File.OpenRead(templateFile)))
			{
				using (ZipOutputStream zos = new ZipOutputStream(File.OpenWrite(fileName)))
				{
					ZipEntry ze;
					while ((ze = zis.GetNextEntry()) != null)
					{
						ZipEntry entry = new ZipEntry(ze.Name);
						entry.DateTime = now;
						zos.PutNextEntry(entry);

						if (ze.Name == "content.xml")
						{
							string text = doc.OuterXml;
							byte[] textBytes = Encoding.UTF8.GetBytes(text);
							zos.Write(textBytes, 0, textBytes.Length);
						}
						else
						{
							while ((count = zis.Read(buf, 0, buf.Length)) > 0)
							{
								zos.Write(buf, 0, count);
							}
						}
					}

					zos.Finish();
				}
			}
		}

		//---------------------------------------------------------------------
		private Process RunOo(string args)
		{
			//TODO LARA sostituire con una ShellExecute del file
			OoExe = "C:\\Program Files (x86)\\OpenOffice 4\\program\\soffice.exe";
			if (OoExe == null)
			{
				throw new Exception("OpenOffice not found - either not installed or at unknown path");
			}

			Process p = new Process();
			p.StartInfo.FileName = OoExe;
			p.StartInfo.WorkingDirectory = Path.GetDirectoryName(OoExe);
			p.StartInfo.Arguments = "-writer " + args;
			p.Start();
			return p;
		}

		//---------------------------------------------------------------------
		private Process RunOo(string args, string fileName)
		{
			return RunOo(args + " \"" + fileName + "\"");
		}

		//---------------------------------------------------------------------
		private string GetTmpFile()
		{
			string fileName;
			int no = 0;

			do
			{
				fileName = Path.Combine(SaveDir, String.Format("{0}_{1:yyyy_MM_dd_HH_mm}{2}.odt",
					Path.GetFileNameWithoutExtension(templateFile),
					DateTime.Now,
					no == 0 ? "" : "(" + (no++) + ")"));
			}
			while (File.Exists(fileName));
			return fileName;
		}

		//---------------------------------------------------------------------
		public void Print(string fileToSave)
		{
			Save(fileToSave);
			RunOo("-p", fileToSave);
		}

		//---------------------------------------------------------------------
		public void Print()
		{
			Print(GetTmpFile());
		}

		//---------------------------------------------------------------------
		public void OpenInOo(string fileToSave)
		{
			Save(fileToSave);
			RunOo("", fileToSave);
		}

		//---------------------------------------------------------------------
		public void OpenInOo()
		{
			OpenInOo(GetTmpFile());
		}

		#region OoExe

		//------------------------------------------------------------------------
		static void SetOoExe(string path)
		{
			if (OoExe == null && File.Exists(path))
			{
				OoExe = path;
			}
		}

		//------------------------------------------------------------------------
		static Odt()
		{
			//SetOoExe(@"C:\Program Files\OpenOffice.org 2.4\program\soffice.exe");
			//SaveDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		}

		#endregion
	}

	//============================================================================
	public class OdtDocFields
	{
		private Odt			odt;
		private Hashtable	ht;
		private string[]	fieldNames;

		public int Count {	get {return ht.Count;}}
		public string this[string fieldName] {	get{return ((XmlNode)(ht[fieldName])).InnerText;}	set{((XmlNode)(ht[fieldName])).InnerText = (string)value;}}
		public string[] FieldNames
		{
			get
			{
				if (fieldNames == null)
				{
					fieldNames = new string[ht.Count];
					ht.Keys.CopyTo(fieldNames, 0);
				}
				return fieldNames;
			}
		}

		//------------------------------------------------------------------------
		public OdtDocFields(Odt odt)
		{
			this.odt = odt;
			this.ht = new Hashtable();
		}

		//------------------------------------------------------------------------
		public void AddNode(XmlNode node)
		{
			string desc = node.Attributes["text:description"].Value;
			ht[desc] = node;
		}
	}
}