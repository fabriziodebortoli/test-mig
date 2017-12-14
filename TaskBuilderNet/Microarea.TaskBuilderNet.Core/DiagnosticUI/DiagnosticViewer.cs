using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	/// <summary>
	/// DiagnosticViewer.
	/// Mostra messaggistica
	/// </summary>
	//=========================================================================
	public class DiagnosticViewer
	{
		// membri - valori di default
		private string           	message			= string.Empty;
		private string           	title			= string.Empty;
		MessageBoxButtons        	showButtons		= MessageBoxButtons.OK;
		MessageBoxIcon			 	showIcon		= MessageBoxIcon.None;
		MessageBoxDefaultButton  	defaultButton   = MessageBoxDefaultButton.Button1;
		private bool			 	errorState		= false;
		private string           	errorMessage	= string.Empty;
		private string           	errorTrace		= string.Empty;
		// proprietà
		//---------------------------------------------------------------------
		public string                  	Message		 	{ get { return message;       } set { message       = value; } }
		public string                  	Title		 	{ get { return title;         } set { title         = value; } }
		public MessageBoxButtons       	ShowButtons	 	{ get { return showButtons;   } set { showButtons   = value; } }
		public MessageBoxIcon		   	ShowIcon		{ get { return showIcon;      } set { showIcon      = value; } }
		public MessageBoxDefaultButton 	DefaultButton	{ get { return defaultButton; } set { defaultButton = value; } }
		public bool					   	ErrorState		{ get { return errorState;	   } set { errorState	 = value; } }
		public string				   	ErrorMessage	{ get { return errorMessage;  } set { errorMessage	 = value; } }
		public string				   	ErrorTrace		{ get { return errorTrace;	   } set { errorTrace	 = value; } }
		
		//---------------------------------------------------------------------
		public DiagnosticViewer()
		{
		}

		//---------------------------------------------------------------------
		public DialogResult Show(IWin32Window owner = null)
		{
			DialogResult result = MessageBox.Show(owner, Message, Title,ShowButtons,ShowIcon,DefaultButton);
			return result;
		}

		//---------------------------------------------------------------------
		static public void ShowDiagnostic(IDiagnostic diagnostic)
		{
			ShowDiagnostic(diagnostic, OrderType.Date);
		}

		//---------------------------------------------------------------------
		static public void ShowDiagnostic(IDiagnostic diagnostic, OrderType orderType)
		{
            if (diagnostic.Elements == null || diagnostic.Elements.Count == 0) return; 
			DiagnosticView notifications = new DiagnosticView(diagnostic, orderType);
			notifications.ShowDialog();

			// se in questo punto incappate nell'eccezione "DragDrop registration did not succeed"
			// e' necessario impostare nel thread dal quale state richiamando il metodo ShowDiagnostic
			// il seguente stato:
			// myThread.SetApartmentState(ApartmentState.STA); 
		}

        //---------------------------------------------------------------------
        static public void ShowDiagnostic(IDiagnostic diagnostic, OrderType orderType, Form owner)
        {
            if (diagnostic.Elements == null || diagnostic.Elements.Count == 0) return;
            DiagnosticView notifications = new DiagnosticView(diagnostic, orderType);
            notifications.StartPosition = FormStartPosition.CenterParent;
            notifications.ShowDialog(owner);
			// se in questo punto incappate nell'eccezione "DragDrop registration did not succeed"
			// e' necessario impostare nel thread dal quale state richiamando la ShowDiagnostic
			// il seguente stato:
			// myThread.SetApartmentState(ApartmentState.STA); 
        }

		//---------------------------------------------------------------------
		static public void ShowDiagnostic(IDiagnostic diagnostic, DiagnosticType diagnosticType)
        {
            if (diagnostic.Elements == null || diagnostic.Elements.Count == 0) return;
			DiagnosticView notifications = new DiagnosticView(diagnostic, diagnosticType);
			notifications.ShowDialog();

			// se in questo punto incappate nell'eccezione "DragDrop registration did not succeed"
			// e' necessario impostare nel thread dal quale state richiamando il metodo ShowDiagnostic
			// il seguente stato:
			// myThread.SetApartmentState(ApartmentState.STA); 
		}

		//---------------------------------------------------------------------
		static public void ShowDiagnostic(IDiagnostic diagnostic, DiagnosticType diagnosticType, Form owner)
        {
			if (owner.InvokeRequired)
			{
				owner.Invoke(new MethodInvoker(() => { ShowDiagnostic(diagnostic, diagnosticType, owner); }));
				return;
			}

            if (diagnostic.Elements == null || diagnostic.Elements.Count == 0) return;
			DiagnosticView notifications = new DiagnosticView(diagnostic, diagnosticType);
			notifications.ShowDialog(owner);

			// se in questo punto incappate nell'eccezione "DragDrop registration did not succeed"
			// e' necessario impostare nel thread dal quale state richiamando il metodo ShowDiagnostic
			// il seguente stato:
			// myThread.SetApartmentState(ApartmentState.STA); 
		}

		//---------------------------------------------------------------------
		static public void ShowDiagnosticAndClear(Diagnostic diagnostic)
        {
			ShowDiagnosticAndClear(diagnostic, OrderType.Date);
		}

        //---------------------------------------------------------------------
        static public void ShowDiagnosticAndClear(Diagnostic diagnostic, OrderType orderType)
        {
            if (diagnostic.Elements == null || diagnostic.Elements.Count == 0) return;
            DiagnosticView notifications = new DiagnosticView(diagnostic, orderType);
            notifications.ShowDialog();
			// se in questo punto incappate nell'eccezione "DragDrop registration did not succeed"
			// e' necessario impostare nel thread dal quale state richiamando la ShowDiagnostic
			// il seguente stato:
			// myThread.SetApartmentState(ApartmentState.STA); 

			diagnostic.Clear();
        }

        //---------------------------------------------------------------------
        static public void ShowDiagnosticAndClear(Diagnostic diagnostic, OrderType orderType, Form owner)
        {
            if (diagnostic.Elements == null || diagnostic.Elements.Count == 0) return;
            DiagnosticView notifications = new DiagnosticView(diagnostic, orderType);
            notifications.ShowDialog(owner);
			// se in questo punto incappate nell'eccezione "DragDrop registration did not succeed"
			// e' necessario impostare nel thread dal quale state richiamando la ShowDiagnostic
			// il seguente stato:
			// myThread.SetApartmentState(ApartmentState.STA); 

            diagnostic.Clear();
        }

		//---------------------------------------------------------------------
		static public void ShowDiagnosticAndClear(IDiagnostic diagnostic, DiagnosticType diagnosticType)
        {
            if (diagnostic.Elements == null || diagnostic.Elements.Count == 0) return;
			DiagnosticView notifications = new DiagnosticView(diagnostic, diagnosticType);
			notifications.ShowDialog();

			// se in questo punto incappate nell'eccezione "DragDrop registration did not succeed"
			// e' necessario impostare nel thread dal quale state richiamando il metodo ShowDiagnostic
			// il seguente stato:
			// myThread.SetApartmentState(ApartmentState.STA); 

			diagnostic.Clear();
		}

		//---------------------------------------------------------------------
		static public void ShowDiagnosticAndClear(IDiagnostic diagnostic, DiagnosticType diagnosticType, Form owner)
        {
            if (diagnostic.Elements == null || diagnostic.Elements.Count == 0) return;
			DiagnosticView notifications = new DiagnosticView(diagnostic, diagnosticType);
			notifications.ShowDialog(owner);

			// se in questo punto incappate nell'eccezione "DragDrop registration did not succeed"
			// e' necessario impostare nel thread dal quale state richiamando il metodo ShowDiagnostic
			// il seguente stato:
			// myThread.SetApartmentState(ApartmentState.STA); 

			diagnostic.Clear();
		}

 
		/// <summary>
		/// message box con il solo bottone OK e l'icona di warning
		/// </summary>
		//---------------------------------------------------------------------
		static public DialogResult ShowWarning(string warningMessage, string title, IWin32Window owner = null)
		{
			DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
			diagnosticViewer.Message	 = warningMessage;
			diagnosticViewer.Title		 = title;
			diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
			diagnosticViewer.ShowIcon    = MessageBoxIcon.Warning;
			DialogResult result = diagnosticViewer.Show(owner);
			return result;
		}

		/// <summary>
		/// message box con il solo bottone OK, l'icona di errore e le varie stringhe
		/// </summary>
		//---------------------------------------------------------------------
		static public DialogResult ShowError
			(
			string errorDescription, 
			string errorMessage, 
			string errorProcedure, 
			string errorNumber, 
			string title, 
            IWin32Window owner = null
            )
		{
			DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
			diagnosticViewer.Message	 = string.Concat(errorDescription, errorNumber, errorProcedure, errorMessage);
			diagnosticViewer.Title		 = title;
			diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
			diagnosticViewer.ShowIcon    = MessageBoxIcon.Error;
			DialogResult result	= diagnosticViewer.Show(owner);
			return result;
		}

		/// <summary>
		/// message box con il solo bottone OK, l'icona di errore
		/// </summary>
		//---------------------------------------------------------------------
		static public DialogResult ShowErrorTrace(string errorDescription, string errorTrace, string title, IWin32Window owner = null)
		{
			DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
			diagnosticViewer.Message	 = string.Concat(errorDescription, errorTrace);
			diagnosticViewer.Title		 = title;
			diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
			diagnosticViewer.ShowIcon    = MessageBoxIcon.Error;
			DialogResult result = diagnosticViewer.Show(owner);
			return result;
		}

		/// <summary>
		/// message box con i bottoni Y/N e l'icona di domanda
		/// </summary>
		//---------------------------------------------------------------------
		static public DialogResult ShowQuestion(string questionMessage, string title, IWin32Window owner = null)
		{
			DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
			diagnosticViewer.Message	 = questionMessage;
			diagnosticViewer.Title		 = title;
			diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
			diagnosticViewer.ShowIcon    = MessageBoxIcon.Question;
			DialogResult result = diagnosticViewer.Show(owner);
			return result;
		}

		/// <summary>
		/// message box con il bottone OK e l'icona di info
		/// </summary>
		//---------------------------------------------------------------------
		static public DialogResult ShowInformation(string infoMessage, string title, IWin32Window owner = null)
		{
			DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
			diagnosticViewer.Message	 = infoMessage;
			diagnosticViewer.Title		 = title;
			diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
			diagnosticViewer.ShowIcon    = MessageBoxIcon.Information;
			DialogResult result = diagnosticViewer.Show(owner);
			return result;
		}

		/// <summary>
		/// mostra un message-box con il solo bottone Ok e con la possibilità di 
		/// passare come parametro il tipo di icona da visualizzare (di default e' Error)
		/// </summary>
		//---------------------------------------------------------------------
		static public DialogResult ShowCustomizeIconMessage(string infoMessage, string title, MessageBoxIcon icon = MessageBoxIcon.Error, IWin32Window owner = null)
		{
			DiagnosticViewer diagnosticViewer = new DiagnosticViewer();
			diagnosticViewer.Message	 = infoMessage;
			diagnosticViewer.Title		 = title;
			diagnosticViewer.ShowButtons = MessageBoxButtons.OK;
			diagnosticViewer.ShowIcon    = icon;
			DialogResult result = diagnosticViewer.Show(owner);
			return result;
		}
	}
}
