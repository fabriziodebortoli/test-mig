using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.WebControls
{
	/// <summary>
	/// Classe che rappresenta il singolo gruppo (immagine, label)
	/// </summary>
	public class GroupLinkLabel : System.Web.UI.WebControls.Panel
	{
		#region Data Member protetti

		#region Oggetti
		/// <summary>
		/// HyperLink che rappresenta il nome del gruppo
		/// </summary>
		protected	HyperLink						groupLabelLink;
		/// <summary>
		/// Image che contiene l'immagine di gruppo
		/// </summary>
		protected	System.Web.UI.WebControls.Image	groupImage;
		#endregion

		#region Stringhe
		/// <summary>
		/// Stringa che rappresenta il NameSpace del gruppo
		/// </summary>
		protected	string		nameSpace				= "";
		#endregion

		#endregion

		#region Data Member privati
		/// <summary>
		/// Intero utilizzato per spaziare gli elementi
		/// </summary>
		private		Int32	appImageOffset = 4;
		/// <summary>
		/// Stringa che contiene il nome dell'applicazione alla qual appartiene
		/// il gruppo
		/// </summary>
		private		string	applicationName = string.Empty;
		#endregion

		#region Proprietà

		#region proprietà sulla LabelLinkText
		/// <summary>
		/// Set e Get sulla proprietà Text della LabelLink
		/// </summary>
		public string LabelLinkText
		{
			set
			{
				groupLabelLink.Text= value;
			}

			get
			{
				return groupLabelLink.Text;
			}
		}

		/// <summary>
		/// Set e Get sulla proprietà CssClass  della LabelLink
		/// </summary>
		public string LabelLinkCss
		{
			set
			{
				groupLabelLink.CssClass= value;
			}

			get
			{
				return groupLabelLink.CssClass;
			}
		}

		/// <summary>
		/// Set e Get sulla proprietà ForeColor della LabelLink
		/// </summary>
		public Color LabelLinkForeColor 
		{
			set
			{
				groupLabelLink.ForeColor = value;
			}

			get
			{
				return groupLabelLink.ForeColor;
			}
		}
		
		/// <summary>
		/// Set e Get sulla proprietà Font della LabelLink
		/// </summary>
		public FontInfo LabelLinkFontInfo
		{
			set
			{
				groupLabelLink.CssClass			= null;
				groupLabelLink.Font.Name		= value.Name;
				groupLabelLink.Font.Size		= value.Size;
				groupLabelLink.Font.Bold		= value.Bold;
				groupLabelLink.Font.Italic		= value.Italic;
				groupLabelLink.Font.Underline	= value.Underline;
			}

			get
			{
				return groupLabelLink.Font;
			}
		}

		/// <summary>
		/// Set e Get sulla proprietà Font.Name della LabelLink
		/// </summary>
		public string LabelLinkFontName
		{
			set
			{
				groupLabelLink.Font.Name = value;
			}
		}

		/// <summary>
		/// Get della collection Attributes della LabelLink
		/// </summary>
		public System.Web.UI.AttributeCollection LabelLinkAttributes
		{
			get
			{
				return groupLabelLink.Attributes;
			}
		}
		
		#endregion

		#region Proprietà sull'immagine
		/// <summary>
		/// Get e Set della proprietà ImageUrl dell'oggetto Image
		/// </summary>
		public string ImagePath
		{
			set
			{
				groupImage.ImageUrl = value;
			}
			get
			{
				return groupImage.ImageUrl;
			}
		}

		#endregion

		#region proprietà sui DataMember
		/// <summary>
		/// Get e Set sul Data Member nameSpace
		/// </summary>
		public string NameSpace
		{
			set
			{
				nameSpace = value;
			}

			get 
			{
				return nameSpace;
			}
		}
		//---------------------------------------------------------------------
		[
		Browsable( true ),
		DefaultValue( null )
		]
		public string ParentApplicationName
		{
			set
			{
				applicationName = value;
			}

			get 
			{
				return applicationName;
			}
		}

		//---------------------------------------------------------------------
		#endregion

		#endregion

		#region Costruttore
		/// <summary>
		/// Costruttore, istanzia i controlli e setta la proprietà Text della 
		/// LabelLink
		/// </summary>
		/// <param name="groupName"></param>
		/// <param name="applicationName"></param>
		public GroupLinkLabel(string groupName, string applicationName)
		{
			this.EnableViewState = true;
			this.applicationName = applicationName;
			groupLabelLink		 = new HyperLink();
			groupLabelLink.Text	 = groupName;
			groupImage			 = new System.Web.UI.WebControls.Image();
		}
		#endregion
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che aggiunge fisicamente i controlli Label e Image a una tabella
		/// (per allinearli meglio) che verrà a sua volta aggiunta alla collection 
		/// dei Controls
		/// </summary>
		protected override void CreateChildControls()
		{
		
			TableRow	groupRow		= new TableRow();
			TableCell	imageCell		= new TableCell();
			TableCell	hyperLinkCell	= new TableCell();

			groupImage.Attributes["hspace"]	= appImageOffset.ToString();
			imageCell.Controls.Add(groupImage);
			this.ID = this.NameSpace;
			groupLabelLink.Style["font-size"] = "12px";
			groupLabelLink.Style["height"] = "20px";
			groupLabelLink.Style["vertical-align"] = "text-bottom";

            string s = this.applicationName + @"\\" + this.nameSpace;
			groupLabelLink.NavigateUrl = "javascript:doLink('MenuArea.aspx?NameSpace=" + s.EncodeBase16() + "', this);";

			hyperLinkCell.Controls.Add(groupLabelLink);
			imageCell.Attributes.Add("align", "center");
			groupRow.Cells.Add(imageCell);
			groupRow.Cells.Add(hyperLinkCell);
			Controls.Add(groupRow);
		}
		//---------------------------------------------------------------------


	}
	//=========================================================================
}
