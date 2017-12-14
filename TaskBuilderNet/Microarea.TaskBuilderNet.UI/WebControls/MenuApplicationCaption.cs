using System;
using System.Drawing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;
//using Microarea.TaskBuilderNet.Core.CoreTypes;

namespace Microarea.TaskBuilderNet.UI.WebControls
{
	/// <summary>
	/// Classe che rappresenta la parte del MenuApplicationPanel relativa all'
	/// applicazione.
	/// </summary>
	public class MenuApplicationCaption : System.Web.UI.WebControls.Panel 
	{
	
		#region dichiarazione controlli
		/// <summary>
		/// Tabella che contiene l'insieme dei controlli
		/// </summary>
		protected	Table									applicationTable;
		/// <summary>
		/// Immagine logo dell'applicazione
		/// </summary>
		protected	System.Web.UI.HtmlControls.HtmlImage	applicationImage;
		/// <summary>
		/// Cella contenitrice di destra
		/// </summary>
		protected	TableCell								rigthTopCell;
		/// <summary>
		/// Label contenente il nome dell'applicazione
		/// </summary>
		protected	Label									titleLable;
		/// <summary>
		/// Campo nascosto che mi dice se il pannello dei gruppi è chiuso oppure
		/// no (lavora con il javascript)
		/// </summary>
		protected	HtmlInputHidden							stateText;
		/// <summary>
		/// Bottone con immagine che apre e chiude il pannello dei gruppi
		/// </summary>
		protected	System.Web.UI.HtmlControls.HtmlImage	expandButtonImage;
		/// <summary>
		/// Cella che conterrà il logo dell'applicazione
		/// </summary>
		protected	TableCell								applicationImageCell;	
		/// <summary>
		/// Intero che viene utilizzato per distanziare l'immagine dai bordi
		/// </summary>
		private		Int32									appImageOffset = 4;
		/// <summary>
		/// Intero che viene utilizzato per distanziare il bottone di 
		/// espandi / collassadai bordi
		/// </summary>
		private		Int32									expandButtonImageOffset = 2;
		/// <summary>
		/// Nome dell'applicazione
		/// </summary>
		private		string									applicationName = string.Empty;
		#endregion

		#region proprietà sull'immagine dell'Applicazione
		/// <summary>
		/// Set e Get sulla proprietà Src del controllo Image
		/// </summary>
		public string ApplicationImageUrl
		{
			set
			{
				applicationImage.Src = value;				
			}

			get
			{
				return	applicationImage.Src;
			}
			
		}
		/// <summary>
		/// Set e Get sulla proprietà Height del controllo Image
		/// </summary>
		public int ApplicationImageHeight
		{
			get
			{
				return applicationImage.Height;
			}
			set
			{
				applicationImage.Height = value;
			}
		}
		#endregion

		#region proprietà sulla label del nome dell'applicazione
		/// <summary>
		/// Set e Get sulla proprietà Text della Label
		/// </summary>
		public string ApplicationTitleText
		{
			set
			{
				titleLable.Text = value;
				ViewState["ApplicationTitleText"] = titleLable.Text;
			}

			get
			{
				return ViewState["ApplicationTitleText"].ToString();
			}
		}

		/// <summary>
		/// Set e Get sulla proprietà CssClass della Label
		/// </summary>
		public string ApplicationTitleClass
		{
			set
			{
				titleLable.CssClass = value;
			}
		}

		/// <summary>
		/// Set e Get sulla proprietà FontName della Label
		/// </summary>
		public string ApplicationTitleFontName
		{
			set
			{
				titleLable.Font.Name = value;
			}
		}

		#endregion

		#region proprietà sull'immagine bottone
		/// <summary>
		/// Set e Get sulla proprietà sul bottone che collassa ed espande il
		/// pannello dei gruppi
		/// </summary>
		public System.Web.UI.HtmlControls.HtmlImage ImageState
		{
			set
			{
				expandButtonImage= value;
			}
			get
			{
				return expandButtonImage;
			}
		}
		/// <summary>
		/// Set e Get sulla proprietà Src dell'Immagine del bottone 
		/// che collassa ed espande il pannello dei gruppi
		/// </summary>
		public string ImageStateUrl
		{
			set
			{
				expandButtonImage.Src = value;
			}
			get
			{
				return expandButtonImage.Src;
			}
		}

		#endregion

		#region proprietà sul campo hidden
		/// <summary>
		/// Get sulla proprietà Value del campo nascosto per l'espansione e la chiusura
		/// del pannello dei gruppi
		/// </summary>
		public string State
		{
			get
			{
				return stateText.Value;
			}
		}
		/// <summary>
		/// Get sulla proprietà ClientID del campo nascosto per l'espansione e la chiusura
		/// del pannello dei gruppi
		/// </summary>
		public string StateClientID
		{
			get
			{
				return stateText.ClientID;
			}
		}

		#endregion

		#region proprietà del contenitore
		/// <summary>
		/// Get della tabella che contiene l'insieme degli oggetti he formano
		/// il MenuApplicationCaption
		/// </summary>
		public Table ApplicationElementContainer
		{
			get
			{
				return applicationTable;
			}
		}
		#endregion

