using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Licence.Activation;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	//=========================================================================
	public class UserInfoForm : UserControl	
	{
		#region Controls
		private ComboBox	ComboBoxCountry;
		private ErrorProvider errorProviderEmail;
		private Label 		LabelCompany;
		private Label 		LabelAddress;
		private Label 		LabelZipCode;
		private Label 		LabelCity;
		private Label 		LabelCityCode;
		private Label 		LabelCountry;
		private Label 		LabelVatNumber;
		private Label 		LabelPhone;
		private Label LabelFax;
		private Label		LblInformative;
		private Label		LblDealerMail;
		private Label		LblMailInfo;
		private TextBox 	TextBoxName;
		private TextBox 	TextBoxCompany;
		private TextBox 	TextBoxAddress;
		private TextBox 	TextBoxZipCode;
		private TextBox 	TextBoxCity;
		private TextBox 	TextBoxCityCode;
		private TextBox 	TextBoxVatNumber;
		private TextBox 	TextBoxPhone;
		private TextBox 	TextBoxFax;
		private TextBox 	TextBoxEmail;
		private TextBox 	TextBoxISOCountry;
		private TextBox		TxtDealerMail;
		private Label		label1;
		private TextBox		TextBoxCodFisc;
		private Label		LblACGroup;
		private Label		LabelName;
		private Label		LblACArea;
		private ComboBox	CmbACGroup;
		private ComboBox	CmbACArea;

		private System.ComponentModel.IContainer components = null;
		#endregion

		#region Private members
		private ClientStub clientStub;
		private		bool		activityCodeLoaded	= false;
		private		bool		dirty			= false;
		private CheckBox CkbConsInt;
		private CheckBox CkbConsEst;
		private RichTextBox TxtConsEst;
		private RichTextBox TxtConsInt;
		private LinkLabel LnkPrivacy;
		private PictureBox pictureBox1;
        private TextBox TxtPECMail;
        private Label label2;
		private UserInfo originalUserInfo;
		public event EventHandler Modified;
		#endregion

		#region Public properties

		//---------------------------------------------------------------------
		public bool Dirty
		{
			get { return dirty; }
			set
			{
				dirty = value;
				if (dirty && Modified != null)
					Modified(this, null);
			}
		}

		
		#endregion

		
		//---------------------------------------------------------------------
		public UserInfoForm(ClientStub clientStub)
		{
			InitializeComponent();
			
			this.clientStub = clientStub;
			originalUserInfo = clientStub.GetUserInfo();
			if (originalUserInfo  == null) originalUserInfo = new UserInfo();
			PostInitializeComponent();
		}
		

		//---------------------------------------------------------------------------
		private void ViewDiagnostic()
		{
			DiagnosticViewer.ShowDiagnostic(clientStub.Diagnostic);
		}

		//---------------------------------------------------------------------
		public bool Save()
		{
			UserInfo current = GetCurrentUserInfo();
			if (originalUserInfo.Equals(current))
				return true;

			if (!String.IsNullOrEmpty(originalUserInfo.CodFisc) &&
				String.Compare(originalUserInfo.CodFisc, current.CodFisc, false, CultureInfo.InvariantCulture) != 0)
			{
				VatNrChange vatChange = new VatNrChange();
				vatChange.StartPosition = FormStartPosition.CenterParent;
				vatChange.ShowDialog(this);
			}
			if (GetErrors().Count != 0) 
				return false;

            //ricarico le info che potrebbero essere state corrette  (per esempio tolto it da partita iva)
            current.CodFisc = TextBoxCodFisc.Text.Trim();
            current.VatNumber = TextBoxVatNumber.Text.Trim();

			bool retVal = clientStub.SaveUserInfo(current);

			if (retVal)
			{
				Dirty = false;
				return true;
			}
			else
			{
				clientStub.SetError(LicenceStrings.MsgSaveError, null, null);
				ViewDiagnostic();
				return false;
			}
		}

		//---------------------------------------------------------------------
		public string GetCountry()
		{
			return TextBoxISOCountry.Text.Trim();
		}

		//---------------------------------------------------------------------
		private UserInfo GetCurrentUserInfo()
		{
			UserInfo currentUserInfo = new UserInfo();

			currentUserInfo.Name = TextBoxName.Text.Trim();
			currentUserInfo.Company = TextBoxCompany.Text.Trim();
			currentUserInfo.Address = TextBoxAddress.Text.Trim();
			currentUserInfo.ZipCode = TextBoxZipCode.Text.Trim();
			currentUserInfo.City = TextBoxCity.Text.Trim();
			currentUserInfo.CityCode = TextBoxCityCode.Text.Trim();
			currentUserInfo.Country = TextBoxISOCountry.Text.Trim();
			currentUserInfo.CodFisc = TextBoxCodFisc.Text.Trim();
			currentUserInfo.VatNumber = TextBoxVatNumber.Text.Trim();
			currentUserInfo.Phone = TextBoxPhone.Text.Trim();
			currentUserInfo.Fax = TextBoxFax.Text.Trim();
			currentUserInfo.Email = TextBoxEmail.Text.Trim();
			currentUserInfo.DealerEmail = TxtDealerMail.Text.Trim();
            currentUserInfo.PECEmail = TxtPECMail.Text.Trim();
			currentUserInfo.ConsensoInterno = CkbConsInt.Checked;
			currentUserInfo.ConsensoEsteso = CkbConsEst.Checked;
			currentUserInfo.UserIdInfos = originalUserInfo == null ? null: originalUserInfo.UserIdInfos;
			if (CmbACGroup.SelectedItem != null && CmbACArea.SelectedItem != null)
			{
				string group = ((ActivityCodeGroup)CmbACGroup.SelectedItem).Group;
				string area = ((ActivityCodeArea)CmbACArea.SelectedItem).Area;
				currentUserInfo.ActivityCode = new ActivityCode(group, area);
			}
			else
				currentUserInfo.ActivityCode = null;

			return currentUserInfo;
		}

		//---------------------------------------------------------------------
		private void RefreshStateView(UserInfo ui)
		{
			if (ui == null) return;
			SafeGui.ControlText(TextBoxName, ui.Name);
			SafeGui.ControlText(TextBoxCompany, ui.Company);
			SafeGui.ControlText(TextBoxAddress, ui.Address);
			SafeGui.ControlText(TextBoxZipCode, ui.ZipCode);
			SafeGui.ControlText(TextBoxCity, ui.City);
			SafeGui.ControlText(TextBoxCityCode, ui.CityCode);
			SafeGui.ControlText(TextBoxVatNumber, ui.VatNumber);
			SafeGui.ControlText(TextBoxCodFisc, ui.CodFisc);
			SafeGui.ControlText(TextBoxPhone, ui.Phone);
			SafeGui.ControlText(TextBoxFax, ui.Fax);
			SafeGui.ControlText(TextBoxEmail, ui.Email);
			SafeGui.ControlText(TxtDealerMail, ui.DealerEmail);
            SafeGui.ControlText(TxtPECMail, ui.PECEmail);
			SafeGui.CheckBoxCheck(CkbConsInt, ui.ConsensoInterno);
			SafeGui.CheckBoxCheck(CkbConsEst, ui.ConsensoEsteso);
			SetActivityCode(ui.ActivityCode);
			SetCountry(ui.Country);
		}

		//---------------------------------------------------------------------
		private void SetActivityCode(ActivityCode ac)
		{
			LoadActivityCode();

			if (ac == null)
				return;

			foreach (object g in CmbACGroup.Items)
				if (((ActivityCodeGroup)g).Group == ac.Group)
				{
					CmbACGroup.SelectedItem = g;
					break;
				}
			foreach (object a in CmbACArea.Items)
				if (((ActivityCodeArea)a).Area == ac.Area)
				{
					CmbACArea.SelectedItem = a;
					break;
				}
	}

		//---------------------------------------------------------------------
		private bool SetCountry(string countryCode)
		{
			foreach (ComboBoxItem cbItem in ComboBoxCountry.Items)
				if (cbItem.Value == countryCode)
				{
					ComboBoxCountry.SelectedItem = cbItem;
					return true;
				}
			return false;
		}


		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		//---------------------------------------------------------------------
		private bool IsEmailValid(string email, out string errorMsg)
		{
			errorMsg = string.Empty;
			if (email == null || email == string.Empty)
				return true;

			string CVEEmail = @"^[a-zA-Z0-9_\+-]+(\.[a-zA-Z0-9_\+-]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.([a-zA-Z]{2,})$";
			bool ok = System.Text.RegularExpressions.Regex.IsMatch(email, CVEEmail);
			if (!ok)
				errorMsg = LicenceStrings.EmailNoValid;

			return ok;
		}

		//---------------------------------------------------------------------
        public IList<FillingError> GetErrors()
		{
            IList<FillingError> fl = new List<FillingError>();
			UserInfo ui = GetCurrentUserInfo() as UserInfo;
			
            StringBuilder sb = new StringBuilder();

            //controllo completezza dati
            if (!ui.PersonalDataComplete())
            {
                //i personal data non sono completi, 
                //vedere cosa manca per dare un messaggio appropriato
                
                string separator = ", ";
                foreach (UserInfo.PersonalData pd in ui.MissingPersonalData)
                {
                    sb.Append(GetMissingDatumValue(pd));
                    sb.Append(separator);
                }
                //tolgo l'ultima virgola
                sb.Remove(sb.Length - separator.Length, separator.Length);

                fl.Add(new FillingError(FillingError.ErrorType.Error, String.Format(LicenceStrings.MsgRequestedDataError, sb.ToString())));
            }
            if (!VerifyVatNumbers())
                fl.Add(new FillingError(FillingError.ErrorType.Error, LicenceStrings.ErrorInCodes)); 
    
            return fl;
            
		}

		//verifica dei codici fiscali e partite iva inserite, SOLO PER ITALIA
        //secondo misteriose regole:
        /*codice fiscale = codice di 16 caratteri , deve contenere almeno una lettera ->
         *              non si può fare affidamento al classico codice fiscale, perchè causa omocodie 
         *              potrebbe essere composto anche solo da lettere, 
         *              quindi non usare regex tropppo specifiche
         * partita iva= codice numerico di 11 cifre, potrebbe essere anteposto il codice nazione, 
         *              nel nostro caso IT, che deve essere tolto perchè PAI non lo vuole. 
         *              lo elimino forzosamente senza dare errore
         * NB: entrambi i codici indicano in ultima posizione una cifra di controllo che si pùo verificare con algoritmi troppo complicati per questo momento.
         * NB: l'agenzia delle entrate volendo offre un servizio di verifica di esistenza delle partite iva.
         * 
         * - entrambi i campi possono contenere una partita iva o un codice fiscale ben formattato.
         * - nel codice fiscale ci può andare una partita iva e allora anche nella partita iva ci va una partitaiva, che potrebbe anche essere diversa (caso cambio provincia, per esempio)
         * - se nella partita iva c'è un  codFisc allora lo stesso codFisc deve essere indicato nel campo codFisc
         * - se i campi indicano partita iva e codicefiscale allora devono essere inseriti nella casella giusta (non accetto se invertiti :cf in pi e pi in cf)
         * 
         * */
        //---------------------------------------------------------------------
        private bool VerifyVatNumbers()
        {
            //setting da console per quel cliente di antos 
            //che lavora in cina con partita iva cinese e serial italiani 
            //(perchè quelli cinesi non fanno funzionare bene il prodotto)
            if (!InstallationData.ServerConnectionInfo.CheckVATNr) return true;
            
            //controllo partita iva e cod fisc per italia se sono compilati.
            if ((String.Compare(GetCountry(), "IT", true, CultureInfo.InvariantCulture) != 0) )
                return true;
            if (String.IsNullOrWhiteSpace(TextBoxCodFisc.Text) || String.IsNullOrWhiteSpace(TextBoxVatNumber.Text))
                return true;
            //e trimmiammo
            TextBoxVatNumber.Text = TextBoxVatNumber.Text.Trim();
            TextBoxCodFisc.Text = TextBoxCodFisc.Text.Trim();

            //applico regex per verifica pi ben formata
            string pivaregex = "^(IT)?[0-9]{11}$";
            bool okpiINpi = Regex.IsMatch(TextBoxVatNumber.Text, pivaregex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            bool okpiINcf = Regex.IsMatch(TextBoxCodFisc.Text, pivaregex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            //tolgo IT se presente in partita iva
            if (okpiINpi && (TextBoxVatNumber.Text.ToUpperInvariant().StartsWith("IT")))
                TextBoxVatNumber.Text = TextBoxVatNumber.Text.Substring(2);

            if (okpiINcf && (TextBoxCodFisc.Text.ToUpperInvariant().StartsWith("IT")))
                TextBoxCodFisc.Text = TextBoxCodFisc.Text.Substring(2);

            //applico regex per verifica cf ben formato
            string codfiscregex = "^[a-zA-Z0-9]{16}$";
           
            bool okcfINcf = Regex.IsMatch(TextBoxCodFisc.Text, codfiscregex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            bool okcfINpi = Regex.IsMatch(TextBoxVatNumber.Text, codfiscregex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            //siccome la regex trova anche un codice fiscale di solo numeri e non va bene verifico con un'altra che non sia solo numeri.
            //se poi troviamo il modo di fare ciò con una  sola regex, meglio!
            string codfiscregexsolonumeri = "^[0-9]{16}$";
            bool okcfINcfSN = Regex.IsMatch(TextBoxCodFisc.Text, codfiscregexsolonumeri, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            bool okcfINpiSN = Regex.IsMatch(TextBoxVatNumber.Text, codfiscregexsolonumeri, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            //quindi devono essere 16 caratteri non solo numerici
            okcfINcf = okcfINcf && !okcfINcfSN;
            okcfINpi = okcfINpi && !okcfINpiSN;

            //ora verifichiamo tutte le combinazioni
            return !( //se sono tutti falsi, errore
                (!okpiINpi && !okpiINcf && !okcfINcf && !okcfINpi) ||
                //se nella partita iva c'è un codfisc anche nel codfisc ci deve essere un cod fisc
                (okcfINpi && !okcfINcf) ||
                //se nel cf c'è un pi anche nel pi ci deve essere un pi
                 (okpiINcf && !okpiINpi) ||
                //o se ogni casella non sia compilata correttamente
                 (!okpiINcf && !okcfINcf) ||
                 (!okcfINpi && !okpiINpi) ||
                //se sono entrambi cf ma sono diversi
                (
                    (okcfINpi && okcfINcf) &&
                   (String.Compare(TextBoxVatNumber.Text, TextBoxCodFisc.Text, true, CultureInfo.InvariantCulture) != 0))
                );
        }

		//---------------------------------------------------------------------
		private void OnCountryChanged(object sender, System.EventArgs e)
		{
			SafeGui.ControlText(TextBoxISOCountry,
				((ComboBoxItem)ComboBoxCountry.SelectedItem).Value);

			SomethingHasChanged(sender, e);
		}


		//questo metodo andrebbe in activation, ma siccome in quella dll non ci sono dizionari
		//mi dispiace aggiungerli solo per queste 4 stringhe.
		//---------------------------------------------------------------------
		public string GetMissingDatumValue(UserInfo.PersonalData datum)
		{
			switch (datum)
			{
				case UserInfo.PersonalData.Address:
					return LicenceStrings.Address;
				case UserInfo.PersonalData.City:
					return LicenceStrings.City;
				case UserInfo.PersonalData.CodFisc:
					return LicenceStrings.CodFisc;
				case UserInfo.PersonalData.Company:
					return LicenceStrings.Company;
				case UserInfo.PersonalData.Country:
					return LicenceStrings.Country;
				case UserInfo.PersonalData.Email:
					return LicenceStrings.Email;
				case UserInfo.PersonalData.Fax:
					return LicenceStrings.Fax;
				case UserInfo.PersonalData.Name:
					return LicenceStrings.Name;
				case UserInfo.PersonalData.Phone:
					return LicenceStrings.Phone;
				case UserInfo.PersonalData.VatNumber:
					return LicenceStrings.VatNumber;
				case UserInfo.PersonalData.ZipCode:
					return LicenceStrings.ZipCode;
				case UserInfo.PersonalData.ActivityCode:
					return LicenceStrings.ActivityCode;
				case UserInfo.PersonalData.InternalAgreement:
					return LicenceStrings.InternalAgreement;
			} return null;
		}
		
		/// <summary>
		/// Abbandonata la selezione delle country dall'elenco delle
		/// CultureInfo, cosa comunque sbagliata da farsi, la lista
		/// delle nazioni dalla classe RegionInfo è incompleta.
		/// Nota 1:
		/// L'elenco ufficiale è definito nella lista ISO 3166; il file xml
		/// incluso nelle risorse è quello ufficiale scaricato dal sito
		/// http://www.iso.org/iso/en/prods-services/iso3166ma/index.html.
		/// Nota 2:
		/// L'elenco è in inglese, l'unica alternativa è il francese.
		/// </summary>
		/// <remarks>Consultare spesso il sito per gli aggiornamenti!</remarks>
		//---------------------------------------------------------------------
		private void PostInitializeComponent()
		{
			XmlDocument doc = new XmlDocument();
			string resourceName = "Microarea.TaskBuilderNet.Licence.Licence.Forms.iso_3166-1_list_en.xml";
			Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			if (s != null)
			{
				doc.Load(s);


				//	<ISO_3166-1_Entry>
				//		<ISO_3166-1_Country_name>
				//		<ISO_3166-1_Alpha-2_Code_element>
				//	</ISO_3166-1_Entry>
				//aggiungo elemento vuoto per demo
				ComboBoxCountry.Items.Add(new ComboBoxItem("", ""));

				foreach (XmlNode aNode in doc.DocumentElement.ChildNodes)
				{
					ComboBoxCountry.Items.Add(new ComboBoxItem
						(
							aNode.SelectSingleNode("ISO_3166-1_Country_name").InnerText,
							aNode.SelectSingleNode("ISO_3166-1_Alpha-2_Code_element").InnerText
						));
				}

				if (ComboBoxCountry.Items.Count > 0)
					ComboBoxCountry.SelectedIndex = 0;
			}
			errorProviderEmail.SetIconAlignment(TextBoxEmail, ErrorIconAlignment.MiddleRight);
			errorProviderEmail.SetIconPadding(TextBoxEmail, 2);
			TextBoxCityCode.MaxLength = 3; //come PAI

			TxtConsInt.Text = LicenceStrings.ConsensoInterno;
			TxtConsEst.Text = LicenceStrings.ConsensoEsteso;
			LoadActivityCode();
			LnkPrivacy.LinkArea = new LinkArea(0, LnkPrivacy.Text.Length);
			RefreshStateView(originalUserInfo);
		}

		//---------------------------------------------------------------------
		private void SomethingHasChanged(object sender, System.EventArgs e)
		{
			Dirty = true;
		}

		//---------------------------------------------------------------------
		private void UserInfoForm_Load(object sender, EventArgs e)
		{
			//ComboBoxCountry.Enabled = TextBoxISOCountry.Text.Length == 0;
		}

		//---------------------------------------------------------------------
		private void LoadActivityCode()
		{
			if (!activityCodeLoaded)
			{
				IList<ActivityCodeGroup> groups = clientStub.LoadActivityCode();				
				if (groups == null)
					return;

				CmbACGroup.DisplayMember = "Description";
				CmbACGroup.ValueMember = "Group";
				foreach (ActivityCodeGroup x in groups)
					CmbACGroup.Items.Add(x);
				activityCodeLoaded = true;
			}
		}

		//---------------------------------------------------------------------
		private void CmbACGroup_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SomethingHasChanged(sender, e);
			CmbACArea.Items.Clear();
			IList<ActivityCodeArea> areas = ((ActivityCodeGroup)CmbACGroup.SelectedItem).Areas;
			CmbACArea.DisplayMember = "Description";
			CmbACArea.ValueMember = "Area";
			foreach (ActivityCodeArea x in areas)
				CmbACArea.Items.Add(x);
		}

		//---------------------------------------------------------------------
		private void CmbACArea_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SomethingHasChanged(sender, e);
		}

		//---------------------------------------------------------------------
		private void TextBoxEmail_Validated(object sender, System.EventArgs e)
		{
			if (sender == null)
				return;
			TextBox tb = sender as TextBox;
			if (tb == null)
				return;
			// If all conditions have been met, clear the ErrorProvider of errors.
			errorProviderEmail.SetError(tb, string.Empty);
		}

		//---------------------------------------------------------------------
		private void TextBoxEmail_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (sender == null)
				return;
			TextBox tb = sender as TextBox;
			if (tb == null)
				return;
			string errorMsg;
			if(!IsEmailValid(tb.Text, out errorMsg))
			{
				// Cancel the event and select the text to be corrected by the user.
				e.Cancel = true;
				TextBoxEmail.Select(0, tb.Text.Length);

				// Set the ErrorProvider error with the text to display. 
				this.errorProviderEmail.SetError(tb, errorMsg);
			}
		}

		//---------------------------------------------------------------------
		private void pictureBox1_Click(object sender, EventArgs e)
		{
			VatNrChange vatChange = new VatNrChange();
			vatChange.StartPosition = FormStartPosition.CenterParent;
			vatChange.ShowDialog(this);
		}
		//---------------------------------------------------------------------
		private void HandOnMouseEnter(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.Hand;
		}

		//---------------------------------------------------------------------
		private void DefaultOnMouseLeave(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.Default;
		}
		//---------------------------------------------------------------------
		private void LnkPrivacy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				string culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name.ToLowerInvariant();
				string address = "http://www.microarea.it/int/NormativaPrivacy.aspx";
				if ( culture.StartsWith("it"))
					address = "http://www.microarea.it/NormativaPrivacy.aspx";
				Process.Start(address);
			}
			catch (Exception exc)
			{
				clientStub.SetError(LicenceStrings.ErrorOpeningPage, LicenceStrings.MsgTitleError, exc.Message);
			}
		}


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserInfoForm));
            this.TextBoxName = new System.Windows.Forms.TextBox();
            this.LblACGroup = new System.Windows.Forms.Label();
            this.TextBoxEmail = new System.Windows.Forms.TextBox();
            this.LabelCityCode = new System.Windows.Forms.Label();
            this.TextBoxCityCode = new System.Windows.Forms.TextBox();
            this.TextBoxFax = new System.Windows.Forms.TextBox();
            this.TextBoxPhone = new System.Windows.Forms.TextBox();
            this.LabelFax = new System.Windows.Forms.Label();
            this.LabelPhone = new System.Windows.Forms.Label();
            this.LabelCountry = new System.Windows.Forms.Label();
            this.LabelCity = new System.Windows.Forms.Label();
            this.LabelZipCode = new System.Windows.Forms.Label();
            this.TextBoxVatNumber = new System.Windows.Forms.TextBox();
            this.TextBoxCompany = new System.Windows.Forms.TextBox();
            this.TextBoxAddress = new System.Windows.Forms.TextBox();
            this.TextBoxZipCode = new System.Windows.Forms.TextBox();
            this.LabelVatNumber = new System.Windows.Forms.Label();
            this.TextBoxCity = new System.Windows.Forms.TextBox();
            this.LabelAddress = new System.Windows.Forms.Label();
            this.LabelCompany = new System.Windows.Forms.Label();
            this.ComboBoxCountry = new System.Windows.Forms.ComboBox();
            this.TextBoxISOCountry = new System.Windows.Forms.TextBox();
            this.errorProviderEmail = new System.Windows.Forms.ErrorProvider(this.components);
            this.LblInformative = new System.Windows.Forms.Label();
            this.LblDealerMail = new System.Windows.Forms.Label();
            this.TxtDealerMail = new System.Windows.Forms.TextBox();
            this.LblMailInfo = new System.Windows.Forms.Label();
            this.TextBoxCodFisc = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CmbACGroup = new System.Windows.Forms.ComboBox();
            this.CmbACArea = new System.Windows.Forms.ComboBox();
            this.LabelName = new System.Windows.Forms.Label();
            this.LblACArea = new System.Windows.Forms.Label();
            this.CkbConsInt = new System.Windows.Forms.CheckBox();
            this.CkbConsEst = new System.Windows.Forms.CheckBox();
            this.TxtConsEst = new System.Windows.Forms.RichTextBox();
            this.TxtConsInt = new System.Windows.Forms.RichTextBox();
            this.LnkPrivacy = new System.Windows.Forms.LinkLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.TxtPECMail = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderEmail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // TextBoxName
            // 
            resources.ApplyResources(this.TextBoxName, "TextBoxName");
            this.TextBoxName.Name = "TextBoxName";
            this.TextBoxName.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // LblACGroup
            // 
            resources.ApplyResources(this.LblACGroup, "LblACGroup");
            this.LblACGroup.Name = "LblACGroup";
            // 
            // TextBoxEmail
            // 
            resources.ApplyResources(this.TextBoxEmail, "TextBoxEmail");
            this.TextBoxEmail.Name = "TextBoxEmail";
            this.TextBoxEmail.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            this.TextBoxEmail.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxEmail_Validating);
            this.TextBoxEmail.Validated += new System.EventHandler(this.TextBoxEmail_Validated);
            // 
            // LabelCityCode
            // 
            resources.ApplyResources(this.LabelCityCode, "LabelCityCode");
            this.LabelCityCode.Name = "LabelCityCode";
            // 
            // TextBoxCityCode
            // 
            this.TextBoxCityCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            resources.ApplyResources(this.TextBoxCityCode, "TextBoxCityCode");
            this.TextBoxCityCode.Name = "TextBoxCityCode";
            this.TextBoxCityCode.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // TextBoxFax
            // 
            resources.ApplyResources(this.TextBoxFax, "TextBoxFax");
            this.TextBoxFax.Name = "TextBoxFax";
            this.TextBoxFax.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // TextBoxPhone
            // 
            resources.ApplyResources(this.TextBoxPhone, "TextBoxPhone");
            this.TextBoxPhone.Name = "TextBoxPhone";
            this.TextBoxPhone.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // LabelFax
            // 
            resources.ApplyResources(this.LabelFax, "LabelFax");
            this.LabelFax.Name = "LabelFax";
            // 
            // LabelPhone
            // 
            resources.ApplyResources(this.LabelPhone, "LabelPhone");
            this.LabelPhone.Name = "LabelPhone";
            // 
            // LabelCountry
            // 
            resources.ApplyResources(this.LabelCountry, "LabelCountry");
            this.LabelCountry.Name = "LabelCountry";
            // 
            // LabelCity
            // 
            resources.ApplyResources(this.LabelCity, "LabelCity");
            this.LabelCity.Name = "LabelCity";
            // 
            // LabelZipCode
            // 
            resources.ApplyResources(this.LabelZipCode, "LabelZipCode");
            this.LabelZipCode.Name = "LabelZipCode";
            // 
            // TextBoxVatNumber
            // 
            resources.ApplyResources(this.TextBoxVatNumber, "TextBoxVatNumber");
            this.TextBoxVatNumber.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.TextBoxVatNumber.Name = "TextBoxVatNumber";
            this.TextBoxVatNumber.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // TextBoxCompany
            // 
            resources.ApplyResources(this.TextBoxCompany, "TextBoxCompany");
            this.TextBoxCompany.Name = "TextBoxCompany";
            this.TextBoxCompany.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // TextBoxAddress
            // 
            resources.ApplyResources(this.TextBoxAddress, "TextBoxAddress");
            this.TextBoxAddress.Name = "TextBoxAddress";
            this.TextBoxAddress.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // TextBoxZipCode
            // 
            this.TextBoxZipCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            resources.ApplyResources(this.TextBoxZipCode, "TextBoxZipCode");
            this.TextBoxZipCode.Name = "TextBoxZipCode";
            this.TextBoxZipCode.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // LabelVatNumber
            // 
            resources.ApplyResources(this.LabelVatNumber, "LabelVatNumber");
            this.LabelVatNumber.Name = "LabelVatNumber";
            // 
            // TextBoxCity
            // 
            resources.ApplyResources(this.TextBoxCity, "TextBoxCity");
            this.TextBoxCity.Name = "TextBoxCity";
            this.TextBoxCity.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // LabelAddress
            // 
            resources.ApplyResources(this.LabelAddress, "LabelAddress");
            this.LabelAddress.Name = "LabelAddress";
            // 
            // LabelCompany
            // 
            resources.ApplyResources(this.LabelCompany, "LabelCompany");
            this.LabelCompany.Name = "LabelCompany";
            // 
            // ComboBoxCountry
            // 
            resources.ApplyResources(this.ComboBoxCountry, "ComboBoxCountry");
            this.ComboBoxCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboBoxCountry.DropDownWidth = 200;
            this.ComboBoxCountry.Name = "ComboBoxCountry";
            this.ComboBoxCountry.Sorted = true;
            this.ComboBoxCountry.SelectedIndexChanged += new System.EventHandler(this.OnCountryChanged);
            // 
            // TextBoxISOCountry
            // 
            resources.ApplyResources(this.TextBoxISOCountry, "TextBoxISOCountry");
            this.TextBoxISOCountry.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.TextBoxISOCountry.Name = "TextBoxISOCountry";
            this.TextBoxISOCountry.TabStop = false;
            this.TextBoxISOCountry.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // errorProviderEmail
            // 
            this.errorProviderEmail.BlinkRate = 300;
            this.errorProviderEmail.ContainerControl = this;
            resources.ApplyResources(this.errorProviderEmail, "errorProviderEmail");
            // 
            // LblInformative
            // 
            resources.ApplyResources(this.LblInformative, "LblInformative");
            this.LblInformative.Name = "LblInformative";
            // 
            // LblDealerMail
            // 
            resources.ApplyResources(this.LblDealerMail, "LblDealerMail");
            this.LblDealerMail.Name = "LblDealerMail";
            // 
            // TxtDealerMail
            // 
            resources.ApplyResources(this.TxtDealerMail, "TxtDealerMail");
            this.TxtDealerMail.Name = "TxtDealerMail";
            this.TxtDealerMail.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            this.TxtDealerMail.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxEmail_Validating);
            this.TxtDealerMail.Validated += new System.EventHandler(this.TextBoxEmail_Validated);
            // 
            // LblMailInfo
            // 
            resources.ApplyResources(this.LblMailInfo, "LblMailInfo");
            this.LblMailInfo.Name = "LblMailInfo";
            // 
            // TextBoxCodFisc
            // 
            this.TextBoxCodFisc.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            resources.ApplyResources(this.TextBoxCodFisc, "TextBoxCodFisc");
            this.TextBoxCodFisc.Name = "TextBoxCodFisc";
            this.TextBoxCodFisc.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // CmbACGroup
            // 
            resources.ApplyResources(this.CmbACGroup, "CmbACGroup");
            this.CmbACGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbACGroup.DropDownWidth = 250;
            this.CmbACGroup.Name = "CmbACGroup";
            this.CmbACGroup.Sorted = true;
            this.CmbACGroup.SelectedIndexChanged += new System.EventHandler(this.CmbACGroup_SelectedIndexChanged);
            // 
            // CmbACArea
            // 
            resources.ApplyResources(this.CmbACArea, "CmbACArea");
            this.CmbACArea.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbACArea.DropDownWidth = 250;
            this.CmbACArea.Name = "CmbACArea";
            this.CmbACArea.Sorted = true;
            this.CmbACArea.SelectedIndexChanged += new System.EventHandler(this.CmbACArea_SelectedIndexChanged);
            // 
            // LabelName
            // 
            resources.ApplyResources(this.LabelName, "LabelName");
            this.LabelName.Name = "LabelName";
            // 
            // LblACArea
            // 
            resources.ApplyResources(this.LblACArea, "LblACArea");
            this.LblACArea.Name = "LblACArea";
            // 
            // CkbConsInt
            // 
            resources.ApplyResources(this.CkbConsInt, "CkbConsInt");
            this.CkbConsInt.Name = "CkbConsInt";
            // 
            // CkbConsEst
            // 
            resources.ApplyResources(this.CkbConsEst, "CkbConsEst");
            this.CkbConsEst.Name = "CkbConsEst";
            // 
            // TxtConsEst
            // 
            resources.ApplyResources(this.TxtConsEst, "TxtConsEst");
            this.TxtConsEst.Name = "TxtConsEst";
            this.TxtConsEst.ReadOnly = true;
            this.TxtConsEst.TabStop = false;
            // 
            // TxtConsInt
            // 
            resources.ApplyResources(this.TxtConsInt, "TxtConsInt");
            this.TxtConsInt.Name = "TxtConsInt";
            this.TxtConsInt.ReadOnly = true;
            this.TxtConsInt.TabStop = false;
            // 
            // LnkPrivacy
            // 
            resources.ApplyResources(this.LnkPrivacy, "LnkPrivacy");
            this.LnkPrivacy.Name = "LnkPrivacy";
            this.LnkPrivacy.TabStop = true;
            this.LnkPrivacy.UseCompatibleTextRendering = true;
            this.LnkPrivacy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkPrivacy_LinkClicked);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseEnter += new System.EventHandler(this.HandOnMouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.DefaultOnMouseLeave);
            // 
            // TxtPECMail
            // 
            resources.ApplyResources(this.TxtPECMail, "TxtPECMail");
            this.TxtPECMail.Name = "TxtPECMail";
            this.TxtPECMail.TextChanged += new System.EventHandler(this.SomethingHasChanged);
            this.TxtPECMail.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxEmail_Validating);
            this.TxtPECMail.Validated += new System.EventHandler(this.TextBoxEmail_Validated);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // UserInfoForm
            // 
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.Lavender;
            this.Controls.Add(this.TxtPECMail);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.TxtConsEst);
            this.Controls.Add(this.TxtConsInt);
            this.Controls.Add(this.CkbConsInt);
            this.Controls.Add(this.CkbConsEst);
            this.Controls.Add(this.LnkPrivacy);
            this.Controls.Add(this.CmbACGroup);
            this.Controls.Add(this.CmbACArea);
            this.Controls.Add(this.TextBoxVatNumber);
            this.Controls.Add(this.TextBoxCodFisc);
            this.Controls.Add(this.TxtDealerMail);
            this.Controls.Add(this.TextBoxISOCountry);
            this.Controls.Add(this.TextBoxName);
            this.Controls.Add(this.TextBoxCityCode);
            this.Controls.Add(this.TextBoxFax);
            this.Controls.Add(this.TextBoxPhone);
            this.Controls.Add(this.TextBoxCompany);
            this.Controls.Add(this.TextBoxAddress);
            this.Controls.Add(this.TextBoxZipCode);
            this.Controls.Add(this.TextBoxCity);
            this.Controls.Add(this.LabelFax);
            this.Controls.Add(this.LabelPhone);
            this.Controls.Add(this.LabelCompany);
            this.Controls.Add(this.LblDealerMail);
            this.Controls.Add(this.TextBoxEmail);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LabelCityCode);
            this.Controls.Add(this.LabelCity);
            this.Controls.Add(this.LabelZipCode);
            this.Controls.Add(this.LblACGroup);
            this.Controls.Add(this.LabelName);
            this.Controls.Add(this.LblACArea);
            this.Controls.Add(this.LblMailInfo);
            this.Controls.Add(this.LblInformative);
            this.Controls.Add(this.ComboBoxCountry);
            this.Controls.Add(this.LabelCountry);
            this.Controls.Add(this.LabelAddress);
            this.Controls.Add(this.LabelVatNumber);
            this.Name = "UserInfoForm";
            this.Load += new System.EventHandler(this.UserInfoForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderEmail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

       
		
	}
}