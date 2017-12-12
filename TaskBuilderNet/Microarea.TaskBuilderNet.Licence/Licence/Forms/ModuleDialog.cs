using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	/// <summary>
	/// Visualizza i controlli per inserire i serial number dei moduli con cal.
	/// </summary>
	//=========================================================================
	public class ModuleDialog : System.Windows.Forms.Form
	{
		#region CONTROLS
		private ListBox		LbSerials;
		private Button		BtnAdd;
		private Button		BtnRemove;
		private Label		LblTotal;
		private Label		LblMessages;
		private Label		LblWrite;
		private Panel		PanelSerial;
		private Label		LblNumber;
		private Label		LblNoChecked;
		private Button		BtnCancel;
		private Button		BtnOk;
		private Label		LblTotalWeb;
		private Label		LblWebNumber;
		private Button BtnModify;
		private ToolTip		toolTip;
		private IContainer	components;
		private SerialTextBoxes StbSerial;
		#endregion

		private ArrayList	serialItemList = new ArrayList();
		private ArrayList	serialItemListOLD = new ArrayList();
		private ArrayList	pKList = new ArrayList();
		private int			totalCal = 0;
		private int totalConcCal = 0;
		private int			totalwebCal = 0;
		private const string postFixed = "Dialog";
		private bool		isMicroarea = true;
		private bool		namedcal = true;
		public bool			NoSerials = false;
		private Label label1;
		private Label LblConcNumber;
		private Button BtnAddPK;
		public bool			modifyNotification = false;
		public event		ModificationManager Modified;
        public event        ExceptionManager ExceptionRaised;

		//---------------------------------------------------------------------
		public IList<SerialNumberInfo> SmsList 
		{ get 
		  {
			  IList<SerialNumberInfo> list = new List<SerialNumberInfo>();
			  foreach (SerialItem si in serialItemList)
				  list.Add(si.Sn);
			  return list; 
		  }
		}
		//---------------------------------------------------------------------
		public ArrayList	PKList 
		{
				get 
		  {
			  ArrayList list = new ArrayList();
			  foreach (string s in pKList)
				  list.Add(s);
			  return list; 
		  }
			set
			{pKList = value;}
		}

		//---------------------------------------------------------------------
		public bool	AllowPKAdding 
		{
			set 
			{
				BtnAddPK.Visible = value;
			}
		}

		//---------------------------------------------------------------------
		public ModuleDialog(bool modifyNotification)
		{
			InitializeComponent();
			BtnAdd.Enabled = BtnRemove.Enabled = BtnModify.Enabled = false;
			this.modifyNotification = modifyNotification;			
		}

		//---------------------------------------------------------------------
		public bool Prepare(SerialsModuleInfo smi, bool isChecked, bool isMicroarea, bool namedcal)
		{
			if (smi == null) return false;
			this.namedcal = namedcal;
			this.isMicroarea = isMicroarea;
			this.Text = String.Format(this.Text, smi.SalesModuleLocalizedName);
			this.Name = BuildPageName(smi.SalesModuleName);
			Tag = smi;
			StbSerial.Modified += new ModificationManager(StbSerial_Modified);
			StbSerial.Completed += new ModificationManager(StbSerial_Completed);
            StbSerial.ExceptionRaised += new ExceptionManager(StbSerial_Exception);
			this.Leave += new EventHandler(Page_LostFocus);
			LblNoChecked.Visible = !isChecked;
			Fill(isChecked);

			if (LbSerials.Items.Count > 0)
				LbSerials.SelectedIndex = 0;
			return true;
		}

		//---------------------------------------------------------------------
		private void BtnModify_Click(object sender, System.EventArgs e)
		{
			if (StbSerial.Complete)
			{
				ShowMessage(LicenceStrings.ModifingSerial, true); 
				return;
			}
			string s = Remove();
			if (s != null && s.Length > 0)
				StbSerial.Fill(s);
		}

		//---------------------------------------------------------------------
		private void LbSerials_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			BtnRemove.Enabled = LbSerials.SelectedIndex != -1;
			BtnModify.Enabled = LbSerials.SelectedIndex != -1 && !StbSerial.Complete;
		}

		//---------------------------------------------------------------------
		private void StbSerial_Modified(object sender, EventArgs e)
		{
			if (Modified != null)
			{
				Modified(sender, e);
				ShowMessage(null, false);
			}

			LbSerials_SelectedIndexChanged(sender, e);

			SerialTextBoxes stb = sender as SerialTextBoxes;
			if (stb == null)
				return ;
			BtnAdd.Enabled = stb.Complete;
		} 
		//---------------------------------------------------------------------
		private void StbSerial_Completed(object sender, EventArgs e)
		{
			SerialTextBoxes stb = sender as SerialTextBoxes;
			if (stb == null)
				return ;
			BtnAdd.Enabled = stb.IsValid;
			if (!stb.IsValid)
				ShowMessage(LicenceStrings.SerialNumberWrongFormat, true);
			SerialsModuleInfo smi = (SerialsModuleInfo)Tag;
		} 

        //---------------------------------------------------------------------
        private void StbSerial_Exception(object sender, ExceptionEventArgs e)
		{
			SerialTextBoxes stb = sender as SerialTextBoxes;
			if (stb == null)
				return ;
                
            if (ExceptionRaised != null)
                ExceptionRaised(sender, e);

		} 
       
		//---------------------------------------------------------------------
		public void Clear()
		{
			StbSerial.Clear();
			ShowMessage(null, false);
		}

		//---------------------------------------------------------------------
		private void Fill(bool isChecked)
		{
			SerialsModuleInfo smi = (SerialsModuleInfo)Tag;
			IList<SerialNumberInfo> serials = smi.SerialList;
			if (serials != null)
			{
				LbSerials.Items.Clear();
				totalCal = totalwebCal = 0;
				foreach (SerialNumberInfo sn in serials)
				{
					int cal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, false,false, namedcal , false);
                    int webcal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, true, false, namedcal, false);
                    int concurrentcal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, false, true, false, false);

					if (!AcceptCALStandardEdition(sn, cal))
					{
						ShowMessage(LicenceStrings.Max3CALSTD, true);
						return;
					}
                    if (!(!LbSerials.Items.Contains(sn.GetSerialWSeparator()) || !isMicroarea))
                        ShowMessage(LicenceStrings.RepeatedSerials, true);
                    LbSerials.Items.Add(sn.GetSerialWSeparator());
					SerialItem si = new SerialItem(sn, cal, webcal);
					RefreshCalCounter(cal, webcal, concurrentcal);
					if (!serialItemList.Contains(si) || !isMicroarea)
						serialItemList.Add(si);
					
				}
			}
			serialItemListOLD = serialItemList.Clone() as ArrayList;
			StbSerial.Activate();
		}

		//---------------------------------------------------------------------
		private bool AcceptCALStandardEdition(SerialNumberInfo sn, int cal)
		{
			if (!isMicroarea) return true;
			return !(SerialNumberInfo.GetEdition(sn) == Edition.Standard && 
					totalCal + cal	> 3);
		}

		//---------------------------------------------------------------------
		private string BuildPageName(string name)
		{
			return String.Concat(name, postFixed);
		}

		//---------------------------------------------------------------------
		public void RefreshCalCounter(int count, int webcount, int concurrentcount)
		{			
			//cal
			if (!(totalCal == Int32.MaxValue && count >= 0))
			{
				if (count == Int32.MaxValue)
					totalCal = Int32.MaxValue;
				else
					totalCal += count;
				string label = String.Empty;
				if (totalCal == Int32.MaxValue)
					label = LicenceStrings.Illimited;
				else
					label = totalCal.ToString();

				LblNumber.Text = label;
			}
			//webcal
			if (!(totalwebCal == Int32.MaxValue && webcount >= 0))
			{
				
				if (webcount == Int32.MaxValue)
					totalwebCal = Int32.MaxValue;
				else
					totalwebCal += webcount;
				string weblabel = String.Empty;
				if (totalwebCal == Int32.MaxValue)
					weblabel = LicenceStrings.Illimited;
				else
					weblabel = totalwebCal.ToString();
				LblWebNumber.Text = weblabel;
			}
			//floating
			if (!(totalConcCal == Int32.MaxValue && concurrentcount >= 0))
			{
				
				if (totalConcCal == Int32.MaxValue)
					totalConcCal = Int32.MaxValue;
				else
					totalConcCal += concurrentcount;
				string c = String.Empty;
				if (totalConcCal == Int32.MaxValue)
					c = LicenceStrings.Illimited;
				else
					c = totalConcCal.ToString();
				LblConcNumber.Text = c;
			}
		}

		//---------------------------------------------------------------------
		private void BtnAdd_Click(object sender, System.EventArgs e)
		{
			ShowMessage(null, false);
			SerialNumberInfo sn = StbSerial.Serial;
			string smsString = sn.GetSerialWSeparator();

			
			if (smsString == null || smsString == String.Empty)
			{
				ShowMessage(LicenceStrings.SerialIncomplete, true); 
				return;
			}
			SerialsModuleInfo smi = (SerialsModuleInfo)Tag;
			//il numero di serie di tipo OFFI lo verifico subito perchè ci sono i messaggi di licenza da accettare.
			if (isMicroarea && !SerialNumberInfo.IsValidMDOfficeLicence(smi.SalesModuleName, sn, smi.CalType))
			{
				ShowMessage(LicenceStrings.InvalidSerialForModule, true); 
				return;
			}
			int cal = 0;
			int webcal = 0;
			int concurrentcal = 0;
			
			try 
			{
                cal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, false, false, namedcal, false);
                webcal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, true, false, namedcal, false);
                concurrentcal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, false, true, false, false);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(LicenceStrings.SerialNumberWrongFormat + smsString + Environment.NewLine + exc.Message);
				ShowMessage(LicenceStrings.SerialNumberWrongFormat, true);
				return;
			}
			
			if (!AcceptCALStandardEdition(sn, cal))
			{
				ShowMessage(LicenceStrings.Max3CALSTD, true);
				return;
			}

			SerialItem si = new SerialItem(sn, cal, webcal);
			if (!isMicroarea || SerialOK(si))//se è di un altro produttore non controllo i doppioni
			{
				//Se è un modulo per accesso web da terze parti devo obbligare 
				//ad inserire anche la producerKey per ogni serial number inserito.
				if (
					((SerialsModuleInfo)Tag).CalType == CalTypeEnum.TPGate ||
					(
					((	SerialsModuleInfo)Tag).CalType == CalTypeEnum.AutoFunctional &&
						String.Compare(SerialNumberInfo.GetModuleShortNameFromSerial(sn, ((SerialsModuleInfo)Tag).CalType), String.Empty) ==0 && 
						SerialNumberInfo.IsTPCal(sn, ((SerialsModuleInfo)Tag).CalType))
					)
					
				{
					ArrayList l = new ArrayList();
					if (si.Sn.PK != null && si.Sn.PK.Trim().Length > 0)
						l.Add(si.Sn.PK);
					ProducerKey pkForm = new ProducerKey(l);
					pkForm.StartPosition = FormStartPosition.CenterParent;
					pkForm.ShowDialog(this);
					IList pks = pkForm.PKs;
					if (pks != null && pks.Count> 0)
					si.Sn.PK = pks[0] as string;

				}
				serialItemList.Add(si);
				LbSerials.Items.Add(smsString);//aggiungo un oggetto che preveda anche la PK
				RefreshCalCounter(cal, webcal, concurrentcal);
				StbSerial.Clear();
				
				StbSerial.Activate();
				if (Modified != null)
					Modified(sender, e);

			}
			else ShowMessage(LicenceStrings.SerialExists, true);  
			
		}


