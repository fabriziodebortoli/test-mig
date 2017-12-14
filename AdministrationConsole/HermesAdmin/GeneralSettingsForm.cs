using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.TbHermesBL.Config;
using System.Collections;
using System.IO;

namespace Microarea.Console.Plugin.HermesAdmin
{
	public partial class GeneralSettingsForm : Form
	{
		HermesSettings oldValue;
		bool processEvents;
		bool dirty;
		Color defColor;
		Color diryColor = Color.RosyBrown;

		//-------------------------------------------------------
		public delegate List<string> GetCompanyListDelegate();
		public GetCompanyListDelegate GetCompanyList { get; set; }

		List<string> compList;

		public GeneralSettingsForm()
        {            
			InitializeComponent();

			this.defColor = this.chkEnable.BackColor; // vediamo cos'è
			this.cmbEnabledCompany.DisplayMember = "Company"; // giusto?
			this.lblTitle.Text = this.Text;
			this.grpEnable.Enabled = false;
            this.txtTickTime.Text = "1";
            this.vScrollBar1.Value = -1;
		}

		private void GeneralSettingsForm_Load(object sender, EventArgs e)
		{
			// carica company abilitate
			this.compList = this.GetCompanyList();
			this.cmbEnabledCompany.DataSource = this.compList;

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
            string company = hs.Company;
            bool srvEnabled = hs.Enabled;
            Int32 iTick = hs.TickRate;

			if (false == string.IsNullOrEmpty(company))
			{
				//this.cmbEnabledCompany.SelectedItem = companyItems.Find(x => string.Compare(x.Company, company, StringComparison.InvariantCultureIgnoreCase) == 0);
				this.cmbEnabledCompany.SelectedItem = compList.Find(x => string.Compare(x, company, StringComparison.InvariantCultureIgnoreCase) == 0);

				// cerco nel datasource, non nella collection originale, perché
				// se la combo fosse impostata con auto-sort l'ordine potrebbe essere diverso
				IList<string> ds = (IList<string>)this.cmbEnabledCompany.DataSource;
				//int idx = -1;
				for (int i = 0; i < ds.Count; ++i)
				{
					string cmp = ds[i];
					if (string.Compare(company, cmp, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						this.cmbEnabledCompany.SelectedIndex = i;
						break;
					}
				}
			}
			else
				this.cmbEnabledCompany.SelectedIndex = -1;

            this.chkEnable.Checked = srvEnabled;
            this.grpEnable.Enabled = srvEnabled;

            if (iTick > 0)
            {
                this.txtTickTime.Text = iTick.ToString();
                vScrollBar1.Value = iTick * (-1);
            }

			this.cmbEnabledCompany.BackColor = this.defColor;
			this.chkEnable.BackColor = this.defColor;
            this.txtTickTime.BackColor = this.defColor;
		}

		private HermesSettings GetSettings()
		{
			HermesSettings hs = new HermesSettings();
            hs = HermesSettings.Load();
            hs.Enabled = this.chkEnable.Checked;
			hs.Company = (string)this.cmbEnabledCompany.SelectedItem;
            hs.TickRate = Convert.ToInt32(this.txtTickTime.Text);

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

		private void chkEnable_CheckedChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;
			this.grpEnable.Enabled = this.chkEnable.Checked;
			SetDirty(sender as Control, (x) => ((CheckBox)x).Checked != this.oldValue.Enabled);
		}

		private void cmbEnabledCompany_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (false == processEvents)
				return;

            SetDirty(sender as Control, (x) => (string)((ComboBox)x).SelectedItem != this.oldValue.Company);
        }


        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (false == processEvents)
                return;
            txtTickTime.Text = Math.Abs(vScrollBar1.Value).ToString();
        }

        private void txtTickTime_TextChanged(object sender, EventArgs e)
        {
            if (false == processEvents)
                return;
            int tickMinutesMinimum = 1;
			int tickMinutesMaximum = 60;
            int aVal;

			if (false == Int32.TryParse(txtTickTime.Text, out aVal))
                aVal = this.oldValue.TickRate;
            if (aVal < tickMinutesMinimum) aVal = tickMinutesMinimum;
            else if (aVal > tickMinutesMaximum)	aVal = tickMinutesMaximum;

            txtTickTime.Text = aVal.ToString();
            vScrollBar1.Value = aVal * (-1);
            SetDirty(sender as Control, (x) => (string)((TextBox)x).Text != this.oldValue.TickRate.ToString());
        }

        //---------------------------------------------------------------------
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


	}
}
