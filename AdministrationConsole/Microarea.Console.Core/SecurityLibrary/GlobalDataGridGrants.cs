using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Microarea.Console.Core.SecurityLibrary
{
	//=========================================================================
	public class GlobalDataGridGrants : DataGrid
	{
		#region Private Data Member

		private System.Windows.Forms.Form			parentForm			= null;
		private System.Windows.Forms.UserControl	parentUserControl	= null;
		private ArrayList	grantTypes	= null;

		private DataSet  dataSource = null;
		
		private BitmapLoader bmpLoader = new BitmapLoader( new string [] {"notexist.bmp",  
																			 "notassigned.bmp", 
																			 "forbidden.bmp", 
																			 "inherit.bmp", 
																			 "allowed.bmp" });
		private int averageCharWidth		= 0;
		private int grantsImageColumnsWidth	= 0;

		private int dataGridCaptionHeight				= 0;
		private int dataGridColumsHeadersHeight			= 0;
		private int dataGridExpandedRelationRowHeight	= 0;

		#endregion

		#region Const
		private const int imageDefaultColumnWidth	= 42;
		// Nei titoli delle colonne conto sempre due caratteri in più rispetto alla
		// lunghezza del titolo per i margini lasciati a destra e sinistra
		private const int columnTitleCharsOffset	= 2;
		private const int dataGridBordersOffset		= 4;
		private const int dataGridRowYOffset		= 8;
		private const int minBottomMargin			= 8;

		#endregion

		#region Property
		public new DataSet DataSource 
		{
			get { return this.dataSource;}
			set { this.dataSource = value;}
		}
	
		#endregion

		#region Costructor
		//---------------------------------------------------------------------------
		public GlobalDataGridGrants(ArrayList grantTypes, System.Windows.Forms.Form aParentForm) 
		{
			parentForm = aParentForm;

			// Per PRIMA COSA imposto il font del DataGrid: in tal modo viene SUBITO scatenato
			// l'evento di cambiamento del font che viene gestito mediante la reimplementazione
			// del metodo virtuale OnFontChanged. In tal modo vengono correttamente calcolate la
			// PreferredRowHeight e la larghezza media dei caratteri averageCharWidth, che serve
			// poi per dimensionare le colonne del DataGrid
			this.Font = (aParentForm != null) ? new System.Drawing.Font(aParentForm.Font, System.Drawing.FontStyle.Regular) : new System.Drawing.Font("Verdana", (float)8.25);
	
			SetDataSource(grantTypes);
			CreateGrantsTableStyle(grantTypes);

			this.AllowSorting	= false;
			this.CaptionText	= String.Empty;
			this.CaptionVisible = false;

			this.VertScrollBar.VisibleChanged += new System.EventHandler(this.VertScrollBar_VisibleChanged);
		}

		#endregion

		//---------------------------------------------------------------------
		public void SetDataSource(ArrayList grantTypes)
		{

			this.grantTypes = grantTypes;
			dataSource = new DataSet();
			
			DataTable grantsDataTable = new DataTable("Grants");

			grantsDataTable.Columns.Add("Name", Type.GetType("System.String") );
			foreach(GrantInfo grantInfo in grantTypes)
				grantsDataTable.Columns.Add(grantInfo.MappingName, Type.GetType("System.Int32") );
			
			dataSource.Tables.Add(grantsDataTable);
			
			DataView aDataView = new DataView(dataSource.Tables["Grants"]);
			aDataView.AllowNew		= false;
			aDataView.AllowDelete	= false;
			aDataView.AllowEdit		= false;

			base.DataSource  = aDataView;
		}

		//---------------------------------------------------------------------------
		protected override void OnFontChanged(EventArgs e)
		{
			// Invoke base class implementation
			base.OnFontChanged(e);

			System.Drawing.Graphics currentGraphics = this.CreateGraphics();

			if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CHS")
			{
				System.Drawing.SizeF sampleStringSize = currentGraphics.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZ", this.Font);
				currentGraphics.Dispose();
				averageCharWidth =(int)(Math.Ceiling(sampleStringSize.Width)/26) + 8;
			}
			else
			{
				System.Drawing.SizeF sampleStringSize = currentGraphics.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", this.Font);
				currentGraphics.Dispose();
				averageCharWidth =(int)Math.Ceiling(sampleStringSize.Width)/52;
			}

			// PreferredRowHeight is the row height (in pixels) of rows in the grid. 
			// This property must be set before resetting the DataSource and DataMember 
			// properties (either separately, or through the SetDataBinding method), or
			// the property will have no effect. 
			this.PreferredRowHeight = (int)this.Font.Height + dataGridRowYOffset;	
			
			dataGridCaptionHeight				= this.PreferredRowHeight - 2;	
			dataGridColumsHeadersHeight			= this.PreferredRowHeight - 2;	
			dataGridExpandedRelationRowHeight	= this.PreferredRowHeight - 2;
		}

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{	
			// Invoke base class implementation
			base.OnResize(e);

		//	ResizeGrantsTextColumnToFit(false);
			ResizeToFit(parentForm);
		}

		//--------------------------------------------------------------------------
		private void VertScrollBar_VisibleChanged(object sender, EventArgs e)
		{	
			if (sender != this.VertScrollBar)
				return;
		
			ResizeGrantsTextColumnToFit(this.VertScrollBar.Visible);
		
			this.Refresh();
		}
		//---------------------------------------------------------------------------
		private void CreateGrantsTableStyle(ArrayList grantTypes)
		{
			TableStyles.Clear();
			DataGridTableStyle grantsTableStyle = new DataGridTableStyle();
			grantsTableStyle.AllowSorting = false;
			grantsTableStyle.MappingName = "Grants";
			grantsTableStyle.HeaderFont = this.Font;
			grantsTableStyle.PreferredRowHeight = this.PreferredRowHeight;

			//Preparo lo Stile 
			DataGridTextBoxColumn grantTextColumn = new DataGridTextBoxColumn();
			grantTextColumn.MappingName = "NAME";
			grantTextColumn.HeaderText = SecurityLibraryStrings.UserRole;
			grantTextColumn.Alignment = HorizontalAlignment.Left;
			grantTextColumn.NullText = String.Empty;
			grantTextColumn.ReadOnly = true;

			grantsTableStyle.GridColumnStyles.Add(grantTextColumn);

			DataGridImageColumn imageColumn = null;

			foreach (GrantInfo grantInfo in grantTypes)
			{
				imageColumn = new DataGridImageColumn(bmpLoader);
				imageColumn.MappingName = grantInfo.MappingName;
				imageColumn.HeaderText = grantInfo.Caption;
				imageColumn.Alignment = HorizontalAlignment.Center;
				imageColumn.NullText = String.Empty;
				grantsTableStyle.GridColumnStyles.Add(imageColumn);
			}

			DataGridImageColumn imageColumn2 = new DataGridImageColumn(bmpLoader);
			imageColumn2.MappingName = "SpecialUserImage";
			imageColumn2.HeaderText = "Special User";
			imageColumn2.Alignment = HorizontalAlignment.Center;
			imageColumn2.NullText = String.Empty;
			grantsTableStyle.GridColumnStyles.Add(imageColumn2);


			TableStyles.Add(grantsTableStyle);

			ResizeGrantsColumns();
		}
		

		//---------------------------------------------------------------------------
		private void ResizeGrantsColumns()
		{		
			if (TableStyles == null || TableStyles.Count == 0)
				return;
			
			DataGridTableStyle grantsTableStyle = TableStyles["Grants"]; 
			if (grantsTableStyle != null)
			{
				grantsImageColumnsWidth	= 0;

				DataGridColumnStyle grantTextColumnStyle = grantsTableStyle.GridColumnStyles["NAME"];
				if (grantTextColumnStyle != null)
					grantTextColumnStyle.Width = averageCharWidth * (columnTitleCharsOffset + grantTextColumnStyle.HeaderText.Length);

				foreach(GrantInfo grant in grantTypes)
				{
					grantsImageColumnsWidth	+= ResizeImageColumn(grantsTableStyle,grant.MappingName);
				}
			}
		}

		//---------------------------------------------------------------------------

		private void ResizeGrantsTextColumnToFit(bool vertScrollBarVisible)
		{	
			if (TableStyles == null || TableStyles.Count == 0)
				return;
			
			DataGridTableStyle grantsTableStyle = TableStyles["Grants"]; 
			if (grantsTableStyle == null)
				return;

			int remainingWidth = this.ClientRectangle.Width -
									dataGridBordersOffset -
									this.RowHeaderWidth -
									grantsImageColumnsWidth;
			
			if (remainingWidth <0 )
			{
		//		this.Height += SystemInformation.HorizontalScrollBarHeight;
				return;
			}
			if (vertScrollBarVisible)
			{
				remainingWidth -= this.VertScrollBar.Width;	
			}
			
			DataGridColumnStyle grantTextColumn = grantsTableStyle.GridColumnStyles["NAME"];
			if (grantTextColumn != null)
				grantTextColumn.Width = remainingWidth;
		}
		

		
		//---------------------------------------------------------------------------
		private int ResizeImageColumn(DataGridTableStyle aTableStyle, string columnMappingName)
		{		
			if (aTableStyle == null)
				return 0;

			DataGridColumnStyle imageColumnStyle = aTableStyle.GridColumnStyles[columnMappingName];
			if (imageColumnStyle == null)
				return 0;
		
			imageColumnStyle.Width = Math.Max(imageDefaultColumnWidth, averageCharWidth * (columnTitleCharsOffset + imageColumnStyle.HeaderText.Length));
			return imageColumnStyle.Width;
		}

		//---------------------------------------------------------------------------

		public void ResizeToFit(System.Windows.Forms.Form form)
		{		
			DataGridTableStyle grantsTableStyle = TableStyles["Grants"]; 
			if (grantsTableStyle != null)
			{
				int rowNumber = dataSource.Tables["Grants"].Rows.Count;
				// Se il grid contiene solo una o due righe e queste sono espandibili (c'è 
				// una relazione di tipo Master\Detail), faccio più alto di una riga il grid,
				// altrimenti è poco "friendly" doversi muovere con la scrollbar verticale
				// in così poco spazio quando si espande una relazione
				if (dataSource.Relations.Count > 0 && rowNumber <= 2)
					rowNumber++;

				bool showVerScrollBar = false;

				int newHeight = dataGridBordersOffset + 
					(this.CaptionVisible ? dataGridCaptionHeight : 0) + 
					dataGridColumsHeadersHeight +
					(this.PreferredRowHeight * rowNumber);
					
					
				DataGridColumnStyle grantTextColumn = grantsTableStyle.GridColumnStyles["NAME"];
				if (grantTextColumn != null)
				{
					int maxGrantDescriptionLength = columnTitleCharsOffset + grantTextColumn.HeaderText.Length;

					foreach (DataRow dr in dataSource.Tables["Grants"].Rows)
					{
						if (maxGrantDescriptionLength < dr["NAME"].ToString().Length)
							maxGrantDescriptionLength = dr["NAME"].ToString().Length;
					}

					grantTextColumn.Width = averageCharWidth * maxGrantDescriptionLength;
					
					int newWidth = dataGridBordersOffset + 
									this.RowHeaderWidth + 
									grantTextColumn.Width + 
									grantsImageColumnsWidth;
					
					if (showVerScrollBar)
						newWidth += this.VertScrollBar.Width;

					if (form != null)
					{
						int maxWidth = parentForm.ClientRectangle.Width - this.Left - 4;
						if (newWidth > maxWidth)
						{
							newWidth = maxWidth;
							newHeight += SystemInformation.HorizontalScrollBarHeight;
						}
					}


					this.Size = new Size(newWidth, newHeight);
					
				}
			}
		}
		//---------------------------------------------------------------------
		public void ResizeToFit(System.Windows.Forms.UserControl userControl)
		{		
			DataGridTableStyle grantsTableStyle = TableStyles["Grants"]; 
			if (grantsTableStyle != null)
			{
				int rowNumber = dataSource.Tables["Grants"].Rows.Count;
				// Se il grid contiene solo una o due righe e queste sono espandibili (c'è 
				// una relazione di tipo Master\Detail), faccio più alto di una riga il grid,
				// altrimenti è poco "friendly" doversi muovere con la scrollbar verticale
				// in così poco spazio quando si espande una relazione
				if (dataSource.Relations.Count > 0 && rowNumber <= 2)
					rowNumber++;

				bool showVerScrollBar = false;

				int newHeight = dataGridBordersOffset + 
					(this.CaptionVisible ? dataGridCaptionHeight : 0) + 
					dataGridColumsHeadersHeight +
					(this.PreferredRowHeight * rowNumber);
					
				if (parentUserControl != null)
				{
					int maxHeight = parentUserControl.ClientRectangle.Height - this.Top - minBottomMargin;
					if (newHeight > maxHeight)
					{
						newHeight = maxHeight;
						showVerScrollBar = true;
					}
				}
				
				DataGridColumnStyle grantTextColumn = grantsTableStyle.GridColumnStyles["NAME"];
				if (grantTextColumn != null)
				{
					int maxGrantDescriptionLength = columnTitleCharsOffset + grantTextColumn.HeaderText.Length;

					foreach (DataRow dr in dataSource.Tables["Grants"].Rows)
					{
						if (maxGrantDescriptionLength < dr[securityGrants.Grant].ToString().Length)
							maxGrantDescriptionLength = dr[securityGrants.Grant].ToString().Length;
					}

					grantTextColumn.Width = averageCharWidth * maxGrantDescriptionLength;

					int newWidth = dataGridBordersOffset + 
						this.RowHeaderWidth + 
						grantTextColumn.Width + 
						grantsImageColumnsWidth;
					
					if (showVerScrollBar)
						newWidth += this.VertScrollBar.Width;

					if (userControl != null)
					{
						int maxWidth = parentUserControl.ClientRectangle.Width - this.Left - 4;
						if (newWidth > maxWidth)
							newWidth = maxWidth;
					}

					this.Size = new Size(newWidth, newHeight);
				}
			}
		}
		//---------------------------------------------------------------------
		public  void AddGrantsRow(string name, ArrayList grantsArrayList)
		{
			if (grantsArrayList == null || grantsArrayList.Count == 0)
				return;

			int index = -1;

			DataRow dr = dataSource.Tables["Grants"].NewRow();

			dr["Name"] = name;

			foreach(GrantInfo grant in grantsArrayList)
			{
				index = dataSource.Tables["Grants"].Columns.IndexOf(grant.MappingName);
				if (index == -1)
					continue;
				dr[grant.MappingName] = grant.TotalIconState;
			}

			dataSource.Tables["Grants"].Rows.Add(dr);
		
		}

		public static void LoadStoredProcedure(string sqlConnectionString, ref ArrayList grantsArrayList, bool isRole, int companyId, int roleOrUserId, int objectId)
		{

			SqlConnection sqlConnection = new SqlConnection(sqlConnectionString);
			SqlCommand storedProcedureCommand = null;

			try
			{

				sqlConnection.Open();

				string commandText = isRole ? "MSD_GetSplitRoleGrant" : "MSD_GetSplitUserGrant";
				storedProcedureCommand = new SqlCommand(commandText, sqlConnection);
				storedProcedureCommand.CommandType = CommandType.StoredProcedure;

				if (!isRole)
				{
					storedProcedureCommand.Parameters.Add("@parout_thereis_usrgrant", SqlDbType.Int).Value = 1;
					storedProcedureCommand.Parameters.Add("@parout_usr_grant", SqlDbType.Int).Value        = 1;
					storedProcedureCommand.Parameters.Add("@parout_usr_inheritmask", SqlDbType.Int).Value  = 1;

					storedProcedureCommand.Parameters["@parout_thereis_usrgrant"].Direction	= ParameterDirection.Output;
					storedProcedureCommand.Parameters["@parout_usr_grant"].Direction			= ParameterDirection.Output;
					storedProcedureCommand.Parameters["@parout_usr_inheritmask"].Direction	= ParameterDirection.Output;
				}
				storedProcedureCommand.Parameters.Add("@par_companyid", SqlDbType.Int).Value = companyId;
				
				if (isRole)
					storedProcedureCommand.Parameters.Add("@par_roleid", SqlDbType.Int).Value = roleOrUserId;
				else
					storedProcedureCommand.Parameters.Add("@par_userid", SqlDbType.Int).Value = roleOrUserId;
					
				storedProcedureCommand.Parameters.Add("@par_objectid", SqlDbType.Int).Value               = objectId;
				storedProcedureCommand.Parameters.Add("@parout_thereis_rolegrant", SqlDbType.Int).Value   = 1;
				storedProcedureCommand.Parameters.Add("@parout_role_grant", SqlDbType.Int).Value          = 1;
				storedProcedureCommand.Parameters.Add("@parout_role_inheritmask", SqlDbType.Int).Value    = 1;
				storedProcedureCommand.Parameters.Add("@parout_thereis_parentgrant", SqlDbType.Int).Value = 1;
				storedProcedureCommand.Parameters.Add("@parout_parent_grant", SqlDbType.Int).Value        = 1;
				storedProcedureCommand.Parameters.Add("@parout_parent_inheritmask", SqlDbType.Int).Value  = 1;
				storedProcedureCommand.Parameters.Add("@parout_thereis_totalgrant", SqlDbType.Int).Value  = 1;
				storedProcedureCommand.Parameters.Add("@parout_total_grant", SqlDbType.Int).Value         = 1;
				storedProcedureCommand.Parameters.Add("@parout_total_inheritmask", SqlDbType.Int).Value   = 1;
				storedProcedureCommand.Parameters.Add("@parout_protected_object", SqlDbType.Int).Value    = 1;
				storedProcedureCommand.Parameters.Add("@parout_existparent_object", SqlDbType.Int).Value  = 1;
					
				storedProcedureCommand.Parameters["@parout_thereis_rolegrant"].Direction   = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_role_grant"].Direction          = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_role_inheritmask"].Direction    = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_thereis_parentgrant"].Direction = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_parent_grant"].Direction        = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_parent_inheritmask"].Direction  = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_thereis_totalgrant"].Direction  = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_total_grant"].Direction         = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_total_inheritmask"].Direction   = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_protected_object"].Direction    = ParameterDirection.Output;
				storedProcedureCommand.Parameters["@parout_existparent_object"].Direction  = ParameterDirection.Output;

				storedProcedureCommand.Parameters.Add("@ReturnVal", SqlDbType.Int);
				storedProcedureCommand.Parameters["@ReturnVal"].Direction = ParameterDirection.ReturnValue;

				storedProcedureCommand.ExecuteNonQuery();
				storedProcedureCommand.Dispose();

				int effective = 0;
				int grant     = 0;
				int mask      = 0;

				GrantInfo grants = null;

				bool existParent     = Convert.ToBoolean(storedProcedureCommand.Parameters["@parout_existparent_object"].Value);
				bool protectedObject = Convert.ToBoolean(storedProcedureCommand.Parameters["@parout_protected_object"].Value);
				
				//EFFETTIVO
				if (storedProcedureCommand.Parameters["@parout_thereis_totalgrant"].Value != DBNull.Value)
					effective = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_thereis_totalgrant"].Value);
				
				grant = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_total_grant"].Value);
				mask  = Convert.ToInt32(storedProcedureCommand.Parameters["@parout_total_inheritmask"].Value);


				if (grantsArrayList != null && grantsArrayList.Count > 0)
				{
					for(int i=0; i < grantsArrayList.Count; i++)
					{
						if (grantsArrayList[i] == null || !(grantsArrayList[i] is GrantInfo))
							continue;
						
						grants = (GrantInfo) grantsArrayList[i];
						((GrantInfo)grantsArrayList[i]).SetTotalIconState(effective != 0, protectedObject, grant, mask, grants.Mask);
												
					}
				}
				sqlConnection.Close();

			}
			catch (SqlException e)
			{
				MessageBox.Show(e.Message + e.Number);
				if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
					sqlConnection.Close();
				if (storedProcedureCommand != null)
					storedProcedureCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			// 
			// GrantsDataGrid
			// 

			((System.ComponentModel.ISupportInitialize)(this)).EndInit();

		}

	

	}
	//=========================================================================
	public class GrantInfo
	{
		public enum IconState { IconNotExist=0, IconNotAssigned, IconForbidden, IconInherit, IconAllowed }
	
		private string		mappingName = String.Empty;
		private string		caption		= String.Empty;
		private int			mask		= 0;
		private IconState	totalIconState;
		private string		roleName	= string.Empty;
		private int			roleId		= 0;
	

		public string	MappingName	{ get{ return mappingName; }}
		public string	Caption		{ get{ return caption; }}
		public int		Mask		{ get{ return mask; }}
		public string	RoleName	{ get{ return roleName; } set{ roleName = value; }}
		public int		RoleId		{ get{ return roleId; }   set{ roleId = value; }}

		public IconState TotalIconState { get { return totalIconState; } }
	
		//---------------------------------------------------------------------
		public GrantInfo(int mask, string mappingName, string caption)
		{
			this.mask			= mask;
			this.mappingName	= mappingName;
			this.caption		= caption;		
		}

		//---------------------------------------------------------------------
		public void SetTotalIconState(bool existGrant, bool protectedObject, int aGrant, int aMask, int grantMask)
		{
		
			int bitG = 0;
			int bitM = 0;

			if (!existGrant)
			{
				totalIconState =  protectedObject ? IconState.IconNotAssigned : IconState.IconNotExist;
				return;
			}

			bitG   = Bit.GetBitByMask(aGrant, grantMask);
			bitM   = Bit.GetBitByMask(aMask,  grantMask);
			if(bitG != 0) 
				totalIconState = IconState.IconAllowed;
			else
				totalIconState = IconState.IconForbidden;

		}
	}

}
