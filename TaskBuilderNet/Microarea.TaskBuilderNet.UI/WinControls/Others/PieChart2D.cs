using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Others
{
	///<summary>
	/// UserControl che consente di disegnare un grafico a torta 2D
	/// specificando le "fette" e i colori ad esse associate
	/// Le proprieta' esposte servono al programmatore per inizializzare il controllo:
	/// 1) Slices: i valori che identificano le "fette"
	/// 2) Colors: i colori che rispettivamente verranno associati alle "fette"
	///</summary>
	//=========================================================================
	public partial class PieChart2D : UserControl
	{
		// Properties
		//----------------------------------------------------------------------
		public float[] Slices { get; set; }
		public Color[] Colors { get; set; }

		//------------------------------------ ----------------------------------
		public PieChart2D()
		{
			InitializeComponent();
		}

		//----------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			DrawPie();
			base.OnPaint(e);
		}

		//----------------------------------------------------------------------
		public void DrawPie()
		{
			List<float> degrees = new List<float>();
			float total = 0.0F;

			// se non sono state specificate delle "fette" non segnalo errore
			if (Slices == null || Slices.Length == 0)
				Slices[0] = 1.0F;

			// se il numero delle "fette" e' maggiore dei colori segnalo errore
			if (Colors == null || Colors.Length == 0 || Slices.Length > Colors.Length)
				return;

			// calcolo il totale delle "fette"
			foreach (float slice in Slices)
				total += slice;

			// memorizzo in una lista di appoggio i gradi da associare ad ogni "fetta"
			foreach (float slice in Slices)
			{
				float deg = (slice / total) * 360;
				degrees.Add(deg);
			}

			Rectangle rect = ClientRectangle;
			rect.Inflate(-1, -1);

			using (Graphics g = this.CreateGraphics())
			{
				using (Pen p = new Pen(this.BackColor, 2))
				{
					g.Clear(this.BackColor);

					// per ogni percentuale disegno la corrispondente fetta nel pie chart, cambiando colore ogni volta
					float degrProg = 0.0F;
					for (int i = 0; i < degrees.Count; i++)
					{
						using (Brush b = new SolidBrush(Colors[i]))
						{
							float currSlice = degrees[i];

							g.DrawPie(p, rect, degrProg, currSlice);
							g.FillPie(b, rect, degrProg, currSlice);

							degrProg += currSlice;
						}
					}
				}
			}
		}
	}
}
