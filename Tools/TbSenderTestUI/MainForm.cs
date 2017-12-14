using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.TbSenderBL;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;

namespace TbSenderTestUI
{
	public partial class MainForm : Form
	{
		string server = "localhost";
		string installation = "Development";
		int port = 80;

		PLProxy.PLProxy plProxy = new PLProxy.PLProxy();

		//-------------------------------------------------------------------------------
		public MainForm()
		{
			InitializeComponent();
			plProxy.Url = string.Format("http://{0}:{1}/{2}/TbSender/PLProxy.asmx", server, port.ToString(), installation);
			plProxy.Timeout = 600000 * 2;
		}

		//-------------------------------------------------------------------------------
		private void btnAddPdf_Click(object sender, EventArgs e)
		{
			TB_MsgQueue msg = msgEditor.CreateEditedMessage();
			//msg.isvalid TODO

			string company = GetSelectedCompany();
			TB_MsgQueue.SaveNewMessage(msg, company);

			MessageBox.Show(this,
				"A new record has been saved :-)",
				"Saved",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		//-------------------------------------------------------------------------------
		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		//-------------------------------------------------------------------------------
		private void btnTest_Click(object sender, EventArgs e)
		{
			string company = "Az38";
			int max = 500;
			for (int i = max; i > 0; --i)
			{
				List<TB_MsgLots> lots = TB_MsgLots.GetLotsToUpload(company);
				Console.WriteLine("iteration " + i.ToString());
				Application.DoEvents();
			}

			string fileName = this.txtTestData.Text;
			//MsgHelper.UploadFileData(fileName);
			//MsgHelper.Test_BuildCover_01();
		}

		//-------------------------------------------------------------------------------
		private void MainForm_Load(object sender, EventArgs e)
		{
			try
			{
				List<string> companies = LoginManagerConnector.GetSubscribedCompaniesDescriptors();
				this.cmbCurrentCompany.DataSource = companies;
			}
			catch (Exception ex)
			{
				string exTxt = ex.ToString();
				Console.WriteLine(exTxt);
			}

			string company = GetSelectedCompany();

			//try
			//{
			//    PostaliteSettingsHelper.Load();
			//}
			//catch (Exception ex)
			//{
			//    string exTxt = ex.ToString();
			//    Console.WriteLine(exTxt);
			//}

			MA_Company cmp = MA_Company.Find(company);
			SubscriberInfo subInfo = new SubscriberInfo()
			{
				Subscriber_Address = cmp.Address,
				Subscriber_City = cmp.City,
				Subscriber_County = cmp.County,
				Subscriber_EMail = cmp.EMail,
				Subscriber_FaxNumber = cmp.Fax,
				Subscriber_TelephoneNumber = cmp.Telephone1,
				//FiscalCode = cmp.SubscriberFiscalCode
				//FiscalCode = cmp.FiscalCode,
				//Gender = cmp.s
				Subscriber_State = cmp.Country,
				Subscriber_SurNameCompanyName = cmp.CompanyName,
				Subscriber_VatNumber = cmp.TaxIdNumber,
				Subscriber_ZipCode = cmp.ZIPCode
			};
			this.subscriptionEditor.SubscriberInfo = subInfo;

			this.cmbCurrentCompany.SelectedIndexChanged += delegate(object snd, EventArgs ev) { UpdateMessagesGrids(); };

			UpdateMessagesGrids();
		}

		//-------------------------------------------------------------------------------
		private void UpdateMessagesGrids()
		{
			string company = GetSelectedCompany();
			this.dgvMessageQueue.DataSource = TB_MsgQueue.GetMessages(company);
			this.dgvLotsQueue.DataSource = TB_MsgLots.GetLots(company);
		}

		//-------------------------------------------------------------------------------
		private void btnRefreshViews_Click(object sender, EventArgs e)
		{
			UpdateMessagesGrids();
		}

		//-------------------------------------------------------------------------------
		private void btnAllot_Click(object sender, EventArgs e)
		{
			string company = GetSelectedCompany();
			string err;
			plProxy.AllotMessages(company, errorMessage: out err);

			UpdateMessagesGrids();
		}

		//-------------------------------------------------------------------------------
		private void btnTick_Click(object sender, EventArgs e)
		{
			plProxy.DoTick();
			UpdateMessagesGrids();
		}

		//-------------------------------------------------------------------------------
		private void btnUploadSingleLot_Click(object sender, EventArgs e)
		{
			if (this.dgvLotsQueue.SelectedRows.Count == 0)
			{
				MessageBox.Show(this, "Select a row first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			DataGridViewRow row = this.dgvLotsQueue.SelectedRows[0];
			TB_MsgLots lot = row.DataBoundItem as TB_MsgLots;
			if (lot == null)
				return; // shouldn't happen
			if (lot.HasAnAlreadyUploadedStatus())
			{
				MessageBox.Show(this, "This lot has already been uploaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// -- da qui in poi potrebbe essere lavoro per il WS, che per TB non può usare complex data types
			// in pratica mi può passare un lodID, e basta.

			//plProxy.TestBreakpoint();
			string company = GetSelectedCompany();
			string errorMessage;
			plProxy.UploadSingleLot(company, lot.LotID, out errorMessage);

			if (errorMessage != null)
				MessageBox.Show(this, errorMessage);

			UpdateMessagesGrids();
		}

		//-------------------------------------------------------------------------------
		private string GetSelectedCompany()
		{
			return (string)cmbCurrentCompany.SelectedItem;
		}

		//-------------------------------------------------------------------------------
		private void btnDoSomething_Click(object sender, EventArgs e)
		{
		}

		//-------------------------------------------------------------------------------
		private void btnSubscribe_Click(object sender, EventArgs e)
		{
			ISubscriberInfo subscriberInfo = this.subscriptionEditor.SubscriberInfo;
			// uso un oggetto complesso qui per comodità nel mock UI, ma tb userà i parametri distinti			
			string err;
			string login;
			string token = plProxy.Subscribe
			    (
				companyName: subscriberInfo.Subscriber_SurNameCompanyName,
				name: "un nome a caso",
				city: subscriberInfo.Subscriber_City,
				address: subscriberInfo.Subscriber_Address,
				zipCode: subscriberInfo.Subscriber_ZipCode,
				county: subscriberInfo.Subscriber_County,
				country: subscriberInfo.Subscriber_State,
				telephoneNumber: "123",
				faxNumber: "456",
				fiscalCode: "aaa",
				vatNumber: subscriberInfo.Subscriber_VatNumber,
				gender: "M",
				eMail: subscriberInfo.Subscriber_EMail,
				loginId: out login,
				errorMessage: out err
			    );
			this.txtLogin.Text = login;
			this.txtToken.Text = token;
			// TODO
		}

		//-------------------------------------------------------------------------------
		private void btnBrowse_Click(object sender, EventArgs e)
		{
			string filename = null;
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Multiselect = false;
				if (dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{
					filename = dialog.FileName;
				}
			}
			if (filename != null)
				this.txtPdfSubscriptionPath.Text = filename;
		}

		//-------------------------------------------------------------------------------
		private void btnCharge_Click(object sender, EventArgs e)
		{
			string filePath = this.txtPdfSubscriptionPath.Text;
			if (string.IsNullOrEmpty(filePath) || false == File.Exists(filePath))
			{
				MessageBox.Show(this, "Select an existing file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			//string company = GetSelectedCompany();
			//string fileName = Path.GetFileName(filePath);
			byte[] bytes = File.ReadAllBytes(filePath);
			string file64 = Convert.ToBase64String(bytes, 0, bytes.Length);
			string token = "TEMPTOKEN"; // TODO
			string err;
			plProxy.Charge(token: token, fileContentBase64: file64, errorMessage: out err);
		}
	}
}
