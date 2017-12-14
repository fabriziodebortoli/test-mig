using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// RowViewer.
	/// </summary>
	//=========================================================================
	public class RowViewer : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private Button		BtnOk;
		private Button		BtnCancel;
		private TextBox		TxtBase;
		private TextBox		TxtTarget;
		private Label		LblBase;
		private Label		LblTarget;

		private bool		isFile			= false;
		private string		baseString		= String.Empty;
		private string		targetString	= String.Empty;
		private System.Windows.Forms.ToolBar tbNavigator;
		private System.Windows.Forms.ToolBarButton tbPrevious;
		private System.Windows.Forms.ImageList ilRowViewer;
		private System.Windows.Forms.ToolBarButton tbNext;
		private System.Windows.Forms.CheckBox CkbTemporary;
		private string		rootPath		= String.Empty;
		private bool		temporary		= false;
		private string		targetFile		= null;

		//---------------------------------------------------------------------
		public string		Base		{get {return baseString;}	set {baseString = value;}}
		public string		Target		{get {return targetString;}	set {targetString = value;}}
		public bool			IsFile		{get {return isFile;}		set {isFile = value;}}
		public string		RootPath	{get {return rootPath;}		set {rootPath = value;}}
		public bool			Temporary	{get {return temporary;}		set {temporary = value;}}
		public Translator	OwnerTranslator	{get {return Owner as Translator;} }

		//---------------------------------------------------------------------
		public RowViewer(Translator translator, string workingFolder)
		{
			InitializeComponent();
			
			this.Owner = translator;
			RootPath = workingFolder;

			RefreshDialogData();
		}

		//---------------------------------------------------------------------
		private void RefreshDialogData()
		{
			try
			{

				Translator translator = OwnerTranslator;

				DataGridCell cell = translator.DgTranslator.CurrentCell;
				Base = translator.ContextMenuSupport.MenuItems[ToolbarButtonIndexer.Support].Checked//RbSupport.Checked 
					? translator.DgTranslator[cell.RowNumber, (int)Translator.Columns.SUPPORT]	as string
					: translator.DgTranslator[cell.RowNumber, (int)Translator.Columns.BASE]		as string;

				Target = translator.DgTranslator[cell.RowNumber, (int)Translator.Columns.TARGET] as string;

				Temporary = translator.DgTranslator[cell.RowNumber, (int)Translator.Columns.TEMPORARY]  is DBNull
					? false
					: ((bool) translator.DgTranslator[cell.RowNumber, (int)Translator.Columns.TEMPORARY]);
				IsFile = translator.DgTranslator[cell.RowNumber, (int)Translator.Columns.FILE] is DBNull 
					? false 
					: translator.DgTranslator[cell.RowNumber, (int)Translator.Columns.FILE] as string == AllStrings.trueTag; 

				if (IsFile)
				{
					targetFile = Path.Combine(RootPath, AllStrings.report);
					targetFile = Path.Combine(targetFile, Target);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
			}
		}
		
		//---------------------------------------------------------------------
		private void RefreshDialogView()
		{
			EnableNavigationButtons();

			if (IsFile)
			{
				try
				{
					string originalFile = CommonFunctions.MapFile(Base);
					if (File.Exists (originalFile))
					{
						using(StreamReader aFile = new StreamReader(originalFile, Encoding.GetEncoding(0)))
						{
							TxtBase.Text = aFile.ReadToEnd();
						}
					}
					else
					{
						TxtBase.Text = String.Format(Strings.FileNotValid, originalFile);	
					}

					
					if (File.Exists(targetFile))
					{
						using(StreamReader aFile = new StreamReader(targetFile, Encoding.GetEncoding(0), true))
						{
							TxtTarget.Text = aFile.ReadToEnd();
						}
					}
					else
					{
						TxtTarget.Text = String.Format(Strings.FileNotValid, targetFile);	
						BtnOk.Enabled = false;
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(DictionaryCreator.ActiveForm, ex.Message, Strings.WarningCaption);
					BtnOk.Enabled = false;
				}
			}
			else
			{
				// per rimpiazzare eventuali \n senza \r
				string baseString = Base.Replace("\r\n", "\n");
				baseString = baseString.Replace("\n", "\r\n");
				
				TxtBase.Text	= baseString;
				TxtTarget.Text	= Target;
				CkbTemporary.Checked = Temporary;
			}

			TxtTarget.Focus();
		}

		//---------------------------------------------------------------------
		private bool ApplyChanges()
		{
			try
			{
				if (IsFile) 
				{
					try
					{
						using(StreamWriter aFile = new StreamWriter(targetFile, false, System.Text.Encoding.Unicode))
						{
							aFile.Write(TxtTarget.Text);
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message, Strings.WarningCaption);
					}
				}
				else
				{
					//controllo la correttezza della stringa, solo se targte è diverso da stringa vuota
					if (TxtTarget.Text != String.Empty)
					{
						PlaceHolderValidity phv = CommonFunctions.IsPlaceHolderValid(TxtBase.Text, TxtTarget.Text, ((Translator)Owner).GetParametersMode(), false);
						if (!phv.TranslationValid)
						{
							MessageBox.Show(this, Strings.TranslationNotValid, Strings.WarningCaption);
							TxtTarget.Focus();
							return false;
						}
					}
					Target = TxtTarget.Text;
					Temporary = CkbTemporary.Checked;
				}
			
				DataGridCell cell = OwnerTranslator.DgTranslator.CurrentCell;
			
				string oldTarget = OwnerTranslator.DgTranslator[cell.RowNumber, (int)Translator.Columns.TARGET] as string;
			
				bool  oldTemporary = OwnerTranslator.DgTranslator[cell.RowNumber, (int)Translator.Columns.TEMPORARY] is DBNull
					? false
					: ((bool)OwnerTranslator.DgTranslator[cell.RowNumber, (int)Translator.Columns.TEMPORARY]);
			
				if (oldTarget == Target && oldTemporary == Temporary)
					return true;

				OwnerTranslator.DgTranslator[cell.RowNumber, (int)Translator.Columns.TARGET] = Target;
				OwnerTranslator.DgTranslator[cell.RowNumber, (int)Translator.Columns.TEMPORARY] = Temporary;
				try	
				{
					OwnerTranslator.IgnoreRowChangeEvents = true;
					OwnerTranslator.StringDataSet.AcceptChanges();
					OwnerTranslator.HasChanges = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show(this, ex.Message, Strings.WarningCaption);
					return false;
				}
				finally
				{
					OwnerTranslator.IgnoreRowChangeEvents = false;				
				}

				return true;
			}
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			if (ApplyChanges())
			{
				DialogResult = DialogResult.OK;
				Close ();
			}		
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close ();
		}
		
		//---------------------------------------------------------------------
		private void tbNavigator_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if (e.Button.ImageIndex == 0)
				OnPrevious();

			if (e.Button.ImageIndex == 1)
				OnNext();

		}
		
		//---------------------------------------------------------------------
		private void OnPrevious()
		{
			if (!ApplyChanges())
				return;

			OwnerTranslator.DgTranslator.CurrentRowIndex = OwnerTranslator.DgTranslator.CurrentRowIndex - 1;

			RefreshDialogData();
			RefreshDialogView();
		}

		//---------------------------------------------------------------------
		private void OnNext()
		{
			if (!ApplyChanges())
				return;

			OwnerTranslator.DgTranslator.CurrentRowIndex = OwnerTranslator.DgTranslator.CurrentRowIndex + 1;

			RefreshDialogData();
			RefreshDialogView();
		}

		//---------------------------------------------------------------------
		private void RowViewer_Load(object sender, System.EventArgs e)
		{
			RefreshDialogView();
		}

		//---------------------------------------------------------------------
		private void EnableNavigationButtons()
		{
			DataGrid dg = OwnerTranslator.DgTranslator;
			int numRows = dg.BindingContext[dg.DataSource, dg.DataMember].Count;

			tbPrevious.Enabled = (dg.CurrentRowIndex > 0);
			tbNext.Enabled = (dg.CurrentRowIndex < numRows - 1);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RowViewer));
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.TxtBase = new System.Windows.Forms.TextBox();
			this.TxtTarget = new System.Windows.Forms.TextBox();
			this.LblBase = new System.Windows.Forms.Label();
			this.LblTarget = new System.Windows.Forms.Label();
			this.tbNavigator = new System.Windows.Forms.ToolBar();
			this.tbPrevious = new System.Windows.Forms.ToolBarButton();
			this.tbNext = new System.Windows.Forms.ToolBarButton();
			this.ilRowViewer = new System.Windows.Forms.ImageList(this.components);
			this.CkbTemporary = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// BtnOk
			// 
			this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
			this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
			this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
			this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
			this.BtnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOk.Dock")));
			this.BtnOk.Enabled = ((bool)(resources.GetObject("BtnOk.Enabled")));
			this.BtnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOk.FlatStyle")));
			this.BtnOk.Font = ((System.Drawing.Font)(resources.GetObject("BtnOk.Font")));
			this.BtnOk.Image = ((System.Drawing.Image)(resources.GetObject("BtnOk.Image")));
			this.BtnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.ImageAlign")));
			this.BtnOk.ImageIndex = ((int)(resources.GetObject("BtnOk.ImageIndex")));
			this.BtnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOk.ImeMode")));
			this.BtnOk.Location = ((System.Drawing.Point)(resources.GetObject("BtnOk.Location")));
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOk.RightToLeft")));
			this.BtnOk.Size = ((System.Drawing.Size)(resources.GetObject("BtnOk.Size")));
			this.BtnOk.TabIndex = ((int)(resources.GetObject("BtnOk.TabIndex")));
			this.BtnOk.Text = resources.GetString("BtnOk.Text");
			this.BtnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.TextAlign")));
			this.BtnOk.Visible = ((bool)(resources.GetObject("BtnOk.Visible")));
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
			this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
			this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
			this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
			this.BtnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCancel.Dock")));
			this.BtnCancel.Enabled = ((bool)(resources.GetObject("BtnCancel.Enabled")));
			this.BtnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCancel.FlatStyle")));
			this.BtnCancel.Font = ((System.Drawing.Font)(resources.GetObject("BtnCancel.Font")));
			this.BtnCancel.Image = ((System.Drawing.Image)(resources.GetObject("BtnCancel.Image")));
			this.BtnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.ImageAlign")));
			this.BtnCancel.ImageIndex = ((int)(resources.GetObject("BtnCancel.ImageIndex")));
			this.BtnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCancel.ImeMode")));
			this.BtnCancel.Location = ((System.Drawing.Point)(resources.GetObject("BtnCancel.Location")));
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCancel.RightToLeft")));
			this.BtnCancel.Size = ((System.Drawing.Size)(resources.GetObject("BtnCancel.Size")));
			this.BtnCancel.TabIndex = ((int)(resources.GetObject("BtnCancel.TabIndex")));
			this.BtnCancel.Text = resources.GetString("BtnCancel.Text");
			this.BtnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.TextAlign")));
			this.BtnCancel.Visible = ((bool)(resources.GetObject("BtnCancel.Visible")));
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// TxtBase
			// 
			this.TxtBase.AcceptsReturn = true;
			this.TxtBase.AcceptsTab = true;
			this.TxtBase.AccessibleDescription = resources.GetString("TxtBase.AccessibleDescription");
			this.TxtBase.AccessibleName = resources.GetString("TxtBase.AccessibleName");
			this.TxtBase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtBase.Anchor")));
			this.TxtBase.AutoSize = ((bool)(resources.GetObject("TxtBase.AutoSize")));
			this.TxtBase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtBase.BackgroundImage")));
			this.TxtBase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtBase.Dock")));
			this.TxtBase.Enabled = ((bool)(resources.GetObject("TxtBase.Enabled")));
			this.TxtBase.Font = ((System.Drawing.Font)(resources.GetObject("TxtBase.Font")));
			this.TxtBase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtBase.ImeMode")));
			this.TxtBase.Location = ((System.Drawing.Point)(resources.GetObject("TxtBase.Location")));
			this.TxtBase.MaxLength = ((int)(resources.GetObject("TxtBase.MaxLength")));
			this.TxtBase.Multiline = ((bool)(resources.GetObject("TxtBase.Multiline")));
			this.TxtBase.Name = "TxtBase";
			this.TxtBase.PasswordChar = ((char)(resources.GetObject("TxtBase.PasswordChar")));
			this.TxtBase.ReadOnly = true;
			this.TxtBase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtBase.RightToLeft")));
			this.TxtBase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtBase.ScrollBars")));
			this.TxtBase.Size = ((System.Drawing.Size)(resources.GetObject("TxtBase.Size")));
			this.TxtBase.TabIndex = ((int)(resources.GetObject("TxtBase.TabIndex")));
			this.TxtBase.Text = resources.GetString("TxtBase.Text");
			this.TxtBase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtBase.TextAlign")));
			this.TxtBase.Visible = ((bool)(resources.GetObject("TxtBase.Visible")));
			this.TxtBase.WordWrap = ((bool)(resources.GetObject("TxtBase.WordWrap")));
			// 
			// TxtTarget
			// 
			this.TxtTarget.AcceptsReturn = true;
			this.TxtTarget.AcceptsTab = true;
			this.TxtTarget.AccessibleDescription = resources.GetString("TxtTarget.AccessibleDescription");
			this.TxtTarget.AccessibleName = resources.GetString("TxtTarget.AccessibleName");
			this.TxtTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtTarget.Anchor")));
			this.TxtTarget.AutoSize = ((bool)(resources.GetObject("TxtTarget.AutoSize")));
			this.TxtTarget.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtTarget.BackgroundImage")));
			this.TxtTarget.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtTarget.Dock")));
			this.TxtTarget.Enabled = ((bool)(resources.GetObject("TxtTarget.Enabled")));
			this.TxtTarget.Font = ((System.Drawing.Font)(resources.GetObject("TxtTarget.Font")));
			this.TxtTarget.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtTarget.ImeMode")));
			this.TxtTarget.Location = ((System.Drawing.Point)(resources.GetObject("TxtTarget.Location")));
			this.TxtTarget.MaxLength = ((int)(resources.GetObject("TxtTarget.MaxLength")));
			this.TxtTarget.Multiline = ((bool)(resources.GetObject("TxtTarget.Multiline")));
			this.TxtTarget.Name = "TxtTarget";
			this.TxtTarget.PasswordChar = ((char)(resources.GetObject("TxtTarget.PasswordChar")));
			this.TxtTarget.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtTarget.RightToLeft")));
			this.TxtTarget.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtTarget.ScrollBars")));
			this.TxtTarget.Size = ((System.Drawing.Size)(resources.GetObject("TxtTarget.Size")));
			this.TxtTarget.TabIndex = ((int)(resources.GetObject("TxtTarget.TabIndex")));
			this.TxtTarget.Text = resources.GetString("TxtTarget.Text");
			this.TxtTarget.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtTarget.TextAlign")));
			this.TxtTarget.Visible = ((bool)(resources.GetObject("TxtTarget.Visible")));
			this.TxtTarget.WordWrap = ((bool)(resources.GetObject("TxtTarget.WordWrap")));
			// 
			// LblBase
			// 
			this.LblBase.AccessibleDescription = resources.GetString("LblBase.AccessibleDescription");
			this.LblBase.AccessibleName = resources.GetString("LblBase.AccessibleName");
			this.LblBase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblBase.Anchor")));
			this.LblBase.AutoSize = ((bool)(resources.GetObject("LblBase.AutoSize")));
			this.LblBase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblBase.Dock")));
			this.LblBase.Enabled = ((bool)(resources.GetObject("LblBase.Enabled")));
			this.LblBase.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblBase.Font = ((System.Drawing.Font)(resources.GetObject("LblBase.Font")));
			this.LblBase.Image = ((System.Drawing.Image)(resources.GetObject("LblBase.Image")));
			this.LblBase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblBase.ImageAlign")));
			this.LblBase.ImageIndex = ((int)(resources.GetObject("LblBase.ImageIndex")));
			this.LblBase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblBase.ImeMode")));
			this.LblBase.Location = ((System.Drawing.Point)(resources.GetObject("LblBase.Location")));
			this.LblBase.Name = "LblBase";
			this.LblBase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblBase.RightToLeft")));
			this.LblBase.Size = ((System.Drawing.Size)(resources.GetObject("LblBase.Size")));
			this.LblBase.TabIndex = ((int)(resources.GetObject("LblBase.TabIndex")));
			this.LblBase.Text = resources.GetString("LblBase.Text");
			this.LblBase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblBase.TextAlign")));
			this.LblBase.Visible = ((bool)(resources.GetObject("LblBase.Visible")));
			// 
			// LblTarget
			// 
			this.LblTarget.AccessibleDescription = resources.GetString("LblTarget.AccessibleDescription");
			this.LblTarget.AccessibleName = resources.GetString("LblTarget.AccessibleName");
			this.LblTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblTarget.Anchor")));
			this.LblTarget.AutoSize = ((bool)(resources.GetObject("LblTarget.AutoSize")));
			this.LblTarget.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblTarget.Dock")));
			this.LblTarget.Enabled = ((bool)(resources.GetObject("LblTarget.Enabled")));
			this.LblTarget.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblTarget.Font = ((System.Drawing.Font)(resources.GetObject("LblTarget.Font")));
			this.LblTarget.Image = ((System.Drawing.Image)(resources.GetObject("LblTarget.Image")));
			this.LblTarget.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTarget.ImageAlign")));
			this.LblTarget.ImageIndex = ((int)(resources.GetObject("LblTarget.ImageIndex")));
			this.LblTarget.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblTarget.ImeMode")));
			this.LblTarget.Location = ((System.Drawing.Point)(resources.GetObject("LblTarget.Location")));
			this.LblTarget.Name = "LblTarget";
			this.LblTarget.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblTarget.RightToLeft")));
			this.LblTarget.Size = ((System.Drawing.Size)(resources.GetObject("LblTarget.Size")));
			this.LblTarget.TabIndex = ((int)(resources.GetObject("LblTarget.TabIndex")));
			this.LblTarget.Text = resources.GetString("LblTarget.Text");
			this.LblTarget.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTarget.TextAlign")));
			this.LblTarget.Visible = ((bool)(resources.GetObject("LblTarget.Visible")));
			// 
			// tbNavigator
			// 
			this.tbNavigator.AccessibleDescription = resources.GetString("tbNavigator.AccessibleDescription");
			this.tbNavigator.AccessibleName = resources.GetString("tbNavigator.AccessibleName");
			this.tbNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tbNavigator.Anchor")));
			this.tbNavigator.Appearance = ((System.Windows.Forms.ToolBarAppearance)(resources.GetObject("tbNavigator.Appearance")));
			this.tbNavigator.AutoSize = ((bool)(resources.GetObject("tbNavigator.AutoSize")));
			this.tbNavigator.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tbNavigator.BackgroundImage")));
			this.tbNavigator.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						   this.tbPrevious,
																						   this.tbNext});
			this.tbNavigator.ButtonSize = ((System.Drawing.Size)(resources.GetObject("tbNavigator.ButtonSize")));
			this.tbNavigator.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tbNavigator.Dock")));
			this.tbNavigator.DropDownArrows = ((bool)(resources.GetObject("tbNavigator.DropDownArrows")));
			this.tbNavigator.Enabled = ((bool)(resources.GetObject("tbNavigator.Enabled")));
			this.tbNavigator.Font = ((System.Drawing.Font)(resources.GetObject("tbNavigator.Font")));
			this.tbNavigator.ImageList = this.ilRowViewer;
			this.tbNavigator.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tbNavigator.ImeMode")));
			this.tbNavigator.Location = ((System.Drawing.Point)(resources.GetObject("tbNavigator.Location")));
			this.tbNavigator.Name = "tbNavigator";
			this.tbNavigator.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tbNavigator.RightToLeft")));
			this.tbNavigator.ShowToolTips = ((bool)(resources.GetObject("tbNavigator.ShowToolTips")));
			this.tbNavigator.Size = ((System.Drawing.Size)(resources.GetObject("tbNavigator.Size")));
			this.tbNavigator.TabIndex = ((int)(resources.GetObject("tbNavigator.TabIndex")));
			this.tbNavigator.TextAlign = ((System.Windows.Forms.ToolBarTextAlign)(resources.GetObject("tbNavigator.TextAlign")));
			this.tbNavigator.Visible = ((bool)(resources.GetObject("tbNavigator.Visible")));
			this.tbNavigator.Wrappable = ((bool)(resources.GetObject("tbNavigator.Wrappable")));
			this.tbNavigator.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbNavigator_ButtonClick);
			// 
			// tbPrevious
			// 
			this.tbPrevious.Enabled = ((bool)(resources.GetObject("tbPrevious.Enabled")));
			this.tbPrevious.ImageIndex = ((int)(resources.GetObject("tbPrevious.ImageIndex")));
			this.tbPrevious.Text = resources.GetString("tbPrevious.Text");
			this.tbPrevious.ToolTipText = resources.GetString("tbPrevious.ToolTipText");
			this.tbPrevious.Visible = ((bool)(resources.GetObject("tbPrevious.Visible")));
			// 
			// tbNext
			// 
			this.tbNext.Enabled = ((bool)(resources.GetObject("tbNext.Enabled")));
			this.tbNext.ImageIndex = ((int)(resources.GetObject("tbNext.ImageIndex")));
			this.tbNext.Text = resources.GetString("tbNext.Text");
			this.tbNext.ToolTipText = resources.GetString("tbNext.ToolTipText");
			this.tbNext.Visible = ((bool)(resources.GetObject("tbNext.Visible")));
			// 
			// ilRowViewer
			// 
			this.ilRowViewer.ImageSize = ((System.Drawing.Size)(resources.GetObject("ilRowViewer.ImageSize")));
			this.ilRowViewer.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilRowViewer.ImageStream")));
			this.ilRowViewer.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// CkbTemporary
			// 
			this.CkbTemporary.AccessibleDescription = resources.GetString("CkbTemporary.AccessibleDescription");
			this.CkbTemporary.AccessibleName = resources.GetString("CkbTemporary.AccessibleName");
			this.CkbTemporary.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbTemporary.Anchor")));
			this.CkbTemporary.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbTemporary.Appearance")));
			this.CkbTemporary.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbTemporary.BackgroundImage")));
			this.CkbTemporary.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbTemporary.CheckAlign")));
			this.CkbTemporary.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbTemporary.Dock")));
			this.CkbTemporary.Enabled = ((bool)(resources.GetObject("CkbTemporary.Enabled")));
			this.CkbTemporary.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbTemporary.FlatStyle")));
			this.CkbTemporary.Font = ((System.Drawing.Font)(resources.GetObject("CkbTemporary.Font")));
			this.CkbTemporary.Image = ((System.Drawing.Image)(resources.GetObject("CkbTemporary.Image")));
			this.CkbTemporary.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbTemporary.ImageAlign")));
			this.CkbTemporary.ImageIndex = ((int)(resources.GetObject("CkbTemporary.ImageIndex")));
			this.CkbTemporary.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbTemporary.ImeMode")));
			this.CkbTemporary.Location = ((System.Drawing.Point)(resources.GetObject("CkbTemporary.Location")));
			this.CkbTemporary.Name = "CkbTemporary";
			this.CkbTemporary.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbTemporary.RightToLeft")));
			this.CkbTemporary.Size = ((System.Drawing.Size)(resources.GetObject("CkbTemporary.Size")));
			this.CkbTemporary.TabIndex = ((int)(resources.GetObject("CkbTemporary.TabIndex")));
			this.CkbTemporary.Text = resources.GetString("CkbTemporary.Text");
			this.CkbTemporary.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbTemporary.TextAlign")));
			this.CkbTemporary.Visible = ((bool)(resources.GetObject("CkbTemporary.Visible")));
			// 
			// RowViewer
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.CkbTemporary);
			this.Controls.Add(this.tbNavigator);
			this.Controls.Add(this.LblBase);
			this.Controls.Add(this.TxtBase);
			this.Controls.Add(this.TxtTarget);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.LblTarget);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "RowViewer";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.RowViewer_Load);
			this.ResumeLayout(false);

		}
		#endregion

		
		
	}
}
