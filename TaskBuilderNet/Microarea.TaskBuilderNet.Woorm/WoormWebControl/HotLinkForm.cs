using System;
using System.Data;
using System.Diagnostics;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;
using Microarea.TaskBuilderNet.Core.StringLoader;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	/// <summary>
	/// Descrizione di riepilogo per WebForm.
	/// </summary>
	/// ================================================================================
	public class HotLinkForm : Control, INamingContainer
	{
		private const int PAGESIZE = 50;	// dimensione della paginazione
		private	ILocalizer	localizer	 = null;
		private AskDialog	askDialog;
		private DataGrid	grid;
		private Guid		formKey;
		private int			currentPageIndex;
		private WoormWebControl woormWebControl;
		
		//--------------------------------------------------------------------------
		WoormWebControl	WoormWebControl { get { return woormWebControl; }}
		RSEngine	StateMachine	{ get { return WoormWebControl == null ? null : WoormWebControl.StateMachine; }}
		Report			Report			{ get { return askDialog.Report; }}
		TbReportSession	Session			{ get { return askDialog.Report.ReportSession; }}
		Enums			Enums			{ get { return Session.Enums; }}
		ReferenceObject	Hotlink			{ get { return askDialog.ActiveAskEntry.Hotlink; }}
		Field			Field			{ get { return askDialog.ActiveAskEntry.Field; }}

		//--------------------------------------------------------------------------
		public HotLinkForm(AskDialog askDialog, Guid formKey, WoormWebControl woormWebControl) : base()
		{
			this.askDialog = askDialog;
			this.formKey = formKey;
			this.woormWebControl = woormWebControl;
		}

		//--------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			Page.RegisterRequiresControlState(this);
		}

		//--------------------------------------------------------------------------
		protected override object SaveControlState()
		{
			object obj = base.SaveControlState();

			if (grid != null)
			{
				if (obj != null)
				{
					return new Pair(obj, grid.CurrentPageIndex);
				}
				else
				{
					return (grid.CurrentPageIndex);
				}
			}
			else
			{
				return obj;
			}

		}

		//--------------------------------------------------------------------------
		protected override void LoadControlState(object state)
		{
			if (state != null)
			{
				Pair p = state as Pair;
				if (p != null)
				{
					base.LoadControlState(p.First);
					currentPageIndex = (int)p.Second;
				}
				else
				{
					if (state is int)
					{
						currentPageIndex = (int)state;
					}
					else
					{
						base.LoadControlState(state);
					}
				}
			}

		}

		// devo prendere la colonna effetivamente selezionata come valore assoluto
		// rispetto alla pagina correntemente selezionata
		//--------------------------------------------------------------------------
		private void AssignResultData()
		{
			int i = grid.CurrentPageIndex * grid.PageSize + grid.SelectedIndex;

			DataView view = (DataView)grid.DataSource;
			DataRowView row = view[i];

			Field.AskData = row[Hotlink.FieldName];
			askDialog.UserChanged.Add(Field.Name);

			//Devo rivalutare le init expression, nel caso un' altro campo di input
			//dipendesse da quello che ho appena assegnato selezionandolo dall'hotlink
			askDialog.EvalAllInitExpression(null, false);
		}

		//--------------------------------------------------------------------------
		private void SelectedIndexChanged(object sender, EventArgs e)
		{
			if (StateMachine == null)
				return;

			Debug.Assert(StateMachine.CurrentState == State.RenderingForm);
			StateMachine.HtmlPage = HtmlPageType.Form;
			AssignResultData();

			StateMachine.Step();
			WoormWebControl.RebuildControls();
		}

		//--------------------------------------------------------------------------
		private void CancelClick(object sender, EventArgs e)
		{
			if (StateMachine == null)
				return;

			Debug.Assert(StateMachine.CurrentState == State.RenderingForm);
			StateMachine.HtmlPage = HtmlPageType.Form;

			StateMachine.Step();
			WoormWebControl.RebuildControls();
		}

		//--------------------------------------------------------------------------
		private void PageIndexChanged(object sender, DataGridPageChangedEventArgs e)
		{
			// Set CurrentPageIndex to the page the user clicked.
			grid.CurrentPageIndex = e.NewPageIndex;

			// Rebind the data. 
			grid.DataBind();
		}

		//--------------------------------------------------------------------------
		protected void SortCommand(Object sender, DataGridSortCommandEventArgs e) 
		{
			// rimette sulla prima pagina perchè il sort è globale
			grid.CurrentPageIndex = 0;

			grid.DataSource = CreateDataSource(e.SortExpression);
			grid.DataBind();
		}

		// riaggiorna il valore di tutti i Field della form su conferma della stessa
		//--------------------------------------------------------------------------
		private void SubmitClick(object sender, EventArgs e)
		{
			if (StateMachine == null)
				return;

			Debug.Assert(StateMachine.CurrentState == State.RenderingForm);
			StateMachine.HtmlPage = HtmlPageType.Form;

			StateMachine.Step();
			WoormWebControl.RebuildControls();
		}

		//--------------------------------------------------------------------------
		DataView CreateDataSource(string currentSort) 
		{
			DataSet currentQuery = StateMachine.StateBag[formKey.ToString()] as DataSet;

			if (currentQuery == null)
			{
				TBConnection connection = null;
				TBCommand command		= null;
				TBDataAdapter adapter	= null;

				try
				{
					connection = new TBConnection(Session.CompanyDbConnection, TBDatabaseType.GetDBMSType(Session.Provider));
					command = new TBCommand(Hotlink.QueryString, connection);					

					for (int i = 0; i < Hotlink.QueryParams.Count; i++)
					{
						string paramName = Hotlink.ParamName(i + 1);
						object paramValue = ObjectHelper.CastToDBData(Hotlink.QueryParams[i]);
						command.Parameters.Add(paramName, paramValue);
					}

					adapter = new TBDataAdapter(connection);
					adapter.SelectCommand = command;

					currentQuery = new DataSet();
					adapter.Fill(currentQuery);
				}
				catch (TBException e)
				{
					Debug.Fail(string.Format(WoormWebControlStrings.CreateDataSourceError, e.Message));
					return null;
				}
				finally
				{
					if (connection != null)	
						connection.Close();
				}

				StateMachine.StateBag[formKey.ToString()] = currentQuery;

			}

			DataView source = currentQuery.Tables[0].DefaultView;
			localizer = new DatabaseLocalizer(Hotlink.Prototype.DbFieldTableName);
			
			if (currentSort != null)
				source.Sort = currentSort;
			
			return source;
		}
 
		// Nel caso la select non restitisca i dati faccio comparire solo una form di avviso
		//--------------------------------------------------------------------------
		private void NoResultForm()
		{
			// esterno a tutto
			Panel bigPanel = new Panel();
			Controls.Add(bigPanel);

			Table table = new Table();
			table.CssClass = "HotLinkTable";
			table.Attributes["align"] = "center";
			bigPanel.Controls.Add(table);

			TableRow row = new TableRow();
			table.Controls.Add(row);
			TableCell cell = new TableCell();
			row.Controls.Add(cell);
			cell.Text = WoormWebControlStrings.NoData;

			TableRow okRow = new TableRow();
			table.Controls.Add(okRow);
			TableCell buttonCell = new TableCell();
			buttonCell.Attributes["align"] = "center";
			okRow.Controls.Add(buttonCell);

			Button Submit = new Button();
			Submit.Text = WoormWebControlStrings.Ok;
			Submit.Width = Unit.Pixel(100);
			Submit.Click += new EventHandler(this.SubmitClick);
			buttonCell.Controls.Add(Submit);
		}
 
		// Use the ItemDataBound event to customize the DataGrid control. 
		// The ItemDataBound event allows you to access the data before 
		// the item is displayed in the control. 
		//--------------------------------------------------------------------------
		void ItemBound(Object sender, DataGridItemEventArgs e) 
		{
			if	(
				(e.Item.ItemType == ListItemType.Item) ||
				(e.Item.ItemType == ListItemType.AlternatingItem)
				)
			{
				// devo ignorare la prima colonna che contiene il command e non dati
				for (int i = 0; i < e.Item.Cells.Count - 1; i++)
				{
					object s = ((DataRowView)e.Item.DataItem).Row.ItemArray[i];
					e.Item.Cells[i + 1].Wrap = false;
					if (s.GetType().Name != "String")
						e.Item.Cells[i + 1].HorizontalAlign = HorizontalAlign.Right;
				}
			}    
		}
		//--------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			DataView dataView = CreateDataSource(null);
			if (dataView == null || dataView.Count == 0)
			{
				NoResultForm();
				return;
			}

			ResultFormBody(dataView);
			ResultFormTitle();
		}

		//--------------------------------------------------------------------------
		protected void ResultFormTitle()
		{
			string title = localizer.Translate(Hotlink.Prototype.DbFieldName);

			//Tabella esterna a tutto
			Table mainTable = new Table();
			mainTable.EnableViewState = false;
			mainTable.Width = Unit.Percentage(100);
			Controls.Add(mainTable);
			
			//1 riga
			TableRow tableRow = new TableRow();
			tableRow.EnableViewState = false;
			mainTable.Rows.Add (tableRow);
			
			//Cella che contiene il titolo
			TableCell titleCell = new TableCell();
			titleCell.EnableViewState = false;
			titleCell.CssClass = "HotLinkTitle";
			titleCell.HorizontalAlign = HorizontalAlign.Left;
			tableRow.Cells.Add (titleCell);
			
			//Label del titolo
			Label titleLabel = new Label();
			titleLabel.ID			= "TitleLabel";
			titleLabel.CssClass		= "HotLinkTitleLabel";
			titleLabel.Text = string.Format(WoormWebControlStrings.HotLinkFormTitle, title);
			titleCell.Controls.Add(titleLabel);
			
			//Aggiungo una riga vuota per distanziare un pò
			TableRow emptyRow = new TableRow();
			emptyRow.EnableViewState = false;
			mainTable.Rows.Add (emptyRow);

			TableCell emptyCell = new TableCell();
			emptyRow.EnableViewState = false;
			emptyRow.Cells.Add(emptyCell);
			LiteralControl space = new LiteralControl("&nbsp;");
			emptyCell.Controls.Add(space);

			TableRow parameterTableRow = new TableRow();
			TableCell parameterTableCell = new TableCell();
			parameterTableRow.EnableViewState = false;
			parameterTableCell.EnableViewState = false;
			mainTable.Rows.Add (parameterTableRow);
			parameterTableRow.Cells.Add(parameterTableCell);
			parameterTableCell.Controls.Add (grid);

			TableRow cancelRow = new TableRow();
			cancelRow.EnableViewState = false;
			mainTable.Rows.Add (cancelRow);
			TableCell cancelCell = new TableCell();
			cancelCell.EnableViewState = false;
			cancelRow.Cells.Add(cancelCell);
			Button cancel = new Button();
			cancel.EnableViewState = false;
			cancel.Text = WoormWebControlStrings.Cancel;
			cancel.TabIndex = 1;
			cancel.Width = Unit.Pixel(100);
			cancel.Click += new EventHandler(this.CancelClick);
			cancelCell.Controls.Add(cancel);
		}

		//--------------------------------------------------------------------------
		private void ResultFormBody(DataView dataView)
		{
			grid = new DataGrid();
			grid.ItemDataBound += new DataGridItemEventHandler(ItemBound);

			grid.Attributes["align"] = "center";
			grid.CssClass = "DataGridResults";
			grid.AllowPaging = true;
			grid.PageSize = PAGESIZE;
			grid.ShowFooter = false;
			grid.CellPadding = 3;
			grid.CellSpacing = 0;
			grid.EnableViewState = false;
			grid.AllowSorting = true;
			grid.PagerStyle.Mode = PagerMode.NumericPages;
			grid.Attributes["align"] = "left";
			grid.Style["Width"]	= "100%";
			grid.AutoGenerateColumns = false;

			// stile dei titoli
			grid.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
			grid.HeaderStyle.VerticalAlign = VerticalAlign.Middle;
			grid.HeaderStyle.Wrap = false;
			

			// stile del singolo item
			grid.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
			grid.ItemStyle.VerticalAlign = VerticalAlign.Middle;

			// aggiunge il link per tornare indietro alla dialog.
			ButtonColumn link = new ButtonColumn();
			link.HeaderText="Link"; 
			link.ButtonType = ButtonColumnType.LinkButton;
			link.Text = WoormWebControlStrings.Select;
			link.CommandName = "Select";
			grid.Columns.Add(link);

			grid.SortCommand += new DataGridSortCommandEventHandler(this.SortCommand);
			grid.SelectedIndexChanged += new EventHandler(this.SelectedIndexChanged);
			grid.PageIndexChanged += new DataGridPageChangedEventHandler(this.PageIndexChanged);

			foreach (DataColumn dt in dataView.Table.Columns)
			{
				BoundColumn  dc = new BoundColumn();
				dc.HeaderText = localizer.Translate(dt.ColumnName);
				dc.DataField = dt.ColumnName;
				dc.SortExpression = dt.ColumnName;
				grid.Columns.Add(dc);
			}

			grid.DataSource = dataView;
			grid.CurrentPageIndex = currentPageIndex;

			grid.DataBind();
		}
	}
}
