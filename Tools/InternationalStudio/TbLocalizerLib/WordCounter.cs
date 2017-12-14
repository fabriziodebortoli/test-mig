using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
//
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	public class WordCounter
	{
		#region Private members
		DictionaryCreator dictionaryCreator;

		private bool applyFilter;
		private int words;
		private int translated;

		// Static members
		private static ProjectFilterDialog filterDialog = new ProjectFilterDialog();
		private static TranslationProgressForm progressForm = new TranslationProgressForm();
		#endregion

		#region Private properties
		//---------------------------------------------------------------------
		private bool ApplyFilter { get { return applyFilter && filterDialog.ApplyFilter; } }
		//---------------------------------------------------------------------
		private ArrayList FilesToExclude { get { return filterDialog.FilesToExclude; } }
        //---------------------------------------------------------------------
        private string[] Filters { get { return filterDialog.Filters; } }
        #endregion

		#region Constructors
		//---------------------------------------------------------------------
		public WordCounter(DictionaryCreator dc)
		{
			dictionaryCreator = dc;
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Dato un nodo di dizionario, effettua il conteggio delle parole
		/// e delle parole tradotte valorizzando la cache.
		/// </summary>
		/// <param name="node">Il nodo di dizionario sotto esame.</param>
		/// <returns>La Hashtable contenente la cache.</returns>
		//---------------------------------------------------------------------
		private Hashtable CountWords(LocalizerTreeNode node)
		{
			if (node == null)
				return null;

			if (node.WordTable != null)
				return node.WordTable;

			if (node.WordTable == null)
				node.WordTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
			else
				node.WordTable.Clear();

			if (node.Type == NodeType.LASTCHILD)
			{
				if (ApplyFilter)
				{
					foreach (string file in FilesToExclude)
						if (string.Compare(((DictionaryTreeNode)node).GroupIdentifier, file, true) == 0)
							return node.WordTable;
				}

				CountWords((DictionaryTreeNode)node);
			}
			else
			{
				if (ApplyFilter && ((LocalizerTreeNode)node.Parent).Type == NodeType.LANGUAGE)
				{
					bool found = false;
					foreach (string filter in Filters)
						if (string.Compare(filter, node.Name, true) == 0)
						{
							found = true;
							break;
						}

					if (!found)
						return node.WordTable;
				}

				foreach (LocalizerTreeNode child in node.Nodes)
					foreach (DictionaryEntry de in CountWords(child))
					{
						WordCount currentWord = node.WordTable[de.Key] as WordCount;
						WordCount childWord = de.Value as WordCount;

						if (currentWord == null)
						{
							currentWord = new WordCount(childWord.TotalWords, childWord.TranslatedWords);
							node.WordTable[de.Key] = currentWord;
						}
						else
						{
							currentWord.TotalWords += childWord.TotalWords;
							currentWord.TranslatedWords += childWord.TranslatedWords;
						}
					}
			}

			return node.WordTable;
		}

		/// <summary>
		/// Dato un nodo di dizionario, effettua il conteggio delle parole
		/// e delle parole tradotte valorizzando la cache.
		/// </summary>
		/// <param name="node">Il nodo di dizionario sotto esame.</param>
		//---------------------------------------------------------------------
		private void CountWords(DictionaryTreeNode node)
		{
			XmlNodeList list = node.GetStringNodes();

			if (list == null)
				return;

			Hashtable wordTable = node.WordTable;
			foreach (XmlElement element in list)
			{
				// Se il nodo è invalidato non deve essere conteggiato.
				if (element.GetAttribute(AllStrings.valid) == AllStrings.falseTag)
					continue;

				string aValue = element.GetAttribute(AllStrings.baseTag);
				if (aValue.Trim().Length == 0)
					continue;

				string target = element.GetAttribute(AllStrings.target);
				string temporary = element.GetAttribute(AllStrings.temporary);
				bool isTranslated =
					target.Trim().Length > 0 &&
					(SolutionDocument.LocalInfo.CountTemporaryAsTranslated ||
					temporary != AllStrings.trueTag);

				string[] split = CommonFunctions.SplitInWords(aValue);
				foreach (string item in split)
				{
					string cleanItem = GlossaryFunctions.TreatForGlossary(item, true);

					if (wordTable.ContainsKey(cleanItem))
					{
						WordCount wc = (WordCount)wordTable[cleanItem];
						wc.TotalWords++;

						if (isTranslated) 
							wc.TranslatedWords++;
					}
					else
						wordTable.Add(cleanItem, new WordCount(1, isTranslated ? 1 : 0));
				}
			}
		}

		/// <summary>
		/// Effettua il conteggio delle parole e delle parole tradotte.
		/// </summary>
		/// <param name="node">Il nodo di dizionario sotto esame.</param>
		/// <param name="totalWords">Il numero totale delle parole nel dizionario.</param>
		/// <param name="translatedWords">Il numero totale delle parole tradotte nel dizionario.</param>
		//---------------------------------------------------------------------
		private void CountTranslatedWords(LocalizerTreeNode node, out int totalWords, out int translatedWords)
		{
			int max = 0;
			int totalNonRepeatedWords = 0;

			lock(node)
			{
				ExtractWordInfo
					(
					CountWords(node),
					out max,
					out totalNonRepeatedWords,
					out totalWords,
					out translatedWords,
					null,
					null
					);
			}
		}

		//---------------------------------------------------------------------
		private void ExtractWordInfo
		(
			Hashtable wordsTable,
			out int max,
			out	int totalNonRepeatedWords,
			out int totalWords,
			out int translatedWords,
			ArrayList frequentWords,
			LocalizerDocument wordsDoc
		)
		{
			max = totalNonRepeatedWords = totalWords = translatedWords = 0;

			if (wordsDoc != null)
				wordsDoc.AppendChild(wordsDoc.CreateElement(AllStrings.wordsTag));

			foreach (DictionaryEntry item in wordsTable)
			{
				string word = item.Key.ToString();
				WordCount wc = (WordCount)item.Value;

				if (wordsDoc != null)
				{
					XmlElement wordNode = wordsDoc.CreateElement(AllStrings.wordTag);
					wordNode.SetAttribute(AllStrings.valueTag, word);
					wordNode.SetAttribute(AllStrings.occurrenciesTag, wc.TotalWords.ToString());
					wordNode.SetAttribute(AllStrings.translatedTag, wc.TranslatedWords.ToString());
					wordsDoc.DocumentElement.AppendChild(wordNode);
				}

				bool isNumber = true;
				foreach (char ch in word.ToCharArray())
				{
					if (!Char.IsDigit(ch)) 
					{
						isNumber = false;
						break;
					}
				}

				if (!isNumber)
				{
					totalNonRepeatedWords++;
					totalWords += wc.TotalWords;
					translatedWords += wc.TranslatedWords;

					if (frequentWords != null)
					{
						if (wc.TotalWords > max)
						{
							max = wc.TotalWords;
							frequentWords.Clear();
							frequentWords.Add(word);
						}
						else if (wc.TotalWords == max)
							frequentWords.Add(word);
					}
				}
			} // foreach (DictionaryEntry item in wordsTable)
		}

		//---------------------------------------------------------------------
		private string[] GetAvailableDictionaries(LocalizerTreeNode node)
		{
			string[] availableDictionaries;

			switch (node.Type)
			{
				case NodeType.SOLUTION:
				case NodeType.PROJECT:
					availableDictionaries = dictionaryCreator.GetAvailableDictionaries(node, true).ToArray(typeof(string)) as string[];
					break;

				case NodeType.LANGUAGE:
					availableDictionaries = new string[] { node.Name };
					break;

				case NodeType.RESOURCE:
				case NodeType.LASTCHILD:
					DictionaryTreeNode dtn = node as DictionaryTreeNode;
					availableDictionaries = new string[] { dtn.Culture };
					break;

				default:
					availableDictionaries = new string[] {};
					break;
			}

			return availableDictionaries;
		}

		/// <summary>
		/// Costruisce il messaggio da visualizzare in una MessageBox
		/// coi risultati del calcolo.
		/// </summary>
		/// <param name="culture">La culture di cui è stata calcolata la percentuale di traduzione.</param>
		/// <param name="verbose">true per un messaggio esplicito, false per un messaggio sintetico.</param>
		/// <returns>Il messaggio da visualizzare.</returns>
		/// <remarks>
		/// Metodo lasciato per retrocompatibilità giacchè viene chiamato
		/// dal metodo pubblico GetWordInfoString, usato in DictionaryCreator.
		/// </remarks>
		//---------------------------------------------------------------------
		private string GetMessage(string culture, bool verbose)
		{
			float percentage = 0F;
			if (translated != 0 && words != 0)
				percentage = ((float)translated / (float)words * 100F);
			if (words == 0)
				percentage = 100F;

			string provider =
				verbose ?
				Strings.TranslationProgressExplication :
				" - {0}/{1} ({3:0.000}%)";
			string message =
				string.Format(provider, translated, words, culture, percentage);

			if (verbose && ApplyFilter)	
			{
				message += Environment.NewLine;
				message += Environment.NewLine;
				message += Strings.CategoryFilter;
				message += Environment.NewLine;

				foreach (string s in Filters)
				{
					if (s != Filters[0])
						message += ", ";
					message += s;
				}
			}

			// There are {0} translated strings over {1} for dictionary '{2}'.
			// Percentage: {3:0.0}%
			// Category filter:
			// xx, yy, zz
			return message;
		}

		//---------------------------------------------------------------------
		private void ShowProgress(IWin32Window owner, CultureInfo aCultureInfo)
		{
			progressForm.Init
			(
				aCultureInfo,
				translated,
				words,
				ApplyFilter,
				Filters
			);

			progressForm.ShowDialog(owner);
		}

		//---------------------------------------------------------------------
		private void ShowMessage(IWin32Window owner, string message)
		{
			MessageBox.Show
			(
				owner,
				message,
				Application.ProductName,
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation
			);
		}
		#endregion

		#region Public methods

        //---------------------------------------------------------------------
        public string[] GetFilters() { return Filters; }

        //---------------------------------------------------------------------
        public bool IsUsingFilters() { return ApplyFilter; }

        //---------------------------------------------------------------------
        public StringCollection AvailableFilters() { return filterDialog.AvailableFilters; }
        
        //---------------------------------------------------------------------
        public bool SelectFilters(LocalizerTreeNode node)
        {
            filterDialog.GlobalFiltersOnly = true;

            if (filterDialog.ShowDialog(null) != DialogResult.OK)
                return false;

            applyFilter = true;

            return true;
        }

		/// <summary>
		/// Dato un nodo di dizionario, effettua il conteggio delle parole
		/// e delle parole tradotte.
		/// </summary>
		/// <param name="node">Il nodo di dizionario sotto esame.</param>
		//---------------------------------------------------------------------
		public void Count(LocalizerTreeNode node)
		{
			if (node == null)
			{
				ShowMessage(node.TreeView, Strings.NoSelectedItem);
				return;
			}

			LocalizerCursor.Current = Cursors.WaitCursor;

			try
			{
				Hashtable wordsTable = CountWords(node);

				if (wordsTable != null)
				{
					LocalizerDocument wordsDoc = new LocalizerDocument();

					int max = 0;
					int totalNonRepeatedWords = 0;
					int totalWords = 0;
					int translatedWords = 0;
					ArrayList frequentWords = new ArrayList();
	
					ExtractWordInfo
					(
						wordsTable,
						out max,
						out totalNonRepeatedWords,
						out totalWords,
						out translatedWords,
						frequentWords,
						wordsDoc
					);

					if (!Directory.Exists(AllStrings.LOGPATH))
						Directory.CreateDirectory(AllStrings.LOGPATH);

					wordsDoc.Save(AllStrings.WORDLOG);

					StringBuilder sb = new StringBuilder();
					sb.Append(string.Format(Strings.Words, totalWords));
					sb.Append(Environment.NewLine);
					sb.Append(string.Format(Strings.NonRepeatedWords, totalNonRepeatedWords));
					sb.Append(Environment.NewLine);

					if (frequentWords.Count != 0)
					{
						sb.Append(Strings.MostRepeatedWords);
						sb.Append(Environment.NewLine);

						foreach (string word in frequentWords)
						{
							sb.Append("\t'");
							sb.Append(word);
							sb.Append("'");
							sb.Append(Environment.NewLine);
						}

						sb.Append(string.Format(Strings.Occurrencies, max));
					}

					sb.Append(Environment.NewLine);
					sb.Append(string.Format(Strings.SeeWordLog, AllStrings.WORDLOG));

					// Total words: {0}
					// Total non repeated words: {0}
					// Most repeated words:
					//  -
					// Words have been listed in file {0}.
					ShowMessage(node.TreeView, sb.ToString());
				} // if (wordsTable != null)
			}
			finally
			{
				LocalizerCursor.Current = Cursors.Default;
			}
		}

		//---------------------------------------------------------------------
		public string GetWordInfoString(LocalizerTreeNode node, bool verbose)
		{
			int totalWords = 0;
			int translatedWords = 0;

			CountTranslatedWords(node, out totalWords, out translatedWords);
			LocalizerTreeNode parentNode = node.GetTypedParentNode(NodeType.LANGUAGE);

			if (parentNode == null)
				return string.Empty;

			translated = translatedWords;
			words = totalWords;

			return GetMessage(parentNode.Name, verbose);
		}

		//---------------------------------------------------------------------
		public void ShowProgressPercentage(LocalizerTreeNode node)
		{
			if (node == null)
			{
				ShowMessage(node.TreeView, Strings.NoSelectedItem);
				return;
			}

			string[] availableDictionaries = GetAvailableDictionaries(node);

			if (availableDictionaries == null || availableDictionaries.Length == 0)
			{
				ShowMessage(node.TreeView, Strings.NoSelectedItem);
				return;
			}

			if (node.IsBaseLanguageNode)
			{
				ShowMessage(node.TreeView, Strings.CantEditBaseDictionary);
				return;
			}

			NodeType type = node.Type;
			CultureInfo aCultureInfo = new CultureInfo(availableDictionaries[0]);

			// Se il nodo può riferirsi a più lingue o può proporre più
			// categorie, si chiedono la lingua e gli eventuali filtri
			if (type == NodeType.SOLUTION || type == NodeType.PROJECT || type == NodeType.LANGUAGE)
			{
				filterDialog.AvailableDictionaries = availableDictionaries;
                filterDialog.GlobalFiltersOnly = false;

				if (filterDialog.ShowDialog(node.TreeView) != DialogResult.OK)
					return;

				aCultureInfo = filterDialog.ChoosedLanguage;
				applyFilter = true;
			}
			else
				applyFilter = false;

			switch (type)
			{
				case NodeType.SOLUTION:
				{
					int totalWords = 0;
					int translatedWords = 0;
					bool found = false;

					foreach (TreeNode prjNode in node.Nodes)
						foreach (LocalizerTreeNode langNode in prjNode.Nodes)
							if (string.Compare(langNode.Name, filterDialog.ChoosedLanguage.Name, true) == 0)
							{
								found = true;
								int totalPart = 0;
								int translatedPart = 0;
								langNode.CleanWordTable(true);
								CountTranslatedWords(langNode, out totalPart, out translatedPart);

								totalWords += totalPart;
								translatedWords += translatedPart;
							}

					if (found)
					{
						words = totalWords;
						translated = translatedWords;
						ShowProgress(node.TreeView, aCultureInfo);
					}
					else
						ShowMessage(node.TreeView, string.Format(Strings.LanguageUnavailable, filterDialog.ChoosedLanguage.EnglishName));

					break;
				}

				case NodeType.PROJECT:
				{
					bool found = false;

					foreach (LocalizerTreeNode langNode in node.Nodes)
						if (string.Compare(langNode.Name, filterDialog.ChoosedLanguage.Name, true) == 0)
						{
							found = true;
							langNode.CleanWordTable(true);
							GetWordInfoString(langNode, true);
						}

					if (found)
						ShowProgress(node.TreeView, aCultureInfo);
					else
						ShowMessage(node.TreeView, string.Format(Strings.LanguageUnavailable, filterDialog.ChoosedLanguage.EnglishName));

					break;
				}

				default:
				{
					node.CleanWordTable(true);
					GetWordInfoString(node, true);
					ShowProgress(node.TreeView, aCultureInfo);

					break;
				}
			}
		}
		#endregion
	}

	//=========================================================================
	public class WordCount
	{
		public int TotalWords;
		public int TranslatedWords;

		//---------------------------------------------------------------------
		public WordCount(int totalWords, int translatedWords)
		{
			this.TotalWords = totalWords;
			this.TranslatedWords = translatedWords;
		}
	}
}
