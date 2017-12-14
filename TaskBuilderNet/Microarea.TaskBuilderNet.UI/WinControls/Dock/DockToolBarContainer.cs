using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.DockToolBar
{
	//============================================================================
	public partial class DockToolBarContainer : System.Windows.Forms.UserControl
	{
		#region DockToolBarContainer private members
		
		private DockToolBarManager	dockManager = null;
		private bool				showContextMenu = false;
		private int					lastLineCount = 1;

		#region DockToolBarContainer private classes

		#region LineHolder class

		//============================================================================
		private class LineHolder 
		{
			public int			Index = 0;		
			public int			Size = 0;
			public ArrayList	Columns = new ArrayList();			

			public LineHolder(int index)
			{
				Index = index;
			}

			public void AddColumn(ColumnHolder aColumnToAdd) 
			{
				int index = 0;
				foreach(ColumnHolder column in Columns) 
				{
					if(column.Position > aColumnToAdd.Position) 
					{
						Columns.Insert(index, aColumnToAdd);
						break;
					}
					index++;
				}
				if(index == Columns.Count)
					Columns.Add(aColumnToAdd);
			}

			public void Distribute() 
			{
				int pos = 0;
				foreach(ColumnHolder column in Columns) 
				{
					if(column.Position < pos)
						column.Position = pos;	
					pos = column.Position + column.Size;
				}
			}
		}

		#endregion

		#region ColumnHolder class

		//============================================================================
		private class ColumnHolder 
		{
			public int Position = 0;	
			public int Size = 0;		
			public DockToolBarHolder Holder;

			public ColumnHolder(int pos, DockToolBarHolder holder, int size)
			{
				Position = pos;
				Holder = holder;
				Size = size;
			}
		}

		#endregion

		#endregion

		#endregion

		#region DockToolBarContainer public properties

		//----------------------------------------------------------------------------
		public DockToolBarManager DockManager {	get { return dockManager; }	}

		//----------------------------------------------------------------------------
		public bool ShowContextMenu { get { return showContextMenu; } set { showContextMenu = value; }	}

		//----------------------------------------------------------------------------
		public bool ApplyHorizontalLayout { get { return this.Dock != DockStyle.Left && this.Dock != DockStyle.Right; } }

		//----------------------------------------------------------------------------
		public ArrayList Holders 
		{
			get 
			{
				ArrayList holders = new ArrayList();

				foreach(Control aControl in this.Controls)
				{
					if (aControl == null || !(aControl is DockToolBarHolder))
						continue;

					holders.Add(aControl);
				}

				return holders; 
			}	
		}

		#endregion

		#region DockToolBarContainer constructor
		//----------------------------------------------------------------------------
		public DockToolBarContainer(DockToolBarManager aDockManager, DockStyle dockStyle)
		{
			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
			this.UpdateStyles();

			dockManager = aDockManager;

			dockManager.DockStation.Controls.Add(this);

			if(dockStyle == DockStyle.Fill || dockStyle == DockStyle.None)
				dockStyle = DockStyle.Top;

			this.Dock = dockStyle;

			this.SendToBack();

			FitHolders();

			this.BackColor = SystemColors.Control;
		}

		#endregion

		#region DockToolBarContainer private methods

		//----------------------------------------------------------------------------
		private int GetPreferredLine(int lineExtension, DockToolBarHolder holder) 
		{
			int pos;

			if(ApplyHorizontalLayout) 
			{
				pos = holder.PreferredDockedLocation.Y;
				if(pos < 0) 
					return Int32.MinValue;
				if(pos > this.Height) 
					return Int32.MaxValue;
			} 
			else 
			{
				pos = holder.PreferredDockedLocation.X;
				if(pos < 0) 
					return Int32.MinValue;
				if(pos > this.Width) 
					return Int32.MaxValue;
			}

			int line = pos / lineExtension;
			int posLine = line * lineExtension;
			
			if (posLine + 3 > pos)
				return 2*line;

			if (posLine + lineExtension - 3 < pos)
				return 2*line + 2;

			return 2*line + 1;
		}

		//----------------------------------------------------------------------------
		private int GetPreferredPosition(DockToolBarHolder holder) 
		{
			return ApplyHorizontalLayout ? holder.PreferredDockedLocation.X : holder.PreferredDockedLocation.Y;
		}

		//----------------------------------------------------------------------------
		private int GetHolderLineSize(DockToolBarHolder holder) 
		{
			return ApplyHorizontalLayout ? holder.Height : holder.Width;
		}
		
		//----------------------------------------------------------------------------
		private int GetHolderWidth(DockToolBarHolder holder) 
		{
			return ApplyHorizontalLayout ? holder.Width : holder.Height;
		}
		
		//----------------------------------------------------------------------------
		private void FitHolders() 
		{
			Size newSize = new Size(0,0);
			foreach(DockToolBarHolder holder in Holders) 
			{
				if (holder.Visible) 
				{
					if (holder.Right > newSize.Width)
						newSize.Width = holder.Right;
					if (holder.Bottom > newSize.Height)
						newSize.Height = holder.Bottom;
				}
			}

			if(ApplyHorizontalLayout) 
				this.Height = newSize.Height;
			else 
				this.Width = newSize.Width;
		}

		#endregion

		#region DockToolBarContainer protected overridden methods
		
		//----------------------------------------------------------------------------
		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);

			this.SuspendLayout();

			int lineSzForCalc = 23;

			SortedList lineList = new SortedList();
			
			foreach(DockToolBarHolder holder in Holders) 
			{
				if(holder.Visible) 
				{
					int lineIndex = GetPreferredLine(lineSzForCalc, holder);	
					int prefPos = GetPreferredPosition(holder);	
					
					LineHolder line = (LineHolder)lineList[lineIndex];
					if(line == null) 
					{
						line = new LineHolder(lineIndex);
						lineList.Add(lineIndex, line);
					}
					int csize = GetHolderWidth(holder);
					int lsize = GetHolderLineSize(holder);
					
					line.AddColumn(new ColumnHolder(prefPos, holder, csize+1));
					
					if ((line.Size - 1) < lsize)
						line.Size = lsize + 1;
				}
			}

			int pos = 0;
			
			lastLineCount = (lineList.Count != 0) ? lineList.Count : 1;

			for(int i = 0; i < lineList.Count; i++) 
			{
				LineHolder line = (LineHolder)lineList.GetByIndex(i);
				if(line != null) 
				{
					line.Distribute();
					foreach(ColumnHolder col in line.Columns) 
					{
						if(ApplyHorizontalLayout) 
						{
							col.Holder.Location = new Point(col.Position, pos);
							col.Holder.PreferredDockedLocation = new Point(col.Holder.PreferredDockedLocation.X, pos + col.Holder.Height/2);
						}
						else
						{
							col.Holder.Location = new Point(pos, col.Position);
							col.Holder.PreferredDockedLocation = new Point(pos + col.Holder.Width/2, col.Holder.PreferredDockedLocation.Y);
						}
					}
					pos += line.Size+1;
				}
			}
			
			FitHolders();
			
			this.ResumeLayout();
		}

		//----------------------------------------------------------------------------
		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			// Invoke base class implementation
			base.OnMouseUp(e);

			if (showContextMenu && e.Button == MouseButtons.Right) 
				dockManager.CreateContextMenu(this.PointToScreen(new Point(e.X, e.Y)));
		}

		#endregion
	}
}
