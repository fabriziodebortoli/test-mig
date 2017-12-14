using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;

namespace Microarea.WebServices.LoginManager
{
	//---------------------------------------------------------------------------
	public partial class PingViaSMS : System.Web.UI.Page
	{
        private string prodName = String.Empty;
        private string errorMessage = String.Empty;
        //"<a href=\"http://www.microarea.it\" target=\"_blank\">Microarea</a>";
        private string producerwebsiteLinkMASK = "<a href=\"{0}\" target=\"_blank\">{1}</a>";
        //---------------------------------------------------------------------------
		protected void Page_Load(object sender, EventArgs e)
		{
            string src = LoginManager.LoginApplication.LoginEngine.GetSMSHeaderImage();

            img1.Src = src;
            imgLnkHP.Src = src;
            img3.Src = src;
            img11.Src = src; 

			LoginManager.LoginApplication.LoginEngine.SetCulture();
            prodName = LoginManager.LoginApplication.LoginEngine.GetMasterProductBrandedName();
			Title = String.Format(LoginManagerStrings.SMSPageTitle, prodName);
			BtnRegister.Text = LoginManagerStrings.SMSPageBtnRegisterText;
            string websiteLink = String.Format(producerwebsiteLinkMASK, LoginManager.LoginApplication.LoginEngine.GetBrandedKey("ProducerSite"), LoginManager.LoginApplication.LoginEngine.GetBrandedProducerName());
            errorMessage = String.Format(LoginManagerStrings.LblError1, prodName, websiteLink) + "<br />" + String.Format(LoginManagerStrings.LblError2, prodName, websiteLink); ;
            LblError.Text = errorMessage;
			LblTitle.Text = Title;
			error.Visible = false;
			ImgRes.Visible = false;
            NotSupported.Visible = false;
			OK.Visible = false;
            string pcode= LoginManager.LoginApplication.LoginEngine.ActivationManager.GetMasterProductID();
            string PhoneNumberVOLA = GetSMSVolaNumber_CompileTimeBrand(pcode);
            string PhoneNumberMACNIL = GetSMSMACNILNumber_CompileTimeBrand(pcode) ;
            string addressSMS = GetSMSMailService_CompileTimeBrand(pcode); 
			//se non è più attivato il ping via sms  non va-----> deve andare lo stesso! (23/07/2010)
			/*bool act = LoginManager.LoginApplication.LoginEngine.IsActivated();
			if (!act)
			{
				LblError.Text = errorMessage;
				error.Visible = true;
				sms.Visible = false;
				OK.Visible = false;
				LblInfo.Visible = false;
				TxtCode.Text = string.Empty;
				return;
			}*/

			//se non è waiting allora tutto ok!
			if (!LoginManager.LoginApplication.LoginEngine.PingNeeded(false))
			{
				LabelOK.Text = LoginManagerStrings.LblSMSNotNeeded;
				error.Visible = false;
				sms.Visible = false;
				OK.Visible = true;
				LblInfo.Visible = false;
                NotSupported.Visible = false;
				return;
			}

           


			//chiama LM e prendi codice
			string Code = LoginManager.LoginApplication.LoginEngine.GetSMSCode();
			string actid = String.Empty;
            string country = String.Empty;
			if (LoginManager.LoginApplication.LoginEngine.ActivationManager != null &&
				LoginManager.LoginApplication.LoginEngine.ActivationManager.User != null &&
				LoginManager.LoginApplication.LoginEngine.ActivationManager.User.UserIdInfos != null &&
				LoginManager.LoginApplication.LoginEngine.ActivationManager.User.UserIdInfos.Length > 0)
				actid = LoginManager.LoginApplication.LoginEngine.ActivationManager.User.UserIdInfos[0].ActivationID;
			if (String.IsNullOrWhiteSpace(Code))
			{
				//todo ERRORE  migliora diagnostica
				sms.Visible = false;
				error.Visible = true;
                NotSupported.Visible = false;
				LblInfo.Visible = false;
			}
			if (string.IsNullOrEmpty(actid))
			{
				//ERRORE manca dato essenziale
				sms.Visible = false;
				error.Visible = true;
				LblInfo.Visible = false;
                NotSupported.Visible = false;
			}

		
           
            // string codeText = String.Format("007 {0} {1}", actid, Code);
           string codeText = String.Format("{0} {1}", actid, Code);//cambio tipologia ricezione: non serve più la chiave da  21/03/11
            string codeTextplusMask = "{0} {1}";
            string boldHtmlCodeText = String.Format("<b>&nbsp;{0}&nbsp;</b>", codeText);



            //FUNZIONE NON SUPPORTATA in automatico ma possibile farla manualemnte
            if (String.IsNullOrEmpty(PhoneNumberMACNIL) || PhoneNumberMACNIL.StartsWith("##"))
            {
                ImgresNotsup.Visible = false ;
                LblResNotsup.Visible = false;
                LabelWip.Text = LoginManagerStrings.LblSMSNotSupported;
                error.Visible = false;
                sms.Visible = false;
                OK.Visible = false;
                LblInfo.Visible = false;
                NotSupported.Visible = true;
                LblSendSMS_NS.Text = boldHtmlCodeText;
                Label1.Text = LoginManagerStrings.LblInsertAndClick;
                Button2.Text = LoginManagerStrings.SMSPageBtnRegisterText;
                return;
            }

            // per semplicità in italia togliamo il prefisso internazionale perchè alcuni lo scrivono male ( senza  + o senza 00 e questo fa si che il messaggio non arrivi) 
            if (String.Compare(LoginManager.LoginApplication.LoginEngine.ActivationManager.User.Country, "it", true, CultureInfo.InvariantCulture) == 0)
                PhoneNumberMACNIL = PhoneNumberMACNIL.Remove(0, 3);

            LblSendSMS.Text = boldHtmlCodeText;
            LblInfo.Text = MakeSmall(LoginManagerStrings.LblInfo3);
            LblInfo0.Text = MakeSmall(LoginManagerStrings.ServiceAvaiability);
            LblSendSmsInfo1.Text = String.Format( LoginManagerStrings.SendSmsInfo1, PhoneNumberMACNIL);
            LblInsertAndClick.Text = LoginManagerStrings.LblInsertAndClick;
            LblInfoCase.Text = MakeSmall(LoginManagerStrings.Case);
            LabelNote.Text = LoginManagerStrings.Notes;

            LblVatNumber1.Text = LoginManagerStrings.SMSInfoVatNumber1;
            string ex1 = String.Format(LoginManagerStrings.Example, String.Format(codeTextplusMask, codeText, "12345678910") );
            LblVatNumber2.Text = MakeSmall(String.Concat(LoginManagerStrings.SMSInfoVatNumber2, "<br>",ex1));

            LblRemark1.Text = LoginManagerStrings.LblRemark1;
            string ex2 = String.Format(LoginManagerStrings.Example, String.Format(codeTextplusMask, codeText, "<b>39</b>3471234567") );
            LblRemark2.Text = MakeSmall(String.Concat(LoginManagerStrings.LblRemark2, "<br>", ex2));

            LblReceivedError1.Text = LoginManagerStrings.LblReceivedError1;
            LblReceivedError2.Text = MakeSmall(LoginManagerStrings.LblReceivedError2); 

            LblSMSNotReceived1.Text = LoginManagerStrings.LblSMSNotReceived1;
            LblSMSNotReceived2.Text = MakeSmall(LoginManagerStrings.LblSMSNotReceived2);

            string ex3 = String.Format(LoginManagerStrings.LblSMSNotReceived3, PhoneNumberVOLA, addressSMS);
            LblSMSNotReceived3Title.Text = LoginManagerStrings.LblSMSNotReceived3Title;
            LblSMSNotReceived3.Text = MakeSmall(ex3);
        }

        
        private string GetSMSVolaNumber_CompileTimeBrand(string pcode)
        {

            if (pcode.ToLower(CultureInfo.InvariantCulture) == "vs")
                return "";
            return "+393480085660";
        }
        private string GetSMSMACNILNumber_CompileTimeBrand(string pcode)
        {

            if (pcode.ToLower(CultureInfo.InvariantCulture) == "vs")
                return "";
            return "+393669622734";
        }
        private string GetSMSMailService_CompileTimeBrand(string pcode)
        {

            if (pcode.ToLower(CultureInfo.InvariantCulture) == "vs")
                return "";
            return "Servizio.SMS@microarea.it";
        }

        //---------------------------------------------------------------------------
        private string MakeSmall(String s)
        {
            return String.Format("<p>{0}</p>", s);
        }

		//---------------------------------------------------------------------------
		protected void BtnRegister_Click(object sender, EventArgs e)
		{
            if (NotSupported.Visible)
            {
                bool res = LoginManager.LoginApplication.LoginEngine.ValidateSMS(TextBox2.Text);

                ImgresNotsup.Visible = true;
                LblResNotsup.Visible = true;
                ImgresNotsup.Src = res ? "img/ResultGreen.png" : "img/ResultRed.png";
                LblResNotsup.Text = res ? String.Format(LoginManagerStrings.SMSOK, prodName) : LoginManagerStrings.SmsCodeError;
                LblResNotsup.ForeColor = res ? System.Drawing.Color.Green : System.Drawing.Color.Red;
            }
            else
            {
                bool res = LoginManager.LoginApplication.LoginEngine.ValidateSMS(TxtCode.Text);

                ImgRes.Visible = true;
                LblRes.Visible = true;
                ImgRes.Src = res ? "img/ResultGreen.png" : "img/ResultRed.png";
                LblRes.Text = res ? String.Format(LoginManagerStrings.SMSOK, prodName) : LoginManagerStrings.SmsCodeError;
                LblRes.ForeColor = res ? System.Drawing.Color.Green : System.Drawing.Color.Red;
            }
			/*//se non è più attivato mostro l'errore----no deve andare lo stesso
			if (!res && !LoginManager.LoginApplication.LoginEngine.IsActivated())
			{
				LblError.Text = errorMessage;
				error.Visible = true;
				sms.Visible = false;
				OK.Visible = false;
				LblInfo.Visible = false;
				return;
			}*/

		}
	}
}
