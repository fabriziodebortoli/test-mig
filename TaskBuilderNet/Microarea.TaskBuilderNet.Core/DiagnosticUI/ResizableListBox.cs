using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	/// <summary>
	/// ResizableListBox.
	/// Deriva da un Panel perchè la ListView di per sè non è resizible
	/// In questo  modo faccio derivare la ListView (MessagesListBox) da
	/// un control resizable
	/// </summary>
	//=========================================================================
	public partial class ResizableListBox : System.Windows.Forms.Panel
	{
		#region Variabili private

		//esposte
		private ArrayList	items				= new ArrayList();
		private ArrayList	selectedItems		= new ArrayList();
		private ArrayList	selectedItemIndices = new ArrayList();

		//uso interno
		private ArrayList	itemHeights			= new ArrayList();				
		private bool		ctrlPressed			= false;
		private bool		allowMultiSelect	= true;
		private int			lastIndexClicked	= -1;
		
        #endregion

		#region Properties
		public ArrayList Items				 { get { return items; }}
		public ArrayList SelectedItems		 { get { return items; }}
		public ArrayList SelectedItemIndices { get { return selectedItemIndices; }}
		public object SelectedItem
		{
			get 
			{
				if (selectedItems.Count > 0)
					return selectedItems[0];
				else
					return null;
			}
			set 
			{
				int pos = items.IndexOf(value);
				if (pos >= 0)
				{
					//clear list
					selectedItemIndices.Clear();
					selectedItems.Clear();

					//add item
					AddSelectedItem(pos);					
				}
			}
		}

		public int SelectedIndex
		{
			get 
			{
				if (selectedItemIndices.Count > 0)
					return (int)selectedItemIndices[0];
				else
					return -1;
			}
			set
			{
				if ((value < items.Count) && (value >= -1))
					AddSelectedItem(value);
			}
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Quale item l'utente ha selezionato
		/// </summary>
		/// <returns>Indice oggetto selezionato</returns>
		//---------------------------------------------------------------------
		private int ItemHitTest(int X, int Y)
		{
			int posY = this.AutoScrollPosition.Y;

			int count = itemHeights.Count;
			for (int i=0; i < count; i++)
			{
				posY += (int)itemHeights[i];
				if (Y < posY)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Aggiunge un item all'array e fa il fired dell'evento
		/// </summary>
		//---------------------------------------------------------------------
		private void AddSelectedItem(int index)
		{
			if (index == -1)
			{
				selectedItemIndices.Clear();
				selectedItems.Clear();
			}
			else
			{
				selectedItemIndices.Add(index);
				selectedItems.Add(items[index]);
			}

			OnSelectedIndexChanged(new EventArgs());
		}

		/// <summary>
		/// Rimuove un item dall'array
		/// </summary>
		//---------------------------------------------------------------------
		private void RemoveSelectedItem(int index)
		{
			selectedItemIndices.Remove(index);
			selectedItems.Remove(items[index]);
			OnSelectedIndexChanged(new EventArgs());
		}
		#endregion

		#region Virtual Methods
		/// <summary>
		/// Implementazione standard ma senza resize (che faccio io)
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void OnDrawItem( DrawItemEventArgs e)
		{
			e.DrawBackground();
			e.DrawFocusRectangle();			
		
			Rectangle bounds = e.Bounds;
			Color TextColor  = System.Drawing.SystemColors.ControlText;

			//draw selected item background
			if (e.State == DrawItemState.Selected)
			{
				using ( Brush b = new SolidBrush(System.Drawing.SystemColors.Highlight) )
				{
					e.Graphics.FillRectangle(b, e.Bounds);
				}

				TextColor = SystemColors.HighlightText;
			}

			using (SolidBrush TextBrush = new SolidBrush(TextColor))
			{
				e.Graphics.DrawString(items[e.Index].ToString(), this.Font, TextBrush, bounds.Left , bounds.Top);
			}

			if (DrawItem != null)
				DrawItem(this, e);
		}

		/// <summary>
		/// Just a standard implementation.
		/// Subscribe to the event in a derived class to implemet your logic
		/// to resize the items.
		/// </summary>
		//---------------------------------------------------------------------
		protected virtual void OnMeasureItem(MeasureItemEventArgs e)
		{
			e.ItemHeight = this.Font.Height;

			if (MeasureItem != null)
				MeasureItem(this, e);

			itemHeights.Add(e.ItemHeight);
		}

		//---------------------------------------------------------------------
		protected virtual void OnSelectedIndexChanged(EventArgs e)
		{
			if (SelectedIndexChanged != null)
				SelectedIndexChanged(this, e);
		}
		#endregion

		#region Events
		//I delegate sono di .NET
		public event EventHandler			 SelectedIndexChanged;		
		public event MeasureItemEventHandler MeasureItem;
		public event DrawItemEventHandler	 DrawItem;
		#endregion

		#region Costruttore
		//---------------------------------------------------------------------
		public ResizableListBox()
		{
			// We are going to do all of the painting so better 
			// setup the control to use double buffering
			SetStyle
				(
					ControlStyles.AllPaintingInWmPaint|ControlStyles.ResizeRedraw|
					ControlStyles.Opaque|ControlStyles.UserPaint|ControlStyles.DoubleBuffer, 
					true
				);

			//Dafault		
			this.BackColor	= System.Drawing.Color.White;
			this.AutoScroll = true;
			this.HScroll	= false;
		}
		#endregion

		#region Override
		//---------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);
			Graphics g			= pe.Graphics;
			Rectangle bounds	= new Rectangle();
			int posY			= this.AutoScrollPosition.Y;
			int totalPosY = 0;

			//clear background
			using ( Brush b = new SolidBrush(this.BackColor) )
			{
				g.FillRectangle(b, this.ClientRectangle);
			}

			itemHeights.Clear();

			//draw
			int count = items.Count;
			for (int i=0; i < count; i++)
			{
				//measure
				MeasureItemEventArgs measureItem = new MeasureItemEventArgs(g,i);
				OnMeasureItem(measureItem);
				bounds.Location	= new Point(0, posY);
				bounds.Size =
					new System.Drawing.Size
					(
						this.ClientRectangle.Right, 
						(int)itemHeights[i]
					);

				//and draw
				DrawItemState state = (selectedItemIndices.Contains(i)) ? DrawItemState.Selected : DrawItemState.Default;
				DrawItemEventArgs drawItem = new DrawItemEventArgs(g, this.Font, bounds, i, state, this.ForeColor, this.BackColor);
				OnDrawItem(drawItem);
				posY += (int)itemHeights[i];
				totalPosY += (int)itemHeights[i];
			}

			this.AutoScrollMinSize = new Size(this.Width - 50, totalPosY);
		}


		//---------------------------------------------------------------------
		protected override void OnDoubleClick(EventArgs e)
		{
			if (lastIndexClicked < 0)
				return;
			base.OnDoubleClick (e);
		}

		//---------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			//make sure we receive key events
			this.Focus();

			if (e.Button != MouseButtons.Left) 
				return;

			//determine which item was clicked
			lastIndexClicked = ItemHitTest(e.X,e.Y);
		}

		//---------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			//make sure we receive key events
			this.Focus();

			if (e.Button != MouseButtons.Left) 
				return;

			//determine which item was clicked
			int index = ItemHitTest(e.X,e.Y);

			if (index < 0)
				return;

			if (ctrlPressed && allowMultiSelect)
			{
				if (selectedItemIndices.Contains(index))
					RemoveSelectedItem(index);
				else
					AddSelectedItem(index);
			}
			else
			{
				if ((selectedItemIndices.Contains(index)) && (selectedItemIndices.Count == 1))
					return;

				selectedItemIndices.Clear();
				selectedItems.Clear();
				AddSelectedItem(index);
			}

			this.Invalidate();
		}

		//---------------------------------------------------------------------
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			ctrlPressed = e.Control;
		}

		//---------------------------------------------------------------------
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			ctrlPressed = e.Control;
		}
		#endregion

	}
}