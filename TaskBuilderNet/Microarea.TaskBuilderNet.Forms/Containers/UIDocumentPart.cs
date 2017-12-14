using System.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Forms
{
    //=========================================================================
    [ToolboxItem(false)] 
    public partial class UIDocumentPart : UIUserControl, IUIDocumentPart
    {
        IDocumentPart documentPart;
        IUIUserControl mainUI;
       
        //---------------------------------------------------------------------
        public IDocumentPart DocumentPart
        {
            get { return documentPart; }
			set { documentPart = value; }
        }

        //---------------------------------------------------------------------
        public IUIUserControl MainUI
        {
            get { return mainUI; }
			set { mainUI = value; }
        }

        //---------------------------------------------------------------------
        public UIDocumentPart()
            : this(null, null)
        {}

        //---------------------------------------------------------------------
        public UIDocumentPart(IUIUserControl mainUI, IDocumentPart documentPart)
        {
            this.documentPart = documentPart;
            this.mainUI = mainUI;
            InitializeComponent();
        }

        //---------------------------------------------------------------------
        public virtual void OnAdded()
        {           
        }
    }
}
