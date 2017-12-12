using System.Collections.Specialized;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	/// <summary>
	/// DiagnosticBox
	/// Dialog per la visualizzazione dei messaggi di errore e/o di warning
	/// </summary>
	//=============================================================================
	public partial class DiagnosticBox : System.Windows.Forms.Form
	{
		//Variabili private
		//---------------------------------------------------------------------
		private StringCollection errors		= new StringCollection();
		private StringCollection warnings	= new StringCollection();


		//Proprietà Metodi
		//---------------------------------------------------------------------
		public StringCollection Errors		{ set { errors		= value; }}
		public StringCollection Warnings	{ set { warnings	= value; }}


        /// <summary>
        /// Costruttore (senza parametri)
        /// </summary>
        //---------------------------------------------------------------------
        public DiagnosticBox()
        {
            InitializeComponent();
        }
        
        /// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="diagnostic">Oggetto di Diagnostica</param>
		//---------------------------------------------------------------------
		public DiagnosticBox(Diagnostic diagnostic) : this()
		{
            if (diagnostic != null)
            {
                IDiagnosticItems errorItems = diagnostic.AllMessages(DiagnosticType.Error);
                if (errorItems != null && errorItems.Count > 0)
                {
                    foreach (DiagnosticItem error in errorItems)
                    {
                        errors.Add(error.Occurred.ToShortDateString() + "\t\r\n" + error.FullExplain);
                        if (error.ExtendedInfo != null)
                            errors.Add("\t" + error.ExtendedInfo.Format(LineSeparator.CrLf));
                    }
                }
				IDiagnosticItems warningItems = diagnostic.AllMessages(DiagnosticType.Warning);
                if (warningItems != null && warningItems.Count > 0)
                {
                    foreach (DiagnosticItem warning in warningItems)
                        warnings.Add(warning.Occurred.ToShortDateString() + "\t\r\n" + warning.FullExplain);
                }
            }
		}
        
        /// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="errors">stringCollection di errori</param>
		/// <param name="warnings">stringCollection di warning</param>
		/// <param name="titleWindow">titolo della finestra</param>
		//---------------------------------------------------------------------
		public DiagnosticBox(StringCollection errors, StringCollection warnings, string titleWindow)
            : this()
		{
			this.errors		= errors;
			this.warnings	= warnings;
			this.Text		= titleWindow;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="errors">stringCollection di errori</param>
		/// <param name="titleWindow">titolo della finestra</param>
		//---------------------------------------------------------------------
		public DiagnosticBox(StringCollection errors, string titleWindow)
            : this()
        {
			this.errors		= errors;
			this.Text		= titleWindow;
		}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="titleWindow">titolo della finestra</param>
		//---------------------------------------------------------------------
		public DiagnosticBox(string titleWindow)
            : this()
        {
			this.Text = titleWindow;
		}
		

		/// <summary>
		/// BtnOk_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// DiagnosticBox_Load
		/// Visualizzo le stringhe di Errori / Warnings
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void DiagnosticBox_Load(object sender, System.EventArgs e)
		{
			foreach(string error in errors)
				errorsTextBox.Text += error ;
			foreach (string warning in warnings)
				warningsTextBox.Text += warning ;
			if (errors.Count > 0)
				tabMessages.SelectedTab = ErrorsPage;
			else if (warnings.Count > 0)
				tabMessages.SelectedTab = WarningsPage;
		}
	}
}
