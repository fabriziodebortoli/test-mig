using System.Windows.Forms;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using Microarea.TaskBuilderNet.TbSenderBL.postalite.api;

namespace TbSenderTestUI
{
	public partial class SubscriptionEditor : UserControl
	{
		//-------------------------------------------------------------------------------
		public SubscriptionEditor()
		{
			InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		private ISubscriberInfo ReadEditedSubscriptionInfo()
		{
			ISubscriberInfo subscriberInfo = new SubscriberInfo()
			{
				Subscriber_SurNameCompanyName = txtCompanyName.Text.Trim(),
				Subscriber_City = txtCity.Text.Trim(),
				Subscriber_Address = txtAddress.Text.Trim(),
				Subscriber_ZipCode = txtZip.Text.Trim(),
				Subscriber_County = txtCounty.Text.Trim(),
				Subscriber_State = txtCountry.Text.Trim(),
				Subscriber_VatNumber = txtVatNumber.Text.Trim(),
				Subscriber_TelephoneNumber = txtTelephoneNumber.Text.Trim(),
				//Subscriber_FaxNumber = txtf
				Subscriber_EMail = txtEMail.Text.Trim()
			};
			return subscriberInfo;
		}

		//-------------------------------------------------------------------------------
		private void DisplaySubscriptionInfo(ISubscriberInfo subscriberInfo)
		{
			txtCompanyName.Text = subscriberInfo.Subscriber_SurNameCompanyName;
			txtCity.Text = subscriberInfo.Subscriber_City;
			txtAddress.Text = subscriberInfo.Subscriber_Address;
			txtZip.Text = subscriberInfo.Subscriber_ZipCode;
			txtCounty.Text = subscriberInfo.Subscriber_County;
			txtCountry.Text = subscriberInfo.Subscriber_State;
			txtVatNumber.Text = subscriberInfo.Subscriber_VatNumber;
			txtTelephoneNumber.Text = subscriberInfo.Subscriber_TelephoneNumber;
			txtEMail.Text = subscriberInfo.Subscriber_EMail;
		}

		//-------------------------------------------------------------------------------
		public ISubscriberInfo SubscriberInfo
		{
			get { return ReadEditedSubscriptionInfo(); }
			set { DisplaySubscriptionInfo(value); }
		}
	}
}
