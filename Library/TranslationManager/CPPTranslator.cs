using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for CPPTranslator.
	/// </summary>
	//================================================================================
	public class CPPTranslator : BaseTranslator
	{
		CPPFunctionReplacer replacer;
		string currentFile;
		bool stop;
		DirectoryIterator iterator;

		//--------------------------------------------------------------------------------
		public CPPTranslator()
		{
			iterator = new DirectoryIterator
				(
				new string[]{"*.cpp", "*.h"},
				new DirectoryIterator.FileProcessingFunction(TranslateFile)
				);
		}

		//--------------------------------------------------------------------------------
		public void Stop()
		{
			stop = true;
			iterator.Stop();
		}

		//--------------------------------------------------------------------------------
		public override void Run(TranslationManager tManager)
		{
			this.transManager = tManager;
			this.replacer = new CPPFunctionReplacer(transManager);
			
			iterator.OnStartProcessingFile += new Microarea.TaskBuilderNet.Core.Generic.DirectoryIterator.FileProcess(Iterator_OnStartProcessingFile);
			iterator.OnEndProcessingFile += new Microarea.TaskBuilderNet.Core.Generic.DirectoryIterator.FileProcess(Iterator_OnEndProcessingFile);
			iterator.OnError += new Microarea.TaskBuilderNet.Core.Generic.DirectoryIterator.FileProcessError(Iterator_OnError);
			
			foreach (BaseModuleInfo bmi in  transManager.GetNewApplicationInfo().Modules) 
			{
				foreach (LibraryInfo li in bmi.Libraries)
				{
					if (stop) break;
					iterator.Start(li.FullPath);
				}
			}

			EndRun(false);
		}

		//--------------------------------------------------------------------------------
		private void TranslateFile(string file)
		{
			replacer.FileName = file;
			replacer.Replace();
		}

		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			return Messages.CPPTranslatorToolName;
		}

		//--------------------------------------------------------------------------------
		private void Iterator_OnStartProcessingFile(DirectoryIterator sender, string path)
		{
			this.currentFile = path;
			this.SetProgressMessage(Messages.StartProcessingFile, path);
		}

		//--------------------------------------------------------------------------------
		private void Iterator_OnEndProcessingFile(DirectoryIterator sender, string path)
		{
			this.SetProgressMessage(Messages.EndProcessingFile, path);
		}

		//--------------------------------------------------------------------------------
		private void Iterator_OnError(DirectoryIterator sender, Exception exception)
		{
			this.SetProgressMessage(Messages.ErrorProcessingFile, currentFile, exception.Message);
			SetLogError("ErrorProcessingFile:" + currentFile +":\r\n"+ exception.Message, this.ToString());
			System.Diagnostics.Debug.WriteLine("ErrorProcessingFile:" + currentFile +":\r\n"+ exception.Message);
		}		
	}


	//================================================================================
	public class CPPFunctionReplacer : RegexTextReplacer
	{
		protected TranslationManager tManager;
		public const string namespaceLocator = "nsLocator";
		
		protected static readonly string GenericNSLocatorPattern =
			"(?<"
			+ namespaceLocator +
			">{0})" +
			zeroOrMoreSpaces +
			openRound +
			zeroOrMoreSpaces +
			"{1}" + 
			zeroOrMoreSpaces + 
			closeRound;
		
		protected static readonly string tokenPattern = 
			Group 
			(
			singleWord,
			stringGroupName
			);

		//--------------------------------------------------------------------------------
		public CPPFunctionReplacer(TranslationManager tManager) 
			: base (null)
		{
			this.tManager = tManager;
		}

		//--------------------------------------------------------------------------------
		protected override void Init()
		{
			string[] tokens = new string[tManager.NamespaceLocators.Count];
			
			int i = 0;
			foreach (string locator in tManager.NamespaceLocators)
			{
				tokens[i++] = string.Format
					(
					GenericNSLocatorPattern, 
					locator, 
					tManager.NeedsInvertedCommas(locator) ? stringPattern : tokenPattern
					);
				
			}

			regularExpression = new Regex
				(
				AlternativeTokens(tokens),
				RegexOptions.ECMAScript | RegexOptions.Compiled
				);
		}

		//--------------------------------------------------------------------------------
		protected override string Translate(System.Text.RegularExpressions.Match m)
		{
			Group stringGroup = m.Groups[stringGroupName];
			if (!stringGroup.Success) return m.Value;
			
			Group nsLocatorGroup = m.Groups[namespaceLocator];

			StringBuilder sb = new StringBuilder();
			int currentPosition = 0;
			for (int i = 0; i <stringGroup.Captures.Count; i++)
			{
				Capture sc = stringGroup.Captures[i];
				Capture nsc = nsLocatorGroup.Captures[i];

				int index = sc.Index - m.Index;
				sb.Append(m.Value.Substring(currentPosition, index - currentPosition));
			
				string oldValue = sc.Value;
				bool invertedCommasFound = true;
				string newValue = AdjustNewString
					(
					Translate(nsc.Value, AdjustOldString(oldValue, out invertedCommasFound)), 
					invertedCommasFound
					);
			
				if (oldValue != newValue)
					modified = true;
				sb.Append(newValue);
				currentPosition = index + oldValue.Length;
			}
			sb.Append(m.Value.Substring(currentPosition));
			return  sb.ToString();
		}

		//--------------------------------------------------------------------------------
		protected virtual string Translate(string namespaceLocator, string oldValue)
		{
			return tManager.GetNameSpaceConversion(tManager.GetLookUpTypeFromToken(namespaceLocator), oldValue);
		}

		//--------------------------------------------------------------------------------
		protected virtual string AdjustOldString(string s, out bool invertedCommasFound)
		{
			if ((s[0] == '"') && (s[s.Length - 1] == '"'))
			{
				invertedCommasFound = true;
				return s.Substring(1, s.Length - 2);
			}

			invertedCommasFound = false;
			return s;
		}
		
		//--------------------------------------------------------------------------------
		protected virtual string AdjustNewString(string s, bool invertedCommasNeeded)
		{
			string fmt = invertedCommasNeeded ? "\"{0}\"" : "{0}";
			return string.Format(fmt, s);
		}
	}
}