//i serial number k non sono mettibili su seriali normali, ma forse non è qui che va messo il controllo @ilaria        
        //---------------------------------------------------------------------
        private bool SerialOK(SerialItem si)
        {
            SerialsModuleInfo smi = (SerialsModuleInfo)Tag;
            foreach (SerialItem s in serialItemList)
            {
                if (s.Equals(si) || (s.Sn.IsSpecial(smi.CalType)))
                    if (!s.Sn.IsDeveloperPlus(smi.CalType))
                        return false; //pero se sono seriali di tbs va bene
                    else if (s.Sn.IsDeveloperPlusK(smi.CalType) && si.Sn.IsDeveloperPlusUser(smi.CalType) ||
                        si.Sn.IsDeveloperPlusK(smi.CalType) && s.Sn.IsDeveloperPlusUser(smi.CalType))
                        return false;
            }
			return true;
		}

		//---------------------------------------------------------------------
		private void Page_LostFocus(object sender, EventArgs e)
		{
			ShowMessage(null, false);
		}

		//---------------------------------------------------------------------
		private void BtnRemove_Click(object sender, EventArgs e)
		{
			Remove();
		}

		//---------------------------------------------------------------------
		private void ShowMessage(string text, bool show)
		{
			LblMessages.Text	= text;
			LblMessages.Visible = show;
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			serialItemList = serialItemListOLD;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
            if (StbSerial.Complete)
                if (MessageBox.Show(String.Format(LicenceStrings.SerialNotAdded, StbSerial.SerialString), LicenceStrings.MsgTitleInfo, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;
			this.DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
		private string Remove()
		{
			string s = null;
			ShowMessage(null, false);
			if (LbSerials.SelectedItem == null)
			{
				ShowMessage(LicenceStrings.NotSelectedItem, true); 
				return s;
			}
			s = LbSerials.SelectedItem.ToString();
			SerialNumberInfo sn = new SerialNumberInfo(s);
			int cal = 0;
			int webcal = 0;
			int concurrentcal = 0;
			SerialsModuleInfo smi = (SerialsModuleInfo)Tag;
            cal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, false, false, namedcal, false);
            webcal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, true, false, namedcal, false);
            concurrentcal = SerialNumberInfo.GetCalNumberFromSerial(smi.SalesModuleName, sn, smi.CalType, false, true, false, false);

			SerialItem si = new SerialItem(sn, cal, webcal);
			if (serialItemList.Contains(si))
			{
				serialItemList.Remove(si);

                object o = LbSerials.SelectedItem;
                while (LbSerials.Items.Contains(o))
                { LbSerials.Items.Remove(o); }//faccio while così li tolgo tutti se fossero duplicati perchè se no ne toglie solo uno e poi non ci riesce più perchè dalla table invece è statao tolto l'unico che era stato inserito.
				
				RefreshCalCounter(cal * -1, webcal * -1, concurrentcal* -1);
				if (Modified != null)
					Modified(null, null);
			}
			if (LbSerials.Items.Count> 0)
				LbSerials.SelectedIndex = LbSerials.Items.Count-1;
			return s;
		}

		//---------------------------------------------------------------------
		private void LnkPK_LinkClicked(object sender, EventArgs e)
		{
			ProducerKey pkForm = new ProducerKey(pKList);
			pkForm.StartPosition = FormStartPosition.CenterParent;
			DialogResult res = pkForm.ShowDialog(this);
			if (res == DialogResult.OK)
			{
				pKList = new ArrayList(pkForm.PKs);
				if (Modified != null)
					Modified(this, EventArgs.Empty);
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		//---------------------------------------------------------------------
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModuleDialog));
            this.LbSerials = new System.Windows.Forms.ListBox();
            this.LblMessages = new System.Windows.Forms.Label();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.BtnRemove = new System.Windows.Forms.Button();
            this.LblTotal = new System.Windows.Forms.Label();
            this.PanelSerial = new System.Windows.Forms.Panel();
            this.StbSerial = new Microarea.TaskBuilderNet.Licence.Licence.Forms.SerialTextBoxes();
            this.LblNumber = new System.Windows.Forms.Label();
            this.LblWrite = new System.Windows.Forms.Label();
            this.LblNoChecked = new System.Windows.Forms.Label();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOk = new System.Windows.Forms.Button();
            this.LblTotalWeb = new System.Windows.Forms.Label();
            this.LblWebNumber = new System.Windows.Forms.Label();
            this.BtnModify = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.LblConcNumber = new System.Windows.Forms.Label();
            this.BtnAddPK = new System.Windows.Forms.Button();
            this.PanelSerial.SuspendLayout();
            this.SuspendLayout();
            // 
            // LbSerials
            // 
            resources.ApplyResources(this.LbSerials, "LbSerials");
            this.LbSerials.Name = "LbSerials";
            this.LbSerials.SelectedIndexChanged += new System.EventHandler(this.LbSerials_SelectedIndexChanged);
            // 
            // LblMessages
            // 
            resources.ApplyResources(this.LblMessages, "LblMessages");
            this.LblMessages.ForeColor = System.Drawing.Color.Red;
            this.LblMessages.Name = "LblMessages";
            // 
            // BtnAdd
            // 
            resources.ApplyResources(this.BtnAdd, "BtnAdd");
            this.BtnAdd.Name = "BtnAdd";
            this.toolTip.SetToolTip(this.BtnAdd, resources.GetString("BtnAdd.ToolTip"));
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // BtnRemove
            // 
            resources.ApplyResources(this.BtnRemove, "BtnRemove");
            this.BtnRemove.Name = "BtnRemove";
            this.toolTip.SetToolTip(this.BtnRemove, resources.GetString("BtnRemove.ToolTip"));
            this.BtnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
            // 
            // LblTotal
            // 
            resources.ApplyResources(this.LblTotal, "LblTotal");
            this.LblTotal.Name = "LblTotal";
            // 
            // PanelSerial
            // 
            this.PanelSerial.Controls.Add(this.StbSerial);
            resources.ApplyResources(this.PanelSerial, "PanelSerial");
            this.PanelSerial.Name = "PanelSerial";
            // 
            // StbSerial
            // 
            this.StbSerial.BackColor = System.Drawing.SystemColors.Window;
            this.StbSerial.Cursor = System.Windows.Forms.Cursors.IBeam;
            resources.ApplyResources(this.StbSerial, "StbSerial");
            this.StbSerial.Name = "StbSerial";
            // 
            // LblNumber
            // 
            resources.ApplyResources(this.LblNumber, "LblNumber");
            this.LblNumber.Name = "LblNumber";
            // 
            // LblWrite
            // 
            resources.ApplyResources(this.LblWrite, "LblWrite");
            this.LblWrite.Name = "LblWrite";
            // 
            // LblNoChecked
            // 
            resources.ApplyResources(this.LblNoChecked, "LblNoChecked");
            this.LblNoChecked.ForeColor = System.Drawing.Color.Red;
            this.LblNoChecked.Name = "LblNoChecked";
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnOk
            // 
            resources.ApplyResources(this.BtnOk, "BtnOk");
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // LblTotalWeb
            // 
            resources.ApplyResources(this.LblTotalWeb, "LblTotalWeb");
            this.LblTotalWeb.Name = "LblTotalWeb";
            // 
            // LblWebNumber
            // 
            resources.ApplyResources(this.LblWebNumber, "LblWebNumber");
            this.LblWebNumber.Name = "LblWebNumber";
            // 
            // BtnModify
            // 
            resources.ApplyResources(this.BtnModify, "BtnModify");
            this.BtnModify.Name = "BtnModify";
            this.toolTip.SetToolTip(this.BtnModify, resources.GetString("BtnModify.ToolTip"));
            this.BtnModify.Click += new System.EventHandler(this.BtnModify_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // LblConcNumber
            // 
            resources.ApplyResources(this.LblConcNumber, "LblConcNumber");
            this.LblConcNumber.Name = "LblConcNumber";
            // 
            // BtnAddPK
            // 
            resources.ApplyResources(this.BtnAddPK, "BtnAddPK");
            this.BtnAddPK.Name = "BtnAddPK";
            this.BtnAddPK.UseVisualStyleBackColor = true;
            this.BtnAddPK.Click += new System.EventHandler(this.LnkPK_LinkClicked);
            // 
            // ModuleDialog
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.Lavender;
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.BtnAddPK);
            this.Controls.Add(this.BtnAdd);
            this.Controls.Add(this.LblConcNumber);
            this.Controls.Add(this.LblWebNumber);
            this.Controls.Add(this.LblTotalWeb);
            this.Controls.Add(this.LblNumber);
            this.Controls.Add(this.LbSerials);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.LblNoChecked);
            this.Controls.Add(this.PanelSerial);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LblTotal);
            this.Controls.Add(this.BtnRemove);
            this.Controls.Add(this.LblWrite);
            this.Controls.Add(this.LblMessages);
            this.Controls.Add(this.BtnModify);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ModuleDialog";
            this.ShowInTaskbar = false;
            this.PanelSerial.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		
		//=========================================================================
		internal class SerialItem
		{
			public SerialNumberInfo Sn = null;
			public int Cal = 0;
			public int WebCal = 0;

			//---------------------------------------------------------------------
			public SerialItem(SerialNumberInfo sn, int cal, int webcal)
			{
				Sn = sn;
				Cal = cal;
				WebCal = webcal;
			}

			//---------------------------------------------------------------------
			public override bool Equals(object obj)
			{
				if (obj == null || !(obj is SerialItem))
					return false;

				SerialItem comp = obj as SerialItem;
				return (this.Sn.Equals(comp.Sn));
			}
				
			//---------------------------------------------------------------------
			public override int GetHashCode()
			{
				return (Sn + "," + Cal+ "," + WebCal).GetHashCode();
			}
		}

		
	}
}

