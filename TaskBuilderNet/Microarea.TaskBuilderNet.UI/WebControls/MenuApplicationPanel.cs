using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;
//using Microarea.TaskBuilderNet.Core.CoreTypes;

namespace Microarea.TaskBuilderNet.UI.WebControls
{
	/// <summary>
	/// Controllo Custom che rappresental'intero pannello collassabile
	/// (application e lista dei moduli)
	/// </summary>
	public class MenuApplicationPanel : System.Web.UI.WebControls.Panel
	{
		#region DataMember protetti
		/// <summary>
		/// Tabella che conterrà l'insieme dei gruppi (quindi l'insieme delle
		/// GroupLinkLabel). 
		/// </summary>
		protected	Table					groupsTable;
		/// <summary>
		/// MenuApplicationCaption rappresentante l'applicazione
		/// </summary>
		protected	MenuApplicationCaption	applicationElement;
		/// <summary>
		/// Cella che conterrà il singolo gruppo
		/// </summary>
		protected	TableCell				groupsCell;

		//---------------------------------------------------------------------
		#endregion

		#region proprietà
		//---------------------------------------------------------------------
		/// <summary>
		/// Set e Get sull'immagine di sfondo del pannello dei gruppi
		/// </summary>
		public string GroupsBackImageUrl
		{
			set
			{
				groupsTable.BackImageUrl	= value;
				groupsTable.BackColor		= Color.Transparent;
			}
			get
			{
				return groupsTable.BackImageUrl;
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Set e Get sul Colore di sfondo del pannello dei gruppi
		/// </summary>
		public Color GroupsBackGroundColor
		{
			set
			{
				groupsTable.BackColor		= value;
				groupsTable.BackImageUrl	= "";
			}
			get
			{
				return groupsTable.BackColor;
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// SEt della proprietà FontName delledelle groupLinkLabel presenti nel pannello
		/// </summary>
		public string GroupsFontName
		{
			set
			{
				foreach(GroupLinkLabel groupLinkLabel in this.groupsCell.Controls)
				{
					groupLinkLabel.LabelLinkFontName = value;
				}
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Set della proprietà CssClass delle groupLinkLabel presenti nel pannello
		/// </summary>
		public string GroupsCssClass
		{
			set
			{
				foreach(GroupLinkLabel groupLinkLabel in this.groupsCell.Controls)
				{
					groupLinkLabel.LabelLinkCss = value;
				}
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Get del MenuApplicationCaption
		/// </summary>
		public MenuApplicationCaption ApplicationElement
		{
			get
			{
				return this.applicationElement;
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Set sulla proprietà Width del controllo
		/// </summary>
		public Unit CollapsiblePanelWidth
		{
			set
			{
				applicationElement.ApplicationElementContainer.Width = value;
				groupsTable.Width = applicationElement.ApplicationElementContainer.Width;
			}
			get
			{
				return applicationElement.ApplicationElementContainer.Width;
			}
		}
		//---------------------------------------------------------------------

		#endregion

		#region Construttore
		/// <summary>
		///  Costruttore, istanzia i controlli e setta le proprietà con i valori di default
		/// </summary>
		/// <param name="appName"></param>
		public MenuApplicationPanel(string appName)
		{
			
			applicationElement = new MenuApplicationCaption(appName);
			groupsTable = new Table();
			groupsTable.ID =  appName + "groupsTable";
			groupsTable.BackImageUrl = string.Empty;
		
			groupsTable.Width = Unit.Percentage(100);
			groupsTable.EnableViewState = false;

			groupsCell = new TableCell();
		}	
		
		#endregion

		#region funzioni per creare i controls
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che aggiunge fisicamente i controlli a una tabella
		/// (per allinearli meglio) che verrà a sua volta aggiunta alla collection 
		/// dei Controls.
		/// Viene anche innestato nel codice il link all file css contenente gli 
		/// stili applicati di default al controllo
		/// </summary>
		protected override void CreateChildControls()
		{
			string html = @"
							<table id=tblMain cellSpacing=0 cellPadding=0 border=0 width=100% >
								<tr>
									<td>";
			Controls.Add(new LiteralControl(html));

            // scaricano le immagini necessarie nella area temporanea
			string collapseImage = ImagesHelper.CreateImageAndGetUrl("Collapse.GIF", Helper.DefaultReferringType);
			string expandImage = ImagesHelper.CreateImageAndGetUrl("Expand.GIF", Helper.DefaultReferringType);

			applicationElement.ApplicationElementContainer.EnableViewState = false;
			this.EnableViewState = true;
            applicationElement.ImageState.Attributes["onClick"] = string.Format
                (
                    "expandContractDiv(this,{0},{1},'{2}','{3}');",
                    groupsTable.ClientID,
                    applicationElement.StateClientID,
                    collapseImage,
                    expandImage
                );			
			Controls.Add(applicationElement);
			Controls.Add(new LiteralControl("</td></tr><tr><td>"));

			TableRow groupsRow	= new TableRow();
			groupsRow.Cells.Add (groupsCell);
			groupsTable.Rows.Add(groupsRow);
			Controls.Add(groupsTable);
			Controls.Add(new LiteralControl("</td></tr></table>"));	
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che aggiunge un nuova GroupLinkLabel al pannello collassabile
		/// </summary>
		/// <param name="groupLabel"></param>
		/// <param name="applicationName"></param>
		/// <returns></returns>
		public GroupLinkLabel AddGroup(string groupLabel, string applicationName)
		{
			GroupLinkLabel group = new GroupLinkLabel(groupLabel, applicationName);
			groupsCell.Controls.Add(group);
			return group;
		}
		//---------------------------------------------------------------------
		#endregion

	

	}
}
