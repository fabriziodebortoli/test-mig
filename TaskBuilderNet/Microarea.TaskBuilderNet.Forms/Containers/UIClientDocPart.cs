using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Forms
{
	/// <summary>
	/// 
	/// </summary>
    //================================================================================================================
    [ToolboxItem(false)] 
    public abstract class UIClientDocPart : UIDocumentPart
	{ 
		//----------------------------------------------------------------------------
		protected UIClientDocPart()
			: this(null, null)
		{

		}
 
        //----------------------------------------------------------------------------
		protected UIClientDocPart(UIUserControl mainUI, IDocumentPart documentPart)
			: base (mainUI, documentPart)
        {
        }
    }
}
