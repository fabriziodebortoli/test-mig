using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	public class ServerFormatter : IHttpHandler, IReadOnlySessionState
	{
		/// <summary>
		/// Process the request for the image
		/// </summary>
		/// <param name="context">The current HTTP context</param>
		//--------------------------------------------------------------------------------
		void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
		{
			int keyCode;
			int selStart = 0;
			int selEnd = 0;
			string handle = "";
			string docSessionID = "";
			string windowText = "";
			bool shift;
			bool ctrl;
			bool alt;
			bool keyIsChar;
			try
			{
				docSessionID = context.Request.Params["a"];
				handle = context.Request.Params["b"];

				if (!int.TryParse(context.Request.Params["c"], out keyCode))
					return;

				selStart = Int32.Parse(context.Request.Params["d"]);
				selEnd = Int32.Parse(context.Request.Params["e"]);
				windowText = context.Request.Params["f"];
				shift = bool.Parse(context.Request["g"]);
				ctrl = bool.Parse(context.Request["h"]);
				alt = bool.Parse(context.Request["i"]);
				keyIsChar = bool.Parse(context.Request["l"]);
				DocumentBag documentBag = context.Session[docSessionID] as DocumentBag;

				documentBag.ActionService.WebProxyObj_DoKey(documentBag.ProxyObjectId, handle, keyCode, shift, ctrl, alt, keyIsChar, ref selStart, ref selEnd, ref windowText);

				context.Response.Expires = 0;
				context.Response.Write(string.Format("{0};{1};{2}", selStart, selEnd, windowText));
			}
			catch
			{
			}
		}


		/// <summary>
		/// This handler can be reused, it doesn't need to be recycled
		/// </summary>
		//--------------------------------------------------------------------------------
		bool IHttpHandler.IsReusable
		{
			get { return false; }
		}
	}

	public class TbActionProvider : IHttpHandler, IReadOnlySessionState
	{
		/// <summary>
		/// Process the request for the image
		/// </summary>
		/// <param name="context">The current HTTP context</param>
		//--------------------------------------------------------------------------------
		void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
		{
			switch (context.Request.QueryString["a"])
			{
				case "1":
					DropDown(context, false);
					break;
				case "2":
					ContextMenuAction(context);
					break;
				case "3":
					SetDateFromCalendar(context);
					break;
				case "4":
					TreeAdvContextMenuAction(context);
					break;
				case "5":
					DropDown(context, true);
					break;

				default:
					Debug.Assert(false);
					break;
			}
		}

		/// <summary>
		/// Apre il menu di contesto sul server (TbLoader) tramite chiamata webmethod  
		/// e si fa restituire il contenuto (elenco di voci)
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void ContextMenuAction(HttpContext context)
		{
			string handle = "";
			string docSessionID = "";
			int cmd = 0;

			try
			{
				docSessionID = context.Request.QueryString["id"];
				handle = context.Request.QueryString["h"];
				int.TryParse(context.Request.QueryString["c"], out cmd);

				DocumentBag documentBag = context.Session[docSessionID] as DocumentBag;

				byte[] description = new byte[0];
				if (cmd > 0)
					documentBag.ActionService.WebProxyObj_OpenDropdownBtn(documentBag.ProxyObjectId, handle, cmd);
				else
					documentBag.ActionService.WebProxyObj_DoContextMenu(documentBag.ProxyObjectId, handle);

				bool ret = documentBag.ActionService.WebProxyObj_GetContextMenu(documentBag.ProxyObjectId, ref description);


				context.Response.Expires = 0;
				context.Response.ContentType = "text/html";
				if (ret)
				{
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					using (TextWriter writer = new StringWriter(sb))
					{
						using (HtmlTextWriter htmlWriter = new HtmlTextWriter(writer))
						{
							MenuDescription desc = new MenuDescription();
							desc.LoadBinary(description);
							TBContextMenu menu = new TBContextMenu(desc, "", false, desc.Id);
							menu.RenderControl(htmlWriter);
							htmlWriter.Flush();
						}
					}
					context.Response.Write(sb);
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Apre il menu di contesto sul server (TbLoader) del treeviewadv c# (Funziona diversamente rispetto agli altri menu di contesto perche
		/// creato lato c#) tramite chiamata webmethod  
		/// e si fa restituire il contenuto (elenco di voci)
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void TreeAdvContextMenuAction(HttpContext context)
		{
			string nodeKey = "";
			string treeId = "";
			string docSessionID = "";

			try
			{
				docSessionID = context.Request.QueryString["id"];
				treeId = context.Request.QueryString["ht"];
				nodeKey = context.Request.QueryString["hn"];

				DocumentBag documentBag = context.Session[docSessionID] as DocumentBag;

				byte[] description = new byte[0];
				//apro menu di contesto del treeViewAdv
				documentBag.ActionService.WebProxyObj_DoContextMenu(documentBag.ProxyObjectId, treeId);
				//recupero il menu di contesto del treeViewAdv
				bool ret = documentBag.ActionService.WebProxyObj_GetTreeViewAdvContextMenu(documentBag.ProxyObjectId,ref description,treeId);

				context.Response.Expires = 0;
				context.Response.ContentType = "text/html";
				if (ret)
				{
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					using (TextWriter writer = new StringWriter(sb))
					{
						using (HtmlTextWriter htmlWriter = new HtmlTextWriter(writer))
						{
							MenuDescription desc = new MenuDescription();
							desc.LoadBinary(description);
							TBContextMenu menu = new TBContextMenu(desc,"",false,desc.Id);
							menu.RenderControl(htmlWriter);
							htmlWriter.Flush();
						}
					}
					context.Response.Write(sb);
				}
			}
			catch
			{
			}


		}

		/// <summary>
		/// Apre la tendina della combo sul server(TbLoader) tramite chiamata webmethod  
		/// e si fa restituire il contenuto (elenco di voci)
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void DropDown(System.Web.HttpContext context, bool ms)
		{
			string handle = "";
			string docSessionID = "";

			try
			{
				docSessionID = context.Request.QueryString["id"];
				handle = context.Request.QueryString["h"];


				DocumentBag documentBag = context.Session[docSessionID] as DocumentBag;

				int selectedIndex;
				string[] items = documentBag.ActionService.WebProxyObj_GetComboItems(documentBag.ProxyObjectId, handle, out selectedIndex);

				context.Response.Expires = 0;
				context.Response.ContentType = "text/html";

				StringBuilder sb = new StringBuilder();

				if (ms)
				{
					// Drop Down multi selection CeckBox
					string idTxtBox = context.Request.QueryString["idTxtBox"];
					string txtBox = context.Request.QueryString["txtBox"];
					CreateMSDropdownList(selectedIndex, items, sb, documentBag.CurrentCulture, idTxtBox, txtBox);
				}
				else
				{
					//brutto if per differenziare il comportamento della combo su dispositivo Ipad-Iphone
					if (Helper.IsMacDevice(context.Request.UserAgent))
						CreateDropdownTable(items, sb, handle, documentBag.CurrentCulture);
					else
						CreateDropdownList(selectedIndex, items, sb, documentBag.CurrentCulture);
				}

				context.Response.Write(sb);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Crea il codice html della tendina della combo (usando oggetto html select-option)
		/// Usato per tutti i browser tranne per safari su ipad/iphone
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void CreateDropdownList(int selectedIndex, string[] items, StringBuilder sb, CultureInfo currentCulture)
		{
			using (TextWriter writer = new StringWriter(sb))
			{
				using (HtmlTextWriter htmlWriter = new HtmlTextWriter(writer))
				{
					ListBox list = new ListBox();
					list.CssClass = "TBDropDownListContent";
					list.ID = "DDContent";
					int idx = -1;
					if (items.Length > 0)
					{
						foreach (string item in items)
						{
							ListItem li = new ListItem(item);
							li.Selected = ++idx == selectedIndex;
							list.Items.Add(li);
						}
						//Imposto il numero di righe uguali al numero di elementi (se ho un solo elemento 
						//imposto numero di righe 2 altrimenti i browser visualizzano le list con una riga 
						//con l'aspetto di una combo).
						//Se gli lementi eccedono le 20 righe imposto 20 per evitare combo troppo lunghe
						list.Rows = Math.Min(20,Math.Max(items.Length,2));
					}
					else
					{	//Se non ci sono elementi inserisce una stringa <no items> per comunicarlo
						//all'utente
						Thread.CurrentThread.CurrentUICulture = currentCulture;
						ListItem li = new ListItem(TBWebFormControlStrings.NoItems);
						list.Items.Add(li);
						//imposto numero di righe 2 altrimenti i browser visualizzano le list con una riga 
						//con l'aspetto di una combo
						list.Rows = 2;
					}
					list.Attributes["onclick"] = "tbSelectedItem(this)";
					list.Attributes["onkeydown"] = "tbDropdownKeydown(event, this)";
					list.Attributes["onblur"] = "closePopupPanel()";

					list.RenderControl(htmlWriter);
					htmlWriter.Flush();
				}
			}
		}

		/// <summary>
		/// Crea il codice html della tendina della combo multi selection (usando oggetto html select-option)
		/// Usato per tutti i browser
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void CreateMSDropdownList(int selectedIndex, string[] items, StringBuilder sb, CultureInfo currentCulture, string idTxtBox, String txtBox)
		{
			//CreateDropdownList(selectedIndex,items,  sb, currentCulture);

			using (TextWriter writer = new StringWriter(sb))
			{
				using (HtmlTextWriter htmlWriter = new HtmlTextWriter(writer))
				{
					CheckBoxList list = new CheckBoxList();
					list.CssClass = "TBMSDropDownListContent";					
					list.ID = "DDMSContent";
					//int idx = -1;
					if (items.Length > 0)
					{
						foreach (string item in items)
						{
							ListItem li = new ListItem(item);
							//li.Selected = ++idx == selectedIndex;
							String ke = li.Text.Trim().Split()[0];
							int start = txtBox.IndexOf(ke);
							if (start >= 0)
							{
								string stCmp = txtBox.Substring(start);
								int end = txtBox.IndexOf(";", start);
								if (end > start)
									stCmp = txtBox.Substring(start, end - start);
								// check, uncheck items
							    li.Selected = (stCmp == ke);
							}
							li.Attributes["onclick"] = "tbMSSelectedItem('" + idTxtBox + "','" + ke + "')";
							list.Items.Add(li);
							
						}
					}
					else
					{	//Se non ci sono elementi inserisce una stringa <no items> per comunicarlo
						//all'utente
						Thread.CurrentThread.CurrentUICulture = currentCulture;
						ListItem li = new ListItem(TBWebFormControlStrings.NoItems);
						list.Items.Add(li);
					}

					// list.Attributes["onkeydown"] = "tbMSDropdownKeydown(event, this)";
					// popupPanel.Attributes["onblur"] = "closePopupPanelMS('"+ idTxtBox + "')";
					
					Panel popupPanel = new Panel();
					popupPanel.TabIndex = -1;
					popupPanel.CssClass = "TBCssPanel";
					popupPanel.ID = "MsPopupPanel";

					popupPanel.Controls.Add(list);
					popupPanel.Height = 200;
					
					popupPanel.ScrollBars = ScrollBars.Vertical;
					popupPanel.BorderWidth = 1;
					popupPanel.BackColor = System.Drawing.Color.White;
					popupPanel.RenderControl(htmlWriter);
					htmlWriter.Flush();
				}
			}
		}

		/// <summary>
		/// Crea il codice html della tendina della combo (usando oggetto html table)
		/// Usato per tutti per safari su ipad/iphone (quasta combo non ha tutte le funzionalita di utilizzo da tastiera come quella 
		/// qui sopra)
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void CreateDropdownTable(string[] items, StringBuilder sb, string handleCombo, CultureInfo currentCulture)
		{
			using (TextWriter writer = new StringWriter(sb))
			{
				using (HtmlTextWriter htmlWriter = new HtmlTextWriter(writer))
				{
					Table listTable = new Table();
					listTable.CssClass = "TBDropDownTableContent";
					listTable.ID = "DDContent";
					int idx = -1;
					if (items.Length > 0)
					{
						foreach (string item in items)
						{
							TableRow tr = new TableRow();
							listTable.Rows.Add(tr);
							TableCell td = new TableCell();
							tr.Cells.Add(td);
							td.Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
							td.Text = item;
							idx++;
							td.Attributes["onclick"] = string.Format("tbDropDownTableSelectedItem('{0}', {1})", handleCombo, idx);
						}
					}
					else
					{	//Se non ci sono elementi inserisce una stringa <no items> per comunicarlo
						//all'utente
						TableRow tr = new TableRow();
						listTable.Rows.Add(tr);
						TableCell td = new TableCell();
						tr.Cells.Add(td);
						Thread.CurrentThread.CurrentUICulture = currentCulture;
						td.Text = TBWebFormControlStrings.NoItems;
					}
					listTable.Attributes["onblur"] = "closePopupPanel()";

					listTable.RenderControl(htmlWriter);
					htmlWriter.Flush();
				}
			}
		}


		/// <summary>
		/// Imposta la data selezionata dal calendario ajax al ParsedControl associato (tramite chiamata
		/// a webmethod) e si fa restituire la data in formato stringa (formattata secondo il formattatore c++ associato)
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void SetDateFromCalendar(HttpContext context)
		{
			string handle = "";
			string docSessionID = "";
			string dateText = "";
			int month;
			int day;
			int year;

			try
			{
				//parso i parametri della query string (vedere costruzione nel file 
				//TBWebFormControl\TBWebFormClientControl.js function formatCalendarDate)
				docSessionID = context.Request.QueryString["id"];
				handle = context.Request.QueryString["h"];
				dateText = context.Request.QueryString["d"];

				//Il formato della stringa che ritorna il calendario ajax e' MM/dd/yyyy
				//vedere la function formatCalendarDate in TBWebFormClientControl.js
				int.TryParse(dateText.Substring(0, 2), out month);
				int.TryParse(dateText.Substring(3, 2), out day);
				int.TryParse(dateText.Substring(6, 4), out year);

				DocumentBag documentBag = context.Session[docSessionID] as DocumentBag;

				//chiamo il webmethod che fa impostare la data a Tbloader sul parsedControl e mi ritorna la data formattata
				string formattedDate = documentBag.ActionService.WebProxyObj_SetDate(documentBag.ProxyObjectId, handle, day, month, year);

				//genero la risposta da mandare sul client
				context.Response.Expires = 0;
				context.Response.ContentType = "text/html";
				context.Response.Write(formattedDate);
			}
			catch
			{
			}
		}

		/// <summary>
		/// This handler can be reused, it doesn't need to be recycled
		/// </summary>
		//--------------------------------------------------------------------------------
		bool IHttpHandler.IsReusable
		{
			get { return false; }
		}
	}
}

