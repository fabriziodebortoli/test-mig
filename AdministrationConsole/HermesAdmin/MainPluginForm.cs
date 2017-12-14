using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.IO;
using Microarea.TaskBuilderNet.TbHermesBL.Config;
using System.Xml;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.Console.Plugin.HermesAdmin
{
	public partial class MainPluginForm : Form
	{
		public MainPluginForm()
		{
			InitializeComponent();

			this.lblTitle.Text = this.Text;
		}

		//---------------------------------------------------------------------
	}
}
