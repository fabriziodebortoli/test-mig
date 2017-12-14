
using Microarea.EasyBuilder.CodeCompletion;
using Microarea.TaskBuilderNet.Core.GenericForms;

namespace Microarea.EasyBuilder.UI
{
	/// <remarks/>
	public partial class CodeViewer : ThemedForm
	{
		//-------------------------------------------------------------------------------
		/// <remarks/>
		internal CodeTextEditor CodeEditor { get { return codeEditor.TextEditor; } }

		/// <remarks/>
		public CodeViewer(string code, int line, int column)
		{
			InitializeComponent();
			codeEditor.InitializeEditor(null, "");
			CodeEditor.Document.Text = code;
			CodeEditor.IsReadOnly = true;
			MarkError(line, column);
		}

		/// <summary>
		/// Inserisce nella text area della finestra di codice un segnaposto che segnala la 
		/// posizione dell'errore individuato da riga e colonna
		/// </summary>
		/// <param name="line"></param>
		/// <param name="column"></param>
		//--------------------------------------------------------------------------------
		public void MarkError(int line, int column)
		{
			if (line < 0 || line >= codeEditor.TextEditor.Document.LineCount)
				return;

			try
			{
				codeEditor.MarkError(line, column);
			}
			catch { }
		}
	}
}
