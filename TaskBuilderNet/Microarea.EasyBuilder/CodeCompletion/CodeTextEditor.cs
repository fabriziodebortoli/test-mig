using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.NRefactory.Editor;

namespace Microarea.EasyBuilder.CodeCompletion
{
	//=============================================================================================
	internal class CodeTextEditor : ICSharpCode.AvalonEdit.TextEditor
    {
		protected EBCompletionWindow completionWindow;
        protected OverloadInsightWindow insightWindow;
		public string FileName { get; set; }

		private Sources sources;
		public CSharpCompletion Completion
		{
			get
			{
				if (sources == null)
					throw new NullReferenceException("sources cannot be null");

				return new CSharpCompletion(sources.ProjectContent);
			}
		}

		//-------------------------------------------------------------------------------
		public CodeTextEditor()
        {
            TextArea.TextEntering += OnTextEntering;
            TextArea.TextEntered += OnTextEntered;
            ShowLineNumbers = true;

            var ctrlSpace = new RoutedCommand();
            ctrlSpace.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
            var cb = new CommandBinding(ctrlSpace, OnCtrlSpaceCommand);

            this.CommandBindings.Add(cb);
        }

		//-------------------------------------------------------------------------------
		public void OpenFile(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                throw new FileNotFoundException(fileName);

            if (completionWindow != null)
                completionWindow.Close();
            if (insightWindow != null)
                insightWindow.Close();

			FileName = fileName;
            Load(fileName);
            Document.FileName = FileName;

            SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(fileName));
        }

		//-------------------------------------------------------------------------------
		public bool SaveFile()
        {
            if (String.IsNullOrEmpty(FileName))
                return false;

            Save(FileName);
            return true;
        }


		//-------------------------------------------------------------------------------
		private void OnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            ShowCompletion(textCompositionEventArgs.Text, false);
        }

		//-------------------------------------------------------------------------------
		internal void SetSources(Sources sources)
		{
			this.sources = sources;
		}

		//-------------------------------------------------------------------------------
		private void OnCtrlSpaceCommand(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            ShowCompletion(null, true);
        }
	
		//-------------------------------------------------------------------------------
		private void ShowCompletion(string enteredText, bool controlSpace)
        {
			
            //only process csharp files and if there is a code completion engine available
            if (String.IsNullOrEmpty(Document.FileName))
                return;

            if (Completion == null)
                return;

            var fileExtension = Path.GetExtension(Document.FileName);
            fileExtension = fileExtension != null ? fileExtension.ToLower() : null;
            //check file extension to be a c# file (.cs, .csx, etc.)
            if (fileExtension == null || (!fileExtension.StartsWith(".cs")))
            {
                return;
            }

            if (completionWindow == null)
            {
                CodeCompletionResult results = null;
                try
                {
                    var offset = 0;
                    var doc = GetCompletionDocument(out offset);
                    results = Completion.GetCompletions(doc, offset, controlSpace);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Error in getting completion: " + exception);
                }
                if (results == null)
                    return;

                if (insightWindow == null && results.OverloadProvider != null)
                {
                    insightWindow = new OverloadInsightWindow(TextArea);
                    insightWindow.Provider = results.OverloadProvider;
                    insightWindow.Show();
					insightWindow.Left += SystemParameters.WorkArea.Width;
                    insightWindow.Closed += (o, args) => insightWindow = null;
                    return;
                }

				  if (completionWindow == null && results != null && results.CompletionData.Any())
                {
                    // Open code completion after the user has pressed dot:
                    completionWindow = new EBCompletionWindow(TextArea);
                    completionWindow.CloseWhenCaretAtBeginning = controlSpace;
                    completionWindow.StartOffset -= results.TriggerWordLength;
                    //completionWindow.EndOffset -= results.TriggerWordLength;
					completionWindow.Width = completionWindow.Width * 3 / 2;

					IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    foreach (var completion in results.CompletionData.OrderBy(item => item.Text))
                        data.Add(completion);

					if (results.TriggerWordLength > 0)
                    {
                        //completionWindow.CompletionList.IsFiltering = false;
                        completionWindow.CompletionList.SelectItem(results.TriggerWord);
                    }
                    completionWindow.Show();
                    completionWindow.Closed += (o, args) => completionWindow = null;
                }
            }


			//update the insight window
			if (!string.IsNullOrEmpty(enteredText) && insightWindow != null)
            {
                //whenever text is entered update the provider
                var provider = insightWindow.Provider as CSharpOverloadProvider;
                if (provider != null)
                {
                    //since the text has not been added yet we need to tread it as if the char has already been inserted
                    var offset = 0;
                    var doc = GetCompletionDocument(out offset);
                    provider.Update(doc, offset);
                    //if the windows is requested to be closed we do it here
                    if (provider.RequestClose)
                    {
                        insightWindow.Close();
                        insightWindow = null;
                    }
                }
            }
        }
	
		//-------------------------------------------------------------------------------
		private void OnTextEntering(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            if (textCompositionEventArgs.Text.Length > 0 && completionWindow != null)
            {
				char lastChar = textCompositionEventArgs.Text[0];
                if (!char.IsLetterOrDigit(lastChar) && lastChar!='_')
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(textCompositionEventArgs);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

		
		/// <summary>
		/// Gets the document used for code completion, can be overridden to provide a custom document
		/// </summary>
		/// <param name="offset"></param>
		/// <returns>The document of this text editor.</returns>
		//-------------------------------------------------------------------------------
		protected virtual IDocument GetCompletionDocument(out int offset)
        {
            offset = CaretOffset;
            return Document;
        }
    }

	internal class EBCompletionWindow : CompletionWindow
	{
		public EBCompletionWindow(TextArea textarea) : base(textarea)
		{

		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (
				(e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)) &&
				(e.KeyboardDevice.IsKeyDown(Key.Up) || e.KeyboardDevice.IsKeyDown(Key.Down) || e.KeyboardDevice.IsKeyDown(Key.Home) || e.KeyboardDevice.IsKeyDown(Key.End))
				)
			{
				this.Close();
				return;
			}
			base.OnKeyDown(e);
		}
	}
}
