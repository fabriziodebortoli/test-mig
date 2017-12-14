using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
namespace Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer
{
	/// <summary>
	/// AdvRendererManager.
	/// </summary>
	//=========================================================================
	public class AdvRendererManager : ClosableControl
	{
		private const string LblPagesIndexMask = "{0}/{1}";

		private System.Windows.Forms.Label LblPagesIndex;
		private System.Windows.Forms.PictureBox PictureBox;
		private ImageButton BtnPrev;
		private ImageButton BtnNext;
		private ImageButton BtnSave;
		private ImageButton BtnClose;
        private Timer closingTimer = null;//serve per i balloon temporizzat
        private bool enableAutoClosing = true;       
        private IList/*<Advertisement>*/ advertisements;
		private BaseAdvRenderer advRenderer;
		private System.Windows.Forms.Label LblCreationDate;
		private int currentAdvertisementIndex = -1;
        private CheckBox CkbBlock;
		private bool canSave = true;
        private LinkLabel linkLabelDisclaimer;
        private PictureBox PbSensation;
        private ImageList imageList1;
        private IContainer components;
        private Label LblNoMessages;
        private ImageButton BtnTrash;
        public event EventHandler<BalloonDataEventArgs> MessageChanging;
        public event EventHandler<BalloonDataEventArgs> MessageShowing;
		private bool compactView = false;

        MessageType blockedType = MessageType.None;

		//--------------------------------------------------------------------------- 
		public BaseAdvRenderer AdvertisementRenderer
		{
			get { return advRenderer; }
			set
			{
				Size currentSize = new Size(424,160);
				Point currentLocation = new Point(8,48);

				if (advRenderer != null)
				{
					currentLocation = advRenderer.Location;
					currentSize = advRenderer.Size;

					this.Controls.Remove(advRenderer);
					advRenderer.Dispose();
				}
				advRenderer = value;
				if (advRenderer != null)
				{
					advRenderer.Anchor = AnchorStyles.Bottom |AnchorStyles.Top |AnchorStyles.Left |AnchorStyles.Right  ;
					advRenderer.BackColor = Color.Transparent;
					advRenderer.Size = currentSize;
					advRenderer.Location = currentLocation;
					advRenderer.Visible = true;

					this.Controls.Add(advRenderer);
                    
				}
			}
		}

        //--------------------------------------------------------------------------- 
        public bool EnableAutoClosing
        {
            get{return enableAutoClosing;}
            set { enableAutoClosing = value; }
        }
            
        //--------------------------------------------------------------------------- 
		public bool CanSave
		{
			get { return canSave; }
			set 
			{ 
				canSave = value;

                this.BtnSave.Visible = this.BtnSave.Enabled = canSave;
                //spostamenti pulsanti a seconda della presenza del save
				if (canSave)
				{
					if (this.BtnNext.Left == this.BtnSave.Left)
					{
						this.BtnPrev.Left -= this.BtnPrev.Width + (this.BtnClose.Left - this.BtnSave.Right);
						this.BtnNext.Left -= this.BtnNext.Width + (this.BtnClose.Left - this.BtnSave.Right);
                        this.BtnTrash.Left -= this.BtnTrash.Width + (this.BtnClose.Left - this.BtnSave.Right);
					}
				}
				else
				{
					if (this.BtnNext.Left < this.BtnSave.Left)
					{ 
                        
                        this.BtnTrash.Left = this.BtnPrev.Left;
						this.BtnPrev.Left = this.BtnNext.Left;
						this.BtnNext.Left = this.BtnSave.Left;
                       
					}
				}
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(true)]
		public bool CompactView
		{
			get { return !PictureBox.Visible; }
			set
			{
				PictureBox.Visible = !value;
				PbSensation.Visible = !value;
				linkLabelDisclaimer.Visible = !value;
				LblCreationDate.Visible = !value;
				compactView = value;
			}
		}

		//--------------------------------------------------------------------------- 
		[Browsable(true)]
		public bool BtnTrashVisible
		{
			get { return BtnTrash.Visible; }
			set { BtnTrash.Visible = value; }
		}
		//--------------------------------------------------------------------------- 
		[Browsable(false)]
		public IList Advertisements
		{
			get { return advertisements; }
			set
			{
                if (advertisements != null) advertisements.Clear();
                else advertisements = new ArrayList(); 
                if (value != null)
                    foreach (Advertisement a in value)
                     advertisements.Add(a);

				//advertisements = value;
                BtnTrash.Enabled = BtnSave.Enabled = (advertisements != null && advertisements.Count > 0);
			}
		}

		//--------------------------------------------------------------------------- 
		[Browsable(false)]
		private new Color BackColor
		{
			set
			{
				this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
				this.UpdateStyles();
				base.BackColor = Color.Transparent;
			}
		}
		
		//--------------------------------------------------------------------------- 
		public AdvRendererManager()
		{
			InitializeComponent();
            string link = WinControlsStrings.Microarea;//"Microarea"; 
            linkLabelDisclaimer.Text =String.Format( WinControlsStrings.Disclaimer, link);// "Ricevi questo messaggio perchè sei un utente attivato, se non vuoi più ricevere questi messaggi accedi al sito " + link;
            linkLabelDisclaimer.LinkArea = new LinkArea(linkLabelDisclaimer.Text.IndexOf(link), link.Length); 
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			this.UpdateStyles();
			this.BackColor = Color.Transparent;
		}

		//--------------------------------------------------------------------------- 
		protected virtual void OnMessageShowing(BalloonDataEventArgs args)
		{
			if (MessageShowing != null)
				MessageShowing(this, args);
		}

        //--------------------------------------------------------------------------- 
        private  void SetSensationImage( IAdvertisement currentAdv)
        {
            
            if (currentAdv == null)
                return;
            int i = (int)currentAdv.Sensation;
            //presuppongo che nella imagelist le immagini 
            //sian messe nello stesso ordine dell'enumerativo, direi che ce la possiamo fare...
            if (i >= 0 && i < imageList1.Images.Count)
                PbSensation.Image = imageList1.Images[i];

        }


		//--------------------------------------------------------------------------- 
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
			currentAdvertisementIndex = 0;
			OnNewAdvertisementShowing();
		}
       
