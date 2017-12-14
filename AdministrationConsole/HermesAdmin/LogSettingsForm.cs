using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.TbHermesBL.Config;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;

using System.Collections;
using System.IO;

namespace Microarea.Console.Plugin.HermesAdmin
{
	public partial class LogSettingsForm : Form
	{
		HermesSettings oldValue;
		bool processEvents;
		bool dirty;
		Color defColor;
		Color diryColor = Color.RosyBrown;


        private string defaultLogPath(string cmp)
        {
            //fabio: percorso di default
            return Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomAllCompaniesPath(), "Log", "Mail");
        }


		public LogSettingsForm()
        {            
			InitializeComponent();

            this.defColor = this.chkLogging.BackColor; // vediamo cos'è
            this.lblTitle.Text = this.Text;
            this.grpLogging.Enabled = false;
        }

		private void LogSettingsForm_Load(object sender, EventArgs e)
		{
			HermesSettings hs = HermesSettings.Load();
			this.oldValue = hs;
			//if (hs != null && false == string.IsNullOrEmpty(hs.EnabledCompany))
			//    this.oldValue = hs.EnabledCompany; // diamo per scontato ve ne sia una sola, in futuro vedremo
			//else
			//    this.oldValue = null;
			SetSettings(this.oldValue);
			this.btnSave.Enabled = false;
			this.btnUndo.Enabled = false;
			processEvents = true;
		}

		private void SetSettings(HermesSettings hs)
		{
            if (hs == null) return;
            string logPath = hs.LoggingPath;
            bool LogEnabled = hs.LoggerEnabled;
            bool LogRawData = hs.LoggerRawData;

            this.txtLogPath.Text = logPath;
            this.chkLogging.Checked = LogEnabled;
            this.chkLimilabsLog.Checked = LogRawData;
            this.grpLogging.Enabled = LogEnabled;

            this.txtLogPath.BackColor = this.defColor;
            this.chkLogging.BackColor = this.defColor;
            this.chkLimilabsLog.BackColor = this.defColor;
        }

		private HermesSettings GetSettings()
		{
			HermesSettings hs = new HermesSettings();
            hs = HermesSettings.Load();
			hs.LoggingPath = this.txtLogPath.Text;
            hs.LoggerEnabled = this.chkLogging.Checked;
            hs.LoggerRawData = this.chkLimilabsLog.Checked;

            return hs;
		}

		//------------------ logica di controllo ------------------------------

		private void SetDirty(Control ctrl, Func<Control, bool> dirtyChecker)
		{
			bool fieldIsDirty = dirtyChecker(ctrl);
			HermesSettings clone = GetSettings();
			this.dirty = false == clone.Equals(oldValue);
			ctrl.BackColor = fieldIsDirty ? this.diryColor : this.defColor;
			this.btnSave.Enabled = this.dirty;
			this.btnUndo.Enabled = this.dirty;
		}

        private void chkLogging_CheckedChanged(object sender, EventArgs e)
        {
            if (false == processEvents)
                return;
            this.grpLogging.Enabled = this.chkLogging.Checked;
            SetDirty(sender as Control, (x) => ((CheckBox)x).Checked != this.oldValue.LoggerEnabled);
            this.chkLimilabsLog.Checked = false;
            chkLimilabsLog_CheckedChanged(chkLimilabsLog, e);
            if ((this.chkLogging.Checked) && (string.IsNullOrEmpty(this.txtLogPath.Text)))
            {
                string cmp = string.IsNullOrEmpty(this.oldValue.Company) ? "" : this.oldValue.Company;
                txtLogPath.Text = defaultLogPath(cmp);
                txtLogPath_Leave(txtLogPath, e);
            }
        }

        private void chkLimilabsLog_CheckedChanged(object sender, EventArgs e)
        {
            if (false == processEvents)
                return;
            SetDirty(sender as Control, (x) => ((CheckBox)x).Checked != this.oldValue.LoggerRawData);

        }

		//---------------------------------------------------------------------
		private void btnSave_Click(object sender, EventArgs e)
		{
			HermesSettings tmp = GetSettings();

            tmp.Save();
            this.oldValue = tmp;
			this.processEvents = false;
            SetSettings(tmp);
			this.processEvents = true;
			this.dirty = false;
			this.btnSave.Enabled = false;
			this.btnUndo.Enabled = false;
		}

		private void btnUndo_Click(object sender, EventArgs e)
		{
			HermesSettings hs = this.oldValue;
			this.processEvents = false;
			SetSettings(hs);
			this.processEvents = true;
			this.dirty = false;
			this.btnSave.Enabled = false;
			this.btnUndo.Enabled = false;
		}

        private void btnBrowseLog_Click(object sender, EventArgs e)
        {
            DialogResult res = folderBrowserDialog1.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                txtLogPath.Text = folderBrowserDialog1.SelectedPath;
                txtLogPath_Leave(txtLogPath, e);
            }
        }

        private void txtLogPath_Leave(object sender, EventArgs e)
        {
            if (false == processEvents)
                return;

            if (this.txtLogPath.Text.Length > 0)
            {
            if (!this.txtLogPath.Text.EndsWith("\\")) this.txtLogPath.Text += "\\";
                try 
	            {
                    if (!Path.IsPathRooted(this.txtLogPath.Text)) throw new Exception();
                    if (!Directory.Exists(this.txtLogPath.Text))
                    {
                        Directory.CreateDirectory(this.txtLogPath.Text);
                    }
	            }
                catch (Exception)
	            {
		            this.txtLogPath.Text = this.oldValue.LoggingPath;
	            }
            }
            else
            {
                chkLogging.Checked = false;
            }

            SetDirty(sender as Control, (x) => (string)((TextBox)x).Text != this.oldValue.LoggingPath);
        }

        private void txtLogPath_TextChanged(object sender, EventArgs e)
            {
            // qui sistemo il make up, i controlli sono nella _Leave
            txtLogPath.BackColor = this.diryColor;
            this.btnSave.Enabled = true;
            this.btnUndo.Enabled = true;
        }

	}
}
