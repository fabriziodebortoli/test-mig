using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
    /// ================================================================================
    public class DynamicImageButton : ImageButton 
	{
		//------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Attributes["onmouseover"] = string.Format("mouseOverButton('{0}')", ClientID);
			Attributes["onmouseout"] = string.Format("mouseOutButton('{0}')", ClientID);
		}
	}

    /// ================================================================================
    public class TBUpdatePanel : Panel
    {
        protected TBUpdatePanel parentPanel;
        private bool updated = false;

        public TBUpdatePanel()
        {
       
        }
		public UpdatePanelUpdateMode UpdateMode;
		public UpdatePanelRenderMode RenderMode;
		public bool ChildrenAsTriggers;
		public Control ContentTemplateContainer { get { return this; } }

        //--------------------------------------------------------------------------------------
        protected override void Render(HtmlTextWriter writer)
        {
			try
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "TbUpdatePanel");

				base.Render(writer);
			}
			catch (IndexOutOfRangeException)
			{
				//dummy style added to bypass UpdatePanel bug
				writer.AddTbStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
				base.Render(writer);

			}
        }
        //--------------------------------------------------------------------------------------
        public void Update()
        {
            updated = true;
        }
        //--------------------------------------------------------------------------------------
        public bool Updated { get { return updated ? true : parentPanel != null ? parentPanel.Updated : false; } }


		//--------------------------------------------------------------------------------------
		internal void RenderAjaxContent(AjaxResponse response)
		{
			if (updated)
			{
				using (StringWriter sw = new StringWriter())
				{
					using (HtmlTextWriter w = new HtmlTextWriter(sw))
					{
						RenderContents(w);
						response.AddPanel(ClientID, sw.ToString());
					}
				}
			}
			else
				foreach (Control c in Controls)
					RenderAjaxContent(c, response);
		}

		

		//--------------------------------------------------------------------------------------
		private void RenderAjaxContent(Control c, AjaxResponse response)
		{
			if (c is TBUpdatePanel)
				((TBUpdatePanel)c).RenderAjaxContent(response);
			else
				foreach (Control cc in c.Controls)
					RenderAjaxContent(cc, response);
		}
	}

    /// ================================================================================
    public abstract class TBWebControl : TBUpdatePanel, ITBWebControl, ICommand
	{
		private int proxyObjectId = 0;
		private string initScriptBody = "";
		protected string originalCssClass = null;
		protected string windowId = "";
		protected ITBWebControl parentTBWebControl;
		protected TBWebFormControl formControl;
		protected List<Control> addedControls = new List<Control>();
		protected new virtual short BorderWidth { get { return 0; } }
        private Point childsOffset = new Point(0,0);
        private Size inflateSize = new Size(0,0);

		public string InitScriptBody
		{
			get { return initScriptBody; }
			set { initScriptBody = value; }
		}
        public virtual Point ChildsOffset
 		{
            get { return childsOffset; }
            set { childsOffset = value; }
		}       
        public virtual Size InflateSize
		{
            get { return inflateSize; }
            set { inflateSize = value; }
		}
        public virtual int X
		{
			get { return ControlDescription.X;}
			set { ControlDescription.X = value;}
		}
		public virtual int Y		
		{
			get { return ControlDescription.Y; }
			set { ControlDescription.Y = value; }
		}
		public new virtual int Width
		{
			get { return ControlDescription.Width; }
			set { ControlDescription.Width = value; }
		}
		public new virtual int Height
		{
			get { return ControlDescription.Height; }
			set { ControlDescription.Height = value; }
		}

		//--------------------------------------------------------------------------------------
		public virtual int ZIndex 
		{
			get { return parentTBWebControl.ZIndex + 1; }
		}
		//--------------------------------------------------------------------------------------
		public virtual TBWebFormControl FormControl
		{
			get { return formControl; }
		}

		//--------------------------------------------------------------------------------------
		public UserInfo UserInfo
		{
			get { return UserInfo.FromSession(); }
		}
		//--------------------------------------------------------------------------------------
		internal virtual bool Focusable
		{
			get { return IsEnabled; }
		}
		//--------------------------------------------------------------------------------------
		public virtual WndObjDescription ControlDescription { get; set; }

		//--------------------------------------------------------------------------------------
		public int ProxyObjectId
		{
			get { return formControl == null ? 0 : formControl.ProxyObjectId; }
		}

		//--------------------------------------------------------------------------------------
		public virtual string WindowId
		{
			get { return windowId; }
			set { windowId = value; }
		}

		//--------------------------------------------------------------------------------------
		public ITBWebControl ParentTBWebControl
		{
			get { return parentTBWebControl; }
		}
      
		//--------------------------------------------------------------------------------------
		public abstract WebControl InnerControl { get; }
		//--------------------------------------------------------------------------------------
		protected new virtual bool IsEnabled
		{
			get { return ControlDescription.Enabled; }
		}

		//--------------------------------------------------------------------------------------
		protected bool HasTabIndex
		{
			get { return ControlDescription.HasTabIndex; }
		}

		//--------------------------------------------------------------------------------------
		public virtual TBForm OwnerForm { get { return (parentTBWebControl == null) ? null : parentTBWebControl.OwnerForm;} }
		//--------------------------------------------------------------------------------------
		protected TBWebControl()
		{
			RenderMode = UpdatePanelRenderMode.Block;
			UpdateMode = UpdatePanelUpdateMode.Conditional;
			ChildrenAsTriggers = false;
		}
     
		//--------------------------------------------------------------------------------------
		internal void SetInitValues(TBWebFormControl formControl, TBWebControl parentTBWebControl, WndObjDescription controlDescription)
		{
			this.formControl = formControl;
			this.parentTBWebControl = parentTBWebControl;
            this.parentPanel = parentTBWebControl;
			if (parentTBWebControl != null)
				this.proxyObjectId = parentTBWebControl.proxyObjectId;
			//resetta lo stato di finestre figlie aggiunte o rimosse
			controlDescription.ChildrenChanged = false;
			this.ControlDescription = controlDescription;
		}

	
		/// <summary>
		///  Crea l'immagine su file system a partire dallo stream di bytes, se imageDisabledName non e' nullo crea anche la versione disabilitata dell'immagine
		/// </summary>
		//--------------------------------------------------------------------------------------
		protected static void CreateDocumentImage (ImageBuffer image, string imageName, string imageDisabledName)
		{
			//prima cerco nella cache in memoria
			if (ImagesHelper.HasImageInCache(imageName))
				return;

			string file = ImagesHelper.GetImagePath(imageName);
			//poi su file system
			if (!File.Exists(file))
			{
				using (Bitmap b = image.CreateBitmap())
				{
					b.MakeTransparent(b.GetPixel(1,1));
					b.Save(file);

					if (!string.IsNullOrEmpty(imageDisabledName))
					{
						using (Bitmap dis = b.CreateDisabled())
							dis.Save(ImagesHelper.GetImagePath(imageDisabledName));
					}
				}
			}

			//infine aggiungo alla cache
			ImagesHelper.AddImageToCache(imageName);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Crea l'immagine su file system a partire dall'oggetto Bitmap passato come argomento, se imageDisabledName non e' nullo crea anche la versione disabilitata dell'immagine
		/// </summary>
		protected static void CreateDocumentImage (Bitmap bmp, string imageName, string imageDisabledName)
		{
			//prima cerco nella cache in memoria
			if (ImagesHelper.HasImageInCache(imageName))
				return;

			string file = ImagesHelper.GetImagePath(imageName);
			//poi su file system
			if (!File.Exists(file))
			{
				bmp.Save(file);

				if (!string.IsNullOrEmpty(imageDisabledName))
				{
					using (Bitmap disabledImage = bmp.CreateDisabled())
						disabledImage.Save(ImagesHelper.GetImagePath(imageDisabledName));
				}
			}

			//infine aggiungo alla cache
			ImagesHelper.AddImageToCache(imageName);
		}
		//--------------------------------------------------------------------------------------
		protected void RegisterInitControlScript()
		{
			formControl.RegisterInitControlScript(this);
		}

        //--------------------------------------------------------------------------------------
		protected virtual string InitFunctionName 
        {
            get
            {
                Debug.Fail(@"When you register control for initialization you must override InitFunctionName property!
This property has to return the name of the javascript function invoked to initialize this control at each roundtrip
(prototype for this function: <function name>('<control id>')");
			    return "";
            }
        }

		//--------------------------------------------------------------------------------------
		public virtual string GetInitScriptBody()
		{
			return string.IsNullOrEmpty(initScriptBody)
				? string.Format("{0}('{1}');", InitFunctionName, InnerControl.ClientID)
				: initScriptBody;
		}

		//--------------------------------------------------------------------------------------
		protected TBWebControl FindTBWebControl(WndObjDescription description)
		{
			string attr = description.Id;
			TBWebControl tbwc = FindControl(attr + "Upd") as TBWebControl;
			if (tbwc != null)
				return tbwc;

			return FindControl(attr) as TBWebControl;
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
		}

		//--------------------------------------------------------------------------------------
		protected void SetId(WndObjDescription description)
		{
			string id = description.Id;
			if (!string.IsNullOrEmpty(id))
				windowId = id;
			if (InnerControl.ID == null)
			{
				if (!string.IsNullOrEmpty(windowId))
				{
					InnerControl.ID = windowId;
					ID = InnerControl.ID + "Upd";
				}
			}
		}

		//--------------------------------------------------------------------------------------
		protected TBWebControl AddControl(WndObjDescription description)
		{
			TBWebControl wc = description.CreateControl(this);
			if (wc != null)
			{
				InnerControl.Controls.Add(wc);
				addedControls.Add(wc);
				wc.AddChildControls(description);
			}
			return wc;
		}

		//--------------------------------------------------------------------------------------
		protected virtual void RenderTBControl(HtmlTextWriter writer)
		{
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.ZIndex, ZIndex.ToString());
		 
            //WARNING! Update panel's size has to be 0,0, otherwise some other controls may be hidden to mouse input (even if you see them!)
        }

		//--------------------------------------------------------------------------------------
		protected override void Render(HtmlTextWriter writer)
		{
			RenderTBControl(writer);
			base.Render(writer);
		}

		//--------------------------------------------------------------------------------------
		public virtual void SetControlAttributes()
		{
            SetControlPosition(); 
            InnerControl.Style[HtmlTextWriterStyle.ZIndex] = ZIndex.ToString();

			AssignCssClass();
			
			InnerControl.ToolTip = ControlDescription.Tooltip;

			InnerControl.Enabled = IsEnabled;
			
			SetId(ControlDescription);

			EnableViewState = false;
			InnerControl.EnableViewState = false;

			if (ControlDescription.HasCustomFont)
			{
				InnerControl.Style[HtmlTextWriterStyle.FontFamily] = ControlDescription.Font.FontFaceName;
				InnerControl.Style[HtmlTextWriterStyle.FontSize] = Unit.Pixel(ControlDescription.Font.FontHeight).ToString();
				if (ControlDescription.Font.Bold)
					InnerControl.Style[HtmlTextWriterStyle.FontWeight] = "bold";
				if (ControlDescription.Font.Italic)
					InnerControl.Style[HtmlTextWriterStyle.FontStyle] = "italic";
			}
		}

        //--------------------------------------------------------------------------------------
		protected virtual void SetControlPosition()
        {
			try
			{
                // aggiungo la mia InflateSize
				//Width e Height non possono essere negative, nel caso di valore negativo le imposto a 0
				InnerControl.Width = Unit.Pixel(Math.Max(0, Width - 2 * BorderWidth + InflateSize.Width));
                InnerControl.Height = Unit.Pixel(Math.Max(0, Height - 2 * BorderWidth + InflateSize.Height));

                // mi sposto se il mio parent me lo chiede
                InnerControl.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + (parentTBWebControl != null? parentTBWebControl.ChildsOffset.X : 0)).ToString();
                InnerControl.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(Y + (parentTBWebControl != null ? parentTBWebControl.ChildsOffset.Y : 0)).ToString();
			}
			catch(Exception ex)
			{
				Debug.Fail(ex.ToString());
			}

        }
		 
		//--------------------------------------------------------------------------------------
		protected virtual void AssignCssClass()
		{
			StringBuilder sb = new StringBuilder();
			if (!String.IsNullOrEmpty(InnerControl.CssClass))
				sb.Append(InnerControl.CssClass);
				
			Type t = GetType();
			while (t != typeof(TBUpdatePanel))
			{
				if (sb.Length > 0)
					sb.Append(' ');
				sb.Append(t.Name);
				t = t.BaseType;
			}
			if (!IsEnabled)
				sb.Insert(0, "Disabled ");

			if (!HasTabIndex)
				sb.Insert(0, "NotFocusable ");
			
			if (originalCssClass == null)
				originalCssClass = InnerControl.CssClass;

			InnerControl.CssClass = sb.ToString();
		}

		//--------------------------------------------------------------------------------------
		public void AddChildControls()
		{
			foreach (WndObjDescription child in ControlDescription.Childs)
				AddControl(child);
		}

		//--------------------------------------------------------------------------------------
		public virtual void AddChildControls(WndObjDescription description)
		{
			foreach (WndObjDescription child in description.Childs)
				AddControl(child);
		}

		//--------------------------------------------------------------------------------------
		public void ClearChildControls()
		{
			foreach (Control c in addedControls)
				InnerControl.Controls.Remove(c);

			addedControls.Clear();
		}

		//--------------------------------------------------------------------------------------
		public virtual void UpdateFromControlDescription(WndObjDescription description)
		{
			//assegno la nuova descrizione alla descrizione del controllo
			ControlDescription = description;
			if (description.State == WndDescriptionState.UPDATED)
			{
				//se la descrizione e' un aggiornamento (stato di UPDATED) dico al controllo che deve ridisegnarsi
				//(la chiamata del metodo Update() fa si che il framework ajax invii il nuovo html per questo controllo al browser)
				Update();
				InnerControl.CssClass = originalCssClass;
				SetControlAttributes();
				description.State = WndDescriptionState.UNCHANGED;
			}
		}

		//--------------------------------------------------------------------------------------
		public void AddToContainer(WebControl control)
		{
			ContentTemplateContainer.Controls.Add(control);
			SetControlAttributes();
			if (formControl != null)
			{
				formControl.RegisterControl(this, this);
				formControl.RegisterControl(InnerControl, this);
				if (ControlDescription.HasAccelerator)
				{
					ControlDescription.Accelerator.WindowId = ClientID;
					OwnerForm.RegisterAccelerator(ControlDescription.Accelerator);
				}
			}
		}

		//--------------------------------------------------------------------------------------
		protected static string GetImageUrl (string hBitmap, bool enabled)
		{
			string imageName = GetImageName(hBitmap, enabled);
			return ImagesHelper.GetImageUrl(imageName);
		}

		//--------------------------------------------------------------------------------------
		protected static string GetImageName(string hBitmap, bool enabled)
		{
			return ImagesHelper.PrefixedName(hBitmap, enabled);
		}

		#region ICommand Members

		//--------------------------------------------------------------------------------------
		public virtual ITBWebControl CommandObject { get { return this; } }

		//--------------------------------------------------------------------------------------
		public virtual int CommandId { get { return String.IsNullOrEmpty(ControlDescription.Cmd) ? -1 : int.Parse(ControlDescription.Cmd); } }

		#endregion

	}
	
	class TBLabel : TBWebControl
	{ 
		protected Label label;

		///<summary>
		///Override della property Focusable, restituendo sempre false, perche in html le label 
		///(renderizzate come <SPAN>) non possono prendere il fuoco
		/// </summary>
		//--------------------------------------------------------------------------------------
		internal override bool Focusable { get { return false; } }

		//--------------------------------------------------------------------------------------
		public override WebControl InnerControl
		{
			get { return label; }
		}
		//--------------------------------------------------------------------------------------
		public TBLabel()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			label = new Label();
			label.EnableViewState = false;

            AddToContainer(label);
		}

		//--------------------------------------------------------------------------------------
		protected Color BackgroundColor
		{
			get { return ((TextObjDescription)ControlDescription).BackgroundColor; }
		}

		//--------------------------------------------------------------------------------------
		protected Color TextColor
		{
			get { return ((TextObjDescription)ControlDescription).TextColor; }
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			//il campo Text della Label puo' contenere anche HTML,e per fargli sentire l'andata a 
			//capo sostituisco il "\r\n" con il <br/>
			label.Text = ControlDescription.HtmlTextAttribute.Replace("\r\n", "<br/>");

			//Se ha sfondo o testo colorato, vengono impostati i corrispettivi stili
			if (BackgroundColor != Color.Empty)
				label.BackColor = BackgroundColor;
			if (TextColor != Color.Empty)
				label.ForeColor = TextColor;


			//allineamento testo
			TextAlignment ta = ((TextObjDescription)ControlDescription).TextAlignment;
			if (ta != TextAlignment.NONE && ta != TextAlignment.LEFT)
			{
				label.Style[HtmlTextWriterStyle.TextAlign] = ta.ToString();
			}

		}
	
	}

	//==========================================================================================
	public class TBTextBox : TBWebControl
	{
		TextBox textBox;
		//DynamicImageButton btnCalendar;

		protected const int roundCornerRadius = 8;  //alcuni browser (es.safari per ipad) renderizzano la textbox  piu larga, 
													//disegnando un bordo arrotondato: questo valore e' il raggio approssimato

		public override WebControl InnerControl
		{
			get { return textBox; }
		}
		//--------------------------------------------------------------------------------------
		protected virtual bool IsStatic
		{
			get { return ((TextObjDescription)ControlDescription).IsStatic; }
		}

		//--------------------------------------------------------------------------------------
		protected virtual bool IsMultiline
		{
			get  { return ((TextObjDescription)ControlDescription).IsMultiline; }
		}

		//--------------------------------------------------------------------------------------
		protected virtual bool IsHyperLink
		{
			get { return ((TextObjDescription)ControlDescription).IsHyperLink; }
		}

		//--------------------------------------------------------------------------------------
		protected virtual bool HasFocus
		{
			get { return ((TextObjDescription)ControlDescription).HasFocus; }
		}
		
		//--------------------------------------------------------------------------------------
		protected virtual bool HasCalendar
		{
			get { return ((TextObjDescription)ControlDescription).HasCalendar; }
		}

		//--------------------------------------------------------------------------------------
		protected override bool IsEnabled
		{
			get { return IsStatic ? false : base.IsEnabled; }
		}

		//--------------------------------------------------------------------------------------
		protected Color BackgroundColor
		{
			get { return ((TextObjDescription)ControlDescription).BackgroundColor; }
		}

		//--------------------------------------------------------------------------------------
		protected Color TextColor
		{
			get { return ((TextObjDescription)ControlDescription).TextColor; }
		}

		//--------------------------------------------------------------------------------------
		public TBTextBox()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			textBox = new TextBox();
			textBox.EnableViewState = false;
			
			SetId(ControlDescription);

			if (IsStatic)
			{
				InnerControl.ControlStyle.BorderWidth = Unit.Pixel(1);
				InnerControl.ControlStyle.BorderStyle = BorderStyle.Inset;
			}

			//Se ha il bottone del calendario associato, gli associo il calendario Ajax
			//Disabilitato perche non usiamo piu il toolkit ajax..riabilitare sostituendolo con un
			//calendario da gestire lato client
			//	if (HasCalendar)
			//		ApplyCalendar();

			AddToContainer(textBox);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			int rad = formControl.IsMacDevice() ? roundCornerRadius : 0;
			InnerControl.Width = Width - 4 - rad > 0 ? Width - 4 - rad : Width;
			InnerControl.Height = Height - 4 > 0 ? Height - 4 : Height;
			
			SetText(ControlDescription.HtmlTextAttribute);

			if (IsMultiline || IsTextWrapped())
                textBox.TextMode = TextBoxMode.MultiLine;

			if (IsHyperLink)
			{
				if (!IsEnabled)
				{
					textBox.TabIndex = -1;
					textBox.Enabled = true;
					textBox.ReadOnly = true;
					textBox.Attributes["onfocus"] = "return false;";
					textBox.Attributes["onkeypress"] = "return false;";
					textBox.Attributes["onkeydown"] = "return false;";
					textBox.Attributes["onclick"] = string.Format("DoHyperLink('{0}')", ClientID);
					textBox.CssClass = "HyperLink " + textBox.CssClass;
				}
				else
				{
					textBox.TabIndex = 0;
					textBox.ReadOnly = false;
					textBox.Attributes.Remove("onclick");
				}
			}
			if (IsEnabled)
			{
				textBox.Attributes["onkeypress"] = string.Format("return textKeyPress(event, this,'{0}','{1}')", windowId, ClientID);
				textBox.Attributes["onkeydown"] = string.Format("return textKeyDown(event, this,'{0}','{1}','{2}')", windowId, ClientID, OwnerForm.InnerControl.ClientID);
				textBox.Attributes["onfocus"] = string.Format("tbMove(this, '{0}')", this.ClientID);
				textBox.Attributes["onContextMenu"] = string.Format("onContextMenu(event,'{0}', '{1}', '0');return false;", InnerControl.ClientID, WindowId);
				//Campo con fuoco, evidenziato in giallo in quando e' in edit
				if (HasFocus)
					textBox.CssClass = "FocusedField " + textBox.CssClass;
			}

			//Se ha sfondo o testo colorato, vengono impostati i corrispettivi stili
			if (BackgroundColor != Color.Empty)
				textBox.BackColor = BackgroundColor;
			if (TextColor != Color.Empty)
				textBox.ForeColor = TextColor;

			
			//Font personalizzato
			if (!ControlDescription.HasCustomFont && (!string.IsNullOrEmpty(FormControl.FontFamily)))
			{
				textBox.Style[HtmlTextWriterStyle.FontFamily] = FormControl.FontFamily;
			}
			//if (btnCalendar != null)
			//{
			//    btnCalendar.Style[HtmlTextWriterStyle.Position] = "absolute";
			//    btnCalendar.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + Width + 2).ToString();
			//    btnCalendar.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(Y + 2).ToString();
			//    btnCalendar.Enabled = textBox.Enabled;
			//}
		}

		//Metodo che associa alla textbox il bottone con l'immagine del calendario che apre il 
		//calendario del toolkit ajax (CalendarExtender)
		//--------------------------------------------------------------------------------------
	/*	private void ApplyCalendar()
		{
			btnCalendar = new DynamicImageButton();
			btnCalendar.ImageUrl = ImagesHelper.CreateImageAndGetUrl("Calendar.png", TBWebFormControl.DefaultReferringType);

			CalendarExtender calendar= new CalendarExtender();
			calendar.TargetControlID = InnerControl.ID;
			calendar.PopupButtonID = btnCalendar.ID = InnerControl.ID + "btn";
			btnCalendar.TabIndex = -1;
			ContentTemplateContainer.Controls.Add(btnCalendar);
			ContentTemplateContainer.Controls.Add(calendar);
			calendar.OnClientDateSelectionChanged = string.Format("function(s,e){{ formatCalendarDate(s,'{0}','{1}');}}", ClientID, windowId);
		}*/

		//--------------------------------------------------------------------------------------
		private bool IsTextWrapped()
		{
			return textBox.Text.Contains("\r\n");
		}
	
		//--------------------------------------------------------------------------------------
		protected void SetText(string text)
		{
			textBox.Text = text;
		}
	}

    /// ================================================================================
    class TBBorderPanel : TBPanel
	{
		private bool working;
		private int zIndex;
		private int pingInterval;
		List<TBWebControl> controlsScript;

		//--------------------------------------------------------------------------------------
		public List<TBWebControl> ControlsScript
		{
			get { return controlsScript; }
			set { controlsScript = value; }
		}

		//--------------------------------------------------------------------------------------
		public bool Working
		{
			get { return working; }
			set { working = value; }
		}

		//--------------------------------------------------------------------------------------
		public int PingInterval
		{
			get { return pingInterval; }
			set { pingInterval = value; }
		}
		//--------------------------------------------------------------------------------------
        public override int ZIndex { get { return zIndex; } }

		//--------------------------------------------------------------------------------------
		public override string ID
		{
			get
			{
				return base.ID;
			}
			set
			{
				base.ID = value;
				panel.ID = value + "Pnl";
			}
		}
		//--------------------------------------------------------------------------------------
        public TBBorderPanel()
		{
          
		}

		//--------------------------------------------------------------------------------------
		internal void AssignZIndex(int zIndex)
		{
			this.zIndex = zIndex;
		} 

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			//do not call father's implementation!!
            AssignCssClass();
            InnerControl.Style[HtmlTextWriterStyle.ZIndex] = ZIndex.ToString();
		}
		//--------------------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);


            Panel.Attributes["pingInterval"] = PingInterval.ToString();
            if (Working)
                Panel.Attributes["working"] = "true";

			if (!FormControl.ThreadAvailable)
				Panel.Attributes["closed"] = "true";
            if (controlsScript.Count > 0)
                CreateControlInitializationFunction();

        }

		///<summary>
		/// Crea gli script di inizializzazione dei controlli, e lo manda al browser mettendolo come 
		/// attributo.
		/// </summary>
        //--------------------------------------------------------------------------------------
		private void CreateControlInitializationFunction()
        {
            StringBuilder script = new StringBuilder();
			//Se il thread e' chiuso chiudo la finestra nel browser
			if (formControl.ThreadClosed)
			{
				script.Append("window.close();");
			}
			else
			{
				foreach (TBWebControl control in controlsScript)
					if (control.Updated || !Page.IsPostBack)
						script.Append(control.GetInitScriptBody());
			}
			Panel.Attributes["initScript"] = script.ToString();
        }
		//--------------------------------------------------------------------------------------
		protected override void RenderTBControl(HtmlTextWriter writer)
		{
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.ZIndex, ZIndex.ToString()); 

			//does nothing
		}
	}

    /// ================================================================================
    public class TBPanel : TBWebControl
	{
		protected Panel panel;

		///<summary>
		///Override della property Focusable, restituendo sempre false, perche in html i panel 
		///(renderizzate come <DIV>) non possono prendere il fuoco
		/// </summary>
		//--------------------------------------------------------------------------------------
		internal override bool Focusable { get { return false; } }

		//--------------------------------------------------------------------------------------
		public override WebControl InnerControl
		{
			get { return panel; }
		}
		//--------------------------------------------------------------------------------------
		public Panel Panel
		{
			get { return panel; }
		}

		//--------------------------------------------------------------------------------------
		public TBPanel()
		{
			panel = new Panel();

		}
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			
			AddToContainer(panel);
		}

		//--------------------------------------------------------------------------------------
		public override void UpdateFromControlDescription(WndObjDescription description)
		{
			if (ControlDescription.ChildrenChanged)
			{
				ClearChildControls();
				AddChildControls(description);
				//Dico al controllo di ridisegnarsi, siccome sono cambiati i suoi figli
				Update();
			}
			else foreach (WndObjDescription childEl in description.Childs)
				{
					TBWebControl tbwc = FindTBWebControl(childEl);
					if (tbwc == null)
						continue;
					tbwc.UpdateFromControlDescription(childEl);

				}
			base.UpdateFromControlDescription(description);
		}

	}

    /// ================================================================================
	public class TBScrollPanel : TBPanel
	{
		//--------------------------------------------------------------------------------------
		public TBScrollPanel()
		{
			
		}
	}


    /// ================================================================================
    public class TBDummyPanel : TBPanel
    {
        //--------------------------------------------------------------------------------------
        public TBDummyPanel()
        {

        }

       	//--------------------------------------------------------------------------------------
		protected override void SetControlPosition()
		{
			base.SetControlPosition();
			InnerControl.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(0).ToString();
            InnerControl.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(0).ToString();
		}

    }


	/// ================================================================================
	public class TBView : TBScrollPanel
	{
		Bitmap bkgBmp = null;
		string idBitmap = String.Empty; 

		//--------------------------------------------------------------------------------------
		public TBView()
		{
			
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
		}
		
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			ImageBuffer bitmap = ((WndImageDescription)ControlDescription).Image;
			if (!bitmap.IsEmpty)
			{
				CreateImage(bitmap);
				InnerControl.Style.Add(HtmlTextWriterStyle.BackgroundImage, ImagesHelper.GetImageUrl(bitmap.Id));
			}
		}

		//--------------------------------------------------------------------------------------
		protected void CreateImage(ImageBuffer bitmap)
		{
			//Creo una nuova bitmap solo se e' cambiata rispetto alla precedente
			if (idBitmap != bitmap.Id)
			{
				if (bkgBmp != null)
					bkgBmp.Dispose();
				bkgBmp = bitmap.CreateBitmap();
				idBitmap = bitmap.Id;
			}

			CreateDocumentImage(bkgBmp, bitmap.Id, null);
		}

	}



    /// ================================================================================
	abstract class TBAbstractButton : TBWebControl
	{
		public TBAbstractButton()
		{ 
		}
	}

    /// ================================================================================
    class TBImage : TBWebControl
	{
		System.Web.UI.WebControls.Image image;
		Bitmap imageBitmap = null;
		string idBitmap = String.Empty; 

		public override WebControl InnerControl
		{
			get { return image; }
		}
		//--------------------------------------------------------------------------------------
		public TBImage()
			: base()
		{
		}
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			image = new System.Web.UI.WebControls.Image();

			AddToContainer(InnerControl);
		}
		
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			ImageBuffer bitmap = ((WndImageDescription)ControlDescription).Image;
			if (!bitmap.IsEmpty)
			{
				image.Visible = true;
				CreateImage(bitmap);
				image.ImageUrl = ImagesHelper.GetImageUrl(bitmap.Id);
			}
			else
				image.Visible = false;
		}
		//--------------------------------------------------------------------------------------
		protected void CreateImage(ImageBuffer bitmap)
		{
			//Creo una nuova bitmap solo se e' cambiata rispetto alla precedente
			if (idBitmap != bitmap.Id)
			{
				if (imageBitmap != null)
					imageBitmap.Dispose();
				imageBitmap = bitmap.CreateBitmap();
				idBitmap = bitmap.Id;
			}
			
			CreateDocumentImage(imageBitmap, bitmap.Id, null);

			int offsetX = (int)Math.Round((Width - imageBitmap.Width) / 2.0);
			int offsetY =  (int)Math.Round((Height - imageBitmap.Height) / 2.0);

			image.Style[HtmlTextWriterStyle.Left] = offsetX > 0 ? Unit.Pixel(X + offsetX).ToString() : Unit.Pixel(X).ToString();
			image.Style[HtmlTextWriterStyle.Top] = offsetY > 0 ? Unit.Pixel(Y + offsetY).ToString() : Unit.Pixel(Y).ToString();

			AssignImageSize();
		}

		//Assegna le dimensioni all'immagine: se non sta nel container la scala per evitare che sbordi.
		//---------------------------------------------------------------------------------------------
		private void AssignImageSize()
		{
			if (ImageHasToScale())
			{
				//calcolo le dimensioni dell'immagine in modo che sfrutti al massimo lo spazio del contenitore
				double hRatio = (double)ControlDescription.Width / imageBitmap.Width;
				double vRatio = (double)ControlDescription.Height / imageBitmap.Height;

				if (hRatio > vRatio)
				{
					image.Width = Unit.Pixel((int)(imageBitmap.Width * vRatio));
					image.Height = Unit.Pixel((int)(imageBitmap.Height * vRatio));
				}
				else
				{
					image.Width = Unit.Pixel((int)(imageBitmap.Width * hRatio));
					image.Height = Unit.Pixel((int)(imageBitmap.Height * hRatio));
				}
			}
			else
			{
				//assegna le sue dimensioni originali perche' ci sta nel contenitore
				image.Width = Unit.Pixel(imageBitmap.Width);
				image.Height = Unit.Pixel(imageBitmap.Height);
			}
		}
		
		//Guarda le dimensioni dell'immagine e del container e determina se l'immagine va scalata o no.
		//---------------------------------------------------------------------------------------------
		private bool ImageHasToScale()
		{
			return imageBitmap != null && imageBitmap.Width > ControlDescription.Width || imageBitmap.Height > ControlDescription.Height;
		}

		//--------------------------------------------------------------------------------------
		public override void Dispose ()
		{
			if (imageBitmap != null)
				imageBitmap.Dispose();
			base.Dispose();
		}
	}



	//==========================================================================================
	class TBReport : TBImage
	{
		ImageMap map;

		WndReportDescription Description
		{
			get { return (WndReportDescription)ControlDescription; }
		}
	
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			if (Description.Links.Count > 0)  //ci sono linkreport o linkform, devo associare la mappa
			{
				BuildMap();
				InnerControl.Attributes.Add("usemap", String.Format("#ImageMap{0}", map.ClientID));
			}
		}

		//--------------------------------------------------------------------------------------
		void BuildMap()
		{
			//rimuove la mappa e la ricrea altrimenti non veniva aggiornata (An. 18376)
			ContentTemplateContainer.Controls.Remove(map);
			//predispone la mappa per eventuali linkreport/linkform
			map = new ImageMap();
			map.HotSpotMode = HotSpotMode.NotSet;
			map.ID  = String.Format("map{0}",InnerControl.ID);
			//pulizia degli hotspot precedenti
			map.HotSpots.Clear();
			
			foreach (LinkDescription linkDescr in Description.Links)
			{
				RectangleHotSpot rectHotSpot = new RectangleHotSpot();
				rectHotSpot.Left = linkDescr.X;
				rectHotSpot.Top = linkDescr.Y;
				rectHotSpot.Right = linkDescr.X + linkDescr.Width;
				rectHotSpot.Bottom = linkDescr.Y + linkDescr.Height;
				rectHotSpot.HotSpotMode = HotSpotMode.NotSet;
				rectHotSpot.NavigateUrl = String.Format("javascript:DoLink('{0}','{1}','{2}');", InnerControl.ClientID, linkDescr.ObjectAlias.ToString(), linkDescr.Row.ToString());
				map.HotSpots.Add(rectHotSpot);
			}
			ContentTemplateContainer.Controls.Add(map);
		}
	}

	//==========================================================================================
	class TBButton : TBAbstractButton
	{
		internal enum ButtonType { Button = 0, ImageButton = 1, DynamicImageButton = 2} 
		protected IButtonControl button;
		ImageBuffer bitmap = null;

		public override WebControl InnerControl
		{
			get { return (WebControl) button; }
		}
		//--------------------------------------------------------------------------------------
		public TBButton()
			: base()
		{
		}
		//--------------------------------------------------------------------------------------
		protected virtual string ClickAction 
		{
			get { return "tbClick(this);"; }
		}

		//--------------------------------------------------------------------------------------
		protected virtual ButtonType GetButtonType 
		{
			get { return bitmap.IsEmpty ? ButtonType.Button : ButtonType.DynamicImageButton; }
		}
		
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			//TODO da rivedere facendo 3 classi distinte
			bitmap = ((WndImageDescription)ControlDescription).Image;
			switch(GetButtonType)
			{	
				case ButtonType.Button:
				{
					Button b = new Button();
					b.OnClientClick = ClickAction;
					if (!ControlDescription.HasTabIndex) 
						b.TabIndex = -1;
					button = b;
					break;
				}
				case ButtonType.ImageButton:
				{
					ImageButton b = new ImageButton();
					b.OnClientClick = ClickAction;
					if (!ControlDescription.HasTabIndex)
						b.TabIndex = -1;
					button = b;
					break;
				}
				case ButtonType.DynamicImageButton:
				{
					DynamicImageButton b = new DynamicImageButton();
					b.OnClientClick = ClickAction;
					if (!ControlDescription.HasTabIndex)
						b.TabIndex = -1;
					button = b;
					break;
				}
			}

			button.CommandName = ((WndImageDescription)ControlDescription).Cmd;
		
			AddToContainer(InnerControl);
		}

		//--------------------------------------------------------------------------------------
		protected static void CreateImage(ImageBuffer bitmap)
		{
			try
			{
				CreateDocumentImage(bitmap, GetImageName(bitmap.Id, true), GetImageName(bitmap.Id, false));
			}
			catch
			{
			}
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			ImageBuffer bitmap = ((WndImageDescription)ControlDescription).Image;
			if (!bitmap.IsEmpty)
			{
				string imageName = GetImageName(bitmap.Id, IsEnabled);
				((ImageButton)button).ImageUrl = ImagesHelper.GetImageUrl(imageName);
				CreateImage(bitmap);

				((ImageButton)button).CssClass += " NotFocusable"; //An. 19346
			}

			button.Text = ControlDescription.HtmlTextAttribute;
			
		}
	}

	//==========================================================================================
	class TBPdfButton : TBButton
	{
		protected override string ClickAction
		{
			get
			{
				var url = string.Format("~/TBWebFormResource.axd?pdf={0}", HttpUtility.UrlEncode(((WndPdfButtonDescription)ControlDescription).PdfFile));
	
				return string.Format("window.setTimeout (function() {{window.open('{0}');}}, 1000); {1}", url, base.ClickAction);
			}
		}
	}

	//==========================================================================================
	class TBSaveFileButton : TBButton
	{
		protected override string ClickAction
		{
			get
			{
				var url = string.Format("~/TBWebFormResource.axd?folder={0}", HttpUtility.UrlEncode(((WndSaveFileButtonDescription)ControlDescription).Folder));

				return string.Format("window.setTimeout (function() {{window.open('{0}');}}, 1000); {1}", url, base.ClickAction);
			}
		}
	}

	//==========================================================================================
	class TBUploadFileButton : TBPanel
	{
		FileUpload uploader;
		Panel dragFilePanel;

	
		//--------------------------------------------------------------------------------------
		public TBUploadFileButton()
			: base()
		{

		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			uploader = new FileUpload();
			dragFilePanel = new Panel();
		
			base.OnInit(e);

			InnerControl.Controls.Add(uploader);
			InnerControl.Controls.Add(dragFilePanel);

			RegisterInitControlScript();
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			dragFilePanel.Height = Unit.Pixel(110);
			dragFilePanel.Width = Unit.Pixel(220);
			dragFilePanel.BackImageUrl = ImagesHelper.CreateImageAndGetUrl("DragDropTarget.png", TBWebFormControl.DefaultReferringType);
			dragFilePanel.BorderStyle = System.Web.UI.WebControls.BorderStyle.Dashed;
			dragFilePanel.BorderColor = Color.LightGray;
			dragFilePanel.BorderWidth = Unit.Pixel(3);
		}

		//--------------------------------------------------------------------------------------
		protected override void SetControlPosition()
		{
			int verticalOffset = 100;
			base.SetControlPosition();
			InnerControl.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(Y - verticalOffset).ToString();
			InnerControl.Width = Unit.Percentage(100);
		}

		//--------------------------------------------------------------------------------------
		public override string GetInitScriptBody()
		{
			return string.Format("{0}('{1}', '{2}', '{3}');", InitFunctionName, ClientID, uploader.ClientID, dragFilePanel.ClientID);
		}

		//--------------------------------------------------------------------------------------
		protected override string InitFunctionName { get { return "initFileUploader"; } }
	}

	//==========================================================================================
	class TBTextImageButton : TBPanel
	{
		ImageButton imgButton;
		Label label;

		//il bordo del bottone e' spesso 1, e devo tenerne conto nei calcoli di dimensionamento
		//--------------------------------------------------------------------------------------
		protected override short BorderWidth { get { return 1; } }

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			imgButton = new ImageButton();
			label = new Label();
			base.OnInit(e);

			//assegnazione ID
			label.ID = ID + "lbl";
			imgButton.ID = ID + "img";

			//Aggiungo i 2 web controls interni al Panel
			InnerControl.Controls.Add(imgButton);
			InnerControl.Controls.Add(label);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			imgButton.Visible = false;
			ImageBuffer bitmap = ((WndButtonDescription)ControlDescription).Image;
			if (!bitmap.IsEmpty)
			{
				string imageName = GetImageName(bitmap.Id, IsEnabled);
				imgButton.ImageUrl = ImagesHelper.GetImageUrl(imageName);
				imgButton.Visible = true;
				imgButton.OnClientClick = string.Format("tbClick($get('{0}'));", ClientID);
				CreateDocumentImage(bitmap, GetImageName(bitmap.Id, true), GetImageName(bitmap.Id, false));
				imgButton.CssClass = "TBTextImageButtonImage";
			}

			label.Text = ControlDescription.HtmlTextAttribute;
			label.Attributes["onclick"] = string.Format("tbClick($get('{0}'));", ClientID);
			label.CssClass = "TBTextImageButtonText";
		}
	}
	

	//==========================================================================================
	class TBGroupBox : TBWebControl
	{
		TBFieldSet panel;
	
		public override WebControl InnerControl
		{
			get { return panel; }
		}
		//--------------------------------------------------------------------------------------
		public TBGroupBox()
		{
		}

		//--------------------------------------------------------------------------------------
		protected Color BackgroundColor
		{
			get { return ((WndColoredObjDescription)ControlDescription).BackgroundColor; }
		}

		//--------------------------------------------------------------------------------------
		protected Color TextColor
		{
			get { return ((WndColoredObjDescription)ControlDescription).TextColor; }
		}

		//--------------------------------------------------------------------------------------
		protected override void RenderTBControl(HtmlTextWriter writer)
		{
			base.RenderTBControl(writer);
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.ZIndex, (ZIndex -1).ToString());
			
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			panel = new TBFieldSet();

			switch (((WndColoredObjDescription)ControlDescription).TextAlignment)
			{
				case TextAlignment.CENTER:
					{
						panel.Align = TBFieldSet.AlignStyle.Center;
						break;
					}
				case TextAlignment.RIGHT:
					{
						panel.Align = TBFieldSet.AlignStyle.Right;
						break;
					}
				default:
						break;
			}
			
			base.OnInit(e);

			AddToContainer(InnerControl);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			panel.Caption = ControlDescription.HtmlTextAttribute;
			
			//Se ha sfondo o testo colorato, vengono impostati i corrispettivi stili
			if (BackgroundColor != Color.Empty)
				panel.BackColor = BackgroundColor;
			if (TextColor != Color.Empty)
				panel.ForeColor = TextColor;
		}
	}

	//==========================================================================================
	class TBRadioButton : TBAbstractButton
	{
		RadioButton radioButton;

		public override WebControl InnerControl
		{
			get { return radioButton; }
		}
		//--------------------------------------------------------------------------------------
		public TBRadioButton()
		{
		}

		//--------------------------------------------------------------------------------------
		protected Color BackgroundColor
		{
			get { return ((WndCheckRadioDescription)ControlDescription).BackgroundColor; }
		}

		//--------------------------------------------------------------------------------------
		protected Color TextColor
		{
			get { return ((WndCheckRadioDescription)ControlDescription).TextColor; }
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			radioButton = new RadioButton();
			base.OnInit(e);
			radioButton.Attributes.Add("onclick", "tbRadioClick(this)");
			AddToContainer(InnerControl);
		}
	
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			radioButton.Text = ControlDescription.HtmlTextAttribute;
			radioButton.Checked = ((WndCheckRadioDescription)ControlDescription).Checked;

			//Se ha sfondo o testo colorato, vengono impostati i corrispettivi stili
			if (BackgroundColor != Color.Empty)
				radioButton.BackColor = BackgroundColor;
			if (TextColor != Color.Empty)
				radioButton.ForeColor = TextColor;
		}

	}

	//==========================================================================================
	class TBCheckBox : TBAbstractButton
	{
		public CheckBox checkBox;

		public override WebControl InnerControl
		{
			get { return checkBox; }
		}

		//--------------------------------------------------------------------------------------
		protected Color BackgroundColor
		{
			get { return ((WndCheckRadioDescription)ControlDescription).BackgroundColor; }
		}

		//--------------------------------------------------------------------------------------
		protected Color TextColor
		{
			get { return ((WndCheckRadioDescription)ControlDescription).TextColor; }
		}

		//--------------------------------------------------------------------------------------
		public TBCheckBox()
		{
		}
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			checkBox = new CheckBox();
			base.OnInit(e);
			checkBox.Attributes.Add("onclick", "tbRadioClick(this)");
			checkBox.EnableViewState = false;
			AddToContainer(checkBox);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			//calcoliamo il posizionamento centrato della checkbox
			//solo nel caso si trovi dentro una cella del bodyedit
			if (parentTBWebControl is TBGridCell)
			{
				int left = parentTBWebControl.ControlDescription.Width / 2 - 10;
				checkBox.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(left).ToString();
			}
			checkBox.Text = ControlDescription.HtmlTextAttribute;
			checkBox.Checked = ((WndCheckRadioDescription)ControlDescription).Checked;

			//Se ha sfondo o testo colorato, vengono impostati i corrispettivi stili
			if (BackgroundColor != Color.Empty)
				checkBox.BackColor = BackgroundColor;
			if (TextColor != Color.Empty)
				checkBox.ForeColor = TextColor;
		}
	}

	//==========================================================================================
	class TBToolbar : TBPanel
	{
		ImageBuffer bitmap;
		Bitmap toolbarBitmap;
		//--------------------------------------------------------------------------------------
		public Bitmap ToolbarBitmap
		{
			get 
			{
				if (toolbarBitmap == null)
					toolbarBitmap = bitmap.CreateBitmap();
				return toolbarBitmap; 
			
			}
		}
		//--------------------------------------------------------------------------------------
		public int ButtonWidth
		{
			get { return ToolbarBitmap.Width; }
		}
		//--------------------------------------------------------------------------------------
		public int ButtonHeight
		{
			get { return((ToolbarDescription)ControlDescription).ImageHeight; }
		}
		//--------------------------------------------------------------------------------------
		public TBToolbar()
		{
		}
		//--------------------------------------------------------------------------------------
		protected override void  OnInit(EventArgs e)
		{
			SetId(ControlDescription);
			bitmap = ((ToolbarDescription)ControlDescription).Image;
			base.OnInit(e);
		}

		//--------------------------------------------------------------------------------------
		protected override void SetControlPosition()
        {
           // ignora i posizionamenti definiti dalla parte C++ perchè la loro posizione
           // è controllata dalla creazione della form. in pratica 
           return;
		}

		//--------------------------------------------------------------------------------------
		public override void Dispose()
		{
			if (toolbarBitmap != null) 
				toolbarBitmap.Dispose();
			base.Dispose();
		}		
	}


	//==========================================================================================
	class TBFloatingToolbar : TBToolbar
	{
		

		//--------------------------------------------------------------------------------------
		protected override void SetControlPosition()
		{
			try
			{
				// aggiungo la mia InflateSize
				//Width e Height non possono essere negative, nel caso di valore negativo le imposto a 0
				InnerControl.Width = Unit.Pixel(Math.Max(0, Width - 2 * BorderWidth + InflateSize.Width));
				InnerControl.Height = Unit.Pixel(Math.Max(0, Height - 2 * BorderWidth + InflateSize.Height));

				// mi sposto se il mio parent me lo chiede
				InnerControl.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + (parentTBWebControl != null? parentTBWebControl.ChildsOffset.X : 0)).ToString();
				InnerControl.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(Y + (parentTBWebControl != null ? parentTBWebControl.ChildsOffset.Y : 0)).ToString();
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
			}

			return;
		}

	}

	//==========================================================================================
	class TBToolbarButton : TBWebControl
	{
		ImageButton button;
		DynamicImageButton dropDownButton;
		string commandId;
		int imageIndex = -1;


		//--------------------------------------------------------------------------------------
		public override WebControl InnerControl
		{
			get { return button; }
		}

		//--------------------------------------------------------------------------------------
		public TBToolbar Toolbar
		{
			get { return parentTBWebControl as TBToolbar; }
		}

		public ToolbarBtnDescription ButtonDescription
		{
			get { return (ToolbarBtnDescription)ControlDescription; }
		}

		//--------------------------------------------------------------------------------------
		public TBToolbarButton()
		{
		}

		//--------------------------------------------------------------------------------------
		public override string GetInitScriptBody()
		{
			return InitScriptBody;
		}
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			commandId = ControlDescription.Cmd;
			button = ButtonDescription.IsCheckButton || IsSeparator() ? new ImageButton() : new DynamicImageButton();
			dropDownButton = new DynamicImageButton();
			base.OnInit(e);
		
			button.ImageAlign = ImageAlign.Middle;
			button.TabIndex = -1;
			if (!IsSeparator())
			{
				button.CommandName = commandId;
				button.OnClientClick = "tbClick(this);";
				imageIndex = ((ToolbarBtnDescription)ControlDescription).Image;
				CreateImage();
			}

			AddToContainer(InnerControl);
			
			if (((ToolbarBtnDescription)ControlDescription).Dropdown)
			{
				dropDownButton.ID = button.ID + "drop";
				dropDownButton.TabIndex = -1;
				dropDownButton.OnClientClick = String.Format("tbDropdownButton('{0}', '{1}', '{2}');", button.ClientID, Toolbar.WindowId, CommandId);
				AddToContainer(dropDownButton);
			}
		}
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			ToolbarBtnDescription buttonDesc = ButtonDescription;

			imageIndex = buttonDesc.Image;

			string imageUrl = GetImageUrl();
			if (button.ImageUrl != imageUrl)
			{
				CreateImage();
			}
			button.ImageUrl = imageUrl;
			
			if (buttonDesc.Dropdown)
			{
				dropDownButton.ImageUrl = ImagesHelper.CreateImageAndGetUrl("DropDownArrow.png", TBWebFormControl.DefaultReferringType);
				dropDownButton.Style[HtmlTextWriterStyle.Position] = "absolute";
				dropDownButton.Style[HtmlTextWriterStyle.ZIndex] = (ZIndex + 1).ToString();
				dropDownButton.Width = Unit.Pixel(10);
				dropDownButton.Height = Unit.Pixel(10);
				dropDownButton.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + Width).ToString();
				dropDownButton.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(Y + (Height - 10)).ToString();
				dropDownButton.Enabled = buttonDesc.Enabled; 
				dropDownButton.CssClass += " NotFocusable";
				dropDownButton.Style.Add(HtmlTextWriterStyle.BackgroundColor, "transparent");
			}
			//separatore non ha cursore che indica lo stato di cliccabile (hand)
			if (IsSeparator())
				button.Style.Add(HtmlTextWriterStyle.Cursor, "default");
			
			//gestione tasto in stato "pressed"
			button.BorderStyle = buttonDesc.IsCheckButton && buttonDesc.IsPressed ? BorderStyle.Inset : BorderStyle.None;
			button.BorderWidth = buttonDesc.IsCheckButton && buttonDesc.IsPressed ? Unit.Pixel(2) : Unit.Pixel(0);
			button.CssClass += " NotFocusable";
			button.Style.Add(HtmlTextWriterStyle.BackgroundColor, "transparent");
		}

		//--------------------------------------------------------------------------------------
        private void CreateImage()
        {
            try
            {
                if (IsSeparator())
                    return;

                string imageName = GetImageName(true);
                if (ImagesHelper.HasImageInCache(imageName))
                    return;

                int width = Toolbar.ButtonWidth;
                int height = Toolbar.ButtonHeight;
                using (Bitmap bmp = new Bitmap(Toolbar.ToolbarBitmap, width, height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                        g.DrawImage(Toolbar.ToolbarBitmap, new Rectangle(0, 0, width, height), new Rectangle(0, imageIndex * height, width, height), GraphicsUnit.Pixel);
                    bmp.MakeTransparent(Color.Magenta);
                    CreateDocumentImage(bmp, imageName, GetImageName(false));
                }
            }
            catch
            {
            }
        }

		//--------------------------------------------------------------------------------------
		private bool IsSeparator()
		{
			return (commandId != null && commandId.Length == 0);
		}

		//--------------------------------------------------------------------------------------
		private string GetImageUrl ()
		{
			return GetImageUrl(IsEnabled);
		}
		//--------------------------------------------------------------------------------------
		private string GetImageUrl (bool enabled)
		{
			if (IsSeparator())
				return ImagesHelper.CreateImageAndGetUrl("Border.png", TBWebFormControl.DefaultReferringType);

			string imageName = GetImageName(enabled);
			return ImagesHelper.GetImageUrl(imageName);
		}

		//--------------------------------------------------------------------------------------
		private string GetImageName(bool enabled)
		{
			//Come chiave per identificare l'immagine associata al bottone della toolbar uso il commandid del bottone + 
			//posizione(ImageIndex) dell'immagine associata all'interno dell'immagine della toolbar che contiene tutte le 
			//immagini dei bottoni. Questo e' necessario perche' alcuni bottoni (es. sfera verde/rossa di run report) hanno piu'
			//immagini abilitate associate
			return string.Format("{0}.png", ImagesHelper.PrefixedName(commandId + imageIndex.ToString(), enabled));
		}

		#region IClickable Members

		//--------------------------------------------------------------------------------------
		public override ITBWebControl CommandObject
		{
			get { return parentTBWebControl; }
		}

		//--------------------------------------------------------------------------------------
		public override int CommandId
		{
			get
			{
				int cmd;
				int.TryParse(((IButtonControl)InnerControl).CommandName, out cmd);
				return cmd;
			}
		}
		#endregion
	}


    
    //==========================================================================================
	class TBTabbedToolbarButton : TBWebControl
	{
		Button button;
		DynamicImageButton dropDownButton;
        Label label;

		string commandId;
		int imageIndex = -1;


		//--------------------------------------------------------------------------------------
		public override WebControl InnerControl
		{
			get { return button; }
		}

		//--------------------------------------------------------------------------------------
		public TBTabberPanel TabberPanel
		{
            get { return parentTBWebControl as TBTabberPanel; }
		}

        //--------------------------------------------------------------------------------------
		public ToolbarBtnDescription ButtonDescription
		{
			get { return (ToolbarBtnDescription)ControlDescription; }
		}

        //--------------------------------------------------------------------------------------
        private bool HasMenu
        {
            get { return ButtonDescription.Dropdown || ButtonDescription.HasMenu; }
        }

		//--------------------------------------------------------------------------------------
        public TBTabbedToolbarButton()
		{
		}

		//--------------------------------------------------------------------------------------
		public override string GetInitScriptBody()
		{
			return InitScriptBody;
		}
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			commandId = ControlDescription.Cmd;
			button = new Button();
			dropDownButton = new DynamicImageButton();
            label = new Label();
			base.OnInit(e);
            AddToContainer(label);
			
            button.TabIndex = -1;
			if (!IsSeparator())
			{
				button.CommandName = commandId;
				button.OnClientClick = "tbClick(this);";
				imageIndex = ((ToolbarBtnDescription)ControlDescription).Image;
				CreateImage();
			}

			AddToContainer(InnerControl);

            if (HasMenu)
			{
				dropDownButton.ID = button.ID + "drop";
				dropDownButton.TabIndex = -1;
                dropDownButton.OnClientClick = String.Format("tbDropdownButton('{0}', '{1}', '{2}');", button.ClientID, TabberPanel.WindowId, CommandId);
				AddToContainer(dropDownButton);
			}
		}
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			ToolbarBtnDescription buttonDesc = ButtonDescription;

			imageIndex = buttonDesc.Image;

            //Se e' un separatore, oppure e' un bottone nascosto perche dimensione 0
            if (IsSeparator() || buttonDesc.Width == 0 || buttonDesc.Height == 0)
            {
                button.Enabled = false;
                button.Visible = false;

                dropDownButton.Visible = false;
                return;
            }


			string imageUrl = GetImageUrl();
            if (button.Style[HtmlTextWriterStyle.BackgroundImage] != imageUrl)
			{
				CreateImage();
			}
			button.Style[HtmlTextWriterStyle.BackgroundImage] = imageUrl;
            button.Style["background-repeat"] = "no-repeat";
            button.Style["background-position"] = "center center";
			
            if (!buttonDesc.Text.IsNullOrWhiteSpace())
            {
             /*   label.Text = buttonDesc.Text;
                label.Visible = true;

                label.Style[HtmlTextWriterStyle.Position] = "absolute";
                label.Width = Unit.Pixel(X);
                label.Height = Unit.Pixel(20);
                label.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X).ToString();
                label.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(Y + (Height - 20)).ToString();*/
            }

			if (HasMenu)
			{
				dropDownButton.ImageUrl = ImagesHelper.CreateImageAndGetUrl("DropDownArrow.png", TBWebFormControl.DefaultReferringType);
				dropDownButton.Style[HtmlTextWriterStyle.Position] = "absolute";
				dropDownButton.Style[HtmlTextWriterStyle.ZIndex] = (ZIndex + 1).ToString();
				dropDownButton.Width = Unit.Pixel(10);
				dropDownButton.Height = Unit.Pixel(10);
				dropDownButton.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + Width).ToString();
				dropDownButton.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(Y + (Height - 10)).ToString();
				dropDownButton.Enabled = buttonDesc.Enabled; 
				dropDownButton.CssClass += " NotFocusable";
				dropDownButton.Style.Add(HtmlTextWriterStyle.BackgroundColor, "transparent");
			}
		
			//gestione tasto in stato "pressed"
			button.BorderStyle = buttonDesc.IsCheckButton && buttonDesc.IsPressed ? BorderStyle.Inset : BorderStyle.None;
			button.BorderWidth = buttonDesc.IsCheckButton && buttonDesc.IsPressed ? Unit.Pixel(2) : Unit.Pixel(0);
			button.CssClass += " NotFocusable";
			button.Style.Add(HtmlTextWriterStyle.BackgroundColor, "transparent");
		}

		//--------------------------------------------------------------------------------------
		private void CreateImage()
		{
			try
			{
				if (IsSeparator())
					return;

				string imageName = GetImageName(true);
				if (ImagesHelper.HasImageInCache(imageName))
					return;

                if (((ToolbarBtnDescription)ControlDescription).ImageContent.Buffer.Length > 0)
                {
                    Bitmap bmp = ((ToolbarBtnDescription)ControlDescription).ImageContent.CreateBitmap();
                    CreateDocumentImage(bmp, imageName, GetImageName(false));
                }
			}
			catch
			{
			}
		}

		//--------------------------------------------------------------------------------------
		private bool IsSeparator()
		{
			return (commandId != null && commandId.Length == 0);
		}

		//--------------------------------------------------------------------------------------
		private string GetImageUrl ()
		{
			return GetImageUrl(IsEnabled);
		}
		//--------------------------------------------------------------------------------------
		private string GetImageUrl (bool enabled)
		{
			if (IsSeparator())
				return ImagesHelper.CreateImageAndGetUrl("Border.png", TBWebFormControl.DefaultReferringType);

			string imageName = GetImageName(enabled);
			return ImagesHelper.GetImageUrl(imageName);
		}

		//--------------------------------------------------------------------------------------
		private string GetImageName(bool enabled)
		{
			//Come chiave per identificare l'immagine associata al bottone della toolbar uso il commandid del bottone + 
			//posizione(ImageIndex) dell'immagine associata all'interno dell'immagine della toolbar che contiene tutte le 
			//immagini dei bottoni. Questo e' necessario perche' alcuni bottoni (es. sfera verde/rossa di run report) hanno piu'
			//immagini abilitate associate
			return string.Format("{0}.png", ImagesHelper.PrefixedName(commandId + imageIndex.ToString(), enabled));
		}

		#region IClickable Members

		//--------------------------------------------------------------------------------------
		public override ITBWebControl CommandObject
		{
			get { return parentTBWebControl; }
		}

		//--------------------------------------------------------------------------------------
		public override int CommandId
		{
			get
			{
				int cmd;
				int.TryParse(((IButtonControl)InnerControl).CommandName, out cmd);
				return cmd;
			}
		}
		#endregion
	}

	//==========================================================================================
	class TBHotLink : TBPanel
	{
		DynamicImageButton upper;
		DynamicImageButton lower;
		//--------------------------------------------------------------------------------------
		public TBHotLink()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			upper = new DynamicImageButton();
			lower = new DynamicImageButton();
			base.OnInit(e);

            upper.CssClass = "HotlinkUpperButton";
			upper.ID = string.Format("hklu{0}", WindowId);
			upper.TabIndex = -1;
			upper.EnableViewState = false;
			upper.OnClientClick = string.Format("tbClickHotLink($get('{0}'), false);", ClientID);				
			InnerControl.Controls.Add(upper);

			lower.CssClass = "HotlinkLowerButton";
			lower.ID = string.Format("hkll{0}", WindowId);
			lower.TabIndex = -1;
			lower.EnableViewState = false;
			lower.OnClientClick = string.Format("tbClickHotLink($get('{0}'), true);", ClientID);				
			InnerControl.Controls.Add(lower);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			upper.ImageUrl = ImagesHelper.CreateImageAndGetUrl(ImagesHelper.PrefixedName("HotLinkUpper.png", IsEnabled), TBWebFormControl.DefaultReferringType);
			lower.ImageUrl = ImagesHelper.CreateImageAndGetUrl(ImagesHelper.PrefixedName("HotLinkLower.png", IsEnabled), TBWebFormControl.DefaultReferringType);
		}
	}

	//==========================================================================================
	class TBListBox : TBWebControl
	{
		ListBox listBox;

		//--------------------------------------------------------------------------------------
		public override WebControl InnerControl
		{
			get { return listBox; }
		}

		//--------------------------------------------------------------------------------------
		public TBListBox()
		{
		}
	
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			listBox = new ListBox();
			base.OnInit(e);

			AddToContainer(InnerControl);
			
			listBox.Attributes["onchange"] = string.Format("tbSelectItemListBox('{0}')", listBox.ClientID);
		}

		//--------------------------------------------------------------------------------------
		private void AddOrUpdateItems()
		{
			listBox.Items.Clear();
			foreach (ItemListBoxDescription controlDescription in ((ListDescription)ControlDescription).Items)
			{
				ListItem itemOption = new ListItem(controlDescription.Text);
				if (controlDescription.Selected)
					itemOption.Selected = true;
				listBox.Items.Add(itemOption);
			}
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			AddOrUpdateItems();
		}
	}

	//==========================================================================================
	class TBChecklListBox : TBScrollPanel
	{
		protected Table table;

		//--------------------------------------------------------------------------------------
		public TBChecklListBox()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			table = new Table();
			base.OnInit(e);
			InnerControl.Controls.Add(table);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			int index = 0;
			table.Rows.Clear();

			foreach (ItemCheckListDescription controlDescription in ((ListDescription)ControlDescription).Items)
			{
				TableRow rowItem = new TableRow();

				TableCell cellCheck = new TableCell();
				CheckBox check = new CheckBox();
				check.ID = string.Format("cb_{0}_{1}", ID, index);
				check.Checked = controlDescription.Checked;
				check.Attributes.Add("onclick", string.Format("onCheckItem('{0}', {1})", ClientID, index));
				cellCheck.Controls.Add(check);

				TableCell cellText = new TableCell();
				cellText.Text = controlDescription.Text;
				cellText.Attributes.Add("ondblclick", string.Format("onDblClickItem('{0}', {1})", ClientID, index));

				cellCheck.ID = string.Format("cellCb_{0}_{1}", ID, index);
				cellText.ID = string.Format("cellTxt_{0}_{1}", ID, index);
				
				rowItem.Cells.Add(cellCheck);
				rowItem.Cells.Add(cellText);
				rowItem.Enabled = !controlDescription.Disabled;

				table.Rows.Add(rowItem);
				index++;
			}
		}
	}

	//==========================================================================================
	class TBMenu : TBPanel
	{
        //--------------------------------------------------------------------------------------
		public TBMenu()
		{	
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			MenuDescription menuDesc = (MenuDescription)ControlDescription;
			panel.Controls.Add(new TBContextMenu(menuDesc, ClientID, false, "tbwc" + menuDesc.Id ));
		}
	}


    /// ================================================================================
    class TBRadarGrid : TBScrollPanel
	{
		protected Table table;


		//Restituisce true se ci sono righe con dati estratte
		//--------------------------------------------------------------------------------------
		private bool HasData
		{
			get { return ((WndRadarTableDescription)ControlDescription).Table.Count > 1; }
		}
		
		//--------------------------------------------------------------------------------------
		public TBRadarGrid()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			table = new Table();
			base.OnInit(e);
			panel.Controls.Add(table);
            table.CssClass = "TBRadarTable";
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			table.Rows.Clear();

			WndRadarTableDescription wndTable = (WndRadarTableDescription)ControlDescription;
			int index = 0;
			int id = 0;
			int numCol = 0;
			foreach (WndRadarTableDescription.RadarRow row in wndTable.Table)
			{
				numCol = row.Cells.Count;
				bool header = table.Rows.Count == 0;
				TableRow tableRow = header ? new TableHeaderRow() : new TableRow();
				tableRow.CssClass = header ? "TBRadarHeaderRow" : "TBRadarRow";
				tableRow.ID = ID + (id++).ToString();
				table.Rows.Add(tableRow);

				TableCell cell = header ? new TableHeaderCell() : new TableCell();

				if (HasData)
				{
					cell.CssClass = row.Active ? "TBRadarCell Active" : "TBRadarCell";
					cell.ID = ID + (id++).ToString();
					tableRow.Cells.Add(cell);
				}
				if (!header)
				{   
					CheckBox cb = new CheckBox();
					cb.ID = ID + (id++).ToString();
					//Devo usare 2 funzioni javascript (tbRadarSelectInternal e tbRadarSelect) per gestire il click singolo e il doppio click 
					//sulla stessa riga di tabella. Se si associano entrambi gli eventi alla table cell, il click singolo prevarra' sempre sul doppio click
					//azione per selezioanre la riga del radar(chiudere il radar se non e' "pinnato")
					cb.Attributes.Add("onclick", string.Format("tbRadarSelectInternal('{0}', {1})", ClientID, index));
					cell.Controls.Add(cb);
				}
				foreach (string s in row.Cells)
				{
					cell = header ? new TableHeaderCell() : new TableCell();
					cell.CssClass = row.Active ? "TBRadarCell Active" : "TBRadarCell";
					cell.ID = ID + (id++).ToString();
					cell.Text = string.IsNullOrEmpty(s) ? "&nbsp;" : s;
					if (!header)
					{
						//azione per selezioanre la riga del radar(chiudere il radar se non e' "pinnato")
						cell.Attributes.Add("ondblclick", string.Format("tbRadarSelect('{0}', {1})", ClientID, index));
						//azione per sostare la selezione sulla riga
						cell.Attributes.Add("onclick", string.Format("tbRadarMoveRow('{0}', {1})", ClientID, index));
					}
					tableRow.Cells.Add(cell);
				}
				if (!header)
					index++;
			}
			//se non ci sono righe estratte aggiungo una riga con messaggio di tabella vuota
			if (!HasData)
			{
				TableRow tableRow = new TableRow();
				TableCell cell = new TableCell();
				cell.CssClass = "TBRadarCellNoData";
				cell.ColumnSpan = numCol;
				//Devo mettere il messaggio in un controllo "focusable" (Button), perche' il fuoco e' 
				//sul controllo TbRadarGrid (il this). Se quest'ultimo non ha all'interno controlli "focusable"
				//non prendera' il fuoco, e quindi non funzioneranno gli acceleratori
				Button buttonText = new Button();
				buttonText.Text = TBWebFormControlStrings.NoDataExtracted;
				buttonText.CssClass = "TBRadarButtonNoData";

				cell.Controls.Add(buttonText);
				tableRow.Cells.Add(cell);
				table.Rows.Add(tableRow);
			}
		}
	}


	//==========================================================================================
	class TBListControl : TBPanel
	{
		List<Control> controls;

		ImageBuffer bitmap;
		Bitmap listControlBitmap;

		//--------------------------------------------------------------------------------------
		public Bitmap ListControlBitmap
		{
			get
			{
				if (listControlBitmap == null)
					listControlBitmap = bitmap.CreateBitmap();
				return listControlBitmap;
			}
		}

		//--------------------------------------------------------------------------------------
		public int IconWidth
		{
			get { return ListControlBitmap.Width; }
		}

		//--------------------------------------------------------------------------------------
		public int IconHeight
		{
			get { return ((ListCtrlDescription)ControlDescription).IconHeight; }
		}
			
		//--------------------------------------------------------------------------------------
		public TBListControl()
		{
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			foreach (Control c in controls)
				panel.Controls.Remove(c);
			controls.Clear();
			
			ListCtrlDescription desc = ((ListCtrlDescription)ControlDescription);
			InnerControl.Style[HtmlTextWriterStyle.Overflow] = "scroll";
			bitmap = desc.Image;
		
			for (int i = 0; i < desc.Items.Count; i++)
				CreateItem(desc.Items[i]);
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			controls = new List<Control>();
			base.OnInit(e);
		}

		//--------------------------------------------------------------------------------------
		public override void Dispose()
		{
			if (listControlBitmap != null)
				listControlBitmap.Dispose();
			base.Dispose();
		}
	
		//--------------------------------------------------------------------------------------
		public void CreateItem(ListCtrlItemDescription itemDesc)
		{
			Panel itemPanel = new Panel();
			ImageButton icon = new ImageButton();
			Label label = new Label();
			controls.Add(itemPanel);

			itemPanel.Style[HtmlTextWriterStyle.Position] = "absolute";
			itemPanel.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(itemDesc.X).ToString();
			itemPanel.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(itemDesc.Y).ToString();
				
			if (itemDesc.IdxItem != -1 && !((ListCtrlDescription)ControlDescription).Image.IsEmpty)
			{
				CreateIconImage(itemDesc.IdxItem);
				icon.ImageUrl = GetImageUrl(itemDesc.IdxItem);
				icon.ID = string.Format("icon{0}{1}", ID, itemDesc.IdxItem);
				itemPanel.Controls.Add(icon);
			}

			if (itemDesc.Cells.Count > 0 && itemDesc.Cells[0].Text != null)
			{
				WndObjDescription labelDesc = itemDesc.Cells[0];
				label.Text = labelDesc.Text;
				label.Style[HtmlTextWriterStyle.Position] = "absolute";
				label.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(labelDesc.X - itemDesc.X).ToString();
				label.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(labelDesc.Y - itemDesc.Y).ToString();
				itemPanel.Controls.Add(label);
			}

			itemPanel.Style[HtmlTextWriterStyle.Cursor] = "hand";
			itemPanel.Attributes.Add("onclick", string.Format("tbSelectItemListCtrl('{0}', {1})", ClientID, itemDesc.IdxItem));
			label.CssClass = itemDesc.Selected ? "TBListCtrlRow Selected" : "TBListCtrlRow";
			panel.Controls.Add(itemPanel);
		}

		//--------------------------------------------------------------------------------------
		void CreateIconImage(int idx)
		{
			try
			{
				if (ListControlBitmap == null || idx < 0)
					return;

				string imageName = GetImageName(idx);
				if (ImagesHelper.HasImageInCache(imageName))
					return;

				int width = ListControlBitmap.Width;
				int height = IconHeight;
				using (Bitmap bmp = new Bitmap(ListControlBitmap, width, height))
				{
					using (Graphics g = Graphics.FromImage(bmp))
						g.DrawImage(ListControlBitmap, new Rectangle(0, 0, width, height), new Rectangle(0, height * idx, width, height), GraphicsUnit.Pixel);

					Color transparentColor = Color.Magenta;
					bmp.MakeTransparent(transparentColor);
					CreateDocumentImage(bmp, imageName, null);
				}
			}
			catch
			{
			}
		}

		//--------------------------------------------------------------------------------------
		string GetImageUrl (int idx)
		{
			return ImagesHelper.GetImageUrl(GetImageName(idx));
		}

		//--------------------------------------------------------------------------------------
		string GetImageName(int idx)
		{
			return string.Format("{0}_{1}_{2}.png", formControl.ProxyObjectId, WindowId, idx);
		}
	}


	
	//==========================================================================================
	class TBListControlDetails : TBPanel
	{
		Table table;

		//--------------------------------------------------------------------------------------
		public TBListControlDetails()
		{
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			table.Rows.Clear();

			FillTable();
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			table = new Table();
			base.OnInit(e);
		}

		//--------------------------------------------------------------------------------------
		public void FillTable()
		{
			ListCtrlDescription desc = ((ListCtrlDescription)ControlDescription);
			//table header
			TableHeaderRow tableHeaderRow = new TableHeaderRow();
			tableHeaderRow.CssClass = "TBListCtrlHeader";

			for (int i = 0; i < desc.ColumnsHeaderText.Count; i++)
			{
				TableHeaderCell tableHeaderCell = new TableHeaderCell();
				tableHeaderCell.Text = desc.ColumnsHeaderText[i];
				tableHeaderRow.Cells.Add(tableHeaderCell);
			}
			table.Rows.Add(tableHeaderRow);

			//table body
			for (int i = 0; i < desc.Items.Count; i++)
			{
				TableRow row = new TableRow();
				ListCtrlItemDescription itemDesc = desc.Items[i];

				for (int j = 0; j < itemDesc.Cells.Count; j++)
				{
					TableCell cell = new TableCell();
					cell.Text = itemDesc.Cells[j].Text;
					cell.Width = itemDesc.Cells[j].Width;
					row.Cells.Add(cell);
				}
				row.Style[HtmlTextWriterStyle.Cursor] = "hand";
				row.Attributes.Add("onclick", string.Format("tbSelectItemListCtrl('{0}', {1})", ClientID, itemDesc.IdxItem));
				row.CssClass = itemDesc.Selected ? "TBListCtrlRow Selected" : "TBListCtrlRow";
				table.Controls.Add(row);
			}
			panel.Controls.Add(table);
		}
	}

	//==========================================================================================
	class TBSpinControl : TBPanel
	{
		DynamicImageButton arrowUp;
		DynamicImageButton arrowDown;
		//--------------------------------------------------------------------------------------
		public TBSpinControl()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			arrowUp = new DynamicImageButton();
			arrowDown = new DynamicImageButton();
			base.OnInit(e);

			arrowUp.ID = string.Format("spinup{0}", WindowId);
			arrowUp.CssClass = "SpinUpperButton";
			arrowUp.TabIndex = -1;
			arrowUp.EnableViewState = false;
			arrowUp.OnClientClick = string.Format("tbClickSpinControl($get('{0}'), false);", ClientID);
			InnerControl.Controls.Add(arrowUp);

			arrowDown.ID = string.Format("spinlow{0}", WindowId);
			arrowDown.CssClass = "SpinLowerButton";
			arrowDown.TabIndex = -1;
			arrowDown.EnableViewState = false;
			arrowDown.OnClientClick = string.Format("tbClickSpinControl($get('{0}'), true);", ClientID);
			InnerControl.Controls.Add(arrowDown);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			arrowUp.ImageUrl = ImagesHelper.CreateImageAndGetUrl(ImagesHelper.PrefixedName("spinUp.png", IsEnabled), TBWebFormControl.DefaultReferringType);
			arrowDown.ImageUrl = ImagesHelper.CreateImageAndGetUrl(ImagesHelper.PrefixedName("spinDown.png", IsEnabled), TBWebFormControl.DefaultReferringType);
		}
	}

    
    ///<summary>
	///TbWebControl che rappresenta una progress bar
	/// </summary>
	/// ================================================================================
    public class TBProgressBar : TBPanel
	{
        Panel progress;  //E' la parte colorata della progressbar, quella che segnala l'avanzamento
            
		//--------------------------------------------------------------------------------------
        public TBProgressBar()
		{
		}
        
        //--------------------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            progress = new Panel();
            base.OnInit(e);
            
            AddToContainer(progress);
        }

        //--------------------------------------------------------------------------------------
        public override void SetControlAttributes()
        {
            base.SetControlAttributes();

            InnerControl.BackColor = Color.LightGray;
            
            int pos = ((WndProgressBarDescription)ControlDescription).Pos;
            int upper = ((WndProgressBarDescription)ControlDescription).Upper;
            
            int factor = pos != 0 ? (int)(upper / pos) : 0;

            progress.BackColor = Color.Blue;
            progress.Style[HtmlTextWriterStyle.Position] = "absolute";
            progress.Style[HtmlTextWriterStyle.Left] = InnerControl.Style[HtmlTextWriterStyle.Left];
            progress.Style[HtmlTextWriterStyle.Top] = InnerControl.Style[HtmlTextWriterStyle.Top];
            int zindex = 0;
            Int32.TryParse(InnerControl.Style[HtmlTextWriterStyle.ZIndex], out zindex);
            progress.Style[HtmlTextWriterStyle.ZIndex] = (zindex++).ToString();
            progress.Height = ControlDescription.Height;

            progress.Width = factor != 0 ? (int)(ControlDescription.Width / factor) : 0;
        }
	}


	///<summary>
	///TbWebControl che rappresenta il singolo pane di una status bar
	/// </summary>
	/// ================================================================================
	public class TbStatusBarPane : TBPanel
	{
		Label textPane;
		//--------------------------------------------------------------------------------------
		public TbStatusBarPane()
		{			
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			textPane = new Label();
			base.OnInit(e);
			InnerControl.Controls.Add(textPane);
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			textPane.Text = ControlDescription.Text;
		}
	}


	///<summary>
	/// Controllo web che renderizza uno snapshot del multichart.
	/// Deriva da scrollpanel perche' l'immagine del multichart e' completa, anche delle parti non visibili
	/// nella finestra GDI, e quindi tramite le scrollbar si riesce a scorrere l'immagine 
	/// </summary>
	//================================================================================
    class TBMultiChart : TBScrollPanel
	{
		System.Web.UI.WebControls.Image multichartImg;
		Bitmap multichartBmp = null;
		string idBitmap = String.Empty; 

		//--------------------------------------------------------------------------------------
		public TBMultiChart()
			: base()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			multichartImg = new System.Web.UI.WebControls.Image();

			base.OnInit(e);

			Panel.Controls.Add(multichartImg);
		}
		
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();

			ImageBuffer bitmap = ((WndImageDescription)ControlDescription).Image;
			if (!bitmap.IsEmpty)
			{
				multichartImg.Visible = true;
				CreateImage(bitmap);
				multichartImg.ImageUrl = ImagesHelper.GetImageUrl(bitmap.Id);
			}
			else
				multichartImg.Visible = false;
		}

		//--------------------------------------------------------------------------------------
		protected void CreateImage(ImageBuffer bitmap)
		{
			//Creo una nuova bitmap solo se e' cambiata rispetto alla precedente
			if (idBitmap != bitmap.Id)
			{
				if (multichartBmp != null)
					multichartBmp.Dispose();
				multichartBmp = bitmap.CreateBitmap();
				idBitmap = bitmap.Id;
			}

			CreateDocumentImage(multichartBmp, bitmap.Id, null);

			int offsetX = (int)Math.Round((Width - multichartBmp.Width) / 2.0);
			int offsetY = (int)Math.Round((Height - multichartBmp.Height) / 2.0);

			multichartImg.Style[HtmlTextWriterStyle.Left] = offsetX > 0 ? Unit.Pixel(X + offsetX).ToString() : Unit.Pixel(X).ToString();
			multichartImg.Style[HtmlTextWriterStyle.Top] = offsetY > 0 ? Unit.Pixel(Y + offsetY).ToString() : Unit.Pixel(Y).ToString();
			multichartImg.Width = Unit.Pixel(multichartBmp.Width);
			multichartImg.Height = Unit.Pixel(multichartBmp.Height);
		}

		//--------------------------------------------------------------------------------------
		public override void Dispose ()
		{
			if (multichartImg != null)
				multichartImg.Dispose();
			base.Dispose();
		}

	}

	//==========================================================================================
	class TBMailAddressTextBox : TBTextBox
	{
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			//se il link mail e' abilitato (pilota un booleano lato framework c++) ed e' in formato esteso
			//aaa@bbb.xx e non come nome associato lo abilita
			if (IsLinkWellFormed())
			{
				TextBox textBox = (TextBox)InnerControl;
				
				if (!IsEnabled)
				{
					textBox.TabIndex = -1;
					textBox.Enabled = true;
					textBox.ReadOnly = true;
					textBox.Attributes["onfocus"] = "return false;";
					textBox.Attributes["onkeypress"] = "return false;";
					textBox.Attributes["onkeydown"] = "return false;";
					textBox.Attributes["onclick"] = GetLink();
					textBox.CssClass = "HyperLink " + textBox.CssClass;
				}
				else
				{
					textBox.TabIndex = 0;
					textBox.ReadOnly = false;
					textBox.Attributes["onclick"] = "";
				}
			}
		}

		//Dice se il testo nella textBox e' una mail ben formata
		//--------------------------------------------------------------------------------------
		public virtual bool IsLinkWellFormed()
		{
			return ((WndMailAddressEditDescription)ControlDescription).EnabledLink && Validation.IsEmail(ControlDescription.HtmlTextAttribute);
		}

		//Ritorna la stringa javascript che permette di aprire il client di posta predefinito con
		//--------------------------------------------------------------------------------------
		public virtual string GetLink()
		{
			TextBox textBox = (TextBox)InnerControl;
			return string.Format("javacript:window.open('mailto:{0}')",textBox.Text);
		}
	}

	//==========================================================================================
	class TBWebLinkTextBox : TBMailAddressTextBox
	{

		//Dice se il testo nella textBox e' una mail ben formata
		//--------------------------------------------------------------------------------------
		public override bool IsLinkWellFormed()
		{
			return Validation.IsWebAddress(ControlDescription.HtmlTextAttribute);
		}
	
		//Ritorna la stringa javascript che apre una nuova finestra del browser sull'indirizzo web
		//--------------------------------------------------------------------------------------
		public override string GetLink()
		{
			string text = ((TextBox)InnerControl).Text;
			if (text.Contains("http"))
				return string.Format("javacript:window.open('{0}')", text);
			else
				return string.Format("javacript:window.open('http://{0}')", text);
		}
	}

	//==========================================================================================
	class TBAddressTextBox : TBMailAddressTextBox
	{

		//il link e' generato lato framework dal parsed edit, quindi qui non si deve controllare
		//--------------------------------------------------------------------------------------
		public override bool IsLinkWellFormed()
		{
			return true;
		}

		//Ritorna la stringa javascript che apre una nuova finestra del browser sulla mappa di google maps
		//--------------------------------------------------------------------------------------
		public override string GetLink()
		{
			string url = HttpUtility.JavaScriptStringEncode(((WndAddressEditDescription)ControlDescription).Url);
			if (url.Contains("http"))
				return string.Format("javacript:window.open('{0}')", url);
			else
				return string.Format("javacript:window.open('http://{0}')", url);
		}
	}
}


/// ================================================================================
public static class Validation
{
	/// <summary>
	/// Regular expression, which is used to validate an E-Mail address.
	/// </summary>
	public const string MatchEmailPattern = 
			@"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
     + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
     + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
     + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

	
	/// <summary>
	/// Checks whether the given Email-Parameter is a valid E-Mail address.
	/// </summary>
	/// <param name="email">Parameter-string that contains an E-Mail address.</param>
	/// <returns>True, when Parameter-string is not null and 
	/// contains a valid E-Mail address;
	/// otherwise false.</returns>
	public static bool IsEmail(string email)
	{
		if (!string.IsNullOrWhiteSpace(email)) 
			return Regex.IsMatch(email, MatchEmailPattern);
		
		return false;
	}

	/// <summary>
	/// Checks whether the given webAdress-Parameter is a valid web address.
	/// </summary>
	/// <param name="email">Parameter-string that contains an web address.</param>
	/// <returns>True, when Parameter-string is not null and 
	/// contains a valid web address;
	/// otherwise false.</returns>
	public static bool IsWebAddress(string webAddress)
	{
		if (!string.IsNullOrWhiteSpace(webAddress))
			return webAddress.StartsWith("www.") || webAddress.StartsWith("http");
		
		return false;
	}
}        

