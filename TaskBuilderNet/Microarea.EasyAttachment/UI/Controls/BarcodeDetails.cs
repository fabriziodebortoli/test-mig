using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.UI.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TBPicComponents;

namespace Microarea.EasyAttachment.UI.Controls
{
	// enum per abilitare/disabilitare dall'esterno i controls della form
	public enum BarcodeEnableStatus { AlwaysEnabled, AlwaysDisabled, EnableIfValueIsEmpty } ;

	///<summary>
	/// UserControl che visualizza il valore del barcode e relativa immagine a barre (in un GdViewer control)
	///</summary>
	//================================================================================
	public partial class BarcodeDetails : UserControl
	{
		private bool forAutomaticBarcodeDetection = true;

		private BarcodeEnableStatus enableStatus = BarcodeEnableStatus.AlwaysEnabled;

		//--------------------------------------------------------------------------------
		// evento da intercettare esternamente per il rendering del barcode nell'area del Gdviewer
		public delegate Barcode RenderingBarcodeDelegate(TypedBarcode barcode);
		public event RenderingBarcodeDelegate RenderingBarcode;

		// evento da intercettare esternamente per effettuare il detect manuale del barcode nell'immagine corrente del GdViewer
		public delegate string DetectingBarcodeDelegate();
		public event DetectingBarcodeDelegate DetectingBarcode;

		// evento da intercettare esternamente per considerare il valore dei settings per pilotare il tipo di detect
		// N.B. se NON intercetto questo evento il click sul pulsante non segue i settings ma esegue sempre e solo la load dell'immagine
		public delegate bool AutomaticBarcodeDetectionDelegate();
		public event AutomaticBarcodeDetectionDelegate AutomaticBarcodeDetection;
        private TypedBarcode originalbc = null;
		// properties
		//--------------------------------------------------------------------------------
		[Browsable(false)]
		public TypedBarcode Barcode
		{
            get { return new TypedBarcode(TxtBCValue.Text, originalbc.Type); }
			set
			{
                originalbc = value;
                TxtBCValue.Text = originalbc.Value;
                RenderImage(false);
				EnableControls();
			}
		}

		// proprieta' che abilita/disabilita l'editing del control dall'esterno (il default e' true)
		[Browsable(false)]
		public BarcodeEnableStatus EnableStatus { get { return enableStatus; } set { enableStatus = value; EnableControls(); } }

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public BarcodeDetails()
		{
			InitializeComponent();
			TxtBCValue.Focus();
		}

		///<summary>
		/// Per impostare il readonly su tutti i controls in un colpo solo
		///</summary>
		//--------------------------------------------------------------------------------
		private void EnableControls()
		{
			switch (enableStatus)
			{
				default:
				case BarcodeEnableStatus.AlwaysEnabled:
					TxtBCValue.Enabled = true;
					BtnShowBarcode.Enabled = true;
					break;
				case BarcodeEnableStatus.EnableIfValueIsEmpty:
					TxtBCValue.Enabled = string.IsNullOrWhiteSpace(TxtBCValue.Text);
					BtnShowBarcode.Enabled = string.IsNullOrWhiteSpace(TxtBCValue.Text);
					break;
				case BarcodeEnableStatus.AlwaysDisabled:
					TxtBCValue.Enabled = false;
					BtnShowBarcode.Enabled = false;
					break;
			}
		}

		///<summary>
		/// Sul mousemove visualizziamo un tooltip diverso a seconda dei parametri
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnShowBarcode_MouseMove(object sender, MouseEventArgs e)
		{
			if (forAutomaticBarcodeDetection)
			{
				if (string.Compare(BCToolTip.GetToolTip((Button)sender), Strings.BarcodeLoadImage, StringComparison.InvariantCultureIgnoreCase) != 0)
					BCToolTip.SetToolTip((Button)sender, Strings.BarcodeLoadImage);
			}
			else
				if (string.Compare(BCToolTip.GetToolTip((Button)sender), Strings.ExecDetectBarcode, StringComparison.InvariantCultureIgnoreCase) != 0)
					BCToolTip.SetToolTip((Button)sender, Strings.ExecDetectBarcode);
		}

		///<summary>
		/// Sul click del pulsante eseguo un'operazione diversa a seconda delle impostazioni dei parametri
		/// posso chiamare il detect manuale del barcode oppure la sola visualizzazione grafica del barcode
		///</summary>
		//--------------------------------------------------------------------------------
		private void BtnShowBarcode_Click(object sender, EventArgs e)
		{
			if (forAutomaticBarcodeDetection)
				RenderImage(true);
			else
				DetectBarcodeInCurrentImage();
		}