        //--------------------------------------------------------------------------- 
        private void OnCurrentAdvertisementChanging()
        {
            if
                (
                advertisements != null &&
                advertisements.Count > 0 &&
                currentAdvertisementIndex >= 0 &&
                currentAdvertisementIndex < advertisements.Count
                )
            {
                IAdvertisement currentAdv = advertisements[currentAdvertisementIndex] as IAdvertisement;
                if (currentAdv == null) 
                    return;

                if (MessageChanging != null)
                    MessageChanging(this, new BalloonDataEventArgs(new BalloonDataBag(CkbBlock.Checked, currentAdv.Type, currentAdv.ID)));

                if (CkbBlock.Checked)
                    {blockedType = blockedType | currentAdv.Type; }
                //se selezionato aggiungo ai tipi bloccati, nel caso ci fossero più messaggi dello stesso tipo da leggere 
                //all'interno della stessa sessione di balloon, li mostro ma col flag già deciso 
                 //se non selezionato lo levo
                else 
                    {blockedType = blockedType & ~ currentAdv.Type; }
                CkbBlock.Checked = false;
                
            } 
        }

       
		//--------------------------------------------------------------------------- 
		private void OnNewAdvertisementShowing()
		{
			if 
				(
				advertisements != null && 
				advertisements.Count > 0 && 
				currentAdvertisementIndex >= 0 && 
				currentAdvertisementIndex < advertisements.Count
				)
			{
                IAdvertisement currentAdv = advertisements[currentAdvertisementIndex] as IAdvertisement;
                if (currentAdv != null)
                {
                    LblNoMessages.Visible = false;

                    if (currentAdv.AutoClosingTime > 0)
                    {
                        if (closingTimer == null)
                        {
                            closingTimer = new Timer();
                            closingTimer.Tick += new EventHandler(closingTimer_Tick);   
                        }
                        else
                            closingTimer.Stop();

                        closingTimer.Interval = currentAdv.AutoClosingTime;
                        closingTimer.Start();
                    }
                    SetSensationImage(currentAdv);

                    switch (currentAdv.Type)
                    {
                        case MessageType.Advrtsm:
                            PictureBox.Image = GetAdvImage();
                            break;
                        case MessageType.Updates:
                            PictureBox.Image = GetUpdatesImage();
                            break;
                        case MessageType.Contract:
                            PictureBox.Image = GetContractImage();
                            break;
                        case MessageType.PostaLite:
                            PictureBox.Image = GetPostaliteImage();
                            break;
                        default:
                            PictureBox.Image = GetDefaultImage();
                            break;
                            
                    }

                    if (advRenderer != null)
                        advRenderer.RenderAdvertisement(currentAdv);
                    CkbBlock.Checked = ((currentAdv.Type & blockedType) == currentAdv.Type);

                    if (advertisements.Count == 1)//se un solo messaggio elimino cose inutilli
                    {
                        LblPagesIndex.Visible = false;
                        BtnNext.Visible = false;
                        BtnPrev.Visible = false;
                        BtnTrash.Location = BtnNext.Location;

                    }
                    else
                    {
                        LblPagesIndex.Text = String.Format(LblPagesIndexMask, currentAdvertisementIndex + 1, advertisements.Count);
                        LblPagesIndex.Visible = true;
                    }

                    LblCreationDate.Visible = true;
                    LblCreationDate.Text = currentAdv.CreationDate.ToShortDateString() + " " + currentAdv.CreationDate.ToShortTimeString();
                    //CkbBlock.Visible = false;// non più usato
                    linkLabelDisclaimer.Visible = !currentAdv.HideDisclaimer; 
                    OnMessageShowing(new BalloonDataEventArgs(new BalloonDataBag(currentAdv.ID)));
                }
                else Clear();
			}
            else Clear();
			
		
			BtnPrev.Enabled =
				(
				advertisements != null &&
				advertisements.Count > 1 &&
				currentAdvertisementIndex < advertisements.Count &&
				currentAdvertisementIndex > 0
				);
			BtnNext.Enabled =
				(
				advertisements != null &&
				advertisements.Count > 1 &&
				currentAdvertisementIndex >= 0 &&
				currentAdvertisementIndex < (advertisements.Count - 1)
				);

           
		}

