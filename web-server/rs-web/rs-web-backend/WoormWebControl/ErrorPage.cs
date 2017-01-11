using System.Collections.Specialized;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Woorm.WoormController;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	
	/// <summary>
	/// Descrizione di riepilogo per ViewerControl.
	/// </summary>
	/// ================================================================================
	public class ErrorPage : Control, INamingContainer
	{
		private StringCollection errorLines = new StringCollection();
		private WoormWebControl woormWebControl;
		
		//--------------------------------------------------------------------------
		WoormWebControl WoormWebControl { get { return woormWebControl;	} }
		RSEngine	StateMachine	{ get { return WoormWebControl.StateMachine;} }

		//--------------------------------------------------------------------------
		public ErrorPage(WoormWebControl woormWebControl)
		{
			this.woormWebControl = woormWebControl;
		}
		
		//--------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			// esterno a tutto
			Panel bigPanel = new Panel();
			Controls.Add(bigPanel);

			Table table = new Table();
			table.ControlStyle.BorderStyle = BorderStyle.Groove;
			table.BackColor = Color.LightBlue;
			table.Attributes["align"] = "center";
			bigPanel.Controls.Add(table);

			foreach (string line in StateMachine.Errors)
			{
				TableRow row = new TableRow();
				table.Controls.Add(row);
				TableCell cell = new TableCell();
				row.Controls.Add(cell);
				cell.Text = line;
			}

			foreach (string line in StateMachine.Warnings)
			{
				TableRow row = new TableRow();
				table.Controls.Add(row);
				TableCell cell = new TableCell();
				row.Controls.Add(cell);
				cell.Text = line;
			}

			TableRow okRow = new TableRow();
			table.Controls.Add(okRow);
			TableCell buttonCell = new TableCell();
			buttonCell.Attributes["align"] = "center";
			okRow.Controls.Add(buttonCell);

			HtmlButton close = new HtmlButton();
			buttonCell.Controls.Add(close);
			close.InnerText = WoormWebControlStrings.Close;
			close.Style.Add("Width", "100px");
			close.Attributes.Add("onclick", "window.close();");
		}
	}
}
