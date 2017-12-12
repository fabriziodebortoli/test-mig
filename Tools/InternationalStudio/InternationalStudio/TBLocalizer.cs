using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using Microarea.Tools.TBLocalizer.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microarea.TaskBuilderNet.UI.WinControls;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Summary description for TBLocalizer.
	/// </summary>
	//================================================================================
	public class TBLocalizer : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer mainFormTimer;
		private System.Windows.Forms.ToolBar localizerToolBar;
		private System.Windows.Forms.ToolBarButton tbbNewSolution;
		private System.Windows.Forms.ImageList toolBarImageList;
		private System.Windows.Forms.ToolBarButton tbbOpenSolution;
		private System.Windows.Forms.ToolBarButton tbbCloseSolution;
		private System.Windows.Forms.ToolBarButton tbbCollapse;
		private System.Windows.Forms.ToolBarButton tbbFind;
		private System.Windows.Forms.ToolBarButton tbbTranslate;
		private System.Windows.Forms.ToolBarButton tbbViewGlossary;
		private System.Windows.Forms.ToolBarButton tbbApplyGlossary;
		private System.Windows.Forms.ToolBarButton tbbProgress;
		private System.Windows.Forms.ToolBarButton tbbSep1;
		private System.Windows.Forms.ToolBarButton tbbSep2;
		private System.Windows.Forms.ToolBarButton tbbSep3;
		private System.Windows.Forms.ToolBarButton tbbBuild;
		private System.Windows.Forms.ToolBarButton tbbCreateDictionary;

		public DictionaryCreator DictionaryCreator;

		//--------------------------------------------------------------------------------
		public TBLocalizer()
		{
			//only english at moment supported
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
			this.HandleCreated += new EventHandler(TBLocalizer_HandleCreated);

			//TODO RICCARDO SplashStarter.Start("img\\Splash.jpg", "");

			// note: this code has to stay here, not after the InitializeComponent
			// because of docking reasons (see toolbar docking)
			CreateDictionaryCreatorControl();
			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		
			CreateToolBarBindings();

			mainFormTimer.Start();
			
		}

		//--------------------------------------------------------------------------------
		void TBLocalizer_HandleCreated (object sender, EventArgs e)
		{
			SplashStarter.Finish();
		}

		//--------------------------------------------------------------------------------
		private void CreateDictionaryCreatorControl()
		{
			DictionaryCreator = new DictionaryCreator(this);
			Controls.Add(DictionaryCreator);
			DictionaryCreator.Dock = DockStyle.Fill;
			Menu = CloneMenu(DictionaryCreator.MainMenu);
			Icon = DictionaryCreator.ApplicationIcon;	
		}

		//--------------------------------------------------------------------------------
		private void CreateToolBarBindings()
		{
			tbbNewSolution.Tag = DictionaryCreator.SolutionNewMenuItem;
			tbbOpenSolution.Tag = DictionaryCreator.SolutionOpenMenuItem;
			tbbCloseSolution.Tag = DictionaryCreator.SolutionCloseMenuItem;

			tbbCollapse.Tag = DictionaryCreator.SolutionCollapseMenuItem;
			tbbApplyGlossary.Tag = DictionaryCreator.GlossaryMenuItem;
			tbbFind.Tag = DictionaryCreator.FindMenuItem;
			tbbTranslate.Tag = DictionaryCreator.TranslateMenuItem;
			tbbProgress.Tag = DictionaryCreator.ShowProgressMenuItem;
			tbbViewGlossary.Tag = DictionaryCreator.OptionsViewGlossaryMenuItem;

			tbbBuild.Tag = DictionaryCreator.BuildSolutionMenuItem;
			tbbCreateDictionary.Tag = DictionaryCreator.DictionaryExNovoMenuItem;

			foreach (ToolBarButton tbb in localizerToolBar.Buttons)
			{
				MenuItem mi = tbb.Tag as MenuItem;
				if (mi != null)
					tbb.ToolTipText = GetMenuItemDescription(mi);
			}
		}

		//--------------------------------------------------------------------------------
		private string GetMenuItemDescription(MenuItem mi)
		{
			if (mi == null) return string.Empty;

			string description = mi.Text;

			string parentDescription = GetMenuItemDescription(mi.Parent as MenuItem);
			
			if (parentDescription != string.Empty) 
				description = parentDescription + " | " + description;

			return description;
		}

		//--------------------------------------------------------------------------------
		MainMenu CloneMenu(Menu menu)
		{
			MainMenu newMenu = new MainMenu();
			newMenu.MergeMenu(menu);
			return newMenu;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//---------------------------------------------------------------------
		[STAThread]
		static int Main(string[] args) 
		{
			TBLocalizer mainForm = new TBLocalizer();
			DictionaryCreator dc = mainForm.DictionaryCreator;
			try
			{
				bool batchBuild = false;
				foreach (string originalArg in args)
				{
					string arg = originalArg.ToLower();
					if (arg.StartsWith("port="))
					{
						string port = arg.Substring(arg.IndexOf("="));
						try
						{
							dc.SocketPort = Int16.Parse(port);
						}
						catch
						{
						}
					}
					else if (arg.EndsWith(AllStrings.slnExtension) && File.Exists(arg))
					{
						Process p = GetCallingProcess();
						if (p != null)
							AttachConsole(p.Id);


                        mainForm.Show();
                        mainForm.Visible = false;
                        dc.DoBatchBuild(arg);

                        batchBuild = true;
					}
				}
				if (!batchBuild)
				{
					//dc.StartListening();
					Application.Run(mainForm);	
				}
                Console.WriteLine("Dictionaries built!");
                return 0;
			}
			catch (Exception ex)
			{
                string message = String.Format("Error building dictionaries:{0}", ex.ToString());
                if (!mainForm.DictionaryCreator.BatchBuild)
                {
                    MessageBox.Show(message);
                }
                else
                {
                    Console.WriteLine(message);
                }

                return -1;
			}
			finally
			{
				dc.PerformClosingOperations();
			}
		}
		[DllImport("kernel32.dll", SetLastError=true)]
		extern static bool AttachConsole(int dwProcessId);

		private static Process GetCallingProcess()
		{
			try
			{
				Process p = Process.GetCurrentProcess();
				int n = 1;
				string name = p.ProcessName;
				while (true)
				{
					PerformanceCounter pc = new PerformanceCounter("Process", "ID Process", name);
				
					if (pc.RawValue == p.Id)
					{
						PerformanceCounter pc1 = new PerformanceCounter("Process", "Creating Process ID", name);
						return Process.GetProcessById((int)pc1.RawValue);
					}
					name = string.Format("{0}#{1}", p.ProcessName, n++);
				}
			}
			catch
			{
				return null;
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TBLocalizer));
			this.localizerToolBar = new System.Windows.Forms.ToolBar();
			this.tbbNewSolution = new System.Windows.Forms.ToolBarButton();
			this.tbbOpenSolution = new System.Windows.Forms.ToolBarButton();
			this.tbbCloseSolution = new System.Windows.Forms.ToolBarButton();
			this.tbbSep1 = new System.Windows.Forms.ToolBarButton();
			this.tbbCollapse = new System.Windows.Forms.ToolBarButton();
			this.tbbFind = new System.Windows.Forms.ToolBarButton();
			this.tbbTranslate = new System.Windows.Forms.ToolBarButton();
			this.tbbProgress = new System.Windows.Forms.ToolBarButton();
			this.tbbSep2 = new System.Windows.Forms.ToolBarButton();
			this.tbbViewGlossary = new System.Windows.Forms.ToolBarButton();
			this.tbbApplyGlossary = new System.Windows.Forms.ToolBarButton();
			this.tbbSep3 = new System.Windows.Forms.ToolBarButton();
			this.tbbBuild = new System.Windows.Forms.ToolBarButton();
			this.tbbCreateDictionary = new System.Windows.Forms.ToolBarButton();
			this.toolBarImageList = new System.Windows.Forms.ImageList(this.components);
			this.mainFormTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// localizerToolBar
			// 
			this.localizerToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.tbbNewSolution,
            this.tbbOpenSolution,
            this.tbbCloseSolution,
            this.tbbSep1,
            this.tbbCollapse,
            this.tbbFind,
            this.tbbTranslate,
            this.tbbProgress,
            this.tbbSep2,
            this.tbbViewGlossary,
            this.tbbApplyGlossary,
            this.tbbSep3,
            this.tbbBuild,
            this.tbbCreateDictionary});
			this.localizerToolBar.ButtonSize = new System.Drawing.Size(31, 30);
			this.localizerToolBar.DropDownArrows = true;
			this.localizerToolBar.ImageList = this.toolBarImageList;
			this.localizerToolBar.Location = new System.Drawing.Point(0, 0);
			this.localizerToolBar.Name = "localizerToolBar";
			this.localizerToolBar.ShowToolTips = true;
			this.localizerToolBar.Size = new System.Drawing.Size(792, 36);
			this.localizerToolBar.TabIndex = 0;
			this.localizerToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.LocalizerToolBar_ButtonClick);
			// 
			// tbbNewSolution
			// 
			this.tbbNewSolution.ImageIndex = 0;
			this.tbbNewSolution.Name = "tbbNewSolution";
			// 
			// tbbOpenSolution
			// 
			this.tbbOpenSolution.ImageIndex = 1;
			this.tbbOpenSolution.Name = "tbbOpenSolution";
			// 
			// tbbCloseSolution
			// 
			this.tbbCloseSolution.ImageIndex = 2;
			this.tbbCloseSolution.Name = "tbbCloseSolution";
			// 
			// tbbSep1
			// 
			this.tbbSep1.Name = "tbbSep1";
			this.tbbSep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbCollapse
			// 
			this.tbbCollapse.ImageIndex = 3;
			this.tbbCollapse.Name = "tbbCollapse";
			// 
			// tbbFind
			// 
			this.tbbFind.ImageIndex = 4;
			this.tbbFind.Name = "tbbFind";
			// 
			// tbbTranslate
			// 
			this.tbbTranslate.ImageIndex = 10;
			this.tbbTranslate.Name = "tbbTranslate";
			// 
			// tbbProgress
			// 
			this.tbbProgress.ImageIndex = 8;
			this.tbbProgress.Name = "tbbProgress";
			// 
			// tbbSep2
			// 
			this.tbbSep2.Name = "tbbSep2";
			this.tbbSep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbViewGlossary
			// 
			this.tbbViewGlossary.ImageIndex = 6;
			this.tbbViewGlossary.Name = "tbbViewGlossary";
			// 
			// tbbApplyGlossary
			// 
			this.tbbApplyGlossary.ImageIndex = 7;
			this.tbbApplyGlossary.Name = "tbbApplyGlossary";
			// 
			// tbbSep3
			// 
			this.tbbSep3.Name = "tbbSep3";
			this.tbbSep3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// tbbBuild
			// 
			this.tbbBuild.ImageIndex = 11;
			this.tbbBuild.Name = "tbbBuild";
			// 
			// tbbCreateDictionary
			// 
			this.tbbCreateDictionary.ImageIndex = 12;
			this.tbbCreateDictionary.Name = "tbbCreateDictionary";
			// 
			// toolBarImageList
			// 
			this.toolBarImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("toolBarImageList.ImageStream")));
			this.toolBarImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.toolBarImageList.Images.SetKeyName(0, "");
			this.toolBarImageList.Images.SetKeyName(1, "");
			this.toolBarImageList.Images.SetKeyName(2, "");
			this.toolBarImageList.Images.SetKeyName(3, "");
			this.toolBarImageList.Images.SetKeyName(4, "");
			this.toolBarImageList.Images.SetKeyName(5, "");
			this.toolBarImageList.Images.SetKeyName(6, "");
			this.toolBarImageList.Images.SetKeyName(7, "");
			this.toolBarImageList.Images.SetKeyName(8, "");
			this.toolBarImageList.Images.SetKeyName(9, "");
			this.toolBarImageList.Images.SetKeyName(10, "");
			this.toolBarImageList.Images.SetKeyName(11, "");
			this.toolBarImageList.Images.SetKeyName(12, "");
			// 
			// mainFormTimer
			// 
			this.mainFormTimer.Tick += new System.EventHandler(this.MainFormTimer_Tick);
			// 
			// TBLocalizer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(7, 16);
			this.ClientSize = new System.Drawing.Size(792, 566);
			this.Controls.Add(this.localizerToolBar);
			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(800, 600);
			this.Name = "InternationalStudio";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "International Studio";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.TBLocalizer_Closing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void TBLocalizer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			DictionaryCreator.InvokeClosing(sender, e);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// updates toolbar button enabilitation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainFormTimer_Tick(object sender, System.EventArgs e)
		{
			foreach (ToolBarButton tbb in localizerToolBar.Buttons)
			{
				MenuItem mi = tbb.Tag as MenuItem;
				if (mi != null)
					tbb.Enabled = mi.Enabled && mi.Visible;
			}
		}

		//--------------------------------------------------------------------------------
		private void LocalizerToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			MenuItem mi = e.Button.Tag as MenuItem;
			if (mi != null)
				mi.PerformClick();
		
		}
	}
}