        //se il balloon è temporizzato si chiude da solo al tempo prestabilito 
        //a meno che non si stia consultando o che si sita visualizzando messaggi vecchi 
        //allora non si deve  chiudere mai se non per il click dell'utente
        //--------------------------------------------------------------------------- 
        void closingTimer_Tick(object sender, EventArgs e)
        { 
            closingTimer.Stop();
            if (EnableAutoClosing )
                BtnClose_Click(sender, e);
        }

        //--------------------------------------------------------------------------- 
        private void Clear()
        {
            PictureBox.Image = GetDefaultImage();
            PbSensation.Image = null;
            if (advRenderer != null)
                advRenderer.Clear();
            LblPagesIndex.Text = String.Empty;
            LblCreationDate.Text = String.Empty;
            LblPagesIndex.Visible = false;
            LblCreationDate.Visible = false;
            linkLabelDisclaimer.Visible = false;
            LblNoMessages.Visible = true;
            BtnNext.Enabled = false;
            BtnSave.Enabled = false;
            BtnPrev.Enabled = false;
            BtnTrash.Enabled = false;
            BtnTrash.Visible = false;
           
        }

         //--------------------------------------------------------------------------- 
        private Image GetImage(string imageName)
        {
            Assembly currentAssembly = typeof(AdvRendererManager).Assembly;
            string ns = "Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer.Images.{0}";
            Stream imageStream = currentAssembly.GetManifestResourceStream(String.Format(ns, imageName));
            return Image.FromStream(imageStream);
        }
 
        //--------------------------------------------------------------------------- 
        private Image GetContractImage()
        {
            return GetImage("ContracWarning.png");
        }

        //--------------------------------------------------------------------------- 
        private Image GetUpdatesImage()
        {
            return GetImage("Updates.png");
        }

        //--------------------------------------------------------------------------- 
        private Image GetPostaliteImage()
        {
            return GetImage("icone baloon_postalite.png");
        }
        
        //--------------------------------------------------------------------------- 
        private Image GetAdvImage()
        {
            return GetImage("Megaphone2.png");
        }

         //--------------------------------------------------------------------------- 
        private Image GetDefaultImage()
        {
            return GetImage("Default.png");
        }
        

		//--------------------------------------------------------------------------- 
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (
					advRenderer != null &&
					!advRenderer.IsDisposed
					)
				{
					advRenderer.Dispose();
					advertisements = null;
				}
                if (PictureBox != null && PictureBox.Image != null)
                    PictureBox.Image.Dispose();

