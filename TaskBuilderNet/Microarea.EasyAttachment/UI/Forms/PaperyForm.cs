using System;
using System.Windows.Forms;

using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.UI.Controls;

namespace Microarea.EasyAttachment.UI.Forms
{
	///<summary>
	/// Form richiamata per la creazione di un attachment da "cartaceo"
	///</summary>
	//================================================================================
	public partial class PaperyForm : Form
	{
        private DMSDocOrchestrator dmsDocOrchestrator;
		private BarcodeManager barcodeMng;

		///<summary>
		/// constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public PaperyForm(DMSDocOrchestrator dmsOrch)
		{
			InitializeComponent();

            this.dmsDocOrchestrator = dmsOrch;

            barcodeMng = new BarcodeManager(dmsDocOrchestrator);
			// istanzio localmente un BarcodeManager e scelgo di non controllare la presenza del valore nel db
			// (non posso utilizzare quello globale dell'orchestrator perche' questa impostazione deve valere solo in questo caso)
			barcodeMng.CheckIfBarcodeExists = false;

			// disabilito il control contenente il barcode del pending papery
			PaperyCtrl.BarcodeEnableStatus = BarcodeEnableStatus.AlwaysEnabled;

			// aggancio gli eventi necessari
            string prefix = String.IsNullOrWhiteSpace(dmsDocOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodePrefix) ? Strings.None : dmsDocOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodePrefix;
			PaperyCtrl.RenderingBarcode += new PaperyUserCtrl.RenderingBarcodeDelegate(PaperyCtrl_RenderingBarcode);
            LblBarcodeType.Text = String.Format
                (LblBarcodeType.Text, dmsDocOrchestrator.SettingsManager.UsersSettingState.Options.BarcodeDetectionOptionsState.BarcodeTypeValue, 
                prefix
                ); 
		}

		///<summary>
		/// Evento scatenato sull'inserimento del valore del barcode
		///</summary>
		//--------------------------------------------------------------------------------
        Barcode PaperyCtrl_RenderingBarcode(TypedBarcode barcode)
		{
			Barcode myBarcode = barcodeMng.GetBarcodeImageFromValue(barcode);
			return myBarcode;
		}

		///<summary>
		/// Sul click del pulsante OK rimando tutto all'ArchiveManager per quanto riguarda la
		/// creazione dell'attachment/papery
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnOK_Click(object sender, EventArgs e)
		{
            PaperyCtrl.RenderImage();
			// eseguo il salvataggio della pending papery oppure creo l'attachment al volo
            if (this.dmsDocOrchestrator.AttachManager.AttachPapery(this.PaperyCtrl.Value.Value, this.PaperyCtrl.Notes, string.Empty) == ArchiveResult.TerminatedSuccess)
				this.Close(); // chiudo la form solo se e' andato a buon fine
		}
	}
}
