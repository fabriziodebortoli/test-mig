using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
//using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
    /// ================================================================================
    public class TBForm : TBPanel
	{
		protected DynamicImageButton closeTBForm;
		private Label titleLabel;
		private Panel titlePanel;
		private Panel toolbarContainer;
		private int toolbarContainerTop = 0;

        internal TBWebControl wcView = null; // view se esiste
        internal TBWebControl wcStatusBar = null; // status bar se esiste;
        internal TBWebControl wcRadar = null; // radar inner data area se esiste;
		//Metto da parte i figli di primo livello ( devo spostarli in basso in caso di form che non hanno view o radar, 
		//come quelle dei Parametri e Servizi di Mago, c'e un tabber figlio diretto del frame con altri bottoni, senza la view)
		internal List<TBWebControl> wcFirstChildControls = new List<TBWebControl>();

		protected int zIndex = 0;

        protected WndObjDescription mainMenuObjDescription = null;  // contiene la descrizione del menu se esiste
        protected WndObjDescription titleObjDescription = null;  // contiene la descrizione del titolo se esiste
        protected WndObjDescription auxRadarToolbarObjDescription = null;  // contiene la descrizione della toolbar ausiliaria del radar se esiste
        protected WndObjDescription radarHeaderObjDescription = null;  // contiene la descrizione del header del radar

        // è il valore restituito dal browser quando scrollo la finestra. Mi basta riposizionare
        // la form in modo che anche i suoi controls (child, reltive) la seguono
        protected System.Drawing.Rectangle overridePositionRect = System.Drawing.Rectangle.Empty;

        // Lista degli acceleratori della form passati dalla parte C++
		private List<AcceleratorDescription> accelerators = new List<AcceleratorDescription>();

    	//--------------------------------------------------------------------------------------
        virtual public string Title { get { return titleObjDescription == null ? string.Empty : titleObjDescription.Text; } }

        //altezza del titolo della Form sul browser che dimensiona in modo diverso dalla parte GDI
        //--------------------------------------------------------------------------------------
        public static int TitleHeight { get { return 42; } }

		//--------------------------------------------------------------------------------------
		public int ToolbarContainerTop {  get { return toolbarContainerTop; }}

		//--------------------------------------------------------------------------------------
        public override int X 
        {
            get { return overridePositionRect.IsEmpty ? Math.Max(base.X, 0) : overridePositionRect.X; } 
            set { base.X = value; } 
        }

		//--------------------------------------------------------------------------------------
        public override int Y 
        { 
            get { return overridePositionRect.IsEmpty ? Math.Max(base.Y, 0) : overridePositionRect.Y; } 
            set { base.Y = value; } 
        }

		//--------------------------------------------------------------------------------------
        public override int Width 
        { 
            get { return overridePositionRect.IsEmpty ? Math.Max(base.Width, 0) : overridePositionRect.Width; } 
            set { base.Width = value; } 
        }

		//--------------------------------------------------------------------------------------
        public override int Height 
        { 
            get { return overridePositionRect.IsEmpty ? Math.Max(base.Height, 0) : overridePositionRect.Height; } 
            set { base.Height = value; } 
        }

		//--------------------------------------------------------------------------------------
		public override int ZIndex { get { return zIndex; } }
		
        //--------------------------------------------------------------------------------------
		protected override string InitFunctionName { get { return  "initForm"; } }
		
        //--------------------------------------------------------------------------------------
		public override WndObjDescription ControlDescription
		{
			get
			{
				if (base.ControlDescription == null)
					base.ControlDescription = new WndObjDescription();
				return base.ControlDescription;
			}
		}

		//--------------------------------------------------------------------------------------
        public override TBForm OwnerForm { get { return this;  } }
		
		//--------------------------------------------------------------------------------------
        public TBForm()
		{
		}

		//--------------------------------------------------------------------------------------
		internal void AssignZIndex(int zIndex)
		{
			this.zIndex = zIndex;
		}

		//--------------------------------------------------------------------------------------
		internal void RegisterAccelerator(AcceleratorDescription acceleratorDescription)
		{
			if (!accelerators.Contains(acceleratorDescription, new AcceleratorDescriptionComparer()))
				accelerators.Add(acceleratorDescription);
		}

        //--------------------------------------------------------------------------------------
        internal virtual int FirstControlOffset
        {
            get
            {
                return  TitleHeight -
                        (titleObjDescription == null ? 0 : titleObjDescription.Height) -
                        (mainMenuObjDescription == null ? 0 : mainMenuObjDescription.Height);
            }
        }

        //--------------------------------------------------------------------------------------
        private void InitSiblingsWndObjDescription(WndObjDescription description)
        {
            foreach (WndObjDescription d in description.Childs)
            {
                // elementi non gestiti ma presenti lato C++
                switch (d.Type)
                {                             
                    case WndObjDescription.WndObjType.MainMenu          : mainMenuObjDescription = d;           break;
                    case WndObjDescription.WndObjType.AuxRadarToolbar   : auxRadarToolbarObjDescription = d;    break;
                    case WndObjDescription.WndObjType.RadarHeader       : radarHeaderObjDescription = d;        break;
                }
            }
        }    
    
        //--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
            titleLabel = new Label();
			titlePanel = new Panel();
			closeTBForm = new DynamicImageButton();

            InitSiblingsWndObjDescription(ControlDescription);
            SetStartingSizeAndPosition();
			base.OnInit(e);

			//creo il titolo nella oninit perche' il bottone di chiusura, 
			//creato al suo interno, deve essere presente da subito per poter ricevere
			//gli eventi di click
			CreateTitle();

			RegisterInitControlScript();
			AddChildControls(ControlDescription);
		}

        //--------------------------------------------------------------------------------------
        protected virtual void SetStartingSizeAndPosition()
        {
        }

        //--------------------------------------------------------------------------------------
        protected virtual void RepositionChilds(int offset)
        {
			if (wcView != null)
			{
				// abbasso la view 
				ChildsOffset = new Point(0,offset);
				wcView.SetControlAttributes();

				// devo anche allungare la Frame perchè ho spostato la View verso il basso
				InflateSize = new Size(0,offset);
				SetControlAttributes();
			}
			else
			{
				// abbasso i controlli figli di primo livello del frame  (come fossero una view)
				ChildsOffset = new Point(0,offset);
				// devo anche allungare la Frame perchè ho spostato i controlli verso il basso
				InflateSize = new Size(0, offset);
				SetControlAttributes();
				for (int i = 0; i < wcFirstChildControls.Count; i++)
				{
					wcFirstChildControls[i].SetControlAttributes();
				}
			}

            // se c'è anche la status bar si deve allargare il frame (anche se non esiste la view)
            if (wcStatusBar != null)
            {
                // devo anche allungare la Frame perchè ho spostato la View verso il basso
                InflateSize = new Size(0, offset);
                wcStatusBar.SetControlAttributes();
                SetControlAttributes();
            }
        }

		//--------------------------------------------------------------------------------------
		public override void AddChildControls(WndObjDescription description)
		{
         	int toolbarContainerHeight = 0;
			toolbarContainerTop = TitleHeight;
			if (toolbarContainer != null)
			{
				//Se era gia valorizzato lo rimuovo perche dopo viene ricreato da zero
				Panel.Controls.Remove(toolbarContainer);
			}
			toolbarContainer = new Panel();
            int top = 0;
            int easyBuilderToolbarOffset = 0;
            bool firstToolbar = true;

			//svuoto la lista dei controlli figli diretti della form
			wcFirstChildControls.Clear();
            
			foreach (WndObjDescription d in description.Childs)
            {
				switch (d.Type)
				{
                    case WndObjDescription.WndObjType.Panel:
                        {
                            if (d.Childs.Count > 0 && ((WndObjDescription)d.Childs[0]).Type == WndObjDescription.WndObjType.TabbedToolbar)
                            {
                                TBWebControl wc = d.CreateControl(this);
                                if (wc != null)
                                {
                                    toolbarContainer.Controls.Add(wc);
                                    addedControls.Add(wc);
                                    wc.AddChildControls(d);

                                    toolbarContainerHeight += d.Height;
                                    wc.InnerControl.Width = Unit.Pixel(wc.Width - 2 * BorderWidth);
                                    wc.InnerControl.Height = Unit.Pixel(d.Height);
                                }
                            }
                            break;

                        }
					case WndObjDescription.WndObjType.Toolbar:
                    case WndObjDescription.WndObjType.EasyBuilderToolbar:
						{
							TBWebControl wc = d.CreateControl(this);
							if (wc != null)
							{
								toolbarContainer.Controls.Add(wc);
								addedControls.Add(wc);
								wc.AddChildControls(d);

								toolbarContainerHeight += wc.Height;
								wc.InnerControl.Width = Unit.Pixel(wc.Width - 2 * BorderWidth);
								wc.InnerControl.Height = Unit.Pixel(wc.Height - 2 * BorderWidth);
								wc.InnerControl.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(top).ToString();

								// solo la prima toolbar non ha il bordo di sopra x collegarsi bene al titolo
								if (firstToolbar) wc.InnerControl.Style["border-top"] = "0px solid #8F9B9C";
								firstToolbar = false;
								top += wc.Height;
                                if (d.Type == WndObjDescription.WndObjType.EasyBuilderToolbar)
                                    easyBuilderToolbarOffset += wc.Height;
							}
							break;
						}
					case WndObjDescription.WndObjType.StatusBar: wcStatusBar = this.AddControl(d); break;
					case WndObjDescription.WndObjType.Radar: wcRadar = this.AddControl(d); break;
					case WndObjDescription.WndObjType.View: wcView = this.AddControl(d); break;

					default:
						{
							TBWebControl tbwc = AddControl(d);
							if (tbwc != null)
								wcFirstChildControls.Add(tbwc);
							break;
						}
				}
            }

            // devo tener conto del bordo dell'ultima toolbar messo tramite css
            RepositionChilds(FirstControlOffset + easyBuilderToolbarOffset + (firstToolbar ? 0 : 1));

            toolbarContainer.Width = Unit.Percentage(100);
            toolbarContainer.Height = toolbarContainerHeight;
			toolbarContainer.CssClass = "TBToolbarContainer";
            toolbarContainer.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(toolbarContainerTop).ToString();
            Panel.Controls.Add(toolbarContainer);
		}

		//--------------------------------------------------------------------------------------
		protected virtual void AddFocusDummyField()
		{
			TextBox focusText = new TextBox(); //used only to give focus to the form even if none of its control has focus (otherwise, accelerators do not work)
			focusText.ID = "DummyFocus" + ID;
			focusText.Height = Unit.Pixel(5);
			focusText.Attributes["onkeydown"] = string.Format("return textKeyDown(event, this,'{0}','{1}','{2}')", windowId, ClientID, OwnerForm.InnerControl.ClientID);
			focusText.Attributes.Add("onfocus", string.Format("focusChild($get('{0}'));", this.ClientID));
			InnerControl.Controls.Add(focusText);

			//if no control has focus, sets focus to a dummy control of the active form (so it can receive keyboard events)
			formControl.AdjustControlFocus(focusText.ID); 
		}
	

		//--------------------------------------------------------------------------------------
		protected virtual void CreateBrandBar()
		{
			//do nothing
		}

        // titolo contenente : pulsante di close, logo prodotto, caption
		//--------------------------------------------------------------------------------------
		private void CreateTitle()
		{
			System.Web.UI.WebControls.Table titleTable = new System.Web.UI.WebControls.Table();
			titleTable.Height = Unit.Percentage(100);
			TableRow trTitle = new TableRow();
			titleTable.Rows.Add(trTitle);

            // pulsante di chiusura della finestra
			TableCell tdClose = new TableCell();
			
            closeTBForm.Style[HtmlTextWriterStyle.Cursor] = "hand";
			closeTBForm.ImageUrl = ImagesHelper.CreateImageAndGetUrl("FullScreenClose.png", TBWebFormControl.DefaultReferringType);
			closeTBForm.TabIndex = -1;
			closeTBForm.CssClass = "NotFocusable";
		
			tdClose.Controls.Add(closeTBForm);
			if (formControl != null)
				formControl.RegisterControl(closeTBForm, this);
			
			trTitle.Cells.Add(tdClose);
			
            // Logo di prodotto solo se stiamo parlando di Mago.Net

			IBrandInfo brandInfo = InstallationData.BrandLoader.GetMainBrandInfo();
			if (brandInfo.ProductTitle == "Mago.Net")
			{
				TableCell tdLogo = new TableCell();
				System.Web.UI.WebControls.Image logo = new System.Web.UI.WebControls.Image();
				logo.ImageUrl = ImagesHelper.CreateImageAndGetUrl("LogoMagoNet.png", TBWebFormControl.DefaultReferringType);
				tdLogo.Style.Add(HtmlTextWriterStyle.PaddingRight, "6px");
				tdLogo.Controls.Add(logo);
				trTitle.Cells.Add(tdLogo);
			}

            // caption della form contenente il titolo vero e proprio
			TableCell tdCaption = new TableCell();
			titleLabel.CssClass = "TBFormTitleLabel";
			titleLabel.Attributes["onmousedown"] = string.Format("dragTitleMouseDown(event, $get('{0}'))", InnerControl.ClientID);
			tdCaption.Controls.Add(titleLabel);
			trTitle.Cells.Add(tdCaption);


			titlePanel.CssClass = "TBFormTitle";
			titlePanel.Height = TitleHeight;
			titlePanel.Width = Unit.Percentage(100);
			titlePanel.ID = "Title" + ID;
			titlePanel.Attributes["onmousedown"] = string.Format("dragTitleMouseDown(event, $get('{0}'))", InnerControl.ClientID);

			titlePanel.Controls.Add(titleTable);
			Panel.Controls.Add(titlePanel);

			//aggiunge gli eventi di chiusura
			AddClosingEvents();
		}

		//--------------------------------------------------------------------------------------
		/// <summary>
		/// Aggiunge gli eventi di chiusura ai bottoni della form dedicati
		/// </summary>
		protected virtual void AddClosingEvents ()
		{
			closeTBForm.OnClientClick = string.Format("tbClose($get('{0}'))", ClientID);
		}

		//--------------------------------------------------------------------------------------
		private void CreateBorder()
		{
			//larghezza dell'area sensibile intorno al bordo della finestra per permettere il ridimensionamento con il mouse
			int resizeAreaWidth = 7;
			//larghezza del bordino della form, che e' disegnato all'interno dell'area sensibile  che permette il ridimensionamento
			int borderFormWidth = 1;
			//top
			Panel panel = new Panel();
			panel.CssClass = "TBFormBorder";
			panel.Height = Unit.Pixel(resizeAreaWidth);
			panel.Width = Panel.Width;
			panel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(0).ToString();
			panel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(-(resizeAreaWidth + borderFormWidth)).ToString();
			panel.Attributes["onmousedown"] = string.Format("dragBorderMouseDown(event, $get('{0}'), true, false, false, false)", InnerControl.ClientID);
			panel.Style[HtmlTextWriterStyle.Cursor] = "n-resize";
			Panel.Controls.Add(panel);

			//left
			panel = new Panel();
			panel.CssClass = "TBFormBorder";
			panel.Height = Panel.Height;
			panel.Width = Unit.Pixel(resizeAreaWidth);
			panel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(-(resizeAreaWidth + borderFormWidth)).ToString();
			panel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(0).ToString();
			panel.Attributes["onmousedown"] = string.Format("dragBorderMouseDown(event, $get('{0}'), false, true, false, false)", InnerControl.ClientID);
			panel.Style[HtmlTextWriterStyle.Cursor] = "w-resize";
			Panel.Controls.Add(panel);

			//bottom
			panel = new Panel();
			panel.CssClass = "TBFormBorder";
			panel.Height = Unit.Pixel(resizeAreaWidth);
			panel.Width = Panel.Width;
			panel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(0).ToString();
			panel.Style[HtmlTextWriterStyle.Top] = (Unit.Pixel((int)(Panel.Height.Value + borderFormWidth))).ToString();
			panel.Attributes["onmousedown"] = string.Format("dragBorderMouseDown(event, $get('{0}'), false, false, true, false)", InnerControl.ClientID);
			panel.Style[HtmlTextWriterStyle.Cursor] = "s-resize";
			Panel.Controls.Add(panel);

			//right
			panel = new Panel();
			panel.CssClass = "TBFormBorder";
			panel.Height = Panel.Height;
			panel.Width = Unit.Pixel(resizeAreaWidth);
			panel.Style[HtmlTextWriterStyle.Left] = (Unit.Pixel((int)(Panel.Width.Value + borderFormWidth))).ToString();
			panel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(0).ToString();
			panel.Attributes["onmousedown"] = string.Format("dragBorderMouseDown(event, $get('{0}'), false, false, false, true)", InnerControl.ClientID);
			panel.Style[HtmlTextWriterStyle.Cursor] = "e-resize";
			Panel.Controls.Add(panel);

			//top-left corner
			panel = new Panel();
			panel.CssClass = "TBFormBorder";
			panel.Width = Unit.Pixel(resizeAreaWidth);
			panel.Height = Unit.Pixel(resizeAreaWidth);
			panel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(-resizeAreaWidth).ToString();
			panel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(-resizeAreaWidth).ToString();
			panel.Attributes["onmousedown"] = string.Format("dragBorderMouseDown(event, $get('{0}'), true, true, false, false)", InnerControl.ClientID);
			panel.Style[HtmlTextWriterStyle.Cursor] = "nw-resize";
			Panel.Controls.Add(panel);

			//top-right corner
			panel = new Panel();
			panel.CssClass = "TBFormBorder";
			panel.Width = Unit.Pixel(resizeAreaWidth);
			panel.Height = Unit.Pixel(resizeAreaWidth);
			panel.Style[HtmlTextWriterStyle.Left] = Panel.Width.ToString();
			panel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(-resizeAreaWidth).ToString();
			panel.Attributes["onmousedown"] = string.Format("dragBorderMouseDown(event, $get('{0}'), true, false, false, true)", InnerControl.ClientID);
			panel.Style[HtmlTextWriterStyle.Cursor] = "ne-resize";
			Panel.Controls.Add(panel);

			//bottom-left corner
			panel = new Panel();
			panel.CssClass = "TBFormBorder";
			panel.Width = Unit.Pixel(resizeAreaWidth);
			panel.Height = Unit.Pixel(resizeAreaWidth);
			panel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(-resizeAreaWidth).ToString();
			panel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel((int)Panel.Height.Value).ToString();
			panel.Attributes["onmousedown"] = string.Format("dragBorderMouseDown(event, $get('{0}'), false, true, true, false)", InnerControl.ClientID);
			panel.Style[HtmlTextWriterStyle.Cursor] = "sw-resize";
			Panel.Controls.Add(panel);

			//bottom-right corner
			panel = new Panel();
			panel.CssClass = "TBFormBorder";
			panel.Width = Unit.Pixel(resizeAreaWidth);
			panel.Height = Unit.Pixel(resizeAreaWidth);
			panel.Style[HtmlTextWriterStyle.Left] = Panel.Width.ToString();
			panel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel((int)Panel.Height.Value).ToString();
			panel.Attributes["onmousedown"] = string.Format("dragBorderMouseDown(event, $get('{0}'), false, false, true, true)", InnerControl.ClientID);
			panel.Style[HtmlTextWriterStyle.Cursor] = "se-resize";
			Panel.Controls.Add(panel);
		}
		//--------------------------------------------------------------------------------------
		protected override void OnPreRender(EventArgs e)
		{
			AddFocusDummyField();
			
			CreateBorder();
			CreateBrandBar();

			base.OnPreRender(e);
            titleLabel.Text = Title;
		}

		//--------------------------------------------------------------------------------------
		internal void GenerateAcceleratorScript()
		{
			if (formControl.AddScriptResource("Accelerator", "Accelerator"))
				Page.Session["Accelerator"] = GetGenericAcceleratorScript();
			
			AcceleratorManager mng = new AcceleratorManager(InnerControl.ClientID);
			string script = mng.GenerateScript(accelerators);
			//genero un guid usato come chiave per salvare in session lo script
			string guid = Guid.NewGuid().ToString();
			//se viene aggiunto, lo salvo in session, se non viene aggiunto vuol dire che era gia' presente
			if (formControl.AddScriptResource(this.ClientID, guid))
				Page.Session[guid] = script;
		}

		private static string GetGenericAcceleratorScript()
		{
			return
					@"function Accelerator(ch, cmd, window) {
					this.ch = ch;
					this.cmd = cmd;
					this.window = window;
				}
			";
		}

		///<summary>
		/// Viene impostato lo stile hidden per la form, in quanto lato javascript viene centrat e messa visibile
		/// se venissa mandata gia in stato di visibile si avrebbe un effetto di refresh sul browser
		/// </summary>
        //--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
        {
            base.SetControlAttributes();
			closeTBForm.ID = string.Format("{0}_Close", ID);

			InnerControl.Style[HtmlTextWriterStyle.Display] = "block";
			if (formControl != null && formControl.GetFormPosition(InnerControl.ClientID, out overridePositionRect))
			{
				//Se sono stato ridimensionato dal browser(quindi ho un overridePositionRect calcolato via Javascript)
				//non devo applicare gli offset e l'inflate size alla form, perche gia' calcolato al passo precedente
				//per evitare una allargamento-restringimento della form ad ogni round trip
				InnerControl.Width = Unit.Pixel(Width - 2 * BorderWidth);
				InnerControl.Height = Unit.Pixel(Height - 2 * BorderWidth);
				InnerControl.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X).ToString();
				InnerControl.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(Y).ToString();
	
				InnerControl.Style[HtmlTextWriterStyle.Visibility] = "visible";
			}
			else
			{
				//starts hidden, then javascript init funcion centers it and sets to visible
				InnerControl.Style[HtmlTextWriterStyle.Visibility] = "hidden";
			}
		}

        //--------------------------------------------------------------------------------------
		internal void AttachParent(TBUpdatePanel mainUpdatePanel)
        {
            parentPanel = mainUpdatePanel;
        }
    }

    //==========================================================================================
    public class TBDialog : TBForm
    {
        //--------------------------------------------------------------------------------------
        public TBDialog() { }

        // la Dialog non ha la View e quindi devo gestire l'offset dei childs mentre con la view
        // offsetto solo la view e i childs sono relativi a lei
        //--------------------------------------------------------------------------------------
        protected override void SetStartingSizeAndPosition()
        {
            InflateSize = new Size(0, FirstControlOffset);
            ChildsOffset = new Point(0, FirstControlOffset);
        }
    }

    //==========================================================================================
    public class TBPrintDialog : TBDialog
    {
    }

	//==========================================================================================
	public class TBFileDialog : TBDialog
	{
	}

    //==========================================================================================
    public class TBPropertyDialog : TBDialog
    {
    }
 
    //==========================================================================================
    public class TBRadar : TBForm
    {   
        //--------------------------------------------------------------------------------------
        public TBRadar() { }

        // la auxToolBar viene passata come descrizione ma non è implementata lato web e quindi
        // devo scalare i control non tenendo conto della sua altezza
        //--------------------------------------------------------------------------------------
        internal override int FirstControlOffset
        {
            get
            {
                return 
                    base.FirstControlOffset -
                    (auxRadarToolbarObjDescription == null ? 0 : auxRadarToolbarObjDescription.Height) -
                    (radarHeaderObjDescription == null ? 0 : radarHeaderObjDescription.Height);
            }
        }
                    
        //--------------------------------------------------------------------------------------
        protected override void RepositionChilds(int offset)
        {
            base.RepositionChilds(offset);
			if (auxRadarToolbarObjDescription != null)
			{
				ChildsOffset = new Point(0, offset);
				InflateSize = new Size(0, offset);
				wcRadar.SetControlAttributes();
				SetControlAttributes();
				if (wcStatusBar != null)
					wcStatusBar.SetControlAttributes();
			}
        }
    }

	//==========================================================================================
	public class TBMainForm : TBForm
	{

		///<summary>
		/// Per la form principale non considero la X della finestra C++.
		/// e' posizionata a 0 se l'utente non l'ha spostata, altrimenti tiene conto della nuova
		/// ascissa dovuta al drag/resize dell'utente sul browser (overridePositionRect)
		/// </summary>
		//--------------------------------------------------------------------------------------
		public override int X
		{
			get { return overridePositionRect.IsEmpty ? 0 : overridePositionRect.X; }
		}

		///<summary>
		/// Per la form principale non considero la Y della finestra C++.
		/// e' posizionata a 0 se l'utente non l'ha spostata, altrimenti tiene conto della nuova
		/// ordinata dovuta al drag/resize dell'utente sul browser (overridePositionRect)
		/// </summary>
		//--------------------------------------------------------------------------------------
		public override int Y
		{
			//COMMENTATO PER USCITA 3_8 (LA MODIFICA E" PER MAGO INFINITY)
			get { return overridePositionRect.IsEmpty ? 0/*40*/ : overridePositionRect.Y; }
		}

		//--------------------------------------------------------------------------------------
		protected override string InitFunctionName { get { return "initMainForm"; } }

		//--------------------------------------------------------------------------------------
		public override string GetInitScriptBody()
		{
			return string.Format("{0}('{1}', '{2}');", InitFunctionName, InnerControl.ClientID, formControl.ThreadId);
		}

		//--------------------------------------------------------------------------------------
		protected override void CreateBrandBar()
		{
            const string logoImage = "LogoMicroarea.png";

			//TODO gestione del BrandInfo in maniera centralizzata
			if (InstallationData.BrandLoader.GetCompanyName() == "Microarea S.p.A.")
			{
				HyperLink brandCompany = new HyperLink();
				brandCompany.TabIndex = -1;
				brandCompany.CssClass = "NotFocusable";
                brandCompany.NavigateUrl = HelpManager.GetProducerSiteURL();
				brandCompany.Target = "_blank";
				brandCompany.Style.Add(HtmlTextWriterStyle.Position, "absolute");
                brandCompany.ImageUrl = ImagesHelper.CreateImageAndGetUrl(logoImage, TBWebFormControl.DefaultReferringType);
				using (Bitmap img = ImagesHelper.GetStaticImage(logoImage, TBWebFormControl.DefaultReferringType))
				{
					brandCompany.Style.Add(HtmlTextWriterStyle.Top, Unit.Pixel((int)Panel.Height.Value + 3).ToString());
					int left = Width - img.Width;
					brandCompany.Style.Add(HtmlTextWriterStyle.Left, Unit.Pixel(left).ToString());
				}

				Panel.Controls.Add(brandCompany);
			}
		}

		/// <summary>
		/// La main form parte sempre visibile, perche non necessita di un riposianmento sul browser, e quindi non ci sara nessun effetto di refresh
		/// Infatti le finestre c++ partono posizionate gia' in (0,0) 
		/// </summary>
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			InnerControl.Style[HtmlTextWriterStyle.Visibility] = "visible";
		}
	}

}