                if (closingTimer != null)
                    closingTimer.Dispose();
				
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		//--------------------------------------------------------------------------- 
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvRendererManager));
            this.BtnPrev = new Microarea.TaskBuilderNet.UI.WinControls.ImageButton();
            this.BtnNext = new Microarea.TaskBuilderNet.UI.WinControls.ImageButton();
            this.BtnSave = new Microarea.TaskBuilderNet.UI.WinControls.ImageButton();
            this.BtnClose = new Microarea.TaskBuilderNet.UI.WinControls.ImageButton();
            this.LblPagesIndex = new System.Windows.Forms.Label();
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.LblCreationDate = new System.Windows.Forms.Label();
            this.CkbBlock = new System.Windows.Forms.CheckBox();
            this.linkLabelDisclaimer = new System.Windows.Forms.LinkLabel();
            this.PbSensation = new System.Windows.Forms.PictureBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.LblNoMessages = new System.Windows.Forms.Label();
            this.BtnTrash = new Microarea.TaskBuilderNet.UI.WinControls.ImageButton();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbSensation)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnPrev
            // 
            this.BtnPrev.AlignImage = Microarea.TaskBuilderNet.UI.WinControls.Alignment.Left;
            resources.ApplyResources(this.BtnPrev, "BtnPrev");
            this.BtnPrev.BackColor = System.Drawing.Color.Transparent;
            this.BtnPrev.Name = "BtnPrev";
            this.BtnPrev.UseVisualStyleBackColor = false;
            this.BtnPrev.Click += new System.EventHandler(this.BtnPrev_Click);
            // 
            // BtnNext
            // 
            this.BtnNext.AlignImage = Microarea.TaskBuilderNet.UI.WinControls.Alignment.Left;
            resources.ApplyResources(this.BtnNext, "BtnNext");
            this.BtnNext.BackColor = System.Drawing.Color.Transparent;
            this.BtnNext.Name = "BtnNext";
            this.BtnNext.UseVisualStyleBackColor = false;
            this.BtnNext.Click += new System.EventHandler(this.BtnNext_Click);
            // 
            // BtnSave
            // 
            this.BtnSave.AlignImage = Microarea.TaskBuilderNet.UI.WinControls.Alignment.Left;
            resources.ApplyResources(this.BtnSave, "BtnSave");
            this.BtnSave.BackColor = System.Drawing.Color.Transparent;
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.UseVisualStyleBackColor = false;
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BtnClose
            // 
            this.BtnClose.AlignImage = Microarea.TaskBuilderNet.UI.WinControls.Alignment.Left;
            resources.ApplyResources(this.BtnClose, "BtnClose");
            this.BtnClose.BackColor = System.Drawing.Color.Transparent;
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.UseVisualStyleBackColor = false;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // LblPagesIndex
            // 
            resources.ApplyResources(this.LblPagesIndex, "LblPagesIndex");
            this.LblPagesIndex.BackColor = System.Drawing.Color.Transparent;
            this.LblPagesIndex.Name = "LblPagesIndex";
            // 
            // PictureBox
            // 
            this.PictureBox.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.PictureBox, "PictureBox");
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.TabStop = false;
            // 
            // LblCreationDate
            // 
            resources.ApplyResources(this.LblCreationDate, "LblCreationDate");
            this.LblCreationDate.Name = "LblCreationDate";
            // 
            // CkbBlock
            // 
            resources.ApplyResources(this.CkbBlock, "CkbBlock");
            this.CkbBlock.Name = "CkbBlock";
            this.CkbBlock.UseVisualStyleBackColor = true;
            // 
            // linkLabelDisclaimer
            // 
            resources.ApplyResources(this.linkLabelDisclaimer, "linkLabelDisclaimer");
            this.linkLabelDisclaimer.Name = "linkLabelDisclaimer";
            this.linkLabelDisclaimer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDisclaimer_LinkClicked);
            // 
            // PbSensation
            // 
            resources.ApplyResources(this.PbSensation, "PbSensation");
            this.PbSensation.Name = "PbSensation";
            this.PbSensation.TabStop = false;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Information.png");
            this.imageList1.Images.SetKeyName(1, "ResultGreen.png");
            this.imageList1.Images.SetKeyName(2, "Warning.png");
            this.imageList1.Images.SetKeyName(3, "Error.png");
            this.imageList1.Images.SetKeyName(4, "AccessDenied.png");
            this.imageList1.Images.SetKeyName(5, "Help.png");
            // 
            // LblNoMessages
            // 
            resources.ApplyResources(this.LblNoMessages, "LblNoMessages");
            this.LblNoMessages.Name = "LblNoMessages";
            // 
            // BtnTrash
            // 
            this.BtnTrash.AlignImage = Microarea.TaskBuilderNet.UI.WinControls.Alignment.Left;
            resources.ApplyResources(this.BtnTrash, "BtnTrash");
            this.BtnTrash.BackColor = System.Drawing.Color.Transparent;
            this.BtnTrash.Name = "BtnTrash";
            this.BtnTrash.UseVisualStyleBackColor = false;
            this.BtnTrash.Click += new System.EventHandler(this.BtnTrash_Click);
            // 
            // AdvRendererManager
            // 
            this.Controls.Add(this.BtnTrash);
            this.Controls.Add(this.LblNoMessages);
            this.Controls.Add(this.PbSensation);
            this.Controls.Add(this.linkLabelDisclaimer);
            this.Controls.Add(this.CkbBlock);
            this.Controls.Add(this.LblCreationDate);
            this.Controls.Add(this.PictureBox);
            this.Controls.Add(this.LblPagesIndex);
            this.Controls.Add(this.BtnNext);
            this.Controls.Add(this.BtnClose);
            this.Controls.Add(this.BtnPrev);
            this.Controls.Add(this.BtnSave);
            this.Name = "AdvRendererManager";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PbSensation)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		//--------------------------------------------------------------------------- 
		private void BtnPrev_Click(object sender, System.EventArgs e)
		{
			if (currentAdvertisementIndex > 0)
            {
                EnableAutoClosing = false;
                OnCurrentAdvertisementChanging();
				currentAdvertisementIndex--;
				OnNewAdvertisementShowing();
			}
		}

		//--------------------------------------------------------------------------- 
		private void BtnNext_Click(object sender, System.EventArgs e)
		{
			if (currentAdvertisementIndex < advertisements.Count - 1)
            {
                EnableAutoClosing = false;
                OnCurrentAdvertisementChanging();
				currentAdvertisementIndex++;
				OnNewAdvertisementShowing();
			}
		}

		//--------------------------------------------------------------------------- 
		private void BtnClose_Click(object sender, System.EventArgs e)
		{
            OnCurrentAdvertisementChanging();
			OnClose(sender, e);
		}

		//--------------------------------------------------------------------------- 
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if 
				(
				advertisements != null && 
				advertisements.Count > 0 && 
				currentAdvertisementIndex >= 0 && 
				currentAdvertisementIndex < advertisements.Count
				)
            {
                EnableAutoClosing = false;
				IAdvertisement currentAdv = advertisements[currentAdvertisementIndex] as IAdvertisement;
				SaveMessage(currentAdv);
			}
		}

		//--------------------------------------------------------------------------- 
		private void SaveMessage(IAdvertisement currentAdv)
		{
			if (currentAdv == null)
				return;
			string AllFileHtmExtension = "*.htm" ;
			SaveFileDialog ofd = new SaveFileDialog();
			ofd.DefaultExt = AllFileHtmExtension;
			ofd.Filter = WinControlsStrings.HtmFile + "|" + AllFileHtmExtension;
			ofd.Title = WinControlsStrings.SelectFile;
			DialogResult res = ofd.ShowDialog(this);

			if (res == DialogResult.OK)
			{
				using (StreamWriter output = new StreamWriter(ofd.FileName))
				{
					output.WriteLine(currentAdv.Body.Html);
				}
			}
		}

        //---------------------------------------------------------------------
        private void linkLabelDisclaimer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                HelpManager.ConnectToSSOLink("http://www.microarea.it/MyAccount/CompanyProfiles.aspx", AdvertisementRenderer.AutToken);
            }                                                                                                                       
            catch { }
        }

        public event BalloonEventHandler RemovingBalloonRequired;
        public delegate void BalloonEventHandler( string messageid);
        //---------------------------------------------------------------------
        /// <summary>
        /// elimina messaggio corrente
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTrash_Click(object sender, EventArgs e)
        {
            if
               (
               advertisements != null &&
               advertisements.Count > 0 &&
               currentAdvertisementIndex >= 0 &&
               currentAdvertisementIndex < advertisements.Count
               )
            {
                IAdvertisement currentAdv = advertisements[currentAdvertisementIndex] as IAdvertisement;
                if (currentAdv == null)
                    return;
                if (RemovingBalloonRequired != null)
                    RemovingBalloonRequired(currentAdv.ID);

                advertisements.RemoveAt(currentAdvertisementIndex);
               
                GoToNext();

            }

        }

        //---------------------------------------------------------------------
        private void GoToNext()
        {
            //dopo una cancellazione per esempio forzo lo spostamento sul messaggio 
            //successivo se diposnibile od eventualmente sul precedente, modificando l'index per correggere il posizionamento.
            if (BtnNext.Enabled)
            {
                currentAdvertisementIndex--;
                BtnNext_Click(this, EventArgs.Empty);
            }
            else if (BtnPrev.Enabled)
            {

                BtnPrev_Click(this, EventArgs.Empty);
            }
            else Clear();

        }
	}

    ///<summary>
    /// Args utilizzato nella visualizzazione dell'attachment nella finestra di Search
    ///</summary>
    //================================================================================
    [Serializable]
    public class BalloonDataEventArgs : EventArgs
    {
        //---------------------------------------------------------------------
        public BalloonDataEventArgs(BalloonDataBag b)
		{
            BalloonData = b;
		}

        public BalloonDataBag BalloonData { get; set; }
 
    }

}
