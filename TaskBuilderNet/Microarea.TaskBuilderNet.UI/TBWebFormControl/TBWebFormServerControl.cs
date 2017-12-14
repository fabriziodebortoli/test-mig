using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using AjaxControlToolkit;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.TBWebFormControl;


[assembly: WebResource(TBWebFormControl.ScriptUrl, "text/javascript")]
[assembly: ScriptResource(TBWebFormControl.ScriptUrl, TBWebFormControl.ScriptName, TBWebFormControl.ScriptResource)]

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	/// <summary>
	/// Controllo principale che gestisce il rendering e l'interazione con le form di Mago
	/// </summary>
	/// <remarks>Si occupa di generare tutto l'HTML necessario per visualizzare una form di Mago, gestisce l'aggiornamento usando AJAX</remarks>
	public class TBWebFormControl : ScriptControlBase, IPostBackEventHandler
	{
        internal const string ScriptUrl = "Microarea.TaskBuilderNet.UI.TBWebFormControl.TBWebFormClientControl.js";
        internal const string ScriptResource = "Microarea.TaskBuilderNet.UI.TBWebFormControl.Resource";
        internal const string ScriptName = "Microarea.TaskBuilderNet.UI.TBWebFormControl.TBWebFormClientControl";
	
		internal static List<string> cssFiles = new List<string>(){"Default.css", "Infinity.css"};
		internal string currentCssFile = cssFiles[0];

		public static readonly Type DefaultReferringType = typeof(TBWebFormControl);
		[DataContract]
		struct FormPositionState
		{
			[DataMember]
			public string Id { get; set; }
			[DataMember]
			public int X { get; set; }
			[DataMember]
			public int Y { get; set; }
			[DataMember]
			public int W { get; set; }
			[DataMember]
			public int H { get; set; }
		}

		[DataContract]
		class TBWebFormControlState
		{
			[DataMember]
			public List<FormPositionState> Forms { get; set; }

			//--------------------------------------------------------------------------------------
			public static TBWebFormControlState FromJsonString(string clientState)
			{
				TBWebFormControlState state = null;
				try
				{
					DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(TBWebFormControlState));
					using (XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(clientState), new XmlDictionaryReaderQuotas()))
					{
						state = json.ReadObject(reader, true) as TBWebFormControlState;
					}
				}
				catch (Exception ex)
				{
					Debug.Fail(ex.ToString());
				}
				return state != null ? state : new TBWebFormControlState();
			}

			//--------------------------------------------------------------------------------------
			public string ToJsonString()
			{
				DataContractJsonSerializer json = new DataContractJsonSerializer(GetType());
				using (MemoryStream ms = new MemoryStream())
				{
					using (XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(ms))
					{
						json.WriteObject(ms, this);
						writer.Flush();
						return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
					}
				}
			}
		}

		

		//*** IMPORTANT! ****
		//this enum has to correspond to 'InnerLoopReason' one declared in TBNameSolver\ThreadContext.h
		internal enum ActionCode 
		{
			NONE = 0,			//il thread e' nel ciclo di messaggi della Run 
			THREAD_LOOP = 1,	//il thread sta effettuando un loop interno a quello della Run (CheckMessage del BatchScheduler)
			MODAL_STATE = 2,	//il thread e' nel loop interno di una dialog modale
			BATCH = 3,			//il thread sta eseguendo una batch
			WOORM_REPORT = 4	//il thread sta eseguendo Woorm C++
		};
		
		/// <summary>
		/// Controlli registrati (mappa per facilitarne il ritrovamento)
		/// </summary>
		private Dictionary<string, TBWebControl> controls = new Dictionary<string, TBWebControl>();
		/// <summary>
		/// lista degli script javascript da generare al client
		/// </summary>
		private List<TBWebControl> initControlScripts = new List<TBWebControl>();
		
		internal const int MaxPingInterval = 60000;	//1 minute
		internal const int ModalPingInterval = 10000;	//10 seconds
		internal const int MinPingInterval = 500;
		internal const int BatchPingInterval = 3000;	//1 second
		internal const int WoormPingInterval = 1000;	//1 second
		internal const int ZIndexDistanceBetweenForms = 100;

		TBWebFormControlState controlClientState = new TBWebFormControlState();
		/// <summary>
		/// chiave per recuperare la sessione di lavoro (in pratica il collegamento col thread di documento)
		/// </summary>
		string docSessionId;
		/// <summary>
		/// Contenitore di tutte le informazioni associate al thread di documento le cui finestre vanno visualizzate
		/// </summary>
		DocumentBag documentBag = null;
		
		/// <summary>
		/// UpdatePanel che contiene tutti i controlli
		/// </summary>
		TBUpdatePanel mainUpdatePanel;
		/// <summary>
		/// Updatepanel che disegna il bordo delle finestre per le operazioni di drag & drop
		/// usato anche per mandare informazioni al client, visto che sempre presente
		/// </summary>
		TBBorderPanel borderUpdatePanel;
		
		/// <summary>
		/// Pannello avente zindex maggiore di tutti gli altri
		/// usato per le div che devono fare da popup su tutte le altre (viene assegnato ad esse come parent)
		/// </summary>
		Panel popupPanel;

		/// <summary>
		/// La prima finestra che si apre (viene presa come form principale)
		/// </summary>
		TBForm mainForm;

		/// <summary>
		/// il numero di form di primo livello e' cambiato
		/// </summary>
		bool formsChanged = false;
		
		private string focusId = "";
		private string fontFamily = "";
		int zIndex = 0;
		private static string imagesPath = null;
		public static string ImagesPath
		{
			get 
			{
				if (imagesPath == null)
				{
					UserInfo ui = UserInfo.FromSession();
					if (ui != null)
						imagesPath = ui.PathFinder.GetWebProxyImagesPath();
				}
				return imagesPath; 
			}
		}

		///<summary>
		///Font usati dal TbWebFormControl
		///</summary>
		public string FontFamily
		{
			get { return fontFamily; }
			set { fontFamily = value; }
		}

		#region [Properties]

		//--------------------------------------------------------------------------------------
		internal Panel PopupPanel { get { return popupPanel; } }
		//--------------------------------------------------------------------------------------
		internal TBUpdatePanel MainUpdatePanel { get { return mainUpdatePanel; } }
		//--------------------------------------------------------------------------------------
		internal ActionCode CurrentActionCode { get { return documentBag.ActionCode; } }
		//--------------------------------------------------------------------------------------
		internal int PingInterval { get { return documentBag == null ? MaxPingInterval : documentBag.PingInterval; } set { documentBag.PingInterval = value; } }
		//--------------------------------------------------------------------------------------
		internal bool ThreadError { get { return documentBag.ThreadState == DocumentBag.State.ERROR; } }
		//--------------------------------------------------------------------------------------
		internal bool ThreadClosed { get { return documentBag.ThreadState == DocumentBag.State.CLOSED; } }
		//--------------------------------------------------------------------------------------
		internal bool ThreadNotYetStarted { get { return documentBag.ThreadState == DocumentBag.State.NONE; } }
		//--------------------------------------------------------------------------------------
		internal bool ThreadAvailable { get { return documentBag.ThreadState == DocumentBag.State.STARTED; } }
		//--------------------------------------------------------------------------------------
		internal int ProxyObjectId { get { return documentBag.ProxyObjectId; } }
		//--------------------------------------------------------------------------------------
		internal int ThreadId { get { return documentBag.ThreadId; } }
		//--------------------------------------------------------------------------------------
		internal IDiagnostic Diagnostic { get { return documentBag.Diagnostic; } }
		//--------------------------------------------------------------------------------------
		internal bool HasMessages { get { return Diagnostic.AllMessages().Count > 0; } }
		//--------------------------------------------------------------------------------------
		public string OwnerApplication { get; set;} 
		//--------------------------------------------------------------------------------------
		public string OwnerModule { get; set;} 

		//--------------------------------------------------------------------------------------
		internal TBWebProxy ActionService { get { return documentBag.ActionService; } }

		///<summary>
		///Metodo che aggiusta il fuoco.
		///Se non c'e' un controllo corrispondente lato web, o il controllo corrispondente 
		///e' un controllo non focusable (es. il bodyedit che e' un TbGridContainer(Panel))
		///deve impostare il fuoco sul campo dummy (per permettere il funzionamento degli acceleratori)
		/// </summary>
		//--------------------------------------------------------------------------------------
		internal void AdjustControlFocus(string dummyFocus)
		{
			if (string.IsNullOrEmpty(focusId))
			{
				focusId = dummyFocus;
				SetFocus();
				return;
			}
			TBWebControl control = FindTBWebControl(focusId);
			if (control == null || !control.Focusable)
			{
				focusId = dummyFocus;
				SetFocus();
				return;
			}
		}
		
		
		//--------------------------------------------------------------------------------------
		public string ObjectNamespace{ get; set; }
		
