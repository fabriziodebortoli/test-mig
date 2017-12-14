using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Editor;
using Microarea.EasyBuilder.CodeCompletion;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.UI
{
	//===================================================================================
	/// <remarks/>
	public partial class CodeEditorControl : UserControl
	{
		private const int codeIndentation = 3;
		private int lastSearchIndex = 0;
		private TextArea textAreaControl;
		ITextMarkerService textMarkerService;
		CodeEditorControlMode codeEditorMode = CodeEditorControlMode.CodeEditor;


		private ICSharpCode.NRefactory.Editor.IDocument textEditorDocument;
		/// <remarks/>
		internal CodeTextEditor TextEditor { get { return textEditorControl; } }
		/// <remarks/>
		public ToolStrip BottomToolStrip { get { return bottomToolStrip; } }
		//--------------------------------------------------------------------------			
		/// <remarks/>
		public CodeEditorControl()
		{
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//--------------------------------------------------------------------------			
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				if (textAreaControl != null)
					textAreaControl.KeyDown -= TextAreaControl_KeyDown;

			}
			base.Dispose(disposing);
		}

		/// <remarks/>
		//--------------------------------------------------------------------------			
		public CodeEditorControlMode CodeEditorControlMode
		{
			get { return codeEditorMode; }
			set
			{
				this.codeEditorMode = value;
			}
		}

		//--------------------------------------------------------------------------			
		/// <remarks/>
		public void InitializeEditor(Sources sources, string fileName)
		{
			//manca il ContextMenu
			InitializeFont();

			SetAndOpenFileName(fileName);


			textEditorControl.SetSources(sources);

			textAreaControl = textEditorControl.TextArea;
			textEditorDocument = textEditorControl.Document;

			textEditorControl.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
			textAreaControl.IndentationStrategy = new CSharpIndentationStrategy();

			textEditorControl.Options.ShowTabs = false;
			textEditorControl.Options.ShowColumnRuler = false;
			textEditorControl.Options.ShowEndOfLine = false;
			textEditorControl.Options.EnableTextDragDrop = true;
			textEditorControl.Options.EnableRectangularSelection = true;
			textEditorControl.Options.ConvertTabsToSpaces = false;
			textEditorControl.AllowDrop = true;
			textAreaControl.AllowDrop = true;
			
			textAreaControl.KeyDown += TextAreaControl_KeyDown;

			textEditorControl.Options.IndentationSize = codeIndentation;		
			tsClose.Visible = codeEditorMode == CodeEditorControlMode.CodeEditor;
			ToolStripSeparator2.Visible = codeEditorMode == CodeEditorControlMode.CodeEditor;

		}

		//--------------------------------------------------------------------------			
		private void SetAndOpenFileName(string fileName)
		{
			if (fileName.IsNullOrEmpty())
				return;

			try
			{
				DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(fileName));
				if (!di.Exists)
					di.Create();

				if (!File.Exists(fileName))
				{
					using (FileStream s = File.Create(fileName))
					{ }
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			textEditorControl.OpenFile(fileName);
		}

		//--------------------------------------------------------------------------			
		internal void InitializeTextMarkerService()
		{
			var textMarkerService = new TextMarkerService(TextEditor.Document);
			TextEditor.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
			TextEditor.TextArea.TextView.LineTransformers.Add(textMarkerService);
			IServiceContainer services = (IServiceContainer)TextEditor.Document.ServiceProvider.GetService(typeof(IServiceContainer));
			if (services != null)
				services.AddService(typeof(ITextMarkerService), textMarkerService);

			this.textMarkerService = textMarkerService;
		}

		//--------------------------------------------------------------------------			
		internal void MarkError(int line, int column)
		{
			int offset = textEditorDocument.GetOffset(line, column);
			ITextMarker marker = textMarkerService.Create(offset, 1);
			marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
			marker.MarkerColor = Colors.Red;
		}

		//--------------------------------------------------------------------------			
		internal void RemoveAllErrors()
		{
			textMarkerService.RemoveAll(m => true);
		}

		//--------------------------------------------------------------------------			
		private void TextAreaControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			DoKeyDown(e);
		}

		//--------------------------------------------------------------------------			
		private void TsText_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			DoKeyDown(e);
		}

		//--------------------------------------------------------------------------			
		private void tsNextFind_Click(object sender, EventArgs e)
		{
			SearchNext();
		}

		//--------------------------------------------------------------------------			
		private void tsPreviousFind_Click(object sender, EventArgs e)
		{
			SearchPrevious();
		}

		//--------------------------------------------------------------------------			
		private void tsClose_Click(object sender, EventArgs e)
		{
			bottomToolStrip.Visible = false;
		}

		//--------------------------------------------------------------------------			
		private void tsText_TextChanged(object sender, EventArgs e)
		{
			lastSearchIndex = 0;
		}

		//--------------------------------------------------------------------------			
		private void InitializeFont()
		{
			bool settingChanged = false;

			if (Settings.Default.CodeEditorFontFamily.IsNullOrEmpty())
			{
				Settings.Default.CodeEditorFontFamily = textEditorControl.FontFamily.Source;
				settingChanged = true;
			}

			if (Settings.Default.CodeEditorFontSize == 0)
			{
				Settings.Default.CodeEditorFontSize = textEditorControl.FontSize;
				settingChanged = true;
			}

			if (settingChanged)
				Settings.Default.Save();

			textEditorControl.FontFamily = new System.Windows.Media.FontFamily(Settings.Default.CodeEditorFontFamily);
			textEditorControl.FontSize = Settings.Default.CodeEditorFontSize;
			tsIncreaseFont_Click(null, null);
			tsIncreaseFont_Click(null, null);
		}

		//--------------------------------------------------------------------------------
		private void tsChangeFont_Click(object sender, EventArgs e)
		{
			FontDialog fd = new FontDialog();
			fd.Font = new Font(textEditorControl.FontFamily.Source, (float)textEditorControl.FontSize);
			DialogResult res = fd.ShowDialog();
			if (res != DialogResult.OK)
				return;

			textEditorControl.FontFamily = new System.Windows.Media.FontFamily(fd.Font.FontFamily.Name);
			textEditorControl.FontSize = fd.Font.Size;

			Settings.Default.CodeEditorFontFamily = fd.Font.FontFamily.Name;
			Settings.Default.CodeEditorFontSize = fd.Font.Size;
			Settings.Default.Save();
		}

		//--------------------------------------------------------------------------------
		private void tsIncreaseFont_Click(object sender, EventArgs e)
		{
			double oldFont = textEditorControl.FontSize;
			oldFont = oldFont + 1;
			try
			{
				textEditorControl.FontSize = oldFont;
			}
			catch { }
		}

		//--------------------------------------------------------------------------------
		private void tsDecreseFont_Click(object sender, EventArgs e)
		{
			double oldFont = textEditorControl.FontSize;
			oldFont = oldFont - 1;
			try
			{
				textEditorControl.FontSize = oldFont;
			}
			catch { }
		}

		//--------------------------------------------------------------------------			
		private void DoKeyDown(KeyEventArgs e)
		{
			if (e.Control && e.KeyValue == (int)Keys.F) //Ctrl+F
			{
				bottomToolStrip.Visible = true;
				tsText.Text = string.Empty;
				tsText.Focus();
			}

			if (e.KeyValue == 13) //Enter
				SearchNext();

			if (e.Control && e.KeyValue == (int)Keys.F3) //Ctrl+F3
			{
				tsText.Text = TextEditor.SelectedText;
				if (textAreaControl.Selection != null)
					lastSearchIndex = textEditorDocument.GetOffset(textAreaControl.Selection.EndPosition.Line, textAreaControl.Selection.EndPosition.Column);
			}

			if (!e.Control && !e.Shift && e.KeyValue == (int)Keys.F3) //F3 Search Forward
				SearchNext();

			if (e.Shift && e.KeyValue == (int)Keys.F3) //Shift+F3 Search Backward
				SearchPrevious();
		}

		//--------------------------------------------------------------------------			
		private void DoKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			if ((e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.F)) //Ctrl+F
			{
				bottomToolStrip.Visible = true;
				tsText.Text = string.Empty;
				tsText.Focus();
			}

			if (e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.Enter)) //Enter
				SearchNext();

			if ((e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightCtrl)) && e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.F3)) //Ctrl+F3
			{
				tsText.Text = TextEditor.SelectedText;
				if (textAreaControl.Selection != null)
					lastSearchIndex = textEditorDocument.GetOffset(textAreaControl.Selection.EndPosition.Line, textAreaControl.Selection.EndPosition.Column);
			}

			if (
				!(e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightCtrl)) &&
				!(e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftShift) || e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightShift)) &&
				e.Key == System.Windows.Input.Key.F3
				)
				SearchNext(); //F3 Search Forward

			if ((e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftShift) || e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightShift)) && e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.F3)) //Shift+F3 Search Backward
				SearchPrevious();
		}

		//--------------------------------------------------------------------------			
		private void SearchNext()
		{
			lastSearchIndex = GetForwardIndex();
			HighlightSelectedText();
		}

		//--------------------------------------------------------------------------			
		private void SearchPrevious()
		{
			lastSearchIndex = GetBackwardIndex();
			HighlightSelectedText();
		}

		//--------------------------------------------------------------------------			
		private void HighlightSelectedText()
		{
			if (lastSearchIndex < 0)
			{
				tslblMatched.Text = Resources.NoMatchesFound;
				return;
			}

			tslblMatched.Text = string.Empty;


			TextLocation p1 = textEditorDocument.GetLocation(lastSearchIndex);
			TextLocation p2 = textEditorDocument.GetLocation(lastSearchIndex + tsText.Text.Length);
			//textAreaControl.Selection.StartSelectionOrSetEndpoint(new TextViewPosition(p1), new TextViewPosition(p2));//textAreaControl.SelectionManager.SetSelection(p1, p2);
			textAreaControl.Selection = new RectangleSelection(textAreaControl, new TextViewPosition(p1), new TextViewPosition(p2));
			TextEditor.ScrollTo(p1.Column, p1.Line);
		}

		//--------------------------------------------------------------------------			
		private int GetBackwardIndex()
		{
			if (lastSearchIndex < 0)
				return lastSearchIndex;

			//Se è minore di zero o ho fatto il giro completo e non ho trovato niente o sto ricominciando il giro
			int tempIndex = textEditorDocument.Text.ToLower().LastIndexOf(tsText.Text.ToLower(), lastSearchIndex);
			if (tempIndex < 0)
				tempIndex = textEditorDocument.Text.ToLower().LastIndexOf(tsText.Text.ToLower());

			//alla fine torno tempIndex se è valido (>= 0), o 0 altrimenti
			return tempIndex;
		}

		//--------------------------------------------------------------------------			
		private int GetForwardIndex()
		{
			//Se è minore di zero o ho fatto il giro completo e non ho trovato niente o sto ricominciando il giro
			int tempIndex = textEditorDocument.Text.ToLower().IndexOf(tsText.Text.ToLower(), lastSearchIndex + tsText.Text.Length);
			if (tempIndex < 0)
				tempIndex = textEditorDocument.Text.ToLower().IndexOf(tsText.Text.ToLower());

			//alla fine torno tempIndex se è valido (>= 0), o 0 altrimenti
			return tempIndex;
		}

		/// <remarks/>
		//--------------------------------------------------------------------------------
		internal void CommentCode()
		{
			if (TextEditor.TextArea.Selection.Length == 0)
			{
				CommentLine(textAreaControl.Caret.Line, textAreaControl.Caret.Line);
				return;
			}

			CommentLine(TextEditor.TextArea.Selection.StartPosition.Line, TextEditor.TextArea.Selection.EndPosition.Line);
		}

		/// <remarks/>
		//--------------------------------------------------------------------------------
		internal void UnCommentCode()
		{
			ICSharpCode.AvalonEdit.Editing.Selection sm = textAreaControl.Selection; //textAreaControl.SelectionManager;
			if (TextEditor.TextArea.Selection.Length == 0) //sm.SelectionCollection.Count == 0)
			{
				UnCommentLine(textAreaControl.Caret.Line, textAreaControl.Caret.Line);
				return;
			}

			UnCommentLine(TextEditor.TextArea.Selection.StartPosition.Line, TextEditor.TextArea.Selection.EndPosition.Line);
		}

		//--------------------------------------------------------------------------------
		private void UnCommentLine(int startLine, int endLine)
		{
			string commentChar = GetCommentString();
			while (startLine <= endLine)
			{
				IDocumentLine ls = textEditorDocument.GetLineByNumber(startLine);
				string line = textEditorDocument.GetText(ls.Offset, ls.Length);
				int index = line.IndexOfNoCase(commentChar);
				if (index < 0)
				{
					startLine++;
					continue;
				}

				line = line.Remove(index, commentChar.Length);

				textEditorDocument.Replace(ls.Offset, line.Length + commentChar.Length, line);
				startLine++;
			}
		}

		//--------------------------------------------------------------------------------
		private void CommentLine(int startLine, int endLine)
		{
			string commentChar = GetCommentString();
			while (startLine <= endLine)
			{
				IDocumentLine line = textEditorDocument.GetLineByNumber(startLine);
				textEditorDocument.Insert(line.Offset, commentChar);
				startLine++;
			}
		}

		//--------------------------------------------------------------------------------
		private string GetCommentString()
		{
			return "//";
		}
	}

	/// <remarks/>
	//===================================================================================
	public enum CodeEditorControlMode
	{
		/// <remarks/>
		CodeEditor,
		/// <remarks/>
		JSONEditor 
	}
}