		#region Costruttore
		//---------------------------------------------------------------------
		/// <summary>
		/// Costruttore; istanzia gli oggetti e setta le loro proprietà con i 
		/// valori di default del controllo 
		/// </summary>
		/// <param name="name"></param>
		public MenuApplicationCaption(string name)
		{
			applicationName	= name;
			this.EnableViewState=true;
			//Table che contiene il tutto
			applicationTable = new Table();

			applicationTable.CellSpacing		= 0;
			applicationTable.BorderWidth		= 0;
			applicationTable.CellPadding		= 0;
			applicationTable.BorderStyle		= BorderStyle.None;
			applicationTable.EnableViewState	= true;
			
			//Immagine dell'Applicazione
			applicationImage						= new System.Web.UI.HtmlControls.HtmlImage();
			applicationImage.Style.Add("filter", "Chroma(Color=#FFFFFF)");
			applicationImage.Src					= string.Empty;
			applicationImage.Attributes["hspace"]	= this.appImageOffset.ToString();

			

			
			//Label del nome dell'applicazione
			titleLable				= new Label();
			titleLable.BackColor	= Color.Transparent;

			//Campo Hidden per lo stato del pannello (collassato / aperto)
			stateText		= new HtmlInputHidden();
			stateText.ID	= applicationName + "stateText";
			stateText.Name	= applicationName + "stateText";
			
			//Immagine a bottone 
			expandButtonImage						= new System.Web.UI.HtmlControls.HtmlImage();
			expandButtonImage.Style["CURSOR"]		= "hand";
			expandButtonImage.Align					= "top";
			expandButtonImage.Attributes["hspace"]	= expandButtonImageOffset.ToString();
			
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzione per aggiungere i controlli
		/// <summary>
		/// Funzione che aggiunge fisicamente i controlli alla tabella contenitore
		/// che a sua volta viene aggiunta alla Collection Controls del MenuApplicationCaption
		/// </summary>
		protected override void CreateChildControls()
		{
			Controls.Add(applicationTable);

			//Riga unica che contiene tutto
			TableRow applicationRow	= new TableRow();
			applicationTable.Controls.Add(applicationRow); //Aggiungo la riga

			//Aggiungo la cella che conterrà l'immagine
			applicationImageCell = new TableCell();
			applicationImageCell.Height				= 20;
			applicationImageCell.VerticalAlign		= VerticalAlign.Bottom;
			applicationImageCell.BackColor			= Color.FromArgb(175,189, 255);  
			applicationImageCell.HorizontalAlign	= HorizontalAlign.Center;
			
			if (applicationImage.Src== string.Empty)
				applicationImage.Src = ImagesHelper.CreateImageAndGetUrl("Application.gif", Helper.DefaultReferringType);
			
			//gli aggiungo l'immagine
			applicationImageCell.Controls.Add(applicationImage);			
			
			applicationRow.Cells.Add(applicationImageCell);			
			


			//Cella di destra
			TableCell rightCell	= new TableCell();
			rightCell.Height = Unit.Percentage(100);
			rightCell.Width = Unit.Percentage(100);

			applicationRow.Cells.Add(rightCell); //la aggiungo

			//Tabella che contiene la parte di destra
			Table rightInternalTable		= new Table();
			rightInternalTable.CellPadding	= 0;
			rightInternalTable.CellSpacing	= 0;
			rightInternalTable.Height		= Unit.Percentage(100);
			rightInternalTable.Width		= Unit.Percentage(100);
			rightCell.Controls.Add (rightInternalTable);	//Aggiungo la tabella alla cella di destra


			//1 riga che avrà lo sfongo come il menu  quindi è un datamenber perche lo devo poter gestire
			TableRow firstInternalRow	= new TableRow();
			//Cella in alto a destra che prende il colore di sfondo dell'intero controllo
			rigthTopCell = new TableCell();
			rigthTopCell.ColumnSpan = 2;
			rigthTopCell.Height = Unit.Percentage(50);
			rigthTopCell.Text			= "&nbsp;";
			firstInternalRow.Cells.Add(rigthTopCell);		//gli aggiungo una cella larga 2 che avrà il colore dello sfondo di tutto
			rightInternalTable.Rows.Add(firstInternalRow);	//la aggiungo alla tabella di destra
															//il controllo di default è bianco

			//Riga che conterrà la label del nome dell'applicazione
			TableRow applicationLabelRow		= new TableRow();
		//	applicationLabelRow.VerticalAlign	= VerticalAlign.Bottom;
			rightInternalTable.Rows.Add (applicationLabelRow);		//Aggiungo la prima riga alla tabella di destra
			
			//Cella che contiene la label del nome app e il campo idden
			TableCell applicationCell		= new TableCell();
			applicationCell.Width			= Unit.Percentage(100);
			applicationCell.VerticalAlign	= VerticalAlign.Bottom;
			applicationCell.BackColor		=  Color.FromArgb(175,189, 255);

			//Proprietà della label
			titleLable.Font.Bold = true;
			titleLable.Text		 = ApplicationTitleText;
			titleLable.ForeColor = Color.FromArgb(0, 0, 145);

			applicationCell.Controls.Add(titleLable);		//Aggiungo la label
			applicationCell.Controls.Add(stateText);		//Aggiungo il campo idden per il javaScript
			applicationLabelRow.Cells.Add(applicationCell);	//Aggiungo la cella alla riga
			applicationCell.Height = titleLable.Font.Size.Unit;

			//Cella per l'immagine cliccabile
			TableCell imageButtonCell		= new TableCell();
			imageButtonCell.VerticalAlign	= VerticalAlign.Middle;
			imageButtonCell.BackColor		= Color.FromArgb(175,189, 255);

			expandButtonImage.Src = ImagesHelper.CreateImageAndGetUrl("Collapse.GIF", Helper.DefaultReferringType); 
			ImageStateUrl =  expandButtonImage.Src;
			imageButtonCell.Controls.Add(expandButtonImage);		//Aggiungo l'immagine cliccabile alla riga
			applicationLabelRow.Cells.Add(imageButtonCell);			//Aggiungo la cella dell'immagine alla riga
	
		}
		//---------------------------------------------------------------------
		#endregion
	}
}
