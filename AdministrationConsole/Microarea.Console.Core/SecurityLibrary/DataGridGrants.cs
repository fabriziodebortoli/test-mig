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
	public class GrantsDataGrid : DataGrid
	{
		private System.Windows.Forms.Form			parentForm			= null;
		private System.Windows.Forms.UserControl	parentUserControl	= null;

		private DataSet  dataSource = null;
		
		private BitmapLoader bmpLoader = new BitmapLoader( new string [] {"notexist.bmp",  
																			 "notassigned.bmp", 
																			 "forbidden.bmp", 
																			 "inherit.bmp", 
																			 "allowed.bmp" });
		private int averageCharWidth = 0;
		private int grantsImageColumnsWidth	= 0;
		private int rolesImageColumnsWidth	= 0;
			
		private const int imageDefaultColumnWidth = 42;
		// Nei titoli delle colonne conto sempre due caratteri in più rispetto alla
		// lunghezza del titolo per i margini lasciati a destra e sinistra
		private const int columnTitleCharsOffset = 2;

		private const int dataGridBordersOffset = 4;

		private const int dataGridRowYOffset = 8;

		private const int minBottomMargin = 8;


		private int dataGridCaptionHeight = 0;
		private int dataGridColumsHeadersHeight = 0;
		private int dataGridExpandedRelationRowHeight = 0;

		public delegate void ModifyColumnValueHandle(object sender, int rowNumber);
		public event ModifyColumnValueHandle OnModifyColumnValueHandle;
		public delegate void ResizeWidth(object sender, int number);
		public event ResizeWidth OnResizeWidth;

		public new DataSet DataSource 
		{
			get { return this.dataSource;}
			set { this.dataSource = value;}
		}
	
		//---------------------------------------------------------------------
		//@@TODO DA TOGLIERE
		public GrantsDataGrid(bool isRapidConfiguration, bool isRole, System.Windows.Forms.Form aParentForm) 
		{
			parentForm = aParentForm;

			// Per PRIMA COSA imposto il font del DataGrid: in tal modo viene SUBITO scatenato
			// l'evento di cambiamento del font che viene gestito mediante la reimplementazione
			// del metodo virtuale OnFontChanged. In tal modo vengono correttamente calcolate la
			// PreferredRowHeight e la larghezza media dei caratteri averageCharWidth, che serve
			// poi per dimensionare le colonne del DataGrid
			this.Font = (aParentForm != null) ? new System.Drawing.Font(aParentForm.Font, System.Drawing.FontStyle.Regular) : new System.Drawing.Font("Verdana", (float)8.25);
			SetDataSource(isRapidConfiguration, isRole);

			CreateGrantsTableStyle(isRapidConfiguration, isRole);
			CreateRolesTableStyle();

			this.AllowSorting = false;

			this.CaptionText = String.Empty;

		//	this.VertScrollBar.VisibleChanged += new System.EventHandler(this.VertScrollBar_VisibleChanged);
	//		this.HorizScrollBar.VisibleChanged += new System.EventHandler(this.HorizScrollBar_VisibleChanged);
		}
		//---------------------------------------------------------------------------
		public GrantsDataGrid(bool isRapidConfiguration, bool isRole, System.Windows.Forms.UserControl aParentForm) 
		{
			parentUserControl = aParentForm;

			// Per PRIMA COSA imposto il font del DataGrid: in tal modo viene SUBITO scatenato
			// l'evento di cambiamento del font che viene gestito mediante la reimplementazione
			// del metodo virtuale OnFontChanged. In tal modo vengono correttamente calcolate la
			// PreferredRowHeight e la larghezza media dei caratteri averageCharWidth, che serve
			// poi per dimensionare le colonne del DataGrid
			this.Font = (aParentForm != null) ? new System.Drawing.Font(aParentForm.Font, System.Drawing.FontStyle.Regular) : new System.Drawing.Font("Verdana", (float)8.25);
			SetDataSource(isRapidConfiguration, isRole);

			CreateGrantsTableStyle(isRapidConfiguration, isRole);
			CreateRolesTableStyle();

			this.AllowSorting = false;

			this.CaptionText = String.Empty;

            //this.VertScrollBar.VisibleChanged += new System.EventHandler(this.VertScrollBar_VisibleChanged);
            //this.HorizScrollBar.VisibleChanged += new System.EventHandler(this.HorizScrollBar_VisibleChanged);
		}
		//---------------------------------------------------------------------------
		public GrantsDataGrid(bool isRole)
		{
			SetDataSource(false, isRole);
		}
		//---------------------------------------------------------------------------
		public void SetDataSource(bool isRapidConfiguration, bool isRole)
		{
			dataSource = new DataSet();
			
			DataTable grantsDataTable = new DataTable("Grants");
			grantsDataTable.Columns.Add(securityGrants.Grant, Type.GetType("System.String") );
			grantsDataTable.Columns.Add(securityGrants.Inherit, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.Role, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.User, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.Total, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.Assign, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.OldValue, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.GrantMask, Type.GetType("System.Int32") );
			dataSource.Tables.Add(grantsDataTable);

			DataTable rolesDataTable = new DataTable("Roles");
			rolesDataTable.Columns.Add(SecurityLibraryStrings.Grant, Type.GetType("System.String"));
			rolesDataTable.Columns.Add(SecurityLibraryStrings.RoleName, Type.GetType("System.String"));
			rolesDataTable.Columns.Add(SecurityLibraryStrings.Inherit, Type.GetType("System.Int32"));
			rolesDataTable.Columns.Add(SecurityLibraryStrings.Role, Type.GetType("System.Int32"));
			rolesDataTable.Columns.Add(SecurityLibraryStrings.Total, Type.GetType("System.Int32"));
			dataSource.Tables.Add(rolesDataTable);			

			DataView aDataView = new DataView(dataSource.Tables["Grants"]);
			aDataView.AllowNew = false;
			aDataView.AllowDelete = false;
			aDataView.AllowEdit = false;
			
			
			DataView aDataViewRoles  = this.DataSource.DefaultViewManager.CreateDataView(rolesDataTable);
			aDataViewRoles.AllowEdit = false;
			aDataViewRoles.AllowDelete = false;
			aDataViewRoles.AllowNew = false;
			base.DataSource  = aDataView;
            this.VertScrollBar.VisibleChanged += new System.EventHandler(this.VertScrollBar_VisibleChanged);
            this.HorizScrollBar.VisibleChanged += new System.EventHandler(this.HorizScrollBar_VisibleChanged);

		}
		//---------------------------------------------------------------------------
		protected override void OnFontChanged(EventArgs e)
		{
			// Invoke base class implementation
			base.OnFontChanged(e);
			System.Drawing.Graphics currentGraphics = this.CreateGraphics();

			System.Drawing.SizeF sampleStringSize;

			if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CHS" || 
				Thread.CurrentThread.CurrentUICulture.Name == "zh-CHT")
			{
				sampleStringSize = currentGraphics.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZ", this.Font);
				currentGraphics.Dispose();
				averageCharWidth =(int)(Math.Ceiling(sampleStringSize.Width)/26) + 8;
			}
			else
			{
				sampleStringSize = currentGraphics.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", this.Font);
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

			ResizeGrantsTextColumnToFit(false);
			
			ResizeRolesTextColumnToFit(false);			
		}

		
		//--------------------------------------------------------------------------
		private void HorizScrollBar_VisibleChanged(object sender, EventArgs e)
		{
            base.SuspendLayout();
			if (sender != this.HorizScrollBar)
				return;
		
			HorizScrollBar.Visible = false;
		}
		//--------------------------------------------------------------------------
		private void VertScrollBar_VisibleChanged(object sender, EventArgs e)
		{
            base.SuspendLayout();
			if (sender != this.VertScrollBar)
				return;
			
			ResizeGrantsTextColumnToFit(this.VertScrollBar.Visible);
			
			ResizeRolesTextColumnToFit(this.VertScrollBar.Visible);

			this.Refresh();
		}

		//---------------------------------------------------------------------------
		public void CreateGrantsTableStyle(bool isRapidConfiguration, bool isRole)
		{
			TableStyles.Clear();
			DataGridTableStyle grantsTableStyle = new DataGridTableStyle();
			grantsTableStyle.AllowSorting = false;
			grantsTableStyle.MappingName = "Grants";
			grantsTableStyle.HeaderFont = this.Font;
			grantsTableStyle.PreferredRowHeight = this.PreferredRowHeight;

			//Preparo lo Stile 
			DataGridTextBoxColumn grantTextColumn = new DataGridTextBoxColumn();
			grantTextColumn.MappingName = securityGrants.Grant;
			grantTextColumn.HeaderText = SecurityLibraryStrings.Grant;
			grantTextColumn.Alignment = HorizontalAlignment.Left;
			grantTextColumn.NullText = String.Empty;
			grantTextColumn.ReadOnly = true;

			grantsTableStyle.GridColumnStyles.Add(grantTextColumn);

			if (!isRapidConfiguration)
			{
				DataGridImageColumn imageColumn = new DataGridImageColumn(bmpLoader);
				imageColumn.MappingName = securityGrants.Inherit;
				imageColumn.HeaderText = SecurityLibraryStrings.Inherit;
				imageColumn.Alignment = HorizontalAlignment.Center;
				imageColumn.NullText = String.Empty;
				grantsTableStyle.GridColumnStyles.Add(imageColumn);
			
				imageColumn = new DataGridImageColumn(bmpLoader);
				imageColumn.MappingName = securityGrants.Role;
				imageColumn.HeaderText = SecurityLibraryStrings.Role;
				imageColumn.Alignment = HorizontalAlignment.Center;
				imageColumn.NullText = String.Empty;
				grantsTableStyle.GridColumnStyles.Add(imageColumn);

				if (!isRole)
				{
					imageColumn = new DataGridImageColumn(bmpLoader);
					imageColumn.MappingName = securityGrants.User;
					imageColumn.HeaderText = SecurityLibraryStrings.User;
					imageColumn.Alignment = HorizontalAlignment.Center;
					imageColumn.NullText = String.Empty;
					grantsTableStyle.GridColumnStyles.Add(imageColumn);
				}
				imageColumn = new DataGridImageColumn(bmpLoader);
				imageColumn.MappingName = securityGrants.Total;
				imageColumn.HeaderText = SecurityLibraryStrings.Total;
				imageColumn.Alignment = HorizontalAlignment.Center;
				imageColumn.NullText = String.Empty;
				
				grantsTableStyle.GridColumnStyles.Add(imageColumn);
			}
			
			DataGridDropDownImgColumn dropDownImgColumn = new DataGridDropDownImgColumn(bmpLoader, this.Font);
			dropDownImgColumn.OnModifyColumnValueHandle += new DataGridDropDownImgColumn.ModifyColumnValueHandle(IsModidyColumnValue);
			
			dropDownImgColumn.MappingName = securityGrants.Assign;
			dropDownImgColumn.HeaderText = SecurityLibraryStrings.Assign;
			dropDownImgColumn.Alignment = HorizontalAlignment.Center;
			dropDownImgColumn.NullText = String.Empty;

			grantsTableStyle.GridColumnStyles.Add(dropDownImgColumn);

			TableStyles.Add(grantsTableStyle);

			ResizeGrantsColumns();
		}
		
		//---------------------------------------------------------------------------
		private void IsModidyColumnValue(object sender, int rowNumber)
		{
			if (OnModifyColumnValueHandle != null)
				OnModifyColumnValueHandle(this, rowNumber);
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

				DataGridColumnStyle grantTextColumnStyle = grantsTableStyle.GridColumnStyles[securityGrants.Grant];
				if (grantTextColumnStyle != null)
					grantTextColumnStyle.Width = averageCharWidth * (columnTitleCharsOffset + grantTextColumnStyle.HeaderText.Length);

				grantsImageColumnsWidth	+= ResizeImageColumn(grantsTableStyle, securityGrants.Inherit);
				grantsImageColumnsWidth	+= ResizeImageColumn(grantsTableStyle, securityGrants.Role);
				grantsImageColumnsWidth	+= ResizeImageColumn(grantsTableStyle, securityGrants.User);
				grantsImageColumnsWidth	+= ResizeImageColumn(grantsTableStyle, securityGrants.Total);

				DataGridColumnStyle grantDropDownColumnStyle = grantsTableStyle.GridColumnStyles[securityGrants.Assign];
				if (grantDropDownColumnStyle != null)
				{
					grantDropDownColumnStyle.Width = Math.Max(grantDropDownColumnStyle.Width, averageCharWidth * (columnTitleCharsOffset + grantDropDownColumnStyle.HeaderText.Length));
	
					grantsImageColumnsWidth	+= grantDropDownColumnStyle.Width;
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
			
			if (vertScrollBarVisible)
				remainingWidth -= this.VertScrollBar.Width;
			
			DataGridColumnStyle grantTextColumn = grantsTableStyle.GridColumnStyles[securityGrants.Grant];
			int dif = 0;
			if (grantTextColumn != null)
			{
				dif = grantTextColumn.Width - remainingWidth;
				
				if (remainingWidth < grantTextColumn.Width)
				{
					this.Width = this.Width + dif;
					if (OnResizeWidth != null)
						OnResizeWidth(this, dif);
				}
			}
		}
		
		//---------------------------------------------------------------------------
		public void CreateRolesTableStyle()
		{
			DataGridTableStyle rolesTableStyle = new DataGridTableStyle();
			rolesTableStyle.AllowSorting = false;
			rolesTableStyle.MappingName = "Roles";
			rolesTableStyle.HeaderFont = this.Font;
			rolesTableStyle.PreferredRowHeight = this.PreferredRowHeight;
			rolesTableStyle.ReadOnly = true;

			//Text
			DataGridTextBoxColumn roleTextColumn = new DataGridTextBoxColumn();
			roleTextColumn.MappingName = SecurityLibraryStrings.RoleName;
			roleTextColumn.HeaderText = SecurityLibraryStrings.RoleName;
			roleTextColumn.Alignment = HorizontalAlignment.Left;
			roleTextColumn.ReadOnly = true;

			rolesTableStyle.GridColumnStyles.Add(roleTextColumn);


			DataGridImageColumn	imageColumnRole = new DataGridImageColumn(bmpLoader);
			imageColumnRole.MappingName = SecurityLibraryStrings.Inherit;
			imageColumnRole.HeaderText = SecurityLibraryStrings.Inherit;
			imageColumnRole.Alignment = HorizontalAlignment.Center;
			imageColumnRole.NullText = String.Empty;

			rolesTableStyle.GridColumnStyles.Add(imageColumnRole);

			imageColumnRole = new DataGridImageColumn(bmpLoader);
			imageColumnRole.MappingName = SecurityLibraryStrings.Role;
			imageColumnRole.HeaderText = SecurityLibraryStrings.Role;
			imageColumnRole.Alignment = HorizontalAlignment.Center;
			imageColumnRole.NullText = String.Empty;

			rolesTableStyle.GridColumnStyles.Add(imageColumnRole);
		
			imageColumnRole = new DataGridImageColumn(bmpLoader);
			imageColumnRole.MappingName = SecurityLibraryStrings.Total;
			imageColumnRole.HeaderText = SecurityLibraryStrings.Total;
			imageColumnRole.Alignment = HorizontalAlignment.Center;
			imageColumnRole.NullText = String.Empty;
			rolesTableStyle.GridColumnStyles.Add(imageColumnRole);

			TableStyles.Add(rolesTableStyle);
			ResizeRolesColumns();
		}
		
		//---------------------------------------------------------------------------
		private void ResizeRolesColumns()
		{		
			if (TableStyles == null || TableStyles.Count == 0)
				return;
			
			DataGridTableStyle rolesTableStyle = TableStyles["Roles"]; 
			if (rolesTableStyle != null)
			{
				rolesImageColumnsWidth	= 0;

				DataGridColumnStyle rolesTextColumnStyle = rolesTableStyle.GridColumnStyles[SecurityLibraryStrings.RoleName];
				if (rolesTextColumnStyle != null)
					rolesTextColumnStyle.Width = averageCharWidth * (columnTitleCharsOffset + rolesTextColumnStyle.HeaderText.Length);

				rolesImageColumnsWidth += ResizeImageColumn(rolesTableStyle, SecurityLibraryStrings.Inherit);
				rolesImageColumnsWidth	+= ResizeImageColumn(rolesTableStyle, securityGrants.Grant);
				rolesImageColumnsWidth += ResizeImageColumn(rolesTableStyle, SecurityLibraryStrings.RoleName);
				rolesImageColumnsWidth += ResizeImageColumn(rolesTableStyle, SecurityLibraryStrings.Total);
			}
		}

		//---------------------------------------------------------------------------
		private void ResizeRolesTextColumnToFit(bool vertScrollBarVisible)
		{		
			if (TableStyles == null || TableStyles.Count == 0)
				return;

			DataGridTableStyle rolesTableStyle = TableStyles["Roles"]; 
			if (rolesTableStyle == null)
				return;

			int remainingWidth = this.ClientRectangle.Width -
				dataGridBordersOffset -
				this.RowHeaderWidth -
				rolesImageColumnsWidth;

			if (vertScrollBarVisible)
				remainingWidth -= this.VertScrollBar.Width;

			DataGridColumnStyle roleTextColumn = rolesTableStyle.GridColumnStyles[SecurityLibraryStrings.RoleName];
			if (roleTextColumn != null)
				roleTextColumn.Width = remainingWidth;
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
		//@@TODO DA TOGLIERE
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
					
				if (form != null)
				{
					int maxHeight = parentForm.ClientRectangle.Height - this.Top - minBottomMargin;
					if (newHeight > maxHeight)
					{
						newHeight = maxHeight;
						showVerScrollBar = true;
					}
				}
				
				DataGridColumnStyle grantTextColumn = grantsTableStyle.GridColumnStyles[securityGrants.Grant];
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

					if (form != null)
					{
						int maxWidth = parentForm.ClientRectangle.Width - this.Left - 4;
						if (newWidth > maxWidth)
							newWidth = maxWidth;
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
				
				DataGridColumnStyle grantTextColumn = grantsTableStyle.GridColumnStyles[securityGrants.Grant];
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
		public  void AddGrantsRow(GrantsRow rowGrants)
		{
			if (rowGrants == null)
				return;

			DataRow dr = dataSource.Tables["Grants"].NewRow();

			dr[securityGrants.Grant] = rowGrants.Description;
			dr[securityGrants.GrantMask] = rowGrants.Mask;
			dr[securityGrants.Inherit] = rowGrants.InheritIconState;
			dr[securityGrants.Role] = rowGrants.RoleIconState;
			dr[securityGrants.User] = rowGrants.UserIconState;
			dr[securityGrants.Total] = rowGrants.TotalIconState;
			dr[securityGrants.Assign] = GrantsRow.IconState.IconNotExist;
			dr[securityGrants.OldValue] = GrantsRow.IconState.IconNotExist;
			
			dataSource.Tables["Grants"].Rows.Add(dr);
		}

		//---------------------------------------------------------------------
		public  void LoadRoleRow(int codRole, string descRole, int codCompany, int objectId, SqlConnection aConnection, ref ArrayList arrayListGrants)
		{
			GrantsRow grants = null;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
				return;

			SqlCommand myCommand  = null;
			
			try
			{
				int role      = 0;
				int effective = 0;
				int parent    = 0;
				int grant     = 0;
				int mask      = 0;

				myCommand  = new SqlCommand("MSD_GetSplitRoleGrant", aConnection);
				myCommand.CommandType = CommandType.StoredProcedure;

				//Parametri StroredProcedur
				myCommand.Parameters.Add("@par_companyid", SqlDbType.Int).Value = codCompany;
				myCommand.Parameters.Add("@par_roleid", SqlDbType.Int).Value    = codRole;
				myCommand.Parameters.Add("@par_objectid", SqlDbType.Int).Value  = objectId;
				myCommand.Parameters.Add("@parout_thereis_rolegrant", SqlDbType.Int).Value   = 1;
				myCommand.Parameters.Add("@parout_role_grant", SqlDbType.Int).Value          = 1;
				myCommand.Parameters.Add("@parout_role_inheritmask", SqlDbType.Int).Value    = 1;
				myCommand.Parameters.Add("@parout_thereis_parentgrant", SqlDbType.Int).Value = 1;
				myCommand.Parameters.Add("@parout_parent_grant", SqlDbType.Int).Value        = 1;
				myCommand.Parameters.Add("@parout_parent_inheritmask", SqlDbType.Int).Value  = 1;
				myCommand.Parameters.Add("@parout_thereis_totalgrant", SqlDbType.Int).Value  = 1;
				myCommand.Parameters.Add("@parout_total_grant", SqlDbType.Int).Value         = 1;
				myCommand.Parameters.Add("@parout_total_inheritmask", SqlDbType.Int).Value   = 1;
				myCommand.Parameters.Add("@parout_protected_object", SqlDbType.Int).Value    = 1;
				myCommand.Parameters.Add("@parout_existparent_object", SqlDbType.Int).Value  = 1;
				
				myCommand.Parameters["@parout_thereis_rolegrant"].Direction = ParameterDirection.Output;
				myCommand.Parameters["@parout_role_grant"].Direction        = ParameterDirection.Output;
				myCommand.Parameters["@parout_role_inheritmask"].Direction  = ParameterDirection.Output;
				myCommand.Parameters["@parout_thereis_parentgrant"].Direction = ParameterDirection.Output;
				myCommand.Parameters["@parout_parent_grant"].Direction        = ParameterDirection.Output;
				myCommand.Parameters["@parout_parent_inheritmask"].Direction  = ParameterDirection.Output;
				myCommand.Parameters["@parout_thereis_totalgrant"].Direction = ParameterDirection.Output;
				myCommand.Parameters["@parout_total_grant"].Direction        = ParameterDirection.Output;
				myCommand.Parameters["@parout_total_inheritmask"].Direction  = ParameterDirection.Output;
				myCommand.Parameters["@parout_protected_object"].Direction   = ParameterDirection.Output;
				myCommand.Parameters["@parout_existparent_object"].Direction = ParameterDirection.Output;

				myCommand.Parameters.Add("@ReturnVal", SqlDbType.Int);
				myCommand.Parameters["@ReturnVal"].Direction = ParameterDirection.ReturnValue;

				myCommand.ExecuteNonQuery();
				
				//ROLE
				if (myCommand.Parameters["@parout_thereis_rolegrant"].Value != DBNull.Value)
					role = Convert.ToInt32(myCommand.Parameters["@parout_thereis_rolegrant"].Value);	
				grant = Convert.ToInt32(myCommand.Parameters["@parout_role_grant"].Value);
				mask  = Convert.ToInt32(myCommand.Parameters["@parout_role_inheritmask"].Value);
				for(int i=0; i < arrayListGrants.Count; i++)
				{
					if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
						continue;

					grants = (GrantsRow) arrayListGrants[i];
					((GrantsRow)arrayListGrants[i]).SetRoleIconState(role != 0, true, grant, mask, grants.Mask, true);
				}		

				//PARENT
				if(myCommand.Parameters["@parout_thereis_parentgrant"].Value != DBNull.Value  )
					parent = Convert.ToInt32(myCommand.Parameters["@parout_thereis_parentgrant"].Value);
				
				grant = Convert.ToInt32(myCommand.Parameters["@parout_parent_grant"].Value);
				mask  = Convert.ToInt32(myCommand.Parameters["@parout_parent_inheritmask"].Value);
				for(int i=0; i < arrayListGrants.Count; i++)
				{
					if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
						continue;
					((GrantsRow)arrayListGrants[i]).SetInheritIconState(parent != 0, true, grant, mask);
				}
				
				//EFFETTIVO
				if (myCommand.Parameters["@parout_thereis_totalgrant"].Value != DBNull.Value)
					effective = Convert.ToInt32(myCommand.Parameters["@parout_thereis_totalgrant"].Value);
				grant = Convert.ToInt32(myCommand.Parameters["@parout_total_grant"].Value);
				mask  = Convert.ToInt32(myCommand.Parameters["@parout_total_inheritmask"].Value);

				
				for(int i=0; i < arrayListGrants.Count; i++)
				{
					if (arrayListGrants[i] == null || !(arrayListGrants[i] is GrantsRow))
						continue;
					grants = (GrantsRow) arrayListGrants[i];
					((GrantsRow)arrayListGrants[i]).SetTotalIconState(effective != 0, true, grant, mask,  grants.Mask);
				}
			}
			catch (SqlException)
			{
			}
			finally
			{
				if (myCommand != null)
					myCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		public void AddRolesRelation()
		{
			if (dataSource == null || dataSource.Tables["Roles"] == null)
				return;

			dataSource.Relations.Clear();
	
			DataView aDataView = dataSource.Tables["Roles"].DefaultView;

			aDataView.AllowNew = false;
			aDataView.AllowDelete = false;
			aDataView.AllowEdit = false;

			dataSource.Relations.Add(SecurityLibraryStrings.RoleGrants,  
				dataSource.Tables["Grants"].Columns[securityGrants.Grant],
				aDataView.Table.Columns[SecurityLibraryStrings.Grant]);

			// Dato che viene aggiunta la relazione le righe diventano "espandibili" e, quindi,
			// occorre ricalcolare le dimensioni in modo opportuno

//			ResizeRolesTextColumnToFit(this.VertScrollBar.Visible);

//			if (parentUserControl == null)
//				ResizeToFit(parentForm);
//			else
//				ResizeToFit(parentUserControl);

		}
		
		//---------------------------------------------------------------------
		public void AddRoleRow(GrantsRow rowGrants)
		{
			if (rowGrants == null)
				return;

			DataRow dr = dataSource.Tables["Roles"].NewRow();

			dr[SecurityLibraryStrings.Grant] = GrantsString.GetGrantDescription(rowGrants.Description);
			dr[SecurityLibraryStrings.RoleName] = rowGrants.RoleDescription;
			dr[SecurityLibraryStrings.Inherit] = rowGrants.InheritIconState;
			dr[SecurityLibraryStrings.Role] = rowGrants.RoleIconState;
			dr[SecurityLibraryStrings.Total] = rowGrants.TotalIconState;
			
			dataSource.Tables["Roles"].Rows.Add(dr);
		}

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
	public class GrantsRow
	{
		public enum IconState { IconNotExist=0, IconNotAssigned, IconForbidden, IconInherit, IconAllowed }
	
		private int			mask = 0;
		private int			objectType = -1;
		private string		description = String.Empty;
		private string		roleDescription = String.Empty;
		
		private IconState	inheritIconState;
		private IconState	roleIconState;
		private IconState	userIconState;
		private IconState	totalIconState;
	
		//---------------------------------------------------------------------
		public GrantsRow(int aGrantMask, int aObjectType, string aDescription)
		{
			mask = aGrantMask;
			objectType = aObjectType;
			description = aDescription;
		}

		//---------------------------------------------------------------------
		public GrantsRow(int aGrantMask, GrantsRow grantRow, string aRoleDescription)
		{
			mask				= aGrantMask;
			description			= grantRow.Description;
			roleDescription		= aRoleDescription;
			inheritIconState	= grantRow.InheritIconState;
			roleIconState		= grantRow.RoleIconState;
			userIconState		= grantRow.UserIconState;
			totalIconState		= grantRow.TotalIconState;

		}
		//---------------------------------------------------------------------
		public GrantsRow(int aGrantMask, string aDescription, string aRoleDescription)
		{
			mask				= aGrantMask;
			description			= aDescription;
			roleDescription		= aRoleDescription;
		}
		//---------------------------------------------------------------------
		public int Mask { get { return mask; } }
		//---------------------------------------------------------------------
		public int ObjectType { get { return objectType; } }
		//---------------------------------------------------------------------
		public string Description { get { return description; } }
		//---------------------------------------------------------------------
		public string RoleDescription { get { return roleDescription; } }
		//---------------------------------------------------------------------
		public IconState InheritIconState { get { return inheritIconState; } }
		//---------------------------------------------------------------------
		public IconState RoleIconState { get { return roleIconState; } }
		//---------------------------------------------------------------------
		public IconState UserIconState { get { return userIconState; } }
		//---------------------------------------------------------------------
		public IconState TotalIconState { get { return totalIconState; } }
	
		//---------------------------------------------------------------------
		public void SetInheritIconState(bool existGrant, bool existParent, int aGrant, int aMask)
		{
			inheritIconState = GetIconState(existGrant, existParent, aGrant, aMask);
		}
		
		//---------------------------------------------------------------------
		public void SetRoleIconState(bool existGrant, bool protectedObject, int aGrant, int aMask, int grantMask, bool isRole)
		{
			roleIconState = GetIconState(existGrant, protectedObject, aGrant, aMask);
		}
		//---------------------------------------------------------------------
		public void SetUserIconState(bool existGrant, bool protectedObject, int aGrant, int aMask)
		{
			userIconState = GetIconState(existGrant, protectedObject, aGrant, aMask);
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
			if (bitG != 0) 
				totalIconState = IconState.IconAllowed;
			else
				totalIconState = bitM != 0 ? IconState.IconInherit : IconState.IconForbidden;
		}
		//---------------------------------------------------------------------
		private IconState GetIconState(bool existGrant, bool assignable, int aGrant, int aMask)
		{
			if (!existGrant)
			{
				if (!assignable)
					return IconState.IconNotExist;

				if (Bit.GetBitByMask(aMask, mask) == Bit.GetBitByMask(aGrant, mask) && (Bit.GetBitByMask(aMask, mask) != 0 ))
					return IconState.IconNotExist;

				return IconState.IconNotAssigned;
			}
			
			if (Bit.GetBitByMask(aMask, mask) == 0) 
			{
				if (Bit.GetBitByMask(aGrant, mask) == 0) 
					return IconState.IconForbidden;
				
				return IconState.IconAllowed;
			}
			
			if (Bit.GetBitByMask(aMask, mask) == Bit.GetBitByMask(aGrant, mask) && (Bit.GetBitByMask(aMask, mask) != 0 ))
				return IconState.IconNotExist;

			return IconState.IconInherit;
		}
	}
}