#endregion

		public TBWebFormControl()
			: base (true, HtmlTextWriterTag.Div)
		{
			
		}

	
		//--------------------------------------------------------------------------------------
		public void SetWarning(string message)
		{
			Diagnostic.SetWarning(message);
			Invalidate();
		}
		//--------------------------------------------------------------------------------------
		public void SetError(string message)
		{
			Diagnostic.SetError(message);
			Invalidate();
		}
		//--------------------------------------------------------------------------------------
		public void SetInformation(string message)
		{
			Diagnostic.SetInformation(message);
			Invalidate();
		}
		//--------------------------------------------------------------------------------------
		public void SetDiagnostic(DiagnosticType diagnosticType, string message)
		{
			Diagnostic.Set(diagnosticType, message);
			Invalidate();
		}
		
		//--------------------------------------------------------------------------------------
		public void RegisterControl(Control control, TBWebControl ownerControl)
		{
			controls[control.ClientID] = ownerControl;
			controls[control.ID] = ownerControl;
		}

		//--------------------------------------------------------------------------------------
		public void RegisterInitControlScript(TBWebControl control)
		{
			//se non e' gia' presente nella lista lo aggiungo 
			foreach (TBWebControl tbwc in initControlScripts)
				if (tbwc.WindowId == control.WindowId)
					return;
		
			initControlScripts.Add(control);
		}
		//--------------------------------------------------------------------------------------
		protected override IEnumerable<ScriptDescriptor> GetScriptDescriptors()
		{
			List<ScriptDescriptor> descriptors = new List<ScriptDescriptor>();

			if (ClientControlType != null)
			{
				IEnumerable<ScriptDescriptor> baseDescriptors = base.GetScriptDescriptors();
				if (baseDescriptors != null)
					descriptors.AddRange(baseDescriptors);
			}
			ScriptControlDescriptor descriptor = new ScriptControlDescriptor("TBWebFormControl.TBWebFormClient", this.ClientID);
			DescribeComponent(descriptor);
			descriptors.Add(descriptor);
			return descriptors;
		}

		// Generate the script reference
		//--------------------------------------------------------------------------------------
		protected override IEnumerable<ScriptReference> GetScriptReferences()
		{
			List<ScriptReference> references = new List<ScriptReference>();
			IEnumerable<ScriptReference> baseReferences = base.GetScriptReferences();
			if (baseReferences != null)
				references.AddRange(baseReferences);
			ScriptReference sr = new ScriptReference();
			sr.Assembly = this.GetType().Assembly.FullName;
			sr.Name = ScriptUrl;
			references.Add(sr);
			return references;
		}

		//--------------------------------------------------------------------------------------
		public override string ToString()
		{
			return string.Format("{0} - {1}", GetType().Name, ID);
		}
		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//se sono in design mode disegno solo una label ed esco
			if (DesignMode)
			{
				Label l = new Label();
				l.Text = this.ToString();
				l.BorderColor = System.Drawing.Color.Red;
				l.BorderWidth = Unit.Pixel(1);
				l.BackColor = System.Drawing.Color.DarkBlue;
				l.ForeColor = System.Drawing.Color.Yellow;
				l.BorderStyle = BorderStyle.Solid;
				l.Width = Unit.Percentage(100);
				l.Height = Unit.Percentage(100);
				Controls.Add(l);
				return;
			}

			//imposto il font dai settings di easylook
			EasyLookCustomization.EasyLookCustomSettings easyLookCustomSettings =
				(EasyLookCustomization.EasyLookCustomSettings)Page.Session[EasyLookCustomization.EasyLookCustomSettings.SessionKey];
			if (easyLookCustomSettings != null)
			{
				FontFamily = easyLookCustomSettings.FontFamily;
				this.Style[HtmlTextWriterStyle.FontFamily] = FontFamily;
			}
			CssClass = "TbWebFormControl";
			ScriptManager.GetCurrent(Page).EnableScriptGlobalization = true;
			EnableViewState = false;
			//recupero le informazioni utente se ho gia` fatto la login
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
			{
				//imposto la lingua del thread corrente
				ui.SetCulture();
				//se sono in demo, aggiungo lo sfondo demo
				if (ui.IsDemo)
				{
					Style.Add(HtmlTextWriterStyle.BackgroundImage, ImagesHelper.CreateImageAndGetUrl("DemoBkgnd.png", TBWebFormControl.DefaultReferringType));
					Style.Add("background-repeat", "repeat");
				}
			
				//recupero l'id di sessione dalla request: inizialmente e' vuoto
				docSessionId = Page.Request["DocSessionId"];
				if (string.IsNullOrEmpty(docSessionId))
				{
					string sep = Page.Request.QueryString.Count == 0 ? "?" : "&";
					//alla prima richiesta l'id e' vuoto, ne genero uno e 'rimpallo' la richiesta su me stesso con un redirect
					Page.Response.Redirect(Page.Request.RawUrl + sep + "DocSessionId=" + HttpUtility.UrlDecode(Guid.NewGuid().ToString()), true);
					return;
				}
			}
			
			//aggiungo l'update panel principale
			mainUpdatePanel = new TBUpdatePanel();
			mainUpdatePanel.ID = "MainUpdatePanel";
			mainUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
			mainUpdatePanel.ChildrenAsTriggers = false;
			Controls.Add(mainUpdatePanel);

			//recupero la bag di documento, dove ho tutte le informazioni relative al thread di documento
			documentBag = Page.Session[docSessionId] as DocumentBag;
			if (documentBag == null)
			{
				//la prima volta non ho una bag, la creo
				documentBag = new DocumentBag(Page.Session);
			}

			//se le user info sono nulle, significa che ho perso la connessione, pulisco le forms ed 
			//imposto un messaggio di errore
			if (ui == null)
			{
				ClearForms();
				documentBag.Clear(DocumentBag.State.ERROR);
				SetError(TBWebFormControlStrings.LoginExpired);
			}

			//alla fine di tutto, disegno le finestre
			DesignForms();

			//Aggiunge il pannello per la selezione degli stili
			
			//COMMENTATO PER USCITA 3_8 (LA MODIFICA E" PER MAGO INFINITY)
			//AddStylePanel(Controls);
			
			

		}

		//--------------------------------------------------------------------------------------
		private void AddStylePanel(ControlCollection Controls)
		{
			Panel container = new Panel();
			container.CssClass = "TBCssPanel";
			
			//aggiunta combo per selezione style
			Label styleLabel = new Label();
			styleLabel.Style.Add(HtmlTextWriterStyle.PaddingLeft, "10px");
			styleLabel.Style.Add(HtmlTextWriterStyle.PaddingRight, "3px");
			styleLabel.Text = TBWebFormControlStrings.SelectStyle;
			container.Controls.Add(styleLabel);
			DropDownList styleCombo = new DropDownList();
			foreach (string s in cssFiles)
			{
				styleCombo.Items.Add(s);
			}
			styleCombo.Attributes["onchange"] = string.Format("tbChangeStyle(this)");
			
			//Legge l'eventuale cookie per impostare il css corrente
			string cssName = (Page.Request.Cookies["cssStyle"] != null) ? HttpUtility.HtmlDecode(Page.Request.Cookies["cssStyle"].Value).Trim() : string.Empty;
			
			if (!cssName.IsNullOrWhiteSpace())
			{
				styleCombo.SelectedIndex = styleCombo.Items.IndexOf(styleCombo.Items.FindByValue(cssName));
				currentCssFile = cssName;
			}

			container.Controls.Add(styleCombo);
			Controls.Add(container);
		}


		//--------------------------------------------------------------------------------------
		private void AddCssLink(string cssFileName, int index, string cssId)
		{
			string file = GetCssFile(cssFileName);
			HtmlLink link = new HtmlLink();
			link.ID = cssId;
			link.Href = TbWebFormResourceProvider.GetCssUrl(file);
			link.Attributes.Add("type", "text/css");
			link.Attributes.Add("rel", "stylesheet");
			Page.Header.Controls.AddAt(index, link);
		}

		//--------------------------------------------------------------------------------------
		private string GetCssFile(string cssFileName)
		{
			string file = cssFileName;
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
			{
				string cssFile = Path.Combine(ui.PathFinder.GetCustomModuleTextPath(ui.Company, ui.User, OwnerApplication, OwnerModule), file);
				if (File.Exists(cssFile))
					file = cssFile;
			}
			return file;
		}
		//--------------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DesignMode)
				return;
			//la prima volta esegue il thread
			if (ThreadNotYetStarted)
				InitMainTbForm();

			//registro lo script dei controlli
			RegisterControlsScript();
		}

		//--------------------------------------------------------------------------------------
		protected override void LoadClientState(string clientState)
		{
			controlClientState = TBWebFormControlState.FromJsonString(clientState);
		}

		//--------------------------------------------------------------------------------------
		protected override string SaveClientState()
		{
			return controlClientState.ToJsonString();
		}

		//--------------------------------------------------------------------------------------
		protected override void OnUnload(EventArgs e)
		{
			if (docSessionId != null)
				Page.Session[docSessionId] = documentBag;
			base.OnUnload(e);
		}

		
		//--------------------------------------------------------------------------------------
		private void CreateWorkingImage(int zIndex)
		{
			System.Web.UI.WebControls.Image image = new System.Web.UI.WebControls.Image();
            string imageName = "Working.gif";
			using (System.Drawing.Bitmap rotatingGears = ImagesHelper.GetStaticImage(imageName, TBWebFormControl.DefaultReferringType))
            {
			    image.Height = Unit.Pixel(rotatingGears.Height);
			    image.Width = Unit.Pixel(rotatingGears.Width);
            } 
            image.Style[HtmlTextWriterStyle.Position] = "absolute";
			image.Style[HtmlTextWriterStyle.Left] = "0px";
			image.Style[HtmlTextWriterStyle.Top] = "0px";
			image.Style[HtmlTextWriterStyle.Visibility] = "hidden";
			image.Style[HtmlTextWriterStyle.Display] = "none";
			image.Style[HtmlTextWriterStyle.ZIndex] = zIndex.ToString();
			image.ImageUrl = ImagesHelper.CreateImageAndGetUrl(imageName, TBWebFormControl.DefaultReferringType);
			image.ID = "workingGif";
			
			mainUpdatePanel.ContentTemplateContainer.Controls.Add(image);

			string script = string.Format("<script>function getWorkingImage() {{ return $get('{0}'); }}</script>", image.ClientID);
			Page.ClientScript.RegisterClientScriptBlock(Page.ClientScript.GetType(), "getWorkingImage", script);
		}
		//--------------------------------------------------------------------------------------
		private void CreateResizeBorder(int zIndex)
		{
			borderUpdatePanel = new TBBorderPanel();
			borderUpdatePanel.SetInitValues(this, null, new WndObjDescription());
			borderUpdatePanel.ControlsScript = initControlScripts;
			borderUpdatePanel.AssignZIndex(zIndex);
			borderUpdatePanel.ID = "resizeBorderPanel";
			mainUpdatePanel.ContentTemplateContainer.Controls.Add(borderUpdatePanel);

			string script = string.Format("<script>function getResizeBorder() {{ return $get('{0}'); }}</script>", borderUpdatePanel.Panel.ClientID);
			Page.ClientScript.RegisterClientScriptBlock(Page.ClientScript.GetType(), "getResizeBorder", script);
			//Aggiorno sempre il borderPanel perche' contiene il ping interval e gli script di inizializzazione dei controlli.
			//possibile miglioria: aggiornarlo solo se cambiato il ping interval  o lo script
			borderUpdatePanel.Update();
		}

		//--------------------------------------------------------------------------------------
		private void CreatePopupPanel(int zIndex)
		{
			popupPanel = new Panel();
			popupPanel.EnableViewState = false;
			popupPanel.TabIndex = -1;
			popupPanel.CssClass = "PopupPanel";
			popupPanel.Style[HtmlTextWriterStyle.ZIndex] = zIndex.ToString();
			popupPanel.ID = "PopupPanel";
			popupPanel.Style[HtmlTextWriterStyle.Visibility] = "hidden";
			popupPanel.Style[HtmlTextWriterStyle.Display] = "none";
			
			mainUpdatePanel.ContentTemplateContainer.Controls.Add(popupPanel);

			string script = string.Format("<script>function getPopupPanel() {{ return $get('{0}'); }}</script>", popupPanel.ClientID);
			Page.ClientScript.RegisterClientScriptBlock(Page.ClientScript.GetType(), "getPopupPanel", script);
		}

		//--------------------------------------------------------------------------------------
		public TBWebControl FindTBWebControl(string clientID)
		{
			// ho trovato il controllo, lo restituisco
			TBWebControl control;
			if (controls.TryGetValue(clientID, out control))
				return control;

			if (controls.TryGetValue(clientID + "Upd", out control))
				return control;	
			
			return null;
		}

		
		///<summary>
		/// Devo fare ovverride di questo metodo per forzare il fire dell'evento RaisePostBackEvent.
		/// </summary>
		//--------------------------------------------------------------------------------------
		protected override bool LoadPostData(String postDataKey, NameValueCollection postCollection)
		{
			Page.RegisterRequiresRaiseEvent(this);
			return base.LoadPostData(postDataKey, postCollection);
		}

		#region IPostBackEventHandler Members
		//--------------------------------------------------------------------------------------
		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			TBWebControl ctrl = FindTBWebControl(Page.Request["__EVENTTARGET"]);
			if (ctrl == null)
				return;

			Action action = Action.ParseAction(ctrl, Page.Request["__EVENTARGUMENT"]);

			if (action != null)
				PerformAction(action);

		}

		#endregion

		//--------------------------------------------------------------------------------------
		public int GetIntServerId(string clientOrServerId)
		{
			int id;
			int.TryParse(GetServerId(clientOrServerId), out id);

			return id;
		}
		
		//--------------------------------------------------------------------------------------
		public string GetServerId(string clientOrServerId)
		{
			TBWebControl ctrl = FindTBWebControl(clientOrServerId);
			if (ctrl != null)
				return ctrl.InnerControl.ID; //client id
			
			Control c = FindControl(clientOrServerId);
			if (c != null)
				return c.ID;		//server id
		
			return string.Empty;
		}

		//--------------------------------------------------------------------------------------
		private void InitMainTbForm()
		{	
			string docId = Page.Request["DocumentHandle"];
			if (!string.IsNullOrEmpty(docId)) //se mi viene passato un handle di documento, devo agganciarmi ad uno gia` aperto in tb
				PerformAction(new AttachToDocumentAction(this, docId));
			else
			{
				//altrimenti eseguo il documento solo se ho una CAL GDI
				UserInfo ui = UserInfo.FromSession();
				if (ui.CalType == LoginSlotType.Gdi)
				{
					documentBag.InitializeThread();
					PerformAction(new RunDocumentAction(this));
				}
				else
				{
					//non posso eseguire il documento: lo segnalo e disegno la finestra di messaggio
					SetInformation(TBWebFormControlStrings.OpenDocumentCAL);
					documentBag.Clear(DocumentBag.State.NOTALLOWED);
					UpdateForms();
				}
			}
		}

		//--------------------------------------------------------------------------------------
		internal byte[] AttachToDocument(int docId)
		{
			return documentBag.AttachToDocumentThread(docId);
		}

		//--------------------------------------------------------------------------------------
		public byte[] RunDocument()
		{
			byte[] description = null;
			bool isLoginValid = true;
			try
			{
				//eseguo la rundocument
				if (ActionService != null)
				{
					NameSpace ns = new NameSpace(ObjectNamespace);
					if (ns.NameSpaceType.Type == NameSpaceObjectType.Document)
					{
						int id = 0;
						description = ActionService.RunDocument(documentBag.ProxyObjectId, ObjectNamespace, ref id);
					}
					else if (ns.NameSpaceType.Type == NameSpaceObjectType.Function)
					{
						description = ActionService.RunFunction(documentBag.ProxyObjectId, ObjectNamespace);
					}
					else if (ns.NameSpaceType.Type == NameSpaceObjectType.Report)
					{
						int id = 0;
						description = ActionService.RunReport(documentBag.ProxyObjectId, ObjectNamespace, ref id);
					}
					else
					{
						Debug.Fail("Invalid object namespace");
					}
				}

				if (description == null)
					description = new byte[0];
				
				if (description.Length == 0)
					SetInformation(string.Format(TBWebFormControlStrings.CannotOpenDocument, ObjectNamespace));
			}
			catch (Exception ex)
			{
				//se non va a buon fine, forse e' per via della login non piu` valida...
				if (!TestLogin(out isLoginValid))
					SetError(ex.ToString());
			}

			if (ActionService == null || !isLoginValid)
				return description;

			try
			{
				string[] messages = new string[0];
				int[] types = new int[0];
				
				//recupero eventuali messaggi dal login context
				ActionService.GetLoginContextMessages(true, ref messages, ref types);
				
				//recupero eventuali messaggi dal thread context
				ActionService.WebProxyObj_GetThreadContextMessages(documentBag.ProxyObjectId, true, ref messages, ref types);

				//se ci sono messaggi li aggiungo alla diagnostica
				if (messages != null && types != null)
					for (int i = 0; i < messages.Length; i++)
						SetDiagnostic((DiagnosticType)types[i], messages[i]);
			}
			catch (Exception ex)
			{ 
				SetError(ex.ToString());
			}

			return description;
		}

		//--------------------------------------------------------------------------------------
		private bool TestLogin(out bool isLoginValid)
		{
			isLoginValid = false;
			try
			{
				if (!(isLoginValid = ActionService.IsLoginValid()))
					SetInformation(TBWebFormControlStrings.LoginExpired);
				return true;
			}
			catch
			{
				return false;
			}
		}

		//--------------------------------------------------------------------------------------
		/// <summary>
		/// Esegue un'azione sul server Mago
		/// </summary>
		/// <param name="action">L'oggetto che descrive l'azione da eseguire</param>
		internal void PerformAction(Action action)
		{
			lock (documentBag)
			{
				try
				{
					//prima esegue l'azione, che restituisce i delta per aggiornare i controllo che sono cambiati
					byte[] description = ExecuteAction(action);
					//poi forza l'aggiornamento dei controlli di finestra
					UpdateWindows(description);
				}
				catch (Exception ex)
				{
					Debug.Fail(ex.ToString());
				}
				finally
				{
					UpdateForms();
				}
			}
		}
		//--------------------------------------------------------------------------------------
		/// <summary>
		/// Cuore dell'algoritmo di dispatch delle azioni dell'utente a TB; Se l'azione e' sincrona,
		/// viene eseguita direttamente e si esce, altrimenti vengono lanciati due thread, uno che esegue
		/// l'azione e l'altro che controlla l'eventuale stato di messagebox; il primo dei due che finisce permette di considerare l'azione completata
		/// </summary>
		/// <param name="action">Azione da eseguire</param>
		/// <returns>Stream di byte che descrive la finestra</returns>
		private byte[] ExecuteAction(Action action)
		{
			try
			{
				if (ThreadClosed || ThreadError)
					return new byte[0];

				//provo cinque volte poi scateno l'eccezione
				int trials = 5;
				while (true)
				{
					try
					{
						ActionService.SetTimeout(action.GetTimeout());
						byte[] description = action.Execute();
						if (description == null)
							description = new byte[0];
						return description;
					}
					catch (TimeoutException)
					{
						trials--;
						if (trials == 0)
							throw;
					}
				}
			}
			catch (Exception ex)
			{
				Invalidate();
				SetError(ex.ToString());
				documentBag.SetThreadAvailability(DocumentBag.State.ERROR); //il thread non ha risposto e mi ha dato un eccezione
				return new byte[0];
			}
		}

		//--------------------------------------------------------------------------------------
		private void DesignForms()
		{
			try
			{
				//resetto lo stato di "cambiamento nelle finestre figlie" del TbWebFormControl prima di iniziare il disegno
				documentBag.ControlDescription.ChildrenChanged = false;
				mainForm = null;
				zIndex = 0;
				
				//per ogni descrizione di finestra di primo livello, creo una TBForm che poi ricorsivamente
				//disegna i controlli di livelli inferiori
				foreach (WndObjDescription description in documentBag.ControlDescription)
				{
					TBForm windowForm = null;
					//la prima e' la form di riferimento
					if (mainForm == null)
					{
						windowForm = new TBMainForm();
						windowForm.SetInitValues(this, mainForm, description);
						windowForm.AssignZIndex((++zIndex) * ZIndexDistanceBetweenForms);
						mainForm = windowForm;
						mainForm.AttachParent(mainUpdatePanel);
					}
					else
					{
                        switch (description.Type)
                        {
                            case WndObjDescription.WndObjType.RadarFrame        : windowForm = new TBRadar(); break;
                            case WndObjDescription.WndObjType.PrintDialog       : windowForm = new TBPrintDialog(); break;
							case WndObjDescription.WndObjType.FileDialog		: windowForm = new TBFileDialog(); break;
                            case WndObjDescription.WndObjType.PropertyDialog    : windowForm = new TBPropertyDialog(); break;
                            case WndObjDescription.WndObjType.Dialog            : windowForm = new TBDialog(); break;
                            case WndObjDescription.WndObjType.Frame             : windowForm = new TBForm(); break;
                            default                                             : windowForm = new TBForm(); break;
                        }
						
						windowForm.SetInitValues(this, mainForm, description);
						windowForm.AssignZIndex((++zIndex) * ZIndexDistanceBetweenForms);
					}

					mainUpdatePanel.ContentTemplateContainer.Controls.Add(windowForm);
					windowForm.GenerateAcceleratorScript();

					//registro i controlli per poterli recuperare quando dovro mandar loro delle action
					RegisterControl(windowForm, windowForm);
					RegisterControl(windowForm.InnerControl, windowForm);
				}
				
				//se non ho finestre e non ho messaggi di errore, do un messaggio di spiegazione
				if (documentBag.ControlDescription.Count == 0 && !HasMessages)
				{
					//se la causa e' la chiusura del thread di documento, do l'informazione
					if (ThreadClosed)
						SetInformation(TBWebFormControlStrings.DocumentClosed);
					else if (ThreadError) //se invece si e' verificato un errore, lo segnalo
						SetError(TBWebFormControlStrings.DocumentNotResponding);
				}
			}
			catch (Exception ex)
			{
				SetError(ex.ToString());
				ClearForms();
			}
				
			//creo l'immagine di attesa per le batch
			CreateWorkingImage((++zIndex) * ZIndexDistanceBetweenForms);
			
			//creo la finestra per visualizzare eventuali messaggi di errore, se non ce ne sono la nascondero' alla fine
			//devo crearla comunque perche non so se si verificheranno errori, e non posso posticiparne la creazione
			//altrimenti non riceve gli eventi di asp net
			CreateMessageControl((++zIndex) * ZIndexDistanceBetweenForms);

			//creo il controllo per il resize tramite drag & drop
			CreateResizeBorder((++zIndex) * ZIndexDistanceBetweenForms);

			//creo il pannello per le poput (deve essere l'ultimo perche deve avere zindex maggiore di tutti)
			CreatePopupPanel((++zIndex) * ZIndexDistanceBetweenForms);

			//il titolo della finestra del browser e' quello della finestra principale di documento
			if (mainForm != null)
				Page.Title = mainForm.Title;

		}

		//--------------------------------------------------------------------------------------
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			
			if (DesignMode)
				return;
		
			SetFocus();
			
			borderUpdatePanel.PingInterval = PingInterval;
			borderUpdatePanel.Working = PingInterval <= TBWebFormControl.BatchPingInterval;
			
			AddCssLink("TBWebFormClientControl.css", 0, "BaseCssID");
			AddCssLink(currentCssFile, 1, "CustomCssID");
		}
		

		//--------------------------------------------------------------------------------------
		protected override void Render(HtmlTextWriter writer)
		{
			if (IsTbAjaxRequest())
				RenderAjaxContent(Page.Response.OutputStream);
			else
			{
				base.Render(writer);
			}
		}

		///<summary>
		///Metodo che mi dice se la richiesta e' di tipo TbAjax o no
		/// </summary>
		//--------------------------------------------------------------------------------------
		private bool IsTbAjaxRequest()
		{
			return Page.Request.Headers["X-TBAjax"] == "true";
		}

		//--------------------------------------------------------------------------------------
		private void RenderAjaxContent(Stream stream)
		{
			//istanzio l'oggetto che conterrà gli update panel da aggiornare
			AjaxResponse response = new AjaxResponse();
			try
			{
				response.FocusId = GetFocusID();

				//aggiungo ricorsivamente gli UpdatePanel aggiornati
				mainUpdatePanel.RenderAjaxContent(response);
				//registro gli script associati ai controlli
				RegisterControlsScriptAsResource(response);
			}
			catch (Exception ex)
			{
				//cancello eventuali pannelli inseriti
				response.ClearPanels();
				//inserisco l'errore
				response.AddError(ex);
			}

			//salvo in formato JSon sulla response
			response.Save(stream);
		}

		//--------------------------------------------------------------------------------------
		/// <summary>
		/// Controllo che ospita eventuali messaggi di errori segnalati dal controllo stesso.
		/// Viene sempre creato per poter ricevere gli eventi di Aspnet, ma se non ci 
		/// sono messaggi non viene visualizzato (Visible  a 'false' significa che nonviene generato codice HTML)
		/// </summary>
		/// <param name="zIndex"></param>
		private void CreateMessageControl(int zIndex)
		{

			TBMessageControl messageForm = new TBMessageControl(Diagnostic);
			messageForm.SetInitValues(this, null, new WndObjDescription());
            messageForm.AttachParent(mainUpdatePanel);
			messageForm.WindowId = "MessageForm";
			messageForm.AssignZIndex(zIndex);
			messageForm.Width = Page.Request.Browser.ScreenPixelsWidth - 100;
			messageForm.Height = Page.Request.Browser.ScreenPixelsHeight - 100;
			messageForm.X = (Page.Request.Browser.ScreenPixelsWidth - messageForm.Width) / 2;
			messageForm.Y = (Page.Request.Browser.ScreenPixelsHeight - messageForm.Height) / 2;
			
			mainUpdatePanel.ContentTemplateContainer.Controls.Add(messageForm);
			if (mainForm == null)
				mainForm = messageForm;

			RegisterControl(messageForm, messageForm);
			RegisterControl(messageForm.InnerControl, messageForm);

			messageForm.Visible = HasMessages;				
		}
		
		//--------------------------------------------------------------------------------------
		internal void Invalidate()
		{
			formsChanged = true;
		}

		//--------------------------------------------------------------------------------------
		internal void UpdateForms()
		{
			if (formsChanged)
			{
				mainUpdatePanel.ContentTemplateContainer.Controls.Clear();
				DesignForms();
				mainUpdatePanel.Update();
				return;
			}

			foreach (WndObjDescription description in documentBag.ControlDescription)
			{
				TBForm windowForm = FindTBWebControl(description.Id) as TBForm;
				if (windowForm != null)
					windowForm.UpdateFromControlDescription(description);
			}
		}

		///<summary>
		///Metodo che imposta il fuoco sul controllo della pagina web (puo essere un controllo che 
		///ha un corrispondente controllo c++, oppure il controllo dummy che serve per far funzionare gli acceleratori)
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void SetFocus()
		{
			string strFocusId = GetFocusID();
			if (string.IsNullOrEmpty(strFocusId))
				return;
			ScriptManager.GetCurrent(Page).SetFocus(strFocusId);
		}

		///<summary>
		///Metodo che ritorna il clientID del controllo che deve avere il fuoco nella pagina web
		/// </summary>
		//--------------------------------------------------------------------------------------
		private string GetFocusID()
		{
			if (string.IsNullOrEmpty(focusId))
				return string.Empty;

			TBWebControl focusControl = FindTBWebControl(focusId);
			if (focusControl != null)
			{
				if (focusControl.Focusable)
					return focusControl.InnerControl != null
						? focusControl.InnerControl.ClientID
						: focusControl.ClientID;
			}
			else
			{
				Control c = FindControl(focusId);
				if (c != null)
					return c.ClientID;
			}
			return string.Empty;
		}

		//--------------------------------------------------------------------------------------
		public void UpdateWindows(byte[] description)
		{
			try
			{
				//chiede ad un thread separato di TB di recuperare la descrizione delle finestre di thread
				if (ThreadAvailable)
				{
					//chiede ad un thread separato di TB di recuperare la descrizione delle finestre di thread
					if (description.Length == 0) //non ottengo descrizione: il thread è morto
					{
						CloseThread();
						return;
					}
					//aggiorna la descrizione con i delta ricevuti
					UpdateWindowsDescription(description);
					//dopo aver aggiornato la descrizione non ho più finestre: il thread è morto
					if (this.documentBag.ControlDescription.Count == 0)
					{
						CloseThread();
						return;
					}
				}
			}
			catch (Exception ex)
			{
				Invalidate();		
				SetError(ex.ToString());
			}
		}

		//pulisco le forms ed imposto lo stato a chiuso. Questo fara' si che nel browser si chiuda la finestra 
		//in cui era aperto il documento (vedere TBBorderPanel.CreateControlInitializationFunction)
		//--------------------------------------------------------------------------------------
		private void CloseThread()
		{
			ClearForms();
			documentBag.Clear(DocumentBag.State.CLOSED);
		}

		//--------------------------------------------------------------------------------------
		private void ClearForms()
		{
			UpdateWindowsDescription(null);
		}
		

		//Metodo che riceve lo stream di byte con i delta di descrizione di finestre, e aggiorna l'albero
		//delle descrizioni c#, con cui verrano poi generati-aggiornati i controlli web TbWebControl
		//--------------------------------------------------------------------------------------
		private void UpdateWindowsDescription(byte[] description)
		{
			//dichiara una lista di descrizioni, e la valorizza leggendo dallo stream binario
			WndObjDescriptionContainerRoot deltaStructure = new WndObjDescriptionContainerRoot();
			if (description != null)
			{
				deltaStructure.LoadBinary(description);

				if (!string.IsNullOrEmpty(deltaStructure.FocusId))
					focusId = deltaStructure.FocusId;
			}
			else
			{
				Invalidate();
			}

			documentBag.ActionCode = deltaStructure.ActionCode;
			//calcolo l'intervallo di ping in funzione di quello che sta facendo il documento
			//(per le batch il ping deve essere piu` ravvicinato)
			documentBag.PingInterval = GetPingInterval(documentBag.ActionCode);

			//Aggiorna l'albero delle descrizioni, a partire dalla lista dei delta
			documentBag.ControlDescription.UpdateFromDelta(deltaStructure);
			//Se e' cambiato il numero di Forms, deve invalidare tutto il controllo per il ridisegno
			if (documentBag.ControlDescription.ChildrenChanged)
				Invalidate();
		}

		//--------------------------------------------------------------------------------------
		private static int GetPingInterval(ActionCode actionCode)
		{
			switch (actionCode)
			{
				case ActionCode.NONE:						return MaxPingInterval;
				case ActionCode.MODAL_STATE:				return ModalPingInterval;
				case ActionCode.THREAD_LOOP:				return ModalPingInterval;
				case ActionCode.BATCH:						return BatchPingInterval;
				case ActionCode.WOORM_REPORT:				return WoormPingInterval;
				default: return MaxPingInterval;
			}
		}

		//--------------------------------------------------------------------------------------
		public bool GetFormPosition(string clientID, out System.Drawing.Rectangle rect)
		{
			rect = System.Drawing.Rectangle.Empty;

			if (controlClientState.Forms != null)
			{
				foreach (FormPositionState fps in controlClientState.Forms)
				{
					if (String.Compare(fps.Id, clientID) == 0)
					{
						rect = new System.Drawing.Rectangle(fps.X, fps.Y, fps.W, fps.H);
						return true;
					}
				}
			}
			return false;
		}

		///<summary>
		///Classe utilizzata per inciare al client gli script associati ai singoli controlli e 
		///tenere traccia di quelli gia renderizzati (che non devono essere reinviati)
		/// </summary>
		//--------------------------------------------------------------------------------------
		internal class RegisteredScript
		{
			public string Id;
			public string Url;
			public bool Rendered;

			internal string ScriptResourceUrl 
			{
				get
				{
					return string.Format("TbWebFormResource.axd?script={0}", Url);
				}
			}
		}
		
		///<summary>
		///Metodo che aggiunge uno script alla lista degli script da serializzare
		///(chiamato dai controlli che devono registrare un loro script di inizializzazione)
		/// </summary>
		//--------------------------------------------------------------------------------------
		internal bool AddScriptResource(string id, string scriptUrl)
		{
			foreach (RegisteredScript script in documentBag.scriptResources)
			{
				if (script.Id == id)
					return false;
			}
			RegisteredScript s = new RegisteredScript();
			s.Id = id;
			s.Url = scriptUrl;
			s.Rendered = false;
			documentBag.scriptResources.Add(s);
			return true;
		}

		///<summary>
		///Metodo che aggiunge alla ResponseAjax la lista degli script associati ai cotnrolli che
		///il client dovra scaricare, sotto forma di Resources
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void RegisterControlsScriptAsResource(AjaxResponse response)
		{
			foreach (RegisteredScript script in documentBag.scriptResources)
				if (!script.Rendered)
				{
					response.Scripts.Add(script.ScriptResourceUrl);
					script.Rendered = true;
				}
		}
		

		///<summary>
		/// Metodo che registra tramite lo ScriptManager la lista degli script associati ai controlli
		/// (invia al client gli script usando lo script manager solo se non sono gia renderizzati o se non si arriva da una 
		/// tb-ajax request (quindi un aggiornamento totale)
		/// </summary>
		//--------------------------------------------------------------------------------------
		private void RegisterControlsScript()
		{
			foreach (RegisteredScript script in documentBag.scriptResources)
				if (!script.Rendered || !IsTbAjaxRequest())
				{
					ScriptManager.RegisterClientScriptInclude(this, GetType(), script.Id, script.ScriptResourceUrl);
					script.Rendered = true;
				}
		}

		///<summary>
		/// Ritorna true se il client connesso e' un dispositivo mobile Mac 
		/// </summary>
		//--------------------------------------------------------------------------------------
		public bool IsMacDevice()
		{
			return Helper.IsMacDevice(Page.Request.UserAgent);
		}
	}


	/// <summary>
	/// Classe statica che racchiude delle funzionalita' generiche utilizzabili da chiunque
	/// </summary>
	public static class Helper
	{
		///<summary>
		/// Dato la stringa di user agent dice se il client e' un Ipad, un ipod o un iphone
		/// </summary>
		//--------------------------------------------------------------------------------------
		public static bool IsMacDevice(string userAgent)
		{
			return userAgent.Contains("iPhone") || 
					userAgent.Contains("iPod") ||
					userAgent.Contains("iPad");
		}
	}
}