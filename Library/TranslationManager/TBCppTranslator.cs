using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for TBCppTranslator.
	/// </summary>
	public class TBCppTranslator : CPPTranslator
	{
		TBCPPFunctionReplacer replacer;

		public TBCppTranslator()
		{
		}

		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			return "TB Cpp Translator";
		}

		//--------------------------------------------------------------------------------
		public override void Run(TranslationManager tManager)
		{
			this.transManager = tManager;
			this.replacer = new TBCPPFunctionReplacer(tManager);

			//TODO: Fabio Deve girare anche su Framework\TbGeneric\FontsTable.h		(per i fonts)
			TranslateFile(@"C:\MICROAREASERVER\Sviluppo\Running\Standard\TaskBuilder\Framework\TbGeneric\FontsTable.h");
			//TODO: Fabio Deve girare anche su Framework\TbGeneric\FormatsTable.cpp (per i formattatori)
			TranslateFile(@"C:\MICROAREASERVER\Sviluppo\Running\Standard\TaskBuilder\Framework\TbGeneric\FormatsTable.cpp");
			//TODO: Fabio Deve girare anche su Framework\TbClientCore\ClientObjects.h (per i macro MAGONET_APP per l'applicazione)
			TranslateFile(@"C:\MICROAREASERVER\Sviluppo\Running\Standard\TaskBuilder\Framework\TbClientCore\ClientObjects.h");
			//TODO: Fabio Deve girare anche su Framework\TbNameSolver\PathFinder.h (per i macro il nome del profilo Predefinito)
			TranslateFile(@"C:\MICROAREASERVER\Sviluppo\Running\Standard\TaskBuilder\Framework\TbNameSolver\PathFinder.h");
			//TODO: Fabio Deve girare anche su XEngine\TBXmlTransfer\ExpCriteriaObj.cpp (per la dicitura Query di personalizzazione)
			TranslateFile(@"C:\MicroareaServer\Sviluppo\Running\Standard\TaskBuilder\Extensions\XEngine\TBXmlTransfer\ExpCriteriaObj.cpp");

			EndRun(false);
		}

		//--------------------------------------------------------------------------------
		private void TranslateFile(string file)
		{
			replacer.FileName = file;
			replacer.Replace();
		}
	}

	//================================================================================
	public class TBCPPFunctionReplacer : CPPFunctionReplacer
	{
		private ArrayList nLocs = new ArrayList();

		public TBCPPFunctionReplacer(TranslationManager tManager)
			: base (tManager)
		{
			nLocs.Add("_NS_FONT");
			nLocs.Add("_NS_FMT");
			nLocs.Add("_NS_APP");
			nLocs.Add("_PREDEFINED");
			nLocs.Add("_QRY");
		}

		//--------------------------------------------------------------------------------
		protected override void Init()
		{
			string[] tokens = new string[nLocs.Count];
			
			int i = 0;
			foreach (string locator in nLocs)
			{
				tokens[i++] = string.Format
					(
					GenericNSLocatorPattern, 
					locator, 
					stringPattern
					);
				
			}

			regularExpression = new Regex
				(
				AlternativeTokens(tokens),
				RegexOptions.ECMAScript | RegexOptions.Compiled
				);
		}

		//--------------------------------------------------------------------------------
		protected override string Translate(string namespaceLocator, string oldValue)
		{
			switch (namespaceLocator)
			{
				case "_NS_FONT":
					return GetFontTranslation(oldValue);
				case "_NS_FMT":
					return GetFormatTranslation(oldValue);
				case "_NS_APP":
					return tManager.GetApplicationTranslation(oldValue);
				case "_PREDEFINED":
					return tManager.GetNameSpaceConversion(LookUpFileType.Profile, oldValue);
				case "_QRY":
					return "User custom query";
			}

			return oldValue;
		}

		//---------------------------------------------------------------------------
		private string GetFontTranslation(string fontName)
		{
			string fontsFileName = @"C:\MICROAREASERVER\Sviluppo\Running\Standard\TaskBuilder.Net\MicroareaConsole\MigrationKitAdmin\Migration_NET\Fonts.xml";
			if (!File.Exists(fontsFileName))
				return fontName;

			XmlDocument xDoc = new XmlDocument();
			xDoc.Load(fontsFileName);
			foreach (XmlNode nFont in xDoc.SelectNodes("Fonts/Font"))
			{
				string oldName = nFont.Attributes["oldName"].Value.ToString();

				if (string.Compare(oldName, fontName, true) == 0)
				{
					string newName = nFont.Attributes["newName"].Value.ToString();
					if (newName != string.Empty)
						return newName;
					else
						return fontName;
				}
			}

			return fontName;
		}

		//---------------------------------------------------------------------------
		private string GetFormatTranslation(string formatterName)
		{
			string formattersFileName = @"C:\MICROAREASERVER\Sviluppo\Running\Standard\TaskBuilder.Net\MicroareaConsole\MigrationKitAdmin\Migration_NET\Formatters.xml";
			if (!File.Exists(formattersFileName))
				return formatterName;

			XmlDocument xDoc = new XmlDocument();
			xDoc.Load(formattersFileName);
			foreach (XmlNode nFormatter in xDoc.SelectNodes("Formatters/Formatter"))
			{
				string oldName = nFormatter.Attributes["oldName"].Value.ToString();

				if (string.Compare(oldName, formatterName, true) == 0)
				{
					string newName = nFormatter.Attributes["newName"].Value.ToString();
					if (newName != string.Empty)
						return newName;
					else
						return formatterName;
				}
			}

			return formatterName;
		}
	}
}
