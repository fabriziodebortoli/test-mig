using System.Collections;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;
//using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.WebControls
{

	/// <summary>
	/// Custom Control contenente l'insieme dei MenuApplicationPanel
	/// </summary>
	public class MenuApplicationsPanelBar : System.Web.UI.WebControls.Panel
	{
			
		#region DataMember privati
		/// <summary>
		/// MenuXmlParser DataSource dei MenuApplicationPanel
		/// </summary>
		private IMenuXmlParser	parserDomMenu;
		/// <summary>
		/// Array che conterrà l'insieme de1 MenuApplicationPanel
		/// </summary>
		private ArrayList		collapsiblePanelArray;

		/// <summary>
		/// Script per espandere e collassare i pannelli
		/// </summary>
		protected string myScript = @"
				<script language='javascript'>
				function expandContractDiv(p_oImg,p_oTbl, p_oPrefText, collapseImage, expandImage)
				{	
					if(p_oTbl.style.visibility == 'hidden')
					{
						p_oImg.src = collapseImage;
						p_oTbl.style.visibility = 'visible';
						p_oTbl.style.display = '';
						p_oPrefText.value = 'true';
					}
					else
					{
						p_oImg.src = expandImage;
						p_oTbl.style.visibility = 'hidden';
						p_oTbl.style.display = 'none';
						p_oPrefText.value = 'false';
					}		
				}
				</script>";
		#endregion

		#region proprietà
		/// <summary>
		/// Set e Get del MenuXmlParser DataSource del controllo
		/// </summary>
		public IMenuXmlParser ParserDomMenu 
		{
			get
			{
				return parserDomMenu;
			}

			set
			{
				parserDomMenu = value;
			}
		}

		/// <summary>
		/// Set e Get dell'array contenete l'insieme dei MenuApplicationPanel
		/// </summary>
		public ArrayList CollapsiblePanelArray 
		{
			get
			{
				return collapsiblePanelArray;
			}
		}


		#endregion

		#region Costruttori

		/// <summary>
		/// Costruttore imposta solo il BackColor con il colore default
		/// </summary>
		public MenuApplicationsPanelBar ()
		{
			if (collapsiblePanelArray == null)
				collapsiblePanelArray	= new ArrayList();

            this.BackColor = Color.FromArgb(132,148, 232);
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Costruttore che imposta la proprietà ParserDomMenu (cioè il DataSource)
		/// che popola i pannelli
		/// </summary>
		/// <param name="menuXmlParser"></param>
		public MenuApplicationsPanelBar (IMenuXmlParser menuXmlParser)
		{
			if (collapsiblePanelArray == null)
				collapsiblePanelArray	= new ArrayList();

			SetMenuXmlParser(menuXmlParser);		
		}
		#endregion

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che istanzia i MenuApplicationPanel leggendo il MenuXmlParser
		/// </summary>
		/// <param name="menuXmlParser"></param>
		public void SetMenuXmlParser(IMenuXmlParser menuXmlParser)
		{
			if (collapsiblePanelArray == null)
				collapsiblePanelArray	= new ArrayList();

			parserDomMenu			= menuXmlParser;
			
			GroupLinkLabel		moduleGroup		 = null;
			ArrayList			ar				 = null;
			MenuApplicationPanel	collapsiblePanel = null;

			foreach (IMenuXmlNode applicationNode in parserDomMenu.Root.ApplicationsItems)
			{
				string applicationName = applicationNode.GetNameAttribute();
				collapsiblePanel = new MenuApplicationPanel(applicationName);
				string applicationTitle = applicationNode.Title;
				collapsiblePanel.ApplicationElement.ApplicationTitleText	= applicationTitle;

				string appImageFile = parserDomMenu.GetApplicationImageFileName(applicationName);
				collapsiblePanel.ApplicationElement.ApplicationImageUrl = ImagesHelper.CopyImageFileIfNeeded(appImageFile);
				ar = applicationNode.GroupItems;

				if(ar == null)
					continue;

				foreach (IMenuXmlNode groupNode in ar)
				{
					moduleGroup = collapsiblePanel.AddGroup(groupNode.Title, applicationNode.GetApplicationName());
				
					if (moduleGroup == null)
						continue;
					
					moduleGroup.NameSpace = groupNode.GetGroupName();
					string[] token = moduleGroup.NameSpace.Split('.');
					if (token.Length >= 2 && string.Compare(token[1], "UserReportsGroup") == 0)
					{
						moduleGroup.ImagePath = ImagesHelper.CreateImageAndGetUrl("DefaultUserReportsGroup.gif", Helper.DefaultReferringType);
					}
					else
					{
						string imagePath = parserDomMenu.GetGroupImageFileName(applicationNode.GetApplicationName(), groupNode.GetGroupName());
						moduleGroup.ImagePath = ImagesHelper.CopyImageFileIfNeeded(imagePath);
						if (string.IsNullOrEmpty(moduleGroup.ImagePath))
							moduleGroup.ImagePath = ImagesHelper.CreateImageAndGetUrl("DefaultGroupImgSmall.gif", Helper.DefaultReferringType);

					}

					moduleGroup.ParentApplicationName = applicationNode.GetApplicationName();
					moduleGroup.LabelLinkFontInfo.Size = FontUnit.Point(9);
				}
				collapsiblePanelArray.Add(collapsiblePanel);
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che aggiunge fisicamente i MenuApplicationPanel alla Collection
		/// Controls e registra lo script per espandere e collassare i pannelli dei gruppi
		/// </summary>
		protected override void CreateChildControls()
		{
			if (!this.Page.ClientScript.IsStartupScriptRegistered("expandContractDiv"))
				this.Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), "expandContractDiv", myScript);

			foreach (MenuApplicationPanel panel in collapsiblePanelArray)
			{
				Controls.Add(new LiteralControl("&nbsp;"));
				panel.Width = Unit.Percentage(100);
				Controls.Add(panel);
				Controls.Add(new LiteralControl("&nbsp;"));
			}
		}
		//---------------------------------------------------------------------
	}
}
