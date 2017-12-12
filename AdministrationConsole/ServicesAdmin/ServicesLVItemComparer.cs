using System;
using System.Collections;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	// Implements the manual sorting of items by columns in a ListView
	//================================================================================
	public class ServicesLVItemComparer : System.Collections.IComparer
	{
		// Specifies the column to be sorted
		private int ColumnToSort;

		// Specifies the order in which to sort (i.e. 'Ascending').
		private SortOrder OrderOfSort;

		// Case insensitive comparer object
		private CaseInsensitiveComparer ObjectCompare;

		//--------------------------------------------------------------------------------
		public int SortColumn { set { ColumnToSort = value; } get { return ColumnToSort; } }
		//--------------------------------------------------------------------------------
		public SortOrder Order { set { OrderOfSort = value; } get { return OrderOfSort; } }

		//--------------------------------------------------------------------------------
		public ServicesLVItemComparer()
		{
			// Initialize the column to '0'
			ColumnToSort = 0;

			// Initialize the sort order to 'none'
			OrderOfSort = SortOrder.None;

			// Initialize the CaseInsensitiveComparer object
			ObjectCompare = new CaseInsensitiveComparer();
		}

		//--------------------------------------------------------------------------------
		public int Compare(object x, object y)
		{
			int compareResult;
			ListViewItem listviewX, listviewY;

			// Cast the objects to be compared to ListViewItem objects
			listviewX = (ListViewItem)x;
			listviewY = (ListViewItem)y;

			// Case insensitive Compare
			compareResult = String.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

			// Calculate correct return value based on object comparison
			if (OrderOfSort == SortOrder.Ascending)
				return compareResult; // Ascending sort is selected, return normal result of compare operation
			else if (OrderOfSort == SortOrder.Descending)
				return (-compareResult); // Descending sort is selected, return negative result of compare operation
			else
				return 0; // Return '0' to indicate they are equal
		}
	}

	///<summary>
	/// Consente di visualizzare la freccina sul column header di una listview, in modo da visualizzare
	/// il sort Ascending o Descending
	/// Per fare in modo di fare apparire la freccina, si tratta di richiamare nell'evento myListView_ColumnClick
	/// la seguente riga di codice: 
	/// myListView.SetSortIcon(0, SortOrder.Ascending);
	/// specificando l'indice della colonna e il SortOrder da applicare
	///</summary>
	//================================================================================
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public static class ListViewExtensions
	{
		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		private struct LVCOLUMN
		{
			public Int32 mask;
			public Int32 cx;
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
			public string pszText;
			public IntPtr hbm;
			public Int32 cchTextMax;
			public Int32 fmt;
			public Int32 iSubItem;
			public Int32 iImage;
			public Int32 iOrder;
		}

		private const Int32 HDI_FORMAT = 0x4;
		private const Int32 HDF_SORTUP = 0x400;
		private const Int32 HDF_SORTDOWN = 0x200;
		private const Int32 LVM_GETHEADER = 0x101f;
		private const Int32 HDM_GETITEM = 0x120b;
		private const Int32 HDM_SETITEM = 0x120c;

		//--------------------------------------------------------------------------------
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		//--------------------------------------------------------------------------------
		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessage")]
		private static extern IntPtr SendMessageLVCOLUMN(IntPtr hWnd, Int32 Msg, IntPtr wParam, ref LVCOLUMN lPLVCOLUMN);

		//--------------------------------------------------------------------------------
		public static void SetSortIcon(this ListView ListViewControl, int ColumnIndex, SortOrder Order)
		{
			IntPtr ColumnHeader = SendMessage(ListViewControl.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

			for (int ColumnNumber = 0; ColumnNumber <= ListViewControl.Columns.Count - 1; ColumnNumber++)
			{
				IntPtr ColumnPtr = new IntPtr(ColumnNumber);
				LVCOLUMN lvColumn = new LVCOLUMN();
				lvColumn.mask = HDI_FORMAT;

				SendMessageLVCOLUMN(ColumnHeader, HDM_GETITEM, ColumnPtr, ref lvColumn);

				if (!(Order == SortOrder.None) && ColumnNumber == ColumnIndex)
				{
					switch (Order)
					{
						case SortOrder.Ascending:
							lvColumn.fmt &= ~HDF_SORTDOWN;
							lvColumn.fmt |= HDF_SORTUP;
							break;
						case SortOrder.Descending:
							lvColumn.fmt &= ~HDF_SORTUP;
							lvColumn.fmt |= HDF_SORTDOWN;
							break;
					}
				}
				else
					lvColumn.fmt &= ~HDF_SORTDOWN & ~HDF_SORTUP;

				SendMessageLVCOLUMN(ColumnHeader, HDM_SETITEM, ColumnPtr, ref lvColumn);
			}
		}
	}
}