		///<summary>
		/// Spara un evento che se intercettato esegue il detect on demand del barcode all'interno del documento
		///</summary>
		//--------------------------------------------------------------------------------
		private void DetectBarcodeInCurrentImage()
		{
			if (DetectingBarcode != null)
			{
				string detectedValue = DetectingBarcode();

				// se la text-box contiene gia' un valore ed e' diverso da quello individuato visualizzo un msg all'utente
				// e l'utente decide di non sovrascrivere il valore corrente, non procedo
				if (!string.IsNullOrWhiteSpace(TxtBCValue.Text) &&
					string.Compare(TxtBCValue.Text, detectedValue, StringComparison.InvariantCultureIgnoreCase) != 0 &&
					MessageBox.Show(string.Format(Strings.OverrideBarcodeValue, detectedValue, TxtBCValue.Text),
						string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
						return;
				
				// altrimenti assegno il valore
				TxtBCValue.Text = detectedValue;
				RenderImage(true);
			}
		}

		///<summary>
		/// Se nella textbox del valore clicco Enter o Tab forzo la visualizzazione del barcode
		///</summary>
		//--------------------------------------------------------------------------------
		private void TxtBCValue_KeyPress(object sender, KeyPressEventArgs e)
		{
			// 9: ascii code for horizontal tab, or \t
			// 13: ascii code for carriage return, or enter, or \r
			if (Convert.ToInt32(e.KeyChar) == 13 || Convert.ToInt32(e.KeyChar) == 9)
				RenderImage(true);
		}

		///<summary>
		/// Sul leave della textbox con il valore forzo la visualizzazione del barcode
		///</summary>
		//--------------------------------------------------------------------------------
		private void TxtBCValue_Leave(object sender, EventArgs e)
		{
			RenderImage(true);
		}

		///<summary>
		/// Appena entro con il mouse sul panel che contiene lo user control,
		/// sparo un evento che esternamente legge il valore dei Settings per pilotare poi le operazioni
		///</summary>
		//--------------------------------------------------------------------------------
		private void BCPanel_MouseEnter(object sender, EventArgs e)
		{
			if (AutomaticBarcodeDetection != null)
				forAutomaticBarcodeDetection = AutomaticBarcodeDetection();
		}

		//--------------------------------------------------------------------------------
		private void BarcodeDetails_VisibleChanged(object sender, EventArgs e)
		{
			// se lo user control e' visible allora renderizzo il barcode
			if (this.Visible)
				RenderImage(false);
		}

		//--------------------------------------------------------------------------------
		internal void RenderImage(bool showMessages)
		{
			if (RenderingBarcode != null)
			{
				Barcode bc = RenderingBarcode(new TypedBarcode(this.TxtBCValue.Text, originalbc.Type));//todo ila ilaria barcode
				if (bc == null)
				{
					this.BarcodeViewer.CloseDocument();
					return;
				}

				switch (bc.Status)
				{
					case BarcodeStatus.OK:
						{
							if (bc.ImageId > 0)
							{
								TBPictureStatus gStatus = this.BarcodeViewer.DisplayFromGdPictureImage(bc.ImageId);
								LblNoBarcode.Visible = gStatus != TBPictureStatus.OK;
							}
							else
								this.BarcodeViewer.CloseDocument();
							break;
						}

					case BarcodeStatus.DrawingError:
					case BarcodeStatus.SyntaxNotValid:
						{
							if (showMessages)
							{
								// se c'e' stato un errore in lettura/scrittura del barcode, mostro la form con un messaggio 
								using (SafeThreadCallContext context = new SafeThreadCallContext())
								{
                                    InvalidBarcode ibForm = new InvalidBarcode();
									ibForm.ShowDialog();
								}
							}

							TxtBCValue.Clear();//.Undo();
							this.BarcodeViewer.CloseDocument();
							//TxtBCValue.ClearUndo();
							//showMessages = false;
							//RenderImage(showMessages);
							TxtBCValue.Focus();
							break;
						}
                    case BarcodeStatus.TypeNotValid:
                        {
                            LblNoBarcode.Visible = true;                          
                            break;
                        }

					/*case BarcodeStatus.AlreadyExists:
						{
							if (showMessages)
							{
								// se il barcode esiste gia' nel db, visualizzo un msg
								using (SafeThreadCallContext context = new SafeThreadCallContext())
								{
									InvalidBarcode ibForm = new InvalidBarcode();
									ibForm.SetMessageText = string.Format(Strings.BarcodeValueAlreadyExists, TxtBCValue.Text);
									ibForm.ShowDialog();
								}
							}
							
							TxtBCValue.Undo();
							TxtBCValue.ClearUndo();
							showMessages = false;
							RenderImage(showMessages);
							TxtBCValue.Focus();
							break;
						}*/
				}
			}

		}
	}
}
