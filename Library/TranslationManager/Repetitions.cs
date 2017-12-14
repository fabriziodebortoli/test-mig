using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for Repetitions.
	/// </summary>
	public partial class Repetitions : System.Windows.Forms.Form
	{
		private Hashtable repetitions = new Hashtable();
		private ArrayList targetWithBlank = new ArrayList();
		private ArrayList targetWithVat = new ArrayList();
		private ArrayList valueTooLong = new ArrayList();
		private string applicationPath = string.Empty;

		public Repetitions()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public Repetitions(string appPath, Hashtable rep, ArrayList tBlank, ArrayList vTooLong, ArrayList tVat)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			repetitions = rep;
			targetWithBlank = tBlank;
			valueTooLong = vTooLong;
			targetWithVat = tVat;
			applicationPath = appPath;
		}

		private void Repetitions_Load(object sender, System.EventArgs e)
		{
//			string txt = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<GlossaryRepetitions>\n";
//			foreach (XmlNode nTerm in repetitions.Values)
//			{
//				txt += string.Format("  {0}\n", nTerm.OuterXml);
//			}
//			txt += "</GlossaryRepetitions>";
//			ENTRepetition.Text = txt;
//			ENTRepetition.SaveFile(Path.Combine(applicationPath, "GlossaryRepetitions.xml"), RichTextBoxStreamType.PlainText);

			ENTBlankSpaces.Text = "<BlankSpaces>\n";
			foreach (XmlNode n in targetWithBlank)
			{
				ENTBlankSpaces.Text += string.Format("  {0}\n", n.OuterXml);
			}
			ENTBlankSpaces.Text += "</BlankSpaces>";
			ENTBlankSpaces.SaveFile(Path.Combine(applicationPath, "BlankSpaces.xml"), RichTextBoxStreamType.PlainText);
			
			ENTValuesTooLong.Text = "<ValuesTooLong>\n";
			foreach (XmlNode n in valueTooLong)
			{
				ENTValuesTooLong.Text += string.Format("  {0}\n", n.OuterXml);
			}
			ENTValuesTooLong.Text += "</ValuesTooLong>";
			ENTValuesTooLong.SaveFile(Path.Combine(applicationPath, "ValuesTooLong.xml"), RichTextBoxStreamType.PlainText);
		}
	}
}
