using System;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Microarea.TaskBuilderNet.UI.CodeEditor.UserControls
{
	//==================================================================================
	public partial class WoormEditorControl : UserControl
	{
		public TextEditor TextEditor { get { return textEditorControl; } }

		//-------------------------------------------------------------------------------
		public ImageList IconList { get { return iconList; } set { iconList = value;  } }
		//-------------------------------------------------------------------------------
		public WoormEditorControl()
		{
			InitializeComponent();

			InitializeEditorStyle();
			this.textEditorControl.TextArea.KeyDown += TextArea_KeyDown;
			WoormToolTipManager.Attach(this);
		}

		private void TextArea_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			//this.textEditorControl.TextArea.ExecuteDialogKey(e.KeyData);
		}

		//-------------------------------------------------------------------------------
		private void InitializeEditorStyle()
		{
			textEditorControl.FontFamily = new System.Windows.Media.FontFamily("Open Sans");
			textEditorControl.FontSize = 14;

			textEditorControl.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
			textEditorControl.Options.ShowTabs = false;
			textEditorControl.Options.ShowColumnRuler = false;
			textEditorControl.Options.ShowEndOfLine = false;
			textEditorControl.Options.EnableTextDragDrop = true;
			textEditorControl.Options.EnableRectangularSelection = true;
			textEditorControl.Options.ConvertTabsToSpaces = false;
			textEditorControl.AllowDrop = true;
			textEditorControl.TextArea.AllowDrop = true;
		}

		//-------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//Lancia su un altro thread un loop di controllo del codice a scopo di controllo sintassi
			/*parserThread = new Thread(CodeAnalyzer);
			parserThread.IsBackground = true;
			parserThread.Start();*/
		}
	}
}
