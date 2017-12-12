using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.EasyAttachment.UI.Controls
{
	///<summary>
	/// UserControl che espone il toolstrip per la ricerca di stato del doc SOS
	///</summary>
	//================================================================================
	public partial class SOSDocStatusSearchToolStrip : UserControl
	{
	
		//--------------------------------------------------------------------------------
		[Browsable(false)]
		public bool ToSend
		{
			get
			{
				// se il menu e' visibile ritorno il suo valore altrimenti torno false
                return TSMISend.Checked; 
			}
		}

		//--------------------------------------------------------------------------------
		[Browsable(false)]
		public bool ToResend
		{
			get
			{
				 return TSMIResend.Checked;
			}
		}

        //--------------------------------------------------------------------------------
        public List<StatoDocumento> GetStatoDocumento()
        {
            List<StatoDocumento > stats = new List<StatoDocumento>();
            if (ToResend) stats.Add(StatoDocumento.TORESEND);
            if (ToSend) stats.Add(StatoDocumento.IDLE);
            return stats;
        }

		///<summary>
		/// costruttore
		///</summary>
		//--------------------------------------------------------------------------------
        public SOSDocStatusSearchToolStrip()
		{
			InitializeComponent();
		}
	}
}
