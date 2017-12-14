using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.EasyAttachment.UI.Controls
{
	[System.ComponentModel.DesignerCategory("code")]
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
	//================================================================================	
	public partial class ToolStripSOSDocStatusSearch : ToolStripControlHost
	{
		//--------------------------------------------------------------------------------
        public ToolStripSOSDocStatusSearch() : base(new SOSDocStatusSearchToolStrip()) { }

		// espone all'esterno lo slider
        public SOSDocStatusSearchToolStrip SOSDocStatusSearch { get { return Control as SOSDocStatusSearchToolStrip; } }

        // set other defaults that are interesting
        //--------------------------------------------------------------------------------
        protected override Size DefaultSize { get { return new Size(200, 100); } }


        //--------------------------------------------------------------------------------
        internal List<StatoDocumento> GetStatoDocumento()
        {
            return SOSDocStatusSearch.GetStatoDocumento();
        }
    }
}