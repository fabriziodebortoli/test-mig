using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
    //===========================================================================
    public partial class TBTabPage : TabPage
    {

        #region Private Data Members                
       
        private Image                   image           = null;
        private Color                   textColor       = Color.Empty;
        
        #endregion
                
        #region Public Properties

        //---------------------------------------------------------------------
        [Browsable(true), Description("")]
        public Image StripImage { get { return image; } set { image = value; } }

        //---------------------------------------------------------------------
        [Browsable(true), Description("")]
        public Color TextColor { get { return textColor; } set { textColor = value; } }
                
        #endregion 
       
        
        //---------------------------------------------------------------------
        public TBTabPage()
        {
            InitializeComponent();                        
            textColor = Color.Black;
        }

        //---------------------------------------------------------------------
        public TBTabPage(Image aImage, Color aColorText)
        {
            InitializeComponent();

            textColor = aColorText;
            image = aImage;

        }

        //------------------------------------------------------------------------------------------------------------------------------
        public TBTabPage(string caption)
			: base(caption)
		{
			InitializeComponent();
		}

        //---------------------------------------------------------------------
        public virtual void Close()
        {             
        }
      
    }
}
