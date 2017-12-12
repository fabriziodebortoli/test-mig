using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

using Microarea.EasyBuilder.CodeEditorProviders;
using ICSharpCode.NRefactory.Completion;

namespace Microarea.EasyBuilder.CodeCompletion
{
	//=========================================================================
	internal class CSharpCompletion
	{
		private IProjectContent projectContent;
		private ICSharpScriptProvider scriptProvider;

		public IProjectContent ProjectContent { get { return projectContent; } }
										   
		//-------------------------------------------------------------------------------
		public CSharpCompletion(IProjectContent projectContent, ICSharpScriptProvider scriptProvider = null)
		{
			this.projectContent = projectContent;
			this.scriptProvider = scriptProvider;
		}

		//-------------------------------------------------------------------------------
		public ICSharpScriptProvider ScriptProvider { get { return scriptProvider; } }
	
		//-------------------------------------------------------------------------------
		public CodeCompletionResult GetCompletions(IDocument document, int offset)
		{
			return GetCompletions(document, offset, false);
		}

		//-------------------------------------------------------------------------------
		public CodeCompletionResult GetCompletions(IDocument document, int offset, bool controlSpace)
		{
			//get the using statements from the script provider
			string usings = null;
			string variables = null;
			if (ScriptProvider != null)
			{
				usings = ScriptProvider.GetUsing();
				variables = ScriptProvider.GetVars();
			}
			return GetCompletions(document, offset, controlSpace, usings, variables);
		}

		/// <summary>
		/// Applica il filtraggio all'intellisense andando ad escludere gli elementi che hanno attributo apposito
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>*/
		//-------------------------------------------------------------------------------
		public static IEnumerable<ICompletionData> FilterIntellisense(IEnumerable<ICompletionData> data)
		{
			if (data == null)
				return null;

			List<ICompletionData> newList = data.ToList();

			if (IntellisenseExcludeList.Instance.IntellisenseExcludes == null)
				return newList;

			Dictionary<string, ICompletionData> dict = new Dictionary<string, ICompletionData>();
			for (int i = 0; i < newList.Count; i++)
			{
				//Per qualche motivo ancora incognito, alcuni elementi dell'array non sono IEntity nel caso ignoro il filtraggio
				IEntityCompletionData ecd = newList[i] as IEntityCompletionData;
				if (ecd != null)
				{
					//prima controllo che gli overload siano da escludere
					foreach (ICompletionData item in ecd.OverloadedData)
					{
						IEntityCompletionData overload = item as IEntityCompletionData;
						string overloadFullName = overload.Entity.FullName;
						if (IntellisenseExcludeList.Instance.IntellisenseExcludes.ContainsKey(overloadFullName))
							continue;

						if (!dict.ContainsKey(overloadFullName))
							dict.Add(overloadFullName, item);
					}

					//e poi controllo il member vero e proprio
					string a = ecd.Entity.FullName;
					if (IntellisenseExcludeList.Instance.IntellisenseExcludes.ContainsKey(a))
						continue;

					if (!dict.ContainsKey(a))
						dict.Add(a, newList[i]);
				}
				else
				{
					if (!dict.ContainsKey(newList[i].ToString()))
						dict.Add(newList[i].ToString(), newList[i]);
				}
			}

			return new List<ICompletionData>(dict.Values);
		}

		//-------------------------------------------------------------------------------
		public CodeCompletionResult GetCompletions(IDocument document, int offset, bool controlSpace, string usings, string variables)
		{
			var result = new CodeCompletionResult();

			if (String.IsNullOrEmpty(document.FileName))
				return result;

			var completionContext = new CSharpCompletionContext(document, offset, projectContent, usings, variables);

			var completionFactory = new CSharpCompletionDataFactory(completionContext.TypeResolveContextAtCaret, completionContext);
			var cce = new CSharpCompletionEngine(
				completionContext.Document,
				completionContext.CompletionContextProvider,
				completionFactory,
				completionContext.ProjectContent,
				completionContext.TypeResolveContextAtCaret
				);

			cce.EolMarker = Environment.NewLine;
			cce.FormattingPolicy = FormattingOptionsFactory.CreateSharpDevelop();


			var completionChar = completionContext.Document.GetCharAt(completionContext.Offset - 1);
			int startPos, triggerWordLength;
			IEnumerable<ICSharpCode.NRefactory.Completion.ICompletionData> completionData;
			if (controlSpace)
			{
				if (!cce.TryGetCompletionWord(completionContext.Offset, out startPos, out triggerWordLength))
				{
					startPos = completionContext.Offset;
					triggerWordLength = 0;
				}
				completionData = cce.GetCompletionData(startPos, true);
			}
			else
			{
				startPos = completionContext.Offset;

				if (char.IsLetterOrDigit(completionChar) || completionChar == '_')
				{
					if (startPos > 1 && char.IsLetterOrDigit(completionContext.Document.GetCharAt(startPos - 2)))
						return result;
					completionData = cce.GetCompletionData(startPos, false);
					startPos--;
					triggerWordLength = 1;
				}
				else
				{
					completionData = cce.GetCompletionData(startPos, false);
					triggerWordLength = 0;
				}
			}

			result.TriggerWordLength = triggerWordLength;
			result.TriggerWord = completionContext.Document.GetText(completionContext.Offset - triggerWordLength, triggerWordLength);


			completionData = FilterIntellisense(completionData);

			//cast to AvalonEdit completion data and add to results
			foreach (var completion in completionData)
			{
				var cshellCompletionData = completion as CompletionData;
				if (cshellCompletionData != null)
				{
					cshellCompletionData.TriggerWord = result.TriggerWord;
					cshellCompletionData.TriggerWordLength = result.TriggerWordLength;
					result.CompletionData.Add(cshellCompletionData);
				}
			}

			//method completions
			if (!controlSpace)
			{
				// Method Insight
				var pce = new CSharpParameterCompletionEngine(
					completionContext.Document,
					completionContext.CompletionContextProvider,
					completionFactory,
					completionContext.ProjectContent,
					completionContext.TypeResolveContextAtCaret
				);

				var parameterDataProvider = pce.GetParameterDataProvider(completionContext.Offset, completionChar);
				result.OverloadProvider = parameterDataProvider as ICSharpCode.AvalonEdit.CodeCompletion.IOverloadProvider;
			}

			return result;
		}
	}
}
