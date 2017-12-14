using System;
using System.Windows.Forms;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	public class NumberBox:TextBox
	{
		//-----------------------------------------------------------------------------
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			if(!IsNumberInRange(e.KeyChar,48,57) && e.KeyChar != 8 && e.KeyChar != 46)
			{
				e.Handled = true;
			}
			else
			{
				if(e.KeyChar==46)
				{
					e.Handled=(this.Text.IndexOf(".")>-1);
				}
			}
		}  

		//-----------------------------------------------------------------------------
		private bool IsNumberInRange(int Val,int Min,int Max)
		{
			return (Val>=Min && Val<=Max);
		}
	}

	public class ParsedListBox : ListBox
	{
		private SqlParserUpdater parser = null;

		//-----------------------------------------------------------------------------
		public SqlParserUpdater Parser { get { return parser; } }

		//-----------------------------------------------------------------------------
		public void Fill(SqlParserUpdater aParser)
		{
			parser = aParser;
			
			Items.Clear();

			if (aParser == null)
				return;

			Items.Add(SqlScriptUtilityControlsStrings.NewTableListBoxItem);

			SqlTableList tablesList = parser.Tables;
			if (tablesList == null || tablesList.Count == 0)
				return;

			foreach (SqlTable aTable in tablesList)
				Items.Add(aTable.ExtendedName);
		}
	}
}
