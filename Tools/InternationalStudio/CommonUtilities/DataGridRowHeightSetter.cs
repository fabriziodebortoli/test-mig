using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.CommonUtilities
{
	//================================================================================
	public class DataGridRowHeightSetter
	{
		private DataGrid dg;
		private ArrayList rowObjects;

		//--------------------------------------------------------------------------------
		public DataGridRowHeightSetter(DataGrid dg)
		{
			this.dg = dg;
			InitHeights();
		}

		//--------------------------------------------------------------------------------
		public int RowCount { get { return rowObjects.Count; } } 

		//--------------------------------------------------------------------------------
		private void InitHeights()
		{
			MethodInfo mi = typeof(DataGrid).GetMethod("get_DataGridRows",BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

			System.Array dgra = (System.Array)mi.Invoke(this.dg,null); 

			rowObjects = new ArrayList(); 
			foreach (object dgrr in dgra) 
			{ 
				if (dgrr.ToString().EndsWith("DataGridRelationshipRow")==true) 
					rowObjects.Add(dgrr); 
			} 
		}

		//--------------------------------------------------------------------------------
		public int this[int row]
		{
			get
			{
				try
				{
					PropertyInfo pi = rowObjects[row].GetType().GetProperty("Height"); 
					return (int) pi.GetValue(rowObjects[row], null);
				}
				catch
				{
					throw new ArgumentException("invalid row index");
				}
			}
			set
			{
				try
				{
					PropertyInfo pi = rowObjects[row].GetType().GetProperty("Height"); 
					pi.SetValue(rowObjects[row], value, null); 
				}
				catch
				{
					throw new ArgumentException("invalid row index");
				}
			}
		}
	}

	//================================================================================
	public class AutoSizeTextColumnStyle : DataGridTextBoxColumn
	{
		//--------------------------------------------------------------------------------
		protected  override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			DataRow row = ((DataRowView) source.List[rowNum]).Row;
			
			string text = row[MappingName] as string;
			SizeF size = g.MeasureString(text, this.DataGridTableStyle.DataGrid.Font);
			int actualWidth = Convert.ToInt32(size.Width) + 5;
			if (actualWidth > Width)
				Width = actualWidth;
				
			DataGridRowHeightSetter setter = new DataGridRowHeightSetter(DataGridTableStyle.DataGrid);
			int actualHeigth = Convert.ToInt32(size.Height) + 5;
			if (actualHeigth > setter[rowNum])
				setter[rowNum] = actualHeigth;
				
			base.Paint (g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}
	}
}
