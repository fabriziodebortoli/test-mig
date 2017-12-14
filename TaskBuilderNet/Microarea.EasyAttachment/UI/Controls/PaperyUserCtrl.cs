using System.ComponentModel;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;

namespace Microarea.EasyAttachment.UI.Controls
{
	///<summary>
	/// UserControl che consente la visualizzazione di un barcode e relativa nota
	/// Usata per l'archiviazione "da cartaceo"
	///</summary>
	//================================================================================
	public partial class PaperyUserCtrl : UserControl
	{
		// Properties
		//--------------------------------------------------------------------------------
		public TypedBarcode Value { get { return BarcodeDetailsCtrl.Barcode; } }
		public string Notes { get { return this.TxtNotes.Text; } }

		[Browsable(false)]
		public BarcodeEnableStatus BarcodeEnableStatus { get { return BarcodeDetailsCtrl.EnableStatus; } set { BarcodeDetailsCtrl.EnableStatus = value; } }

		// Events
		//--------------------------------------------------------------------------------
		// evento da intercettare esternamente per il rendering del barcode nell'area del Gdviewer
		//--------------------------------------------------------------------------------
		public delegate Barcode RenderingBarcodeDelegate(TypedBarcode barcode);
		public event RenderingBarcodeDelegate RenderingBarcode;

		///<summary>
		/// Costruttore
		///</summary>
		//--------------------------------------------------------------------------------
		public PaperyUserCtrl()
		{
			InitializeComponent();

			BarcodeDetailsCtrl.RenderingBarcode += new BarcodeDetails.RenderingBarcodeDelegate(BarcodeDetailsCtrl_RenderingBarcode);
		}

		//--------------------------------------------------------------------------------
        Barcode BarcodeDetailsCtrl_RenderingBarcode(TypedBarcode barcode)
		{
			if (RenderingBarcode != null)
				return RenderingBarcode(barcode);
			return null;
		}

        //--------------------------------------------------------------------------------
        internal void RenderImage()
        {
            BarcodeDetailsCtrl.RenderImage(true);
        }

		///<summary>
		/// Per valorizzare i controls dall'esterno
		///</summary>
		//--------------------------------------------------------------------------------
		internal void Valorize(TypedBarcode barcode, string notes)
		{
            BarcodeDetailsCtrl.Barcode = barcode;
			TxtNotes.Text = notes;
		}
	}
}