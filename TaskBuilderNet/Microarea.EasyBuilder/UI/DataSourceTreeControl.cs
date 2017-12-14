using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.EasyBuilder.MVC;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.EasyBuilder.CppData;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.UI
{
	partial class DataSourceTreeControl : UserControl
	{


		public TreeView DSTreeView
		{
			get { return treeViewManager; }
		}

		public TreeFinder DSTreeFinder
		{
			get { return treeFinder; }
		}

		public DataSourceTreeControl()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		internal void tsText_TextChanged(object sender, EventArgs e)
		{
			DSTreeFinder.RestartSelectNext();
		}

	
		//---------------------------------------------------------------------
		public void dstc_Load(object sender, System.EventArgs e)
		{
			this.ActiveControl = treeFinder.TsText.TextBox;
		}
	}
}
