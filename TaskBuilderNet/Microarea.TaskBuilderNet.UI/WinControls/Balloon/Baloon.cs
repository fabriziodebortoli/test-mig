using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
    /// <summary>
    /// Window to show quick help information.
    /// </summary>
    //=========================================================================
    public sealed partial class Balloon : System.Windows.Forms.Form
    {
        //=====================================================================
        [Flags]
        public enum Position
        {
            None = 0,
            Up = 1,
            Down = 2,
            Right = 4,
            Left = 8
        }

        private const string errorNamespace = "Microarea.TaskBuilderNet.UI.WinControls.Balloon.Images.Error.png";
		private const string warningNamespace = "Microarea.TaskBuilderNet.UI.WinControls.Balloon.Images.Warning.png";
		private const string infoNamespace = "Microarea.TaskBuilderNet.UI.WinControls.Balloon.Images.Information.png";
		private const string helpNamespace = "Microarea.TaskBuilderNet.UI.WinControls.Balloon.Images.Help.png";
        private const double fadeStep = 0.1;

		private int				borderSize			= 1;
		private	int				animationTime		= 500;
		private string			titleText			= String.Empty;
		private string			contentText			= String.Empty;
		private Point			startPosition		= Point.Empty;
		private Color			borderColor			= Color.Transparent;
		private Control			associatedControl;
		private Form			associatedControlParentForm;
		private Image			titleImage;
		private BalloonType		baloonType			= BalloonType.None;

		private IContentManager	contentManager;

		#region Balloon constructors

		//---------------------------------------------------------------------
		[Obsolete("Use 'Balloon(BaloonType, Control)' instead", false)]
        public Balloon(string aHelpContent, string aTitle, BalloonType aType, Color aBorderColor, int aSizeBorder, Control associatedControl)
        {
            InitializeComponent();

            contentManager = new DefaultContentManager();

            if (associatedControl != null)
            {
                this.associatedControl = associatedControl;
                startPosition = associatedControl.PointToScreen(new Point(associatedControl.Width / 2, associatedControl.Height / 2));
            }
            else
                startPosition = MousePosition;

            titleText = aTitle;
            contentText = aHelpContent;

            BaloonType = aType;
        }

        //---------------------------------------------------------------------
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            ClosableControl cc = e.Control as ClosableControl;
            if (cc != null)
                cc.Close += new EventHandler(cc_Close);

        }

        //---------------------------------------------------------------------
        private void cc_Close(object sender, EventArgs e)
        {
            Close();
        }

        //---------------------------------------------------------------------
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            ClosableControl cc = e.Control as ClosableControl;
            if (cc != null)
                cc.Close -= new EventHandler(cc_Close);
        }

        //---------------------------------------------------------------------
        public Balloon(BalloonType aType, Control associatedControl)
        {
            InitializeComponent();

			contentManager = new DefaultContentManager();
			
			if (associatedControl != null)
			{
				this.associatedControl = associatedControl;
				startPosition = associatedControl.PointToScreen(new Point(associatedControl.Width / 2, associatedControl.Height / 2));

				Control aControl = associatedControl;
				while (aControl != null && !(aControl is Form))
					aControl = aControl.Parent;

				if (aControl != null)
				{
					associatedControlParentForm = aControl as Form;
					associatedControlParentForm.LocationChanged += new EventHandler(AssociatedControlParentForm_LocationChanged);
				}
			}
			else
				startPosition = MousePosition;
			
			BaloonType = aType;
		}

        ////---------------------------------------------------------------------
        //[Obsolete("Use 'Baloon(BaloonType, Control)' instead", false)]
        //public Balloon(string aHelpContent, string aTitle, BalloonType aType, Color aBorderColor, int aSizeBorder)
        //    : this(aHelpContent, aTitle, aType, aBorderColor, aSizeBorder, null)
        //{
        //}

        #endregion

		#region Balloon Properties

		//--------------------------------------------------------------------------------
        [Browsable(true)]
        public BalloonType BaloonType
        {
            get
            {
                return baloonType;
            }
            set
            {
                baloonType = value;
                string ns = null;
                switch (value)
                {
                    case BalloonType.Help:
                        ns = helpNamespace;
                        break;
                    case BalloonType.Error:
                        ns = errorNamespace;
                        break;
                    case BalloonType.Info:
                        ns = infoNamespace;
                        break;
                    case BalloonType.Warning:
                        ns = warningNamespace;
                        break;
                }

                if (ns != null)
                    titleImage = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(ns));

            }
        }

        //--------------------------------------------------------------------------------
        [Browsable(false)]
        public IContentManager ContentManager
        {
            get { return contentManager; }
            set { contentManager = value; }
        }

        //--------------------------------------------------------------------------------
        [Browsable(false)]
        public Control AssociatedControl
        {
            get { return associatedControl; }
        }

        //--------------------------------------------------------------------------------
        [Browsable(false)]
        public Point BaloonStartPosition
        {
            get { return startPosition; }
        }

        //--------------------------------------------------------------------------------
        [Browsable(true)]
        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; }
        }

        //--------------------------------------------------------------------------------
        [
        Browsable(true),
        DefaultValue(3)
        ]
        public int BorderSize
        {
            get { return borderSize; }
            set { borderSize = value; }
        }

        //--------------------------------------------------------------------------------
        [
        Browsable(true),
        DefaultValue(500)
        ]
        public int AnimationTime
        {
            get { return animationTime; }
            set { animationTime = value; }
        }

        //---------------------------------------------------------------------
        public string TitleText
        {
            get { return titleText; }
            set { titleText = value; }
        }

        //---------------------------------------------------------------------
        [Obsolete("Use 'ContentText' instead", false)]
        public string HelpContent { get { return contentText; } }

        //---------------------------------------------------------------------
        public string ContentText
        {
            get { return contentText; }
            set { contentText = value; }
        }

        //---------------------------------------------------------------------
        public Image TitleImage { get { return titleImage; } }

        #endregion

		#region Balloon private methods

		//---------------------------------------------------------------------
        private void SetBackgroundImage()
        {
            if (contentManager == null)
                throw new InvalidOperationException("Set a not null 'ContentManager' before");

            contentManager.SetBackgroundImage(this);
        }


		//---------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetBackgroundImage();
		}

        #endregion

        //---------------------------------------------------------------------
        public new void Show()
        {
            Opacity = 0;

            base.Show();

            if (animationTime > 0)
            {
                int interval = (int)(animationTime * fadeStep);
                for (double opax = 0; opax < 1.00; opax += fadeStep)
                {
                    this.Opacity = opax;
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(interval);
                }
            }
            else Opacity = 1.00;
        }

		//---------------------------------------------------------------------
		private void AssociatedControlParentForm_LocationChanged(object sender, EventArgs e)
		{
			if (associatedControl != null)
			{
				startPosition = associatedControl.PointToScreen(new Point(associatedControl.Width / 2, associatedControl.Height / 2));
				this.Location = new Point(startPosition.X, startPosition.Y - this.Height);
			}
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if (associatedControlParentForm != null)
					associatedControlParentForm.LocationChanged -= new EventHandler(AssociatedControlParentForm_LocationChanged);
				
			}
			base.Dispose(disposing);
		}

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        //---------------------------------------------------------------------
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Balloon));
            this.SuspendLayout();
            // 
			// Balloon
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackColor = System.Drawing.SystemColors.Window;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximizeBox = false;
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimizeBox = false;
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Balloon";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.TransparencyKey = System.Drawing.Color.DimGray;
            this.ResumeLayout(false);

		}
		#endregion
	}

    //=====================================================================
    public enum BalloonType
    {
        None,
        Error,
        Warning,
        Info,
        Help
    }
}
