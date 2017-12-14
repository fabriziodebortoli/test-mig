using System;
using System.Data.SqlTypes;
using System.Drawing;
using System.Windows.Forms;

using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.EasyAttachment.UI.Controls
{
	///<summary>
	/// UserControl per la visualizzazione delle informazioni dei riferimenti di un allegato
	/// relativi alla conservazione in SOS
	///</summary>
	//================================================================================
	public partial class SOSUserCtrl : UserControl
	{
		private SOSManager sosManager = null;
		
		// Events
		//--------------------------------------------------------------------------------
		public delegate AttachmentInfo CreateSOSDocumentDelegate();
		public event CreateSOSDocumentDelegate CreateSOSDocument;

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public SOSUserCtrl()
		{
			InitializeComponent();
		}

		///<summary>
		/// Init del SOSManager (da richiamare sempre)
		///</summary>
		//--------------------------------------------------------------------------------
		public void Init(SOSManager sm)
		{
			sosManager = sm;
		}

		///<summary>
		/// Valorizza i controls dall'esterno
		///</summary>
		//--------------------------------------------------------------------------------
		public void InitControls(AttachmentInfo ai)
		{
			if (sosManager == null)
			{
				System.Diagnostics.Debug.Fail("SOSManager is null! Please call 'Init' method of SOSInfoUserCtrl!");
				return;
			}

			string msg;
            CanBeSentToSOSType canBeSentType= sosManager.CanBeSentToSOS(ai, out msg);

			LblInfo.Visible = false; // sempre invisibile (compare solo sul click del BtnSOS)

			LblDocStatus.Visible = PictBoxDocStatus.Visible = (ai.SOSDocumentStatus != StatoDocumento.EMPTY);

			if (LblDocStatus.Visible)
			{
				StatoDocumento sd;
				if (Enum.TryParse(ai.SOSDocumentStatus.ToString(), out sd))
				{
					LblDocStatus.Text = Utils.GetDocumentStatusDescription(sd);
					PictBoxDocStatus.Image = Utils.GetDocumentStatusImage(sd);
				}
				else
				{
					LblDocStatus.Text = string.Empty;
					PictBoxDocStatus.Image = null;
				}
			}

			// se il sosdoc e' null oppure sono in uno stato inferiore al SENT faccio sparire i controls non necessari
			if (ai.SOSDocumentStatus == StatoDocumento.EMPTY || (int)ai.SOSDocumentStatus < (int)StatoDocumento.SENT)
			{
				BtnSendToSOS.Visible = (ai.SOSDocumentStatus == StatoDocumento.EMPTY);
				BtnSendToSOS.Location = new Point(BtnSendToSOS.Location.X, LblAbsoluteCode.Location.Y);

                if (canBeSentType != CanBeSentToSOSType.BeSent)
				{
					LblInfo.Location = new Point(LblInfo.Location.X, LblLotID.Location.Y + 10);
					LblInfo.Visible = true;
                    LblInfo.Text = msg;
                }
                BtnSendToSOS.Enabled = ai.CreateSOSBookmark = (canBeSentType != CanBeSentToSOSType.NoPDFA); // il pulsante di Add e' abilitato solo se il doc e' "inviabile" al SOS                
				LblAbsoluteCode.Visible = LblLotID.Visible = LblRegDate.Visible = false;
				LblAbsoluteCodeValue.Visible = LblLotIDValue.Visible = LblRegDateValue.Visible = false;
			}
			else
			{
				BtnSendToSOS.Visible = false;

				LblAbsoluteCode.Visible = LblLotID.Visible = LblRegDate.Visible = true;
				LblAbsoluteCodeValue.Visible = LblLotIDValue.Visible = LblRegDateValue.Visible = true;

				LblAbsoluteCodeValue.Text = ai.SOSAbsoluteCode;
				LblLotIDValue.Text = ai.SOSLotID;

				LblRegDateValue.Text = (ai.SOSRegistrationDate == null || ai.SOSRegistrationDate == (DateTime)SqlDateTime.MinValue)
					? Strings.Notavailable : ((DateTime)(ai.SOSRegistrationDate)).ToShortDateString();
			}
		}

		///<summary>
		/// Click del pulsante di Add del SOSDocument
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnSendToSOS_Click(object sender, EventArgs e)
		{
			AttachmentInfo ai = null;
			if (CreateSOSDocument != null)
				ai = CreateSOSDocument();

			if (ai != null)
			{
				((Button)(sender)).Enabled = (ai.SOSDocumentStatus == StatoDocumento.EMPTY);

				// vado ad aggiornare i controls 
				InitControls(ai);
			}
		}
	}
}
