using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Forms
{
    //================================================================================================================
    /// <summary>
    ///   Tb Window Form 
    /// </summary>
    [ToolboxItem(false)] 
    public partial class UITabDialog : UIDocumentPart
    {
        //-------------------------------------------------------------------------
        [Browsable(false)]
        public override NameSpaceObjectType NameSpaceObjectType
        {
            get { return NameSpaceObjectType.TabDialog; }
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        //-------------------------------------------------------------------------
        public UITabDialog()
        {
        }
    }
}
