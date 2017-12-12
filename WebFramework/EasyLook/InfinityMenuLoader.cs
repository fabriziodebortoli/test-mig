using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Web.EasyLook
{
	# region SPVMNMenu elements
	//=========================================================================
	public sealed class VMNMenuXML
	{
		//-----------------------------------------------------------------
		private VMNMenuXML() { }

		//================================================================================
		public sealed class Element
		{
			//-----------------------------------------------------------------
			private Element()
			{ }

			public const string SPVMNMenu = "SPVMNMenu";
			public const string MenuItem = "MenuItem";
			public const string Caption = "caption";
			public const string Icon = "icon";
			public const string Link = "link";
			public const string Target = "target";
			public const string Uid = "uid";
			public const string Elements = "elements";
		}
	}
	# endregion

	//================================================================================
	public class InfinityMenuLoader
	{
		private XmlDocument doc = null;
		private Random rndCPCCCHK = new Random();
		private Dictionary<string, string> menuItemsDict = new Dictionary<string, string>();
		private string baseUrl = null;

		private string menu = string.Empty;
		private static Dictionary<string, InfinityMenuLoader> menuDict = new Dictionary<string, InfinityMenuLoader>();
		private DateTime lastAccess = DateTime.Now;

		// Properties
		//--------------------------------------------------------------------------------
		private string BaseUrl
		{
			get
			{
				if (baseUrl == null)
				{
					baseUrl = HttpContext.Current.Request.Url.ToString();
					int idx = baseUrl.LastIndexOf(Path.AltDirectorySeparatorChar) + 1;
					if (idx != 0)
						baseUrl = baseUrl.Substring(0, idx); 
				}
				return baseUrl;
			}
		}
	

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public InfinityMenuLoader()
		{
		}

		///<summary>
		/// Entry-point per il caricamento del menu di Mago
		/// usata per le applicazioni di esempio come la GettingStarted
		///</summary>
		//--------------------------------------------------------------------------------
		public static string GetInfinityMenuDescription(string company, string user)
		{
			lock (menuDict)
			{
				try
				{
					string currentSessionInfo = string.Concat(company, "###", user);

					InfinityMenuLoader menuLoader;

					if (!menuDict.TryGetValue(currentSessionInfo, out menuLoader))
					{
						// istanzio il menuLoader ed eseguo il caricamento del menu
						menuLoader = new InfinityMenuLoader();
						menuLoader.GetCompleteMenu(company, user);

						// il sessionid potrebbe essere empty qualora il processo di sign-on non fosse completato con successo
						if (!string.IsNullOrWhiteSpace(currentSessionInfo))
						{
							menuDict.Add(currentSessionInfo, menuLoader); // aggiungo il sessionid solo se diverso da empty!
							System.Diagnostics.Debug.WriteLine("### Add item with date " + menuLoader.lastAccess + " for session: " + currentSessionInfo);
						}
						else
							System.Diagnostics.Debug.WriteLine("Session empty!");
					}
					else
					{
						menuLoader.lastAccess = DateTime.Now;

						System.Diagnostics.Debug.WriteLine("### Get existing item with date " + menuLoader.lastAccess + " for session: " + currentSessionInfo);
					}

					RemoveExpiredMenus();

					return menuLoader.menu;
				}
				catch
				{
					return "<SPVMNMenu></SPVMNMenu>";
				}
			}
		}

		///<summary>
		/// Cicla sulla lista dei menuLoader e rimuove quelli che sono stati letti 
		/// da piu' di 20 minuti
		///</summary>
		//--------------------------------------------------------------------------------
		private static void RemoveExpiredMenus()
		{
			lock (menuDict)
			{
				List<string> itemsToRemove = new List<string>();

				foreach (KeyValuePair<string, InfinityMenuLoader> pair in menuDict)
				{
					// sottraggo le due date e analizzo i minuti
					if (DateTime.Now.Subtract(pair.Value.lastAccess).Minutes > 20)
					{
						System.Diagnostics.Debug.WriteLine("Session " + pair.Key + " is expired! It will be removed from dictionary.");
						itemsToRemove.Add(pair.Key);
					}
				}

				foreach (string item in itemsToRemove)
					menuDict.Remove(item);
			}
		}

		///<summary>
		/// Entry-point per il caricamento del menu di Mago (con load on demand per codice cpccchk)
		///</summary>
		//--------------------------------------------------------------------------------
		public static string GetInfinityMenuDescription(string company, string user, string menuFile, string start_uid, int depth)
		{
			try
			{
				// start_uid arriva vuoto o con spazio (a seconda di chi mi chiama)
				start_uid = start_uid.Trim();

				// skippo subito i menu di Infinity (per evitare multipli caricamenti dei menu di Mago)
				if (start_uid == "" && menuFile.IndexOf("private") < 0)
					return "<SPVMNMenu></SPVMNMenu>";

				// leggo dalla session il menu
				InfinityMenuLoader menuLoader = HttpContext.Current.Session["InfinityMenuLoader"] as InfinityMenuLoader;

				if (menuLoader == null)
				{
					menuLoader = new InfinityMenuLoader();
					HttpContext.Current.Session["InfinityMenuLoader"] = menuLoader;
					menuLoader.LoadMagoMenu(company, user);
				}

				return menuLoader.GetMenu(start_uid, depth);
			}
			catch
			{
				return "<SPVMNMenu></SPVMNMenu>";
			}
		}

		///<summary>
		/// Caricamento del menu di Mago
		///</summary>
		//--------------------------------------------------------------------------------
		internal void LoadMagoMenu(string company, string user)
		{
			// carico il menu di Mago dal file xml lato server
			MenuXmlParser parserDomMenu = Helper.GetMenuXmlParser();
			if (parserDomMenu == null)
			{
				PathFinder pf = new PathFinder(company, user);
				LoginManager lm = new LoginManager();
				parserDomMenu = Helper.LoadMenuXmlParser(lm, pf);
			}

			if (doc == null)
			{
				doc = new XmlDocument();
				XmlDeclaration xDec = doc.CreateXmlDeclaration("1.0", "UTF-8", null); //<?xml version="1.0" encoding="UTF-8"?>
				doc.AppendChild(xDec);
				doc.AppendChild(doc.CreateElement(VMNMenuXML.Element.SPVMNMenu));
			}

			StringBuilder appsSb = new StringBuilder();

			// scorro il file xml partendo dalle applicazioni
			foreach (IMenuXmlNode applicationNode in parserDomMenu.Root.ApplicationsItems)
			{
				string appGuid = CreateRandomCPCCCHK();
				appsSb.Append(CreateElement(doc, (MenuXmlNode)applicationNode, appGuid));

				if (applicationNode.GroupItems == null)
					continue;

				// scorro tutti i gruppi
				StringBuilder groupSb = new StringBuilder();
				foreach (IMenuXmlNode groupNode in applicationNode.GroupItems)
				{
					string groupGuid = CreateRandomCPCCCHK();
					groupSb.Append(CreateElement(doc, (MenuXmlNode)groupNode, groupGuid));

					// se il gruppo contiene dei menu vado in ricorsione
					if (((MenuXmlNode)groupNode).MenuItems == null)
						continue;
					StringBuilder menuSb = Recurse(doc, (MenuXmlNode)groupNode);

					AddMenuItem(groupGuid, menuSb);
				}

				AddMenuItem(appGuid, groupSb);
			}

			// il nodo root contiene le sole applicazioni e i gruppi (il resto viene caricato on-demand)
			AddMenuItem("", appsSb);
		}

		///<summary>
		/// Funzione ricorsiva per la lettura dei vari nodi del menu
		///</summary>
		//--------------------------------------------------------------------------------
		private StringBuilder Recurse(XmlDocument doc, MenuXmlNode aNode)
		{
			StringBuilder menuSB = new StringBuilder();
			if (aNode.MenuItems != null)
			{
				foreach (MenuXmlNode node in aNode.MenuItems)
				{
					string guid = CreateRandomCPCCCHK();
					menuSB.Append(CreateElement(doc, node, guid));
					AddMenuItem(guid, Recurse(doc, node));
				}
			}

			if (aNode.CommandItems != null)
			{
				foreach (MenuXmlNode item in aNode.CommandItems)
				{
					if (!item.IsRunReport && !item.IsRunBatch && !item.IsRunDocument && !item.IsRunFunction)
						continue;
					menuSB.Append(CreateElement(doc, item, CreateRandomCPCCCHK()));
				}
			}

			return menuSB;
		}

		///<summary>
		/// Crea un elemento di tipo MenuItem con la sintassi prevista da SitePainter
		/// I nodi del menu di Mago sono di questi tipi:
		/// - Application
		///		- Group
		///			- Menu
		///				- Document / Report / ...
		///</summary>
		//--------------------------------------------------------------------------------
		private string CreateElement(XmlDocument doc, MenuXmlNode currentNode, string cpccchk)
		{
			XmlElement myElement = doc.CreateElement(VMNMenuXML.Element.MenuItem);

			myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Caption)).InnerText = currentNode.Title;
			myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Icon)).InnerText =
				(currentNode.IsApplication || currentNode.IsGroup || currentNode.IsMenu) ? "folder" : "master";

			if (currentNode.IsRunReport || currentNode.IsRunBatch || currentNode.IsRunDocument || currentNode.IsRunFunction)
			{
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Link)).InnerText = GetCommandLink(currentNode);
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Target)).InnerText = "_blank"; // apre il doc in un popup //"main";
			}
			else
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Link)).InnerText = "";

			myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Uid)).InnerText = cpccchk;

			if (currentNode.IsApplication)
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Elements)).InnerText = currentNode.HasChildNodes ? currentNode.GroupItems.Count.ToString() : "0";
			else if (currentNode.CommandItems != null)
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Elements)).InnerText = currentNode.CommandItems.Count.ToString();
			else
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Elements)).InnerText = currentNode.HasMenuChildNodes() ? currentNode.MenuItems.Count.ToString() : "0";

			return myElement.OuterXml;
		}

		///<summary>
		/// Calcola il link del comando di EasyLook da eseguire
		///</summary>
		//--------------------------------------------------------------------------------
		private string GetCommandLink(MenuXmlNode commandNode)
		{
			string cmd = string.Empty;

			if (commandNode.IsRunReport)
				cmd = string.Concat(BaseUrl, "WoormWebForm.aspx?namespace=", HttpUtility.UrlEncode(commandNode.ItemObject));
			else if (commandNode.IsRunFunction)
				cmd = string.Concat(BaseUrl, "TBWindowForm.aspx?ObjectNamespace=", NameSpaceSegment.Function, ".", HttpUtility.UrlEncode(commandNode.ItemObject));
			else
				cmd = string.Concat(BaseUrl, "TBWindowForm.aspx?ObjectNamespace=", NameSpaceSegment.Document, ".", HttpUtility.UrlEncode(commandNode.ItemObject));

			return cmd;// +"&target=dialogwindow";
			//return string.Concat("http://localhost:8090/GettingStarted/easylookdocumenthost.jsp?url=", HttpUtility.UrlEncode(cmd));
		}

		///<summary>
		/// Metodo che ritorna la porzione di menu identificata da uno specifico CPCCCHK
		///</summary>
		//--------------------------------------------------------------------------------
		public string GetMenu(string start_uid, int depth)
		{
			string menuItem = GetMenuItem(start_uid, depth);
			if (menuItem.IndexOf("SPVMNMenu>") > 0)
				return menuItem;

			return string.Concat("<SPVMNMenu>", GetMenuItem(start_uid, depth), "</SPVMNMenu>");
		}

		///<summary>
		/// Crea un codice alfabetico random di dieci lettere (analogo al valore CPCCCHK di Infinity)
		///</summary>
		//--------------------------------------------------------------------------------
		private string CreateRandomCPCCCHK()
		{
			char[] arrayOfChar = new char[10];
			for (int i = 0; i < 10; i++)
				arrayOfChar[i] = (char)(int)(rndCPCCCHK.NextDouble() * 26.0D + 97.0D);
			return new String(arrayOfChar).ToUpperInvariant();
		}

		///<summary>
		/// Inserisce un cpccchk dentro il dictionary globale
		///</summary>
		//--------------------------------------------------------------------------------
		private void AddMenuItem(string cpccchk, StringBuilder sb)
		{
			string menuItem = string.Empty;
			if (!menuItemsDict.TryGetValue(cpccchk, out menuItem))
				menuItemsDict.Add(cpccchk, sb.ToString());
		}

		///<summary>
		/// Ritorna il valore associato ad un cpccchk
		///</summary>
		//--------------------------------------------------------------------------------
		private string GetMenuItem(string cpccchk, int depth)
		{
			string menuItem = string.Empty;
			if (menuItemsDict.TryGetValue(cpccchk, out menuItem))
			{
				// se depth > 1 procedo ad analizzare i nodi figli (visualizzazione mappa del sito)
				if (depth > 1)
					return AddChilds(string.Concat("<SPVMNMenu>", menuItem, "</SPVMNMenu>"), depth);

				return menuItem;
			}
			return string.Empty;
		}

		///<summary>
		/// Nel caso di profondita' di caricamento del menu superiore a 1 
		/// vengono esplorati ed aggiunti in cascata i livelli inferiori, se presenti
		///</summary>
		//--------------------------------------------------------------------------------
		private string AddChilds(string parentMenuItem, int depth)
		{
			XmlDocument dom = new XmlDocument();

			try
			{
				// carico in un dom la porzione di xml del menuitem
				// attenzione che la stringa contiene anche il nodo root <SPVMNMenu> per evitare che la Load si arrabbi
				dom.LoadXml(parentMenuItem);

				// considero tutti i nodi <MenuItem>
				// per ognuno individuo il child <uid> e vado ad esplorare il suo contenuto, aggiungendolo in coda
				foreach (XmlNode itemNode in dom.SelectNodes("//MenuItem"))
				{
					string menuItem = GetMenuItem(itemNode.SelectSingleNode(VMNMenuXML.Element.Uid).InnerText, depth - 1);

					if (!string.IsNullOrWhiteSpace(menuItem))
						CreateFragment(dom, menuItem, itemNode);
				}
			}
			catch (Exception)
			{
				throw;
			}

			return dom.OuterXml;
		}

		///<summary>
		/// Crea un XmlDocumentFragment che viene aggiunto all'XmlNode padre
		///</summary>
		//--------------------------------------------------------------------------------
		private void CreateFragment(XmlDocument dom, string menuItem, XmlNode itemNode)
		{
			try
			{
				//Create a document fragment.
				XmlDocumentFragment fragment = dom.CreateDocumentFragment();
				//Set the contents of the document fragment.
				fragment.InnerXml = menuItem;
				//Add the children of the document fragment to the node.
				itemNode.AppendChild(fragment);
			}
			catch (Exception)
			{
				throw;
			}
		}

		//--------------------------------------------------------------------------------
		internal void GetCompleteMenu(string company, string user)
		{
			MenuXmlParser parserDomMenu = Helper.GetMenuXmlParser();
			if (parserDomMenu == null)
			{
				PathFinder pf = new PathFinder(company, user);
				LoginManager lm = new LoginManager();
				parserDomMenu = Helper.LoadMenuXmlParser(lm, pf);
			}

			if (doc == null)
			{
				doc = new XmlDocument();
				XmlDeclaration xDec = doc.CreateXmlDeclaration("1.0", "UTF-8", null); //<?xml version="1.0" encoding="UTF-8"?>
				doc.AppendChild(xDec);
				doc.AppendChild(doc.CreateElement(VMNMenuXML.Element.SPVMNMenu));
			}

			foreach (IMenuXmlNode applicationNode in parserDomMenu.Root.ApplicationsItems)
			{
				// per non caricare le applicazioni e saltare il primo livello
				//XmlElement appEl = CreateElement(doc, (MenuXmlNode)applicationNode, doc.DocumentElement);

				if (applicationNode.GroupItems == null)
					continue;

				foreach (IMenuXmlNode groupNode in applicationNode.GroupItems)
				{
					// per non caricare le applicazioni e saltare il primo livello
					//XmlElement groupEl = CreateElement(doc, (MenuXmlNode)groupNode, appEl);

					XmlElement groupEl = CreateElement(doc, (MenuXmlNode)groupNode, doc.DocumentElement);

					if (((MenuXmlNode)groupNode).IsGroup)
					{
						// calcolo il path per impostare l'immagine del gruppo
						string[] token = ((MenuXmlNode)groupNode).GetGroupName().Split('.');
						string groupSrc = string.Empty;
						if (token.Length >= 2 && string.Compare(token[1], "UserReportsGroup") == 0)
							groupSrc = ImagesHelper.CreateImageAndGetUrl("DefaultUserReportsGroup.gif", TaskBuilderNet.UI.WebControls.Helper.DefaultReferringType);
						else
						{
							string imagePath = parserDomMenu.GetGroupImageFileName(applicationNode.GetApplicationName(), groupNode.GetGroupName());
							groupSrc = ImagesHelper.CopyImageFileIfNeeded(imagePath);
							if (string.IsNullOrEmpty(groupSrc))
								groupSrc = ImagesHelper.CreateImageAndGetUrl("DefaultGroupImgSmall.gif", TaskBuilderNet.UI.WebControls.Helper.DefaultReferringType);
						}

						// prova di inserimento del path per le icone (directory Microarea\Images\*.gif)
						groupEl.SelectSingleNode(VMNMenuXML.Element.Icon).InnerText = string.Format("/Microarea/images/{0}", Path.GetFileName(groupSrc));
					}
					
					if (((MenuXmlNode)groupNode).MenuItems != null)
						LoadMenuItemRecursive((MenuXmlNode)groupNode, groupEl);
				}
			}

			menu = doc.InnerXml;
		}

		#region Metodi per la generazione di una pagina HTML con le voci di Menu
		///<summary>
		/// Ritorna una stringa contenente tutto il menu di Mago formattato secondo la sintassi di Infinity
		///</summary>
		//--------------------------------------------------------------------------------
		internal string GetCompleteHtmlMenu(
			string company,
			string user,
			IPathFinder pathFinder,
			LoginManager loginManager
			)
		{
			MenuXmlParser parserDomMenu = Helper.GetMenuXmlParser();
			if (parserDomMenu == null)
				parserDomMenu = Helper.LoadMenuXmlParser(loginManager, pathFinder);

			StringWriter outputHtml = new StringWriter();
			HtmlTextWriter writer = new HtmlTextWriter(outputHtml);

			writer.RenderBeginTag(HtmlTextWriterTag.Html);

			writer.RenderBeginTag(HtmlTextWriterTag.Head);
				writer.RenderBeginTag(HtmlTextWriterTag.Title);
					writer.Write("Mago.net Infinity menu");
				writer.RenderEndTag(); // TITLE

				// includo un foglio di stile CSS
				writer.AddAttribute(HtmlTextWriterAttribute.Href, "TreeMenu.css");
				writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
				writer.RenderBeginTag(HtmlTextWriterTag.Link);
				writer.RenderEndTag(); // LINK

				writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
				writer.AddAttribute(HtmlTextWriterAttribute.Src, "TreeMenu.js");
				writer.RenderBeginTag(HtmlTextWriterTag.Script);
				writer.RenderEndTag(); // SCRIPT

			writer.RenderEndTag(); // HEAD

			writer.RenderBeginTag(HtmlTextWriterTag.Body);

			writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
			writer.RenderBeginTag(HtmlTextWriterTag.Script);
			writer.Write(@"
							window.onload = function() 
							{  var fwMenu = new TreeMenu('Framework'); 
								fwMenu.click('Framework-0');
								var erpMenu = new TreeMenu('ERP'); 
								erpMenu.click('ERP-0');
							}");
			writer.RenderEndTag(); // SCRIPT

			writer.RenderBeginTag(HtmlTextWriterTag.Table);
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			foreach (IMenuXmlNode applicationNode in parserDomMenu.Root.ApplicationsItems)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
				writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
				writer.RenderBeginTag(HtmlTextWriterTag.Td);

				writer.AddAttribute(HtmlTextWriterAttribute.Id, applicationNode.GetApplicationName());
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "tree-menu");
				writer.RenderBeginTag(HtmlTextWriterTag.Ul);

				writer.RenderBeginTag(HtmlTextWriterTag.Li);
				writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
				writer.RenderBeginTag(HtmlTextWriterTag.A);

				// calcolo il path dell'immagine da caricare per l'applicazione (con salvataggio nella temp se necessario)
				string appSrc = ImagesHelper.CopyImageFileIfNeeded(parserDomMenu.GetApplicationImageFileName(applicationNode.GetNameAttribute()));
				writer.AddAttribute(HtmlTextWriterAttribute.Src, appSrc);
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();

				writer.Write(applicationNode.Title);
				writer.RenderEndTag(); //A

				if (applicationNode.GroupItems == null)
					continue;

				writer.RenderBeginTag(HtmlTextWriterTag.Ul);

				foreach (IMenuXmlNode groupNode in applicationNode.GroupItems)
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Li);
					writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
					writer.RenderBeginTag(HtmlTextWriterTag.A);

					// calcolo il path per impostare l'immagine del gruppo
					string[] token = groupNode.GetGroupName().Split('.');
					string groupSrc = string.Empty;
					if (token.Length >= 2 && string.Compare(token[1], "UserReportsGroup") == 0)
						groupSrc = ImagesHelper.CreateImageAndGetUrl("DefaultUserReportsGroup.gif", TaskBuilderNet.UI.WebControls.Helper.DefaultReferringType);
					else
					{
						string imagePath = parserDomMenu.GetGroupImageFileName(applicationNode.GetApplicationName(), groupNode.GetGroupName());
						groupSrc = ImagesHelper.CopyImageFileIfNeeded(imagePath);
						if (string.IsNullOrEmpty(groupSrc))
							groupSrc = ImagesHelper.CreateImageAndGetUrl("DefaultGroupImgSmall.gif", TaskBuilderNet.UI.WebControls.Helper.DefaultReferringType);
					}
					writer.AddAttribute(HtmlTextWriterAttribute.Src, groupSrc);
					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();

					writer.Write(groupNode.Title);

					writer.RenderEndTag(); //A

					if (((MenuXmlNode)groupNode).MenuItems != null)
						LoadHtmlMenuItemRecursive((MenuXmlNode)groupNode, writer);

					writer.RenderEndTag(); //LI
				}

				writer.RenderEndTag(); //UL
				writer.RenderEndTag(); //UL
				writer.RenderEndTag(); //TD
			}
			
			writer.RenderEndTag(); // TR
			writer.RenderEndTag(); // TABLE

			writer.RenderEndTag(); // BODY
			writer.RenderEndTag(); // HTML

			return outputHtml.GetStringBuilder().ToString();
		}

		///<summary>
		/// Funzione ricorsiva per la lettura dei vari nodi del menu
		///</summary>
		//--------------------------------------------------------------------------------
		private void LoadHtmlMenuItemRecursive(MenuXmlNode aNode, HtmlTextWriter writer)
		{
			if (aNode.MenuItems != null)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Ul);

				foreach (MenuXmlNode node in aNode.MenuItems)
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Li);

					writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:void(0)");
					writer.RenderBeginTag(HtmlTextWriterTag.A);

					writer.AddAttribute(HtmlTextWriterAttribute.Src, Helper.GetImageUrl("Menu.GIF"));
					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();

					writer.Write(node.Title);

					writer.RenderEndTag(); //A

					if (node.HasMenuChildNodes())
						LoadHtmlMenuItemRecursive(node, writer);

					if (node.CommandItems != null)
					{
						writer.RenderBeginTag(HtmlTextWriterTag.Ul);

						foreach (MenuXmlNode item in node.CommandItems)
						{
							if (!item.IsRunReport && !item.IsRunBatch && !item.IsRunDocument && !item.IsRunFunction)
								continue;

							writer.RenderBeginTag(HtmlTextWriterTag.Li);
							CreateHtmlLeafItem(item, writer);
							writer.RenderEndTag(); //LI
						}
						writer.RenderEndTag(); // UL
					}
					writer.RenderEndTag(); //LI
				}
				writer.RenderEndTag(); // UL
			}
		}

		///<summary>
		/// Crea un elemento di tipo MenuItem con la sintassi prevista da SitePainter
		/// I nodi del menu di Mago sono di questi tipi:
		/// - Application
		///		- Group
		///			- Menu
		///				- Document / Report / ...
		///</summary>
		//--------------------------------------------------------------------------------
		private void CreateHtmlLeafItem(MenuXmlNode currentNode, HtmlTextWriter writer)
		{
			if (currentNode.IsRunReport || currentNode.IsRunBatch || currentNode.IsRunDocument || currentNode.IsRunFunction)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Href, GetCommandLink(currentNode));
				writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank"); // apriamo un popup

				writer.RenderBeginTag(HtmlTextWriterTag.A);

				if (currentNode.IsRunReport)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Src, PathImageStrings.ReportImagePath);
					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();
				}
				else if (currentNode.IsRunBatch)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Src, PathImageStrings.BatchImagePath);
					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();
				}
				else if (currentNode.IsRunDocument)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Src, PathImageStrings.DocumentImagePath);
					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();
				}
				else if (currentNode.IsRunFunction)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Src, PathImageStrings.FunctionImagePath);
					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();
				}

				writer.Write(currentNode.Title);
				writer.RenderEndTag();
			}
			else
				writer.Write(currentNode.Title);
		}
		# endregion

		# region Metodo per il caricamento di tutto il menu (utilizzato dalla GettingStarted)
		///<summary>
		/// Funzione ricorsiva per la lettura dei vari nodi del menu
		///</summary>
		//--------------------------------------------------------------------------------
		private void LoadMenuItemRecursive(MenuXmlNode aNode, XmlElement ownerElement)
		{
			if (aNode.MenuItems != null)
			{
				foreach (MenuXmlNode node in aNode.MenuItems)
				{
					XmlElement docEl = CreateElement(ownerElement.OwnerDocument, node, ownerElement);
					if (node.HasMenuChildNodes())
						LoadMenuItemRecursive(node, docEl);

					if (node.CommandItems != null)
					{
						foreach (MenuXmlNode item in node.CommandItems)
						{
							if (!item.IsRunReport && !item.IsRunBatch && !item.IsRunDocument && !item.IsRunFunction)
								continue;
							CreateElement(doc, item, docEl);
						}
					}
				}
			}
		}

		///<summary>
		/// Crea un elemento di tipo MenuItem con la sintassi prevista da SitePainter
		/// I nodi del menu di Mago sono di questi tipi:
		/// - Application
		///		- Group
		///			- Menu
		///				- Document / Report / ...
		///</summary>
		//--------------------------------------------------------------------------------
		private XmlElement CreateElement(XmlDocument doc, MenuXmlNode currentNode, XmlElement ownerElement)
		{
			XmlElement myElement = doc.CreateElement(VMNMenuXML.Element.MenuItem);
			ownerElement.AppendChild(myElement);

			myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Caption)).InnerText = currentNode.Title;

			if (currentNode.IsApplication || currentNode.IsGroup || currentNode.IsMenu)
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Icon)).InnerText = "folder";
			else
			{
				if (currentNode.IsRunReport)
					myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Icon)).InnerText = string.Format("http://localhost:8090/MagoInfinity2/Microarea/images/{0}", "RunReport.GIF");
				else if (currentNode.IsRunBatch)
					myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Icon)).InnerText = string.Format("http://localhost:8090/MagoInfinity2/Microarea/images/{0}", "RunBatch.GIF");
				else if (currentNode.IsRunDocument)
					myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Icon)).InnerText = string.Format("http://localhost:8090/MagoInfinity2/Microarea/images/{0}", "RunDocument.GIF");
				else if (currentNode.IsRunFunction)
					myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Icon)).InnerText = string.Format("http://localhost:8090/MagoInfinity2/Microarea/images/{0}", "RunFunction.GIF");
			}
				
			if (currentNode.IsRunReport || currentNode.IsRunBatch || currentNode.IsRunDocument || currentNode.IsRunFunction)
			{
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Link)).InnerText = GetCommandLink(currentNode);
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Target)).InnerText = "_blank"; // apre il doc in un popup //"main";
			}
			else
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Link)).InnerText = "";

			string cpccchk = CreateRandomCPCCCHK(); // genero un cpccchk univoco
			myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Uid)).InnerText = cpccchk;

			if (currentNode.IsApplication)
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Elements)).InnerText = currentNode.HasChildNodes ? currentNode.GroupItems.Count.ToString() : "0";
			else if (currentNode.CommandItems != null)
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Elements)).InnerText = currentNode.CommandItems.Count.ToString();
			else
				myElement.AppendChild(doc.CreateElement(VMNMenuXML.Element.Elements)).InnerText = currentNode.HasMenuChildNodes() ? currentNode.MenuItems.Count.ToString() : "0";

			return myElement;
		}
		# endregion
	}
}