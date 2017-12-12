using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	///<summary>
	/// Classe che deriva il TabContainer del toolkitAjax, per aggiungere i bottoni di scorrimento delle linguette delle tab 
	///(se non riescono a essere visualizzate tutte) e per aggiungere degli aggiustamenti di posizionamento
	/// </summary>
	/// ================================================================================
	class TBTabContainer : Panel
	{
		internal const int headerHeight = 22;
		internal const int buttonWidth = 30;
		private List<TBTabPanel> tabs = new List<TBTabPanel>();

		public List<TBTabPanel> Tabs { get { return tabs;  } }
		TBTabPanel activeTab;
		//-----------------------------------------------------------------	---------------------
		protected string HeaderId { get { return ClientID + "_h"; } }
		
		//-----------------------------------------------------------------	---------------------
		public TBTabPanel ActiveTab 
		{
			get 
			{
				return activeTab;
			}
			set 
			{
				if (activeTab == value)
					return;

				if (activeTab != null)
					Controls.Remove(activeTab);
				activeTab = value;
				if (activeTab != null)
					Controls.Add(activeTab);
				
			}
		}
		//-----------------------------------------------------------------	---------------------
		public TBTabContainer()
		{

		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			CssClass = "tb__tab_xp tb__tab_default";
		}

		//--------------------------------------------------------------------------------------
		protected override void RenderContents(HtmlTextWriter writer)
		{
			Page.VerifyRenderingInServerForm(this);

			RenderHeader(writer);

			writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_body");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Padding, "0px");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.BorderStyle, "solid");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.BorderColor, "#8F9B9C");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.BorderWidth, "1px");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Overflow, "hidden");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Height, Unit.Pixel(((int)Height.Value - 2/*border*/ - headerHeight/*header*/)).ToString());
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Width, Unit.Pixel(((int)Width.Value - 2/*border*/)).ToString());
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			RenderChildren(writer);

			writer.RenderEndTag();
		}
		//--------------------------------------------------------------------------------------
		protected void RenderHeader(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, HeaderId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "tb__tab_header");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Height, Unit.Pixel(headerHeight).ToString());
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			
			foreach (TBTabPanel panel in Tabs)
			{
				if (panel.Visible)
					panel.RenderHeader(writer);
			}
			writer.RenderEndTag();


			writer.AddAttribute(HtmlTextWriterAttribute.Id, "spanBtns_" + HeaderId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "TabberButtonsLayer");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Height, "20px");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Left, Unit.Pixel(((int)this.Width.Value - buttonWidth * 2)).ToString());
			//I bottoni per visualizzare le linguette delle tab che non stanno nella pagina partono nascosti,
			//verrano visualizzati dal client via javascript solo quando le tab sforano 
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
			writer.AddTbStyleAttribute(HtmlTextWriterStyle.Display, "none");
			
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			writer.AddAttribute(HtmlTextWriterAttribute.Type, "button");

			writer.AddAttribute(HtmlTextWriterAttribute.Id, "btnPrev_" + HeaderId);
			writer.AddAttribute(HtmlTextWriterAttribute.Value, " < ");
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, String.Format("onNavigate(false, '{0}')", HeaderId));
			writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "true");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "TabberButton NotFocusable");

			writer.RenderBeginTag("input");
			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Type, "button");
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "btnNext_" + HeaderId);
			writer.AddAttribute(HtmlTextWriterAttribute.Value, " > ");
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, String.Format("onNavigate(true, '{0}')", HeaderId));
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "TabberButton NotFocusable");

			writer.RenderBeginTag("input");
			writer.RenderEndTag();

			writer.RenderEndTag(); //div

		}


		internal TBTabPanel FindTab(string id)
		{
			foreach (TBTabPanel t in Tabs)
				if (t.ID == id)
					return t;
			return null;
		}
	}

	///<summary>
	/// Classe che deriva il TabPanel del toolkitAjax, per aggiungere un'immagine di attesa
	/// sul cambio tab, visto che il tabContainer Ajax cambia la tab sul browser prima che gli sia arrivata la nuova da ridisegnare
	/// </summary>
	/// ================================================================================
	public class TBTabPanel : Panel
	{
		TBTabberPanel tabPanel;

		bool isActive = false;

		internal TBTabberPanel TabPanel
		{
			get { return tabPanel; }
		}
		//property che indica se la tab e' quella selezionata (in primo piano) nel tabber
		//--------------------------------------------------------------------------------------
		public bool IsActive
		{
			get { return isActive; }
			set { isActive = value; }
		}
		public string HeaderText { get; set; }

		//--------------------------------------------------------------------------------------
		internal void AddTBControl(TBTabberPanel tabPanel)
		{
			this.tabPanel = tabPanel;
			Controls.Add(tabPanel);
		}

		//--------------------------------------------------------------------------------------
		internal void RenderHeader(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_tab");
			if (IsActive)
				writer.AddAttribute("Active", "true");
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "tb__tab_outer");

			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "tb__tab_inner");
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.AddAttribute(HtmlTextWriterAttribute.Class,"tb__tab_tab");
		
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "__tab_" + ClientID);
			writer.RenderBeginTag(HtmlTextWriterTag.Span);

			WriteTabIcon(writer);
			
			writer.Write(HeaderText);
			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		///<summary>
		///Metodo che scrive l'eventuale icona "lucchetto"(tab protetta via security) o "x rossa" (tab disabilitata)
		///nell'output html
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void WriteTabIcon(HtmlTextWriter writer)
		{
			string imageUrl = "";
			if (tabPanel.Protected)
				imageUrl = ImagesHelper.CreateImageAndGetUrl("Padlock.png", TBWebFormControl.DefaultReferringType);
			else if (!Enabled)
				imageUrl = ImagesHelper.CreateImageAndGetUrl("AccessDenied.png",TBWebFormControl.DefaultReferringType);
			if (!string.IsNullOrWhiteSpace(imageUrl))
				writer.Write(string.Format("<img src='{0}'style='vertical-align:middle'/>", imageUrl));
		}
	}

	//==========================================================================================
	class TBTabber : TBWebControl
	{
		protected TBTabContainer tabber;

		//--------------------------------------------------------------------------------------
		public override WebControl InnerControl
		{
			get { return tabber; }
		}

		//--------------------------------------------------------------------------------------
		internal override bool Focusable { get { return false; } }

		//--------------------------------------------------------------------------------------
		public TBTabPanel ActiveTab
		{
			get { return tabber.ActiveTab; }
		}
		//--------------------------------------------------------------------------------------
		protected override string InitFunctionName { get { return "initTabsBtn"; } }

		//--------------------------------------------------------------------------------------
		public TBTabber()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			tabber = new TBTabContainer();
			base.OnInit(e);
	
			AddToContainer(tabber);

			RegisterInitControlScript();

			tabber.ID = ControlDescription.Id;

			AddTabs();
		}

		//--------------------------------------------------------------------------------------
		protected virtual int AddTabs()
		{
			int index = 0;
			
			foreach (TabDescription tabDesc in ControlDescription.Childs)
			{
				TBTabPanel tab = new TBTabPanel();
				tab.ID = tabDesc.Id;
				tab.Enabled = tabDesc.Enabled;
				tab.HeaderText = tabDesc.HtmlTextAttribute;
			
				tabber.Tabs.Add(tab);

				TBTabberPanel tabPanel = new TBTabberPanel();
				tabPanel.SetInitValues(this.FormControl, this, tabDesc);
				tabPanel.ID = string.Format("TabPnl_{0}", ControlDescription.Id);
				tabPanel.InnerControl.ID = string.Format("Pnl_{0}", ControlDescription.Id);
				tab.AddTBControl(tabPanel);

				if (((TabDescription)tabDesc).Active)
				{
					tab.IsActive = true;
					tabber.ActiveTab = tab;
					tabPanel.AddChildControls(tabDesc);
				}

				index++;

			}
			return index;
		}


		//--------------------------------------------------------------------------------------
		public override void UpdateFromControlDescription(WndObjDescription description)
		{
            UpdateTabs(description);
			
            base.UpdateFromControlDescription(description);
		}

        //--------------------------------------------------------------------------------------
        protected virtual void UpdateTabs(WndObjDescription description)
        {
            foreach (TabDescription tabEl in description.Childs)
            {
                TBTabPanel tab = tabber.FindTab(tabEl.Id);
                if (tab == null)
                    continue;
                tab.HeaderText = tabEl.HtmlTextAttribute;
                tab.Enabled = tabEl.Enabled;

                TBPanel tabPanel = tab.TabPanel;
                tab.IsActive = tabEl.Active;
                if (tab.IsActive)
                    tabber.ActiveTab = tabPanel.Parent as TBTabPanel;

                tabPanel.UpdateFromControlDescription(tabEl);

                //se si e' aggiornato il tabPanel, forzo un refresh del tabber per far si che aggiorni
                //l'header del tabber
                if (tabPanel.Updated)
                    this.Update();
            }
        }

		//--------------------------------------------------------------------------------------
		public override void AddChildControls(WndObjDescription description)
		{
			//does nothing
		}

		//--------------------------------------------------------------------------------------
		internal string GetTabId(int tabIndex)
		{
			return tabber.Tabs[tabIndex].ID;
		}
	}



    //==========================================================================================
    class TBTabbedToolbar : TBTabber
    {
        //--------------------------------------------------------------------------------------
        public TBTabbedToolbar()
        {
        }

        //--------------------------------------------------------------------------------------
        protected override int AddTabs()
        {
            int index = 0;

            foreach (ToolbarDescription toolbarDesc in ControlDescription.Childs)
            {
                TBTabPanel tab = new TBTabPanel();
                tab.ID = toolbarDesc.Id;
                tab.Enabled = toolbarDesc.Enabled;
                tab.HeaderText = toolbarDesc.Text;

                tabber.Tabs.Add(tab);

                TBTabberPanel tabPanel = new TBTabberPanel();
                tabPanel.SetInitValues(this.FormControl, this, toolbarDesc);
                tabPanel.ID = string.Format("TabPnl_{0}", ControlDescription.Id);
                tabPanel.InnerControl.ID = string.Format("Pnl_{0}", ControlDescription.Id);
                tabPanel.InnerControl.Style[HtmlTextWriterStyle.OverflowX] = "hidden";
                tab.AddTBControl(tabPanel);

                if (((TabbedToolbarDescription)ControlDescription).ActiveIndex == index)
                {
                    tab.IsActive = true;
                    tabber.ActiveTab = tab;
                    tabPanel.AddChildControls(toolbarDesc);
                }

                index++;

            }
            return index;
        }

        //--------------------------------------------------------------------------------------
        protected override void SetControlPosition()
        {
            base.SetControlPosition();
            // ignora i posizionamenti definiti dalla parte C++ perchè la loro posizione
            // è controllata dalla creazione della form. 
            InnerControl.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(0).ToString();
            InnerControl.Style[HtmlTextWriterStyle.Top] = Unit.Pixel(0).ToString();
        }

        //--------------------------------------------------------------------------------------
        protected override void UpdateTabs(WndObjDescription description)
        {
            int index = 0;

            foreach (ToolbarDescription tabEl in description.Childs)
            {
                TBTabPanel tab = tabber.FindTab(tabEl.Id);
                if (tab == null)
                    continue;
                tab.HeaderText = tabEl.HtmlTextAttribute;
                tab.Enabled = tabEl.Enabled;
                tab.IsActive = false;
                TBPanel tabPanel = tab.TabPanel;
                if (((TabbedToolbarDescription)ControlDescription).ActiveIndex == index)
                {
                    tab.IsActive = true;
                    tabber.ActiveTab = tabPanel.Parent as TBTabPanel;
                }
                tabPanel.UpdateFromControlDescription(tabEl);

                //se si e' aggiornato il tabPanel, forzo un refresh del tabber per far si che aggiorni
                //l'header del tabber
                if (tabPanel.Updated)
                    this.Update();

                index++;
            }
        }
    }

	//==========================================================================================
	class TBTabberPanel : TBPanel
	{
		Bitmap bkgBmp = null;
		string idBitmap = String.Empty; 
		
		//indica se la tab e' protetta via Security
		//--------------------------------------------------------------------------------------
        public bool Protected 
        { 
            get { 
                    return ControlDescription is TabDescription ? 
                                    ((TabDescription)ControlDescription).Protected
                                    :
                                    false; 
                } 
        }

		//--------------------------------------------------------------------------------------
		public TBTabberPanel()
		{
		}

		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			//to fix offset errors due to tab headers
			base.SetControlAttributes();
			InnerControl.Style[HtmlTextWriterStyle.Left] = "0px";
			InnerControl.Style[HtmlTextWriterStyle.Top] = "0px";
			
			//bkg image
			ImageBuffer bitmap = ((WndImageDescription)ControlDescription).Image;
			if (!bitmap.IsEmpty)
			{
				CreateImage(bitmap);
				InnerControl.Style.Add(HtmlTextWriterStyle.BackgroundImage, ImagesHelper.GetImageUrl(bitmap.Id));
			}

			//Imposto altezza e larghezza della tab in modo che occupi tutto lo spazio del tabcontainer
			if (parentTBWebControl != null && parentTBWebControl.ControlDescription != null)
			{
				InnerControl.Width = parentTBWebControl.ControlDescription.Width;
				InnerControl.Height = parentTBWebControl.ControlDescription.Height - 20/*altezza header della tab*/;
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

		//--------------------------------------------------------------------------------------
		public override void UpdateFromControlDescription(WndObjDescription description)
		{
            if (description is TabDescription)
            {
                if (((TabDescription)description).Active)
                    base.UpdateFromControlDescription(description);
                else
                {
                    InnerControl.Controls.Clear();
                }
            }
            else
            {
                base.UpdateFromControlDescription(description);
            }
		}
	}
}
