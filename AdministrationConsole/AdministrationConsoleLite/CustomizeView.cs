using System;
using System.Windows.Forms;

namespace Microarea.Console
{
	/// <summary>
	/// CustomizeView
	/// Finestra per l'impostazione dei settaggi (layout) di Console
	/// </summary>
	//=========================================================================
	public partial class CustomizeView : System.Windows.Forms.Form
	{
		private ConsoleStatus currentConsoleStatus;

		//Delegates ed Events
		//---------------------------------------------------------------------
		public delegate void EnabledConsoleTree(object sender, EnabledConsoleElementEventArgs e);
		public delegate void EnabledConsoleToolbar(object sender, EnabledConsoleElementEventArgs e);
		public delegate void EnabledConsoleStatusBar(object sender, EnabledConsoleElementEventArgs e);
		public delegate void EnabledConsoleStandardMenu(object sender, EnabledConsoleElementEventArgs e);
		public delegate void EnabledConsoleMenuPlugIn(object sender, EnabledConsoleElementEventArgs e);
		public delegate void EnabledLoginAdvancedOptions(object sender, EnabledConsoleElementEventArgs e);

		public event EnabledConsoleTree OnEnabledConsoleTree;
		public event EnabledConsoleToolbar OnEnabledConsoleToolbar;
		public event EnabledConsoleStatusBar OnEnabledConsoleStatusBar;
		public event EnabledConsoleMenuPlugIn OnEnabledConsoleMenuPlugIn;
		public event EnabledLoginAdvancedOptions OnEnabledLoginAdvancedOptions;


		/// <summary>
		/// Costruttore - Inizializza la visualizzazione delle Liste nella Console
		/// </summary>
		/// <param name="currentConsoleStatus"></param>
		//---------------------------------------------------------------------
		public CustomizeView(ConsoleStatus currentConsoleStatus)
		{
			InitializeComponent();
            this.currentConsoleStatus = currentConsoleStatus;
		}

		/// <summary>
		/// DefaultSettings
		/// Imposto i settaggi di Default (tutto a True, con la View = Details
		/// </summary>
		//---------------------------------------------------------------------
		private void DefaultSettings()
		{
			treeConsole.Checked				= currentConsoleStatus.IsVisibleConsoleTree;
			standardToolbarConsole.Checked	= currentConsoleStatus.IsVisibleStandardToolbarConsole;
			statusBarConsole.Checked		= currentConsoleStatus.IsVisibleStatusBarConsole;
			menuPlugIn.Checked				= currentConsoleStatus.IsVisibleMenuPlugIn;
			LoginOptionsCheckBox.Checked	= currentConsoleStatus.IsVisibleLoginAdvancedOptions;
			StartPosition					= FormStartPosition.CenterParent;
		}

		/// <summary>
		/// btnOk_Click
		/// Chiude la Form
		/// </summary>
		//---------------------------------------------------------------------
		private void btnOk_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Visualizzo / Nascondo il Tree
		/// </summary>
		//---------------------------------------------------------------------
		private void treeConsole_CheckedChanged(object sender, System.EventArgs e)
		{
			if (treeConsole.Checked)
				OnEnabledConsoleTree(this, new EnabledConsoleElementEventArgs(true));
			else
				OnEnabledConsoleTree(this, new EnabledConsoleElementEventArgs(false));
		}

		/// <summary>
		/// Carico i settings di Default
		/// </summary>
		//---------------------------------------------------------------------
		private void CustomizeView_Load(object sender, System.EventArgs e)
		{
			DefaultSettings();
		}
		
		/// <summary>
		/// Visualizzo / nascondo la Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		private void standardToolbarConsole_CheckedChanged(object sender, System.EventArgs e)
		{
			if (standardToolbarConsole.Checked)
				OnEnabledConsoleToolbar(this, new EnabledConsoleElementEventArgs(true));
			else
				OnEnabledConsoleToolbar(this, new EnabledConsoleElementEventArgs(false));
		}

		/// <summary>
		/// Visualizzo / nascondo la Status Bar
		/// </summary>
		//---------------------------------------------------------------------
		private void statusBarConsole_CheckedChanged(object sender, System.EventArgs e)
		{
			if (statusBarConsole.Checked)
				OnEnabledConsoleStatusBar(this, new EnabledConsoleElementEventArgs(true));
			else
				OnEnabledConsoleStatusBar(this, new EnabledConsoleElementEventArgs(false));
		}

		/// <summary>
		/// Visualizzo / nascondo le voci di Menù dei PlugIns
		/// </summary>
		//---------------------------------------------------------------------
		private void menuPlugIn_CheckedChanged(object sender, System.EventArgs e)
		{
			if (menuPlugIn.Checked)
				OnEnabledConsoleMenuPlugIn(this, new EnabledConsoleElementEventArgs(true));
			else
				OnEnabledConsoleMenuPlugIn(this, new EnabledConsoleElementEventArgs(false));
		}

		/// <summary>
		/// Visualizzo / nascondo le opzioni avanzate delle login per gli utenti associati all'azienda
		/// </summary>
		//---------------------------------------------------------------------
		private void LoginOptionsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (LoginOptionsCheckBox.Checked)
				OnEnabledLoginAdvancedOptions(this, new EnabledConsoleElementEventArgs(true));
			else
				OnEnabledLoginAdvancedOptions(this, new EnabledConsoleElementEventArgs(false));
		}
	}

	/// <summary>
	/// EnabledConsoleElementEventArgs
	/// </summary>
	//=========================================================================
	public class EnabledConsoleElementEventArgs : EventArgs
	{
		private bool isVisible;

		public  bool IsVisible { get { return isVisible; }}
		
		//---------------------------------------------------------------------
		public EnabledConsoleElementEventArgs(bool isVisible) : base()
		{
			this.isVisible = isVisible;
		}
	}
}