using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Xml;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    public partial class SetFunctionParameters : Form
    {
        FunctionPrototype fun;
        DataTable dt = new DataTable();
        public string CommandParameters;

        class Col
        {
            public const string Name = "Name";
            public const string Title = "Title";
            public const string Value = "Value";
            public const string Type = "Type";
        }

        public SetFunctionParameters(FunctionPrototype f, string cmdParameters)
        {
            fun = f;
            CommandParameters = cmdParameters;

            InitializeComponent();
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        protected override void OnLoad(System.EventArgs e)
        {
            // Invoke base class implementation
            base.OnLoad(e);

            ParametersList pars = fun.Parameters;
            
            DataColumn col = dt.Columns.Add(Col.Name, typeof(System.String));
            col.Caption = TaskSchedulerWindowsControlsStrings.ParName;
            col.ReadOnly = true;
            
            col = dt.Columns.Add(Col.Title, typeof(System.String));
            col.Caption = TaskSchedulerWindowsControlsStrings.ParTitle;
            col.ReadOnly = true;

            col = dt.Columns.Add(Col.Type, typeof(System.String));
            col.Caption = TaskSchedulerWindowsControlsStrings.ParType;
            col.ReadOnly = true;
           
            col = dt.Columns.Add(Col.Value, typeof(System.String));
            col.Caption = TaskSchedulerWindowsControlsStrings.ParValue;
             
            //----
            XmlDocument doc = null;
            XmlElement root = null;
            if (!CommandParameters.IsNullOrEmpty())
            {
                doc = new XmlDocument();
				doc.LoadXml(CommandParameters);
				root = doc.DocumentElement;
            }

            foreach (Parameter p in pars)
            {
                DataRow r = dt.NewRow();
                    r[Col.Name] = p.Name;
                    r[Col.Title] = p.Title;
                    r[Col.Type] = p.TbType;

                if (root == null)
                    r[Col.Value] = p.ValueString;
                else
                {
                    XmlElement xpar = root.SelectSingleNode("Param[@name=\"" + p.Name + "\"]") as XmlElement;
                    if (xpar != null)
                        r[Col.Value] = xpar.GetAttribute(WebMethodsXML.Attribute.Value);
                }

                dt.Rows.Add(r);
            }
           //----
            this.dataGridViewParameters.DataSource = dt;

            this.dataGridViewParameters.AllowUserToAddRows = false;
            this.dataGridViewParameters.AllowUserToDeleteRows = false;
            this.dataGridViewParameters.AllowUserToOrderColumns = false;
            this.dataGridViewParameters.Columns[Col.Name].DefaultCellStyle.BackColor = Color.LightGray;
            this.dataGridViewParameters.Columns[Col.Title].DefaultCellStyle.BackColor = Color.LightGray;
            this.dataGridViewParameters.Columns[Col.Type].DefaultCellStyle.BackColor = Color.LightGray;

            this.dataGridViewParameters.Update();
        }

        //---------------------------------------------------------------------
        private void OkBtn_Click(object sender, EventArgs e)
        {
            ParametersList pars = fun.Parameters;
            for  (int i = 0; i < pars.Count; i++)
            {
                Parameter p = pars[i];
                DataRow r = dt.Rows[i];

                string sval = r[Col.Value] as string;

                p.ValueString = sval;
            }

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.AppendChild(doc.CreateElement(WebMethodsXML.Element.Function));
            fun.Parameters.Unparse(doc.DocumentElement);

            CommandParameters = doc.OuterXml;
       }
    }
}
