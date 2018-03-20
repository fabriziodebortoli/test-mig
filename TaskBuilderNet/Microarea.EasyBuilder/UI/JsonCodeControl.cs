using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Editor;
using Microarea.EasyBuilder.CodeCompletion;
using System.Diagnostics;

namespace Microarea.EasyBuilder.UI
{
	/// <summary>
	/// Internal Use
	/// </summary>
	public partial class JsonCodeControl : UserControl
	{
		//-----------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		public event EventHandler CodeChanged;
		internal bool SuspendApplyEnabling = false;
		private System.Windows.Forms.ToolStripButton tsApply;
		string lastSelectedId = "";
		//-----------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		public JsonCodeControl()
		{
			InitializeComponent();

		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Internal use
		/// </summary>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			codeEditor.CodeEditorControlMode = CodeEditorControlMode.JSONEditor;
			codeEditor.InitializeEditor(null, "");

			codeEditor.TextEditor.TextChanged += TextEditor_TextChanged;
			codeEditor.TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");

			tsApply = new System.Windows.Forms.ToolStripButton();
			tsApply.Click += tsApply_Click;
			tsApply.Name = "tsApply";
			tsApply.Text = "Apply Changes";
			codeEditor.BottomToolStrip.Items.Insert(0, tsApply);
			tsApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			tsApply.Enabled = false;
		}

		/// <summary>
		/// Internal Use
		/// </summary>
		//-----------------------------------------------------------------------------
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case (Keys.Control | Keys.S):
					{
						if (tsApply.Enabled)
							tsApply.PerformClick(); break;
					}
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
		//-----------------------------------------------------------------------------
		void TextEditor_TextChanged(object sender, EventArgs e)
		{
			if (SuspendApplyEnabling)
				return;
			tsApply.Enabled = true;
		}

		//-----------------------------------------------------------------------------
		void tsApply_Click(object sender, EventArgs e)
		{
			ApplyCodeChanges();
		}

		//-----------------------------------------------------------------------------
		private void ApplyCodeChanges()
		{
			CodeChanged?.Invoke(this, EventArgs.Empty);
			tsApply.Enabled = false;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		public string Code
		{
			get { return codeEditor.TextEditor.Text; }
			set
			{
				SuspendApplyEnabling = true;
				codeEditor.TextEditor.Text = value;
				SuspendApplyEnabling = false;
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		public void SelectCode(string id)
		{
			if (string.IsNullOrEmpty(id))
				id = lastSelectedId;
			else
				lastSelectedId = id;

			if (string.IsNullOrEmpty(id))
				return;
			TextLocation idLocation, openingBrace, closingBrace;
			if (!FindControlPosition(id, out idLocation, out openingBrace, out closingBrace))
				return;
			HighlightSelection(openingBrace, closingBrace, idLocation);
		}


		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		public bool FindControlPosition(string id, out TextLocation idLocation, out TextLocation openingBrace, out TextLocation closingBrace)
		{
			int l;
			bool result = false;
			string fragment = string.Empty;
			if (id.Contains("href : "))
				fragment = string.Concat("\"href\"\\s*:\\s*\"", id.Substring(7), "\"");
			else if (!string.IsNullOrEmpty(id))
				fragment = string.Concat("\"id\"\\s*:\\s*\"", id, "\"");
			if (!string.IsNullOrEmpty(fragment))    //lo cerco prima come id o href
				result = Find(fragment, out idLocation, out l, out openingBrace, out closingBrace);
			if (!result)                            //se non lo trovo faccio un ultimo tentativo con name
				fragment = string.Concat("\"name\"\\s*:\\s*\"", id, "\"");
			result = Find(fragment, out idLocation, out l, out openingBrace, out closingBrace);
			return result;
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal Use
		/// </summary>
		public bool Find(string pattern, out TextLocation id, out int length, out TextLocation openingBrace, out TextLocation closingBrace)
		{
			IDocument document = codeEditor.TextEditor.Document;
			openingBrace = new TextLocation();
			closingBrace = new TextLocation();
			id = new TextLocation();
			length = 0;
			//mi aspetto di trovare una graffa di apertura
			int openBracesToFind = 1;
			//mi aspetto di trovare una graffa di chiusura
			int closeBracesToFind = 1;
			try
			{
				for (int r = 1; r < document.LineCount; r++)
				{
					IDocumentLine line = document.GetLineByNumber(r);
					string sLine = document.GetText(line.Offset, line.Length);

					Match fMatch = Regex.Match(sLine, pattern);
					if (!fMatch.Success)
						continue;
					length = fMatch.Length;

					id = new TextLocation(r, fMatch.Index);

					//cerco la graffa di apertura
					for (int braceRow = r - 1; braceRow >= 0; braceRow--)
					{
						IDocumentLine s = document.GetLineByNumber(braceRow);
						sLine = document.GetText(s.Offset, s.Length);
						Match fBraceOpen = Regex.Match(sLine, "{");
						Match fBraceClose = Regex.Match(sLine, "}");
						if (!fBraceOpen.Success && !fBraceClose.Success)
							continue;
						if (fBraceOpen.Value == "{") //se trovo una graffa di apertura, devo aspettarmi una graffa di chiusura in più
							openBracesToFind--;
						if (fBraceClose.Value == "}")
							openBracesToFind++;
						if (openBracesToFind == 0)
						{
							openingBrace = new TextLocation(braceRow, fBraceOpen.Index);
							if (openingBrace.Line == 1)
							{//se la openingBrace è la prima del file, allora devo evidenziare tutto il file
								closingBrace = new TextLocation(document.LineCount, fBraceOpen.Index);
								return true;
							}
							break;
						}
					}
					//cerco la graffa di apertura
					for (int braceRow = r + 1; braceRow < document.LineCount; braceRow++)
					{
						IDocumentLine s = document.GetLineByNumber(braceRow);
						sLine = document.GetText(s.Offset, s.Length);
						Match fBrace = Regex.Match(sLine, "{|}");
						if (!fBrace.Success) continue;
						if (fBrace.Value == "}")//se trovo una graffa di chiusura, devo aspettarmi una graffa di apertura in più
							closeBracesToFind--;
						else
							closeBracesToFind++;
						if (closeBracesToFind == 0)
						{
							closingBrace = new TextLocation(braceRow, fBrace.Index);
							break;
						}
					}

					return openBracesToFind == 0 && closeBracesToFind == 0;
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}

		}
		/// <summary>
		/// Inserisce nella text area della finestra di codice un segnaposto che segnala la 
		/// posizione individuata da riga e colonna
		/// </summary>

		//--------------------------------------------------------------------------------
		public void HighlightSelection(TextLocation start, TextLocation end, TextLocation id)
		{
			IDocument document = codeEditor.TextEditor.Document;
			TextArea txtArea = codeEditor.TextEditor.TextArea;

			codeEditor.TextEditor.SyntaxHighlighting.GetNamedColor("Selection");

			txtArea.Selection = new RectangleSelection(txtArea, new TextViewPosition(start), new TextViewPosition(end));

			codeEditor.TextEditor.ScrollTo(end.Line, end.Column); //selezionando prima la graffa di chiusura, la selezione dell'id (riga succ) risulta in cima alla textArea
			codeEditor.TextEditor.ScrollTo(start.Line, start.Column);
			codeEditor.Refresh();//forzo il disegno

		}
	}
}
