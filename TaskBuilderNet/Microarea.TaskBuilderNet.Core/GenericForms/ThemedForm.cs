using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.GenericForms
{
	//================================================================================
	public partial class ThemedForm : Form
	{
		ITheme theme = null;

		//--------------------------------------------------------------------------------
		public ThemedForm()
		{
			InitializeComponent();
			
		
		}

		//--------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			ApplyTheme();
		}

		//--------------------------------------------------------------------------------
		public virtual void ApplyTheme()
		{
			if (DesignMode)
				return;

			theme = DefaultTheme.GetTheme();
			if (theme == null)
				return;

			Color color = theme.GetThemeElementColor("WinFormDefaultBackgroundColor");
			if (!color.IsEmpty)
				this.BackColor = color;
		}
	}
}
