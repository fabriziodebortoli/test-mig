using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.Console.Core.PlugIns
{
	///<summary>
	/// TreeView dell'Administration Console
	///</summary>
	//=====================================================================================
    public partial class PlugInsTreeView : System.Windows.Forms.TreeView
    {
		//---------------------------------------------------------------------------------
        public PlugInsTreeView()
        {
            InitializeComponent();
			InitializeImageList();
        }

		/// <summary>
		/// SetTreeViewEnabled
		/// Imposta il TreeView della Console abilitato o meno
		/// </summary>
		//---------------------------------------------------------------------
		public void SetConsoleTreeViewEnabled(bool enable)
		{
			// InvokeRequired required compares the thread ID of the calling thread to the 
			// thread ID of the creating thread. If these threads are different, it returns true.
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { SetConsoleTreeViewEnabled(enable); });
				return;
			}

			this.Enabled = enable;
		}

		# region Inizializzazione ImageList generiche e metodi correlati
		//---------------------------------------------------------------------------
		private void InitializeImageList()
		{
			base.ImageList = new ImageList();
			base.ImageList.ColorDepth = ColorDepth.Depth32Bit;
			base.ImageList.ImageSize = new Size(16, 16);
			base.ImageList.TransparentColor = Color.Magenta;

			AddBitmapFromCurrentAssemblyToNodesImageList("Default.bmp");			// 0
			AddBitmapFromCurrentAssemblyToNodesImageList("Company.bmp");			// 1
			AddBitmapFromCurrentAssemblyToNodesImageList("Companies.bmp");			// 2
			AddBitmapFromCurrentAssemblyToNodesImageList("Logins.bmp");				// 3
			AddBitmapFromCurrentAssemblyToNodesImageList("User.bmp");				// 4
			AddBitmapFromCurrentAssemblyToNodesImageList("UsersGroup.bmp");			// 5
			AddBitmapFromCurrentAssemblyToNodesImageList("Users.bmp");				// 6
			AddBitmapFromCurrentAssemblyToNodesImageList("Profiles.bmp");			// 7
			AddBitmapFromCurrentAssemblyToNodesImageList("Profile.bmp");			// 8
			AddBitmapFromCurrentAssemblyToNodesImageList("Role.bmp");				// 9
			AddBitmapFromCurrentAssemblyToNodesImageList("Roles.bmp");				// 10
			AddBitmapFromCurrentAssemblyToNodesImageList("Database.bmp");			// 11
			AddBitmapFromCurrentAssemblyToNodesImageList("DatabaseBackup.bmp");		// 12
			AddBitmapFromCurrentAssemblyToNodesImageList("DatabaseManagement.bmp");	// 13
			AddBitmapFromCurrentAssemblyToNodesImageList("SqlServer.bmp");			// 14
			AddBitmapFromCurrentAssemblyToNodesImageList("SqlServerGroup.bmp");		// 15
			AddBitmapFromCurrentAssemblyToNodesImageList("SqlUser.bmp");			// 16
			AddBitmapFromCurrentAssemblyToNodesImageList("Table.bmp");				// 17
			AddBitmapFromCurrentAssemblyToNodesImageList("View.bmp");				// 18
			AddBitmapFromCurrentAssemblyToNodesImageList("StoredProc.bmp");			// 19
			AddBitmapFromCurrentAssemblyToNodesImageList("Tools.bmp");				// 20
			AddBitmapFromCurrentAssemblyToNodesImageList("ConfigSettings.bmp");		// 21
			AddBitmapFromCurrentAssemblyToNodesImageList("Application.bmp");		// 22
			AddBitmapFromCurrentAssemblyToNodesImageList("Module.bmp");				// 23
			AddBitmapFromCurrentAssemblyToNodesImageList("Messages.bmp");           // 24
			AddBitmapFromCurrentAssemblyToNodesImageList("TableUnchecked.bmp");		// 25
			AddBitmapFromCurrentAssemblyToNodesImageList("ViewUnchecked.bmp");		// 26
			AddBitmapFromCurrentAssemblyToNodesImageList("StoredProcUnchecked.bmp");// 27
			AddBitmapFromCurrentAssemblyToNodesImageList("MagoNet16.png");			// 28
			AddBitmapFromCurrentAssemblyToNodesImageList("EasyAttachment16.png");	// 29
			AddBitmapFromAnotherAssemblyToNodesImageList("Microarea.Console.Core.DataManager.Images.", "Column.bmp"); //30
			AddBitmapFromCurrentAssemblyToNodesImageList("Information.bmp");		// 31

			base.StateImageList = new ImageList();
			base.StateImageList.ColorDepth = ColorDepth.Depth32Bit;
			base.StateImageList.ImageSize = new Size(16, 16);
			base.StateImageList.TransparentColor = Color.Magenta;

			AddBitmapFromCurrentAssemblyToStatesImageList("DummyState.bmp");		// 0
			AddBitmapFromCurrentAssemblyToStatesImageList("Search.bmp");			// 1
			AddBitmapFromCurrentAssemblyToStatesImageList("Key.bmp");				// 2
			AddBitmapFromCurrentAssemblyToStatesImageList("Lock.bmp");				// 3
			AddBitmapFromCurrentAssemblyToStatesImageList("Check.bmp");				// 4
			AddBitmapFromCurrentAssemblyToStatesImageList("Uncheck.bmp");			// 5
			AddBitmapFromCurrentAssemblyToStatesImageList("GreenSemaphore.bmp");    // 6//obsoleto
			AddBitmapFromCurrentAssemblyToStatesImageList("RedSemaphore.bmp");	    // 7
			AddBitmapFromCurrentAssemblyToStatesImageList("GreenFlag.bmp");		    // 8
			AddBitmapFromCurrentAssemblyToStatesImageList("RedFlag.bmp");		    // 9
			AddBitmapFromCurrentAssemblyToStatesImageList("ArrivalFlag.bmp");	    // 10
			AddBitmapFromCurrentAssemblyToStatesImageList("CompanyToMigrate.bmp");	// 11
			AddBitmapFromCurrentAssemblyToStatesImageList("Information.bmp");		// 12
			AddBitmapFromCurrentAssemblyToStatesImageList("LightBulb_Green.png");	// 13
			AddBitmapFromCurrentAssemblyToStatesImageList("LightBulb_Red.png");		// 14
			AddBitmapFromCurrentAssemblyToStatesImageList("ResultGreen.png");		// 15
			AddBitmapFromCurrentAssemblyToStatesImageList("ResultRed.png");			// 16
			AddBitmapFromCurrentAssemblyToStatesImageList("Error.bmp");				// 17
			AddBitmapFromCurrentAssemblyToStatesImageList("Warning.bmp");			// 18
			AddBitmapFromCurrentAssemblyToStatesImageList("EasyAttachment16.png");	// 19
            AddBitmapFromCurrentAssemblyToStatesImageList("TBSender.png");          // 20
			AddBitmapFromCurrentAssemblyToStatesImageList("DataSynchro.png");       // 21
		}

		//---------------------------------------------------------------------------
		private void AddBitmapFromCurrentAssemblyToNodesImageList(string resourceName)
		{
			if (this.ImageList == null || resourceName == null || resourceName == String.Empty)
				return;

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.PlugIns.PlugInsTreeViewImages." + resourceName);
			if (imageStream == null)
				return;

			Image image = Image.FromStream(imageStream);
			if (image == null)
				return;

			this.ImageList.Images.Add(image);
		}

		//---------------------------------------------------------------------------
		private void AddBitmapFromCurrentAssemblyToStatesImageList(string resourceName)
		{
			if (this.StateImageList == null || resourceName == null || resourceName == String.Empty)
				return;

			Stream imageStream =
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.PlugIns.PlugInsTreeViewImages." + resourceName);
			if (imageStream == null)
				return;

			Image image = Image.FromStream(imageStream);
			if (image == null)
				return;

			this.StateImageList.Images.Add(image);
		}

		//---------------------------------------------------------------------------
		private void AddBitmapFromAnotherAssemblyToNodesImageList(string assemblyName, string resourceName)
		{
			if (this.ImageList == null || resourceName == null || resourceName == String.Empty)
				return;

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(assemblyName + resourceName);
			if (imageStream == null)
				return;

			Image image = Image.FromStream(imageStream);
			if (image == null)
				return;

			this.ImageList.Images.Add(image);
		}

		# endregion

		# region ImageList
		[Browsable(false)]
        public new ImageList ImageList { get { return base.ImageList; } }
        //---------------------------------------------------------------------------
        public static int GetDefaultImageIndex { get { return 0; } }
        //---------------------------------------------------------------------------
        public static int GetCompanyDefaultImageIndex { get { return 1; } }
        //---------------------------------------------------------------------------
        public static int GetCompaniesDefaultImageIndex { get { return 2; } }
        //---------------------------------------------------------------------------
        public static int GetLoginsDefaultImageIndex { get { return 3; } }
        //---------------------------------------------------------------------------
        public static int GetUserDefaultImageIndex { get { return 4; } }
        //---------------------------------------------------------------------------
        public static int GetUsersGroupDefaultImageIndex { get { return 5; } }
        //---------------------------------------------------------------------------
        public static int GetUsersDefaultImageIndex { get { return 6; } }
        //---------------------------------------------------------------------------
        public static int GetProfilesDefaultImageIndex { get { return 7; } }
        //---------------------------------------------------------------------------
        public static int GetProfileDefaultImageIndex { get { return 8; } }
        //---------------------------------------------------------------------------
        public static int GetRoleDefaultImageIndex { get { return 9; } }
        //---------------------------------------------------------------------------
        public static int GetRolesDefaultImageIndex { get { return 10; } }
        //---------------------------------------------------------------------------
        public static int GetDatabaseDefaultImageIndex { get { return 11; } }
        //---------------------------------------------------------------------------
        public static int GetDatabaseBackupDefaultImageIndex { get { return 12; } }
        //---------------------------------------------------------------------------
        public static int GetDatabaseManagementDefaultImageIndex { get { return 13; } }
        //---------------------------------------------------------------------------
        public static int GetSqlServerDefaultImageIndex { get { return 14; } }
        //---------------------------------------------------------------------------
        public static int GetSqlServerGroupDefaultImageIndex { get { return 15; } }
        //---------------------------------------------------------------------------
        public static int GetSqlUserDefaultImageIndex { get { return 16; } }
        //---------------------------------------------------------------------------
        public static int GetTableDefaultImageIndex { get { return 17; } }
        //---------------------------------------------------------------------------
        public static int GetViewDefaultImageIndex { get { return 18; } }
        //---------------------------------------------------------------------------
        public static int GetStoredProcedureDefaultImageIndex { get { return 19; } }
        //---------------------------------------------------------------------------
        public static int GetToolsDefaultImageIndex { get { return 20; } }
        //---------------------------------------------------------------------------
        public static int GetConfigSettingsDefaultImageIndex { get { return 21; } }
        //---------------------------------------------------------------------------
        public static int GetApplicationImageIndex { get { return 22; } }
        //---------------------------------------------------------------------------
        public static int GetModuleImageIndex { get { return 23; } }
        //---------------------------------------------------------------------------
        public static int GetMessagesImageIndex { get { return 24; } }
        //---------------------------------------------------------------------------
        public static int GetTableUncheckedImageIndex { get { return 25; } }
        //---------------------------------------------------------------------------
        public static int GetViewUncheckedImageIndex { get { return 26; } }
        //---------------------------------------------------------------------------
        public static int GetStoredProcedureUncheckedImageIndex { get { return 27; } }
		//---------------------------------------------------------------------------
		public static int GetMagoNet16ImageIndex { get { return 28; } }
		//---------------------------------------------------------------------------
		public static int GetEasyAttachment16ImageIndex { get { return 29; } }
		//---------------------------------------------------------------------------
		public static int GetColumnDefaultImageIndex { get { return 30; } }
		//---------------------------------------------------------------------------
		public static int GetInformationImageIndex { get { return 31; } }

        
        [Browsable(false)]
        public new ImageList StateImageList { get { return base.StateImageList; } }
        //------------------------------------------------------------------------
        public static int GetDummyStateImageIndex { get { return 0; } }
        //------------------------------------------------------------------------
        public static int GetSearchStateImageIndex { get { return 1; } }
        //------------------------------------------------------------------------
        public static int GetKeyStateImageIndex { get { return 2; } }
        //------------------------------------------------------------------------
        public static int GetLockStateImageIndex { get { return 3; } }
        //------------------------------------------------------------------------
        public static int GetCheckStateImageIndex { get { return 4; } }
        //------------------------------------------------------------------------
        public static int GetUncheckStateImageIndex { get { return 5; } }
        //------------------------------------------------------------------------
        public static int GetGreenSemaphoreStateImageIndex { get { return 6; } }
        //------------------------------------------------------------------------
        public static int GetRedSemaphoreStateImageIndex { get { return 7; } }
        //------------------------------------------------------------------------
        public static int GetGreenFlagStateImageIndex { get { return 8; } }
        //------------------------------------------------------------------------
        public static int GetRedFlagStateImageIndex { get { return 9; } }
        //------------------------------------------------------------------------
        public static int GetArrivalFlagStateImageIndex { get { return 10; } }
        //------------------------------------------------------------------------
        public static int GetCompaniesToMigrateStateImageIndex { get { return 11; } }
        //------------------------------------------------------------------------
        public static int GetInformationStateImageIndex { get { return 12; } }
		//------------------------------------------------------------------------
		public static int GetGreenLampStateImageIndex { get { return 13; } }
		//------------------------------------------------------------------------
		public static int GetRedLampStateImageIndex { get { return 14; } }
		//------------------------------------------------------------------------
		public static int GetResultGreenStateImageIndex { get { return 15; } }
		//------------------------------------------------------------------------
		public static int GetResultRedStateImageIndex { get { return 16; } }
		//------------------------------------------------------------------------
		public static int GetErrorStateImageIndex { get { return 17; } }
		//------------------------------------------------------------------------
		public static int GeWarningStateImageIndex { get { return 18; } }
		//---------------------------------------------------------------------------
		public static int GetEasyAttachmentStateImageIndex { get { return 19; } }
		//---------------------------------------------------------------------------
		public static int GetTBSenderStateImageIndex { get { return 20; } }
		//---------------------------------------------------------------------------
		public static int GetDataSynchroStateImageIndex { get { return 21; } }
		#endregion
	}
}
