using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Localization;
using Microarea.EasyBuilder.MVC;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.EasyBuilder.UI;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.UI.WinControls.Dock;
using WeifenLuo.WinFormsUI.Docking;

using System.IO;
using System.Runtime.InteropServices;
using Microarea.EasyBuilder.CppData;
using ICSharpCode.NRefactory.CSharp;

namespace Microarea.EasyBuilder
{
	/// <summary>
	/// The core object for all customization operations.
	/// It allows the user to modify the user interface of a document, it exposes all
	/// widget useful to create a customization (Toolbox for dragging new controls,
	/// property grid to inspect customization objects, methods outline to manage all
	/// source code added by the user and so on).
	/// </summary>
	//-----------------------------------------------------------------------------
	public partial class FormEditor : Editor, IContainer
	{
		class SuspendObjectModelUpdate : IDisposable
		{
			FormEditor editor;
			public SuspendObjectModelUpdate(FormEditor editor)
			{
				this.editor = editor;
				editor.suspendObjectModelUpdate++;
			}
			public void Dispose()
			{
				editor.suspendObjectModelUpdate--;
			}
		}

        bool askAndSaveOnExit = true;
		bool formEditorValid = true;
		private string currentJsonFile;

		MParsedStatic dummy; //usato per evitare problemi di refresh. Misteri di MFC che non abbiamo ancora
							 //capito: se non c'è un controllo che occupi tutta l'area (almeno una dimensione, X o Y)
							 //gli invalidate della view non hanno effetto e si hanno problemi di refresh

		SourcesBuilder sourcesBuilder = new SourcesBuilder();

		bool runAsEasyStudioDesigner;

		// form editor objects and status
		private List<IComponent> componentsList = new List<IComponent>();
		private EditingMode editingMode = EditingMode.None;
		private MDocument document;
		private DocumentView view;
		private DocumentController controller;
		private INameSpace documentNamespace;
		private INameSpace customizationNamespace;
		
		private ControlsManager controlsManager;
		internal IJsonEditor jsonEditorConnector = null;

		private ITBDockContent<MainForm> mainForm;

		private ContainerEditingHighlighter containerHighlighter = null;
		private SerializationAddOnService serializationAddOnService = null;
		private TemplatesService templateService = null;

		/// <remarks />
		public DocumentControllers controllers;

		// graphical utility members
		//-----------------------------------------------------------------------------
		private int numberOfFollowingKeyDown;
		private const int followingKeyDownThreshold = 20;
		private const int initialArrowKeysMovementOffset = 1;
		private int arrowKeysMovementOffset = initialArrowKeysMovementOffset;
		private const int arrowKeysMovementMaxOffset = 5;
		internal const int handleMidSize = 3;
		internal Brush mainForeBrush;
		internal Brush foreBrush;
		internal Pen mainForePen;
		internal Pen forePen;

		Selections selections;

		//Istante in cui è avvenuto l'ultimo ridisegno durante un mouse move.
		private long lastDrawTickCount;
		private Point lastDrawPosition;
		private const int delta = 50;
		private bool newDocument;
		private int suspendObjectModelUpdate = 0;//flag per impedire l'aggiornamento continuo dell'object model a seguito di eventi component added/removed durante la dispose / createcomponents

		//classe per disegnare i marker dello zindex
		ZIndexManager zIndexManager;

		//punti per disegnare il rettangolo di selezione
		Point startSelectionClientPoint = Point.Empty;
		Point endSelectionClientPoint = Point.Empty;
		IWindowWrapperContainer selectionContainer;

		//-----------------------------------------------------------------------------
		internal string CurrentJsonFile { get { return currentJsonFile; } }
		internal bool CurrentJsonIsDocOutline { get; set; }
		internal DocumentView View { get { return view; } }
		//---------------------------------------------------------------------
		internal override bool CanUpdateObjectModel { get { return suspendObjectModelUpdate == 0; } }
		//---------------------------------------------------------------------
		internal bool RunAsEasyStudioDesigner
		{
			get { return runAsEasyStudioDesigner; }
			set { runAsEasyStudioDesigner = value; }
		}
		//---------------------------------------------------------------------
		internal NameSpace JsonFormContext
		{
			get
			{
				if (string.IsNullOrEmpty(currentJsonFile))
					return null;
				string[] tokens = currentJsonFile.Split(Path.DirectorySeparatorChar);
				if (Array.IndexOf(tokens, NameSolverStrings.ModuleObjects) == -1)
				{
					//cerco la posizione della cartella jsonforms
					int pos = tokens.Length - 2;//l'ultimo token è il nome del file, non lo considero
					while (pos >= 0 && tokens[pos] != NameSolverStrings.JsonForms)
					{
						pos--;
					}

					if (tokens.Length < pos)
					{
						Debug.Assert(false);
						return null;
					}
					return new NameSpace(string.Concat(NameSpaceSegment.Module, '.', tokens[pos - 1], '.', tokens[pos - 2]));
				}
				else
				{
					if (tokens.Length < 6)
					{
						Debug.Assert(false);
						return null;
					}
					return new NameSpace(string.Concat(NameSpaceSegment.Document, '.', tokens[tokens.Length - 6], '.', tokens[tokens.Length - 5], '.', tokens[tokens.Length - 4], '.', tokens[tokens.Length - 3]));
				}
			}
		}

		//-----------------------------------------------------------------------------
		internal bool ImEditingOnlyThisTile	{ get { return containerHighlighter != null && containerHighlighter.ImEditingOnlyThisTile;	}}

		//---------------------------------------------------------------------
		internal ControllerSources ControllerSources { get { return this.sources; }	}

		//--------------------------------------------------------------------------------
		internal override Sources Sources { get { return this.sources; } }

		//--------------------------------------------------------------------------------
		private IntPtr[] HandlesToSkip
		{
			get { return dummy == null ? new IntPtr[] { Handle } : new IntPtr[] { Handle, dummy.Handle }; }
		}

		//-----------------------------------------------------------------------------
		internal SourcesBuilder SourcesBuilder { get { return sourcesBuilder; } }

		/// <summary>
		/// Gets a copy of the collection of children components.
		/// </summary>
		//-----------------------------------------------------------------------------
		public ComponentCollection Components { get { return new ComponentCollection(componentsList.ToArray()); } }

		//--------------------------------------------------------------------------------
		internal MDocument Document { get { return document; } }

		//--------------------------------------------------------------------------------
		internal bool EditTabOrder
		{
			get { return zIndexManager.Editing; }
			set
			{
				zIndexManager.Editing = value;
				InvalidateEditor();
			}
		}

		//-----------------------------------------------------------------------------
		internal void ExitFromEditing()
		{
			if (RunAsEasyStudioDesigner || containerHighlighter == null || !ImEditingOnlyThisTile)
				return;
			containerHighlighter.OnMouseDownOn(SelectionService.GetSelectedComponents(), null, true);
		}

		/// <summary>
		/// Show the online help.
		/// </summary>
		//-----------------------------------------------------------------------------
		public static void ShowHelp(Type objectType = null, bool isEasyStudioDesigner = false)
		{
            string pageName = objectType == null ? NameSolverStrings.EasyStudio : objectType.Name;

            if (isEasyStudioDesigner)
				pageName += "Designer";
            HelpManager.CallOnlineHelp("RefGuide.TBS." + pageName, "en");
        }

		//-----------------------------------------------------------------------------
		private void ShowHelp(bool isEasyStudioDesigner)
		{
			string prefix = "RefGuide.TBS.";
			HelpManager.CallOnlineHelp(prefix + (isEasyStudioDesigner ? NameSolverStrings.EasyStudioDesigner : NameSolverStrings.EasyStudio), "en");
		}

		/// <summary>
		/// Occurs when the current document has to be restarted
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler<RestartEventArgs> RestartDocument;

		///<remarks />
		//-----------------------------------------------------------------------------
		internal event EventHandler CreateControllerCompleted;

		///<remarks />
		//-----------------------------------------------------------------------------
		internal event EventHandler ControllerChanged;

		///<remarks />
		//-----------------------------------------------------------------------------
		internal event EventHandler ControllerChanging;

		///<remarks />
		//-----------------------------------------------------------------------------
		internal event EventHandler SelectedObjectUpdated;

		/// <summary>
		/// Occurs when the user request to open the Methods outline window.
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler RequestedOpenCodeEditor;

		/// <summary>
		/// Occurs when a new hotlink has been added to object model during design operations
		/// </summary>
		//-----------------------------------------------------------------------------
		public event EventHandler<EventArgs> HotLinkAdded;
		private ControllerSources sources;

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		protected void OnOpenProperties()
		{
			mainForm.HostedControl.CreatePropertyEditor();
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected void OnControllerChanged(EventArgs e)
		{
			if (ControllerChanged != null)
				ControllerChanged(this, e);
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected void OnCreateControllerCompleted(EventArgs e)
		{
			if (CreateControllerCompleted != null)
				CreateControllerCompleted(this, e);
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected void OnControllerChanging(EventArgs e)
		{
			if (ControllerChanging != null)
				ControllerChanging(this, e);
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected void OnSelectedObjectUpdated(EventArgs e)
		{
			if (SelectedObjectUpdated != null)
				SelectedObjectUpdated(this, e);
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected void OnRequestedOpenCodeEditor(EventArgs e)
		{
			if (RequestedOpenCodeEditor != null)
				RequestedOpenCodeEditor(this, e);
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		internal void OnRestartDocument(RestartEventArgs e)
		{
			if (RestartDocument != null)
				RestartDocument(this, e);
		}

		//-----------------------------------------------------------------------------
		static FormEditor()
		{
			TypeDescriptor.AddAttributes
			(
				typeof(IDataBinding),
				new EditorAttribute(typeof(Microarea.EasyBuilder.UI.DataBindingUITypeEditor), typeof(UITypeEditor))
			);

			TypeDescriptor.AddAttributes
			(
				typeof(Microarea.Framework.TBApplicationWrapper.FieldDataBinding),
				new EditorAttribute(typeof(Microarea.EasyBuilder.UI.DataBindingUITypeEditor), typeof(UITypeEditor))
			);

			TypeDescriptor.AddAttributes
			(
				typeof(Microarea.Framework.TBApplicationWrapper.DBTDataBinding),
				new EditorAttribute(typeof(Microarea.EasyBuilder.UI.DataBindingUITypeEditor), typeof(UITypeEditor))
			);

			TypeDescriptor.AddAttributes
			(
				typeof(DataType),
				new EditorAttribute(typeof(Microarea.EasyBuilder.UI.FieldUITypeEditor), typeof(UITypeEditor))
			);

			Functions.LowPriorityThread(() => { SourcesBuilder.DeleteUnusedPdbs(); });
		}

		/// <summary>
		/// Initializes a new instance of FormEditor.
		/// </summary>
		//-----------------------------------------------------------------------------
		public FormEditor(bool runAsEasyStudioDesigner)
		{
			this.SuspendDirtyChanges = true;
			try
			{
				InitializeComponent();

				this.runAsEasyStudioDesigner = runAsEasyStudioDesigner;
				InitializeContextMenu();

				foreBrush = new SolidBrush(Color.FromArgb(255, 0, 255));
				mainForeBrush = new SolidBrush(Color.Blue);

				forePen = new Pen(foreBrush);
				forePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
				forePen.Width = 1.5F;

				mainForePen = new Pen(mainForeBrush);
				mainForePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
				mainForePen.Width = 1.5F;

				OnDesignerCreated(new DesignerEventArgs(null));
				zIndexManager = new ZIndexManager(this);
				selections = new Selections(this);

				this.ComponentDeleted += new EventHandler<DeleteObjectEventArgs>(FormEditor_ComponentDeleted);

				serializationAddOnService = new SerializationAddOnService();
			}
			finally
			{
				this.SuspendDirtyChanges = false;
			}
		}

		//-----------------------------------------------------------------------------
		void FormEditor_ComponentDeleted(object sender, DeleteObjectEventArgs e)
		{
			DeleteObjectEventArgs delEventArgs = e as DeleteObjectEventArgs;
			MToolbar toolbar = view.ToolBar as MToolbar;
			// ho appena tolto l'ultimo elemento della toolbar, la rimuovo dalla serializzazione fino 
			// a quando non è necessario riserializzarla
			if (
					toolbar != null &&
					toolbar.Components.Count == 0 &&
					delEventArgs?.Component is MToolbarItem
				)
			{
				view.Remove(toolbar);
				view.Add(toolbar);
			}
		}


		/// <summary>
		/// Gets the DocumentController for the current customization.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentController"/>
		//-----------------------------------------------------------------------------
		public DocumentController Controller
		{
			get { return controller; }
			private set
			{
				if (controller == value)
					return;
				using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
				{
					OnControllerChanging(EventArgs.Empty);

					SelectionService.SetSelectedComponents(null);
					if (controller != null)
						controller.Dispose();

					controller = value;
					if (controller != null)
					{
						//se esiste già nella lista tolgo il vecchio
						IDocumentController dc = controllers.GetControllerByName(controller.Name);
						if (dc != null)
							controllers.Remove(dc);
						//e aggiungo il nuovo
						controllers.Add(controller);
						if (ComponentDeclarator != null)
							ComponentDeclarator.CurrentControllerType = controller.GetType();

						controller.Site = new TBSite(controller, null, this, "DocumentController");

						InitViewAndDocument(controller.View, controller.Document);
					}
					else
					{
						view = null;
						document = null;
						if (ComponentDeclarator != null)
							ComponentDeclarator.CurrentControllerType = null;
					}
					OnControllerChanged(EventArgs.Empty);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void InitViewAndDocument(DocumentView view, MDocument document)
		{
			if (this.view != null)
			{
				this.view.SizeChanged -= new EventHandler<EasyBuilderEventArgs>(Parent_SizeChanged);
				this.view.ScrollChanged -= new EventHandler<EasyBuilderEventArgs>(Parent_ScrollChanged);
				this.view.SetFocus -= new EventHandler<EasyBuilderEventArgs>(Parent_SetFocus);
			}
			this.view = view;
			this.view.Site = new TBSite(this.view, controller, this, this.view.SerializedName);
			this.document = document;
			this.document.Site = new TBSite(this.document, controller, this, EasyBuilderSerializer.DocumentPropertyName);

			view.SizeChanged += new EventHandler<EasyBuilderEventArgs>(Parent_SizeChanged);
			view.ScrollChanged += new EventHandler<EasyBuilderEventArgs>(Parent_ScrollChanged);
			view.SetFocus += new EventHandler<EasyBuilderEventArgs>(Parent_SetFocus);
		}

		//--------------------------------------------------------------------------------
		internal void InvalidateEditor(Rectangle rect)
		{
			if (view != null)
				view.Invalidate(RectangleToScreen(rect));
			Invalidate(rect);
		}

		//--------------------------------------------------------------------------------
		internal void InvalidateEditor()
		{
			if (view != null)
				view.Invalidate();
			Invalidate();
		}

		//--------------------------------------------------------------------------------
		internal void Invalidate(bool onlyMargins, bool backGround, bool foreGround, Rectangle highlightingRectangle, Rectangle tooltipRectangle)
		{
			if (highlightingRectangle == Rectangle.Empty)
				return;

			int k = 3 * handleMidSize;
			int alignmentPenWidth = 2;

			//Precalcolo coordinate e dimensioni per non perdere tempo dopo.
			Point formEditorScreenRectLocation = RectangleToScreen(ClientRectangle).Location;
			int formEditorScreenRectLocationX = formEditorScreenRectLocation.X;
			int formEditorScreenRectLocationY = formEditorScreenRectLocation.Y;

			Size formEditorSize = ClientSize;
			int formEditorSizeWidth = formEditorSize.Width;
			int formEditorSizeHeight = formEditorSize.Height;

			Rectangle rect = highlightingRectangle;

			Point rectScreenLocation = rect.Location;
			int rectScreenLocationX = rectScreenLocation.X;
			int rectScreenLocationY = rectScreenLocation.Y;

			int rectSizeWidth = rect.Width;
			int rectSizeHeight = rect.Height;

			Rectangle upperLineLeft = new Rectangle(
				formEditorScreenRectLocationX,
				rectScreenLocationY,
				rectScreenLocationX - formEditorScreenRectLocationX,
				alignmentPenWidth
				);
			Rectangle upperLineMid = new Rectangle(
				rectScreenLocationX,
				rectScreenLocationY,
				rectSizeWidth,
				0
				);
			upperLineMid.Inflate(k, k);
			Rectangle upperLineRight = new Rectangle(
				rectScreenLocationX + rectSizeWidth,
				rectScreenLocationY,
				formEditorScreenRectLocationX + formEditorSizeWidth - rectScreenLocationX - rectSizeWidth,
				alignmentPenWidth
				);

			Rectangle lowerLineLeft = new Rectangle(
					formEditorScreenRectLocationX,
					rectScreenLocationY + rectSizeHeight,
					rectScreenLocationX - formEditorScreenRectLocationX,
					alignmentPenWidth
				);
			Rectangle lowerLineMid = new Rectangle(
					rectScreenLocationX,
					rectScreenLocationY + rectSizeHeight,
					rectSizeWidth,
					0
				);
			lowerLineMid.Inflate(k, k);
			Rectangle lowerLineRight = new Rectangle(
					rectScreenLocationX + rectSizeWidth,
					rectScreenLocationY + rectSizeHeight,
					formEditorScreenRectLocationX + formEditorSizeWidth - rectScreenLocationX - rectSizeWidth,
					alignmentPenWidth
				);

			Rectangle leftLineUpper = new Rectangle(
				rectScreenLocationX,
				formEditorScreenRectLocationY,
				alignmentPenWidth,
				rectScreenLocationY - formEditorScreenRectLocationY
				);
			Rectangle leftLineMid = new Rectangle(
				rectScreenLocationX,
				rectScreenLocationY,
				alignmentPenWidth,
				rectSizeHeight
				);
			leftLineMid.Inflate(k, k);
			Rectangle leftLineBottom = new Rectangle(
				rectScreenLocationX,
				rectScreenLocationY + rectSizeHeight,
				alignmentPenWidth,
				formEditorScreenRectLocationY + formEditorSizeHeight - rectScreenLocationY - rectSizeHeight
				);

			Rectangle rightLineUpper = new Rectangle(
				rectScreenLocationX + rectSizeWidth,
				formEditorScreenRectLocationY,
				alignmentPenWidth,
				rectScreenLocationY - formEditorScreenRectLocationY
				);
			Rectangle rightLineMid = new Rectangle(
				rectScreenLocationX + rectSizeWidth,
				rectScreenLocationY,
				0,
				rectSizeHeight
				);
			rightLineMid.Inflate(k, k);
			Rectangle rightLineBottom = new Rectangle(
				rectScreenLocationX + rectSizeWidth,
				rectScreenLocationY + rectSizeHeight,
				alignmentPenWidth,
				formEditorScreenRectLocationY + formEditorSizeHeight - rectScreenLocationY - rectSizeHeight
				);
			Rectangle workingToolTipRect = tooltipRectangle;
			workingToolTipRect.Inflate(k, k);

			if (view != null && backGround)
			{
				view.Invalidate(upperLineMid);
				view.Invalidate(rightLineMid);
				view.Invalidate(lowerLineMid);
				view.Invalidate(leftLineMid);
				view.Invalidate(workingToolTipRect);

				//Invalido l'area delle linee di allineamento solo se è premuto il mouse.
				if (Capture)
				{
					view.Invalidate(upperLineLeft);
					view.Invalidate(upperLineRight);
					view.Invalidate(rightLineUpper);
					view.Invalidate(rightLineBottom);
					view.Invalidate(lowerLineLeft);
					view.Invalidate(lowerLineRight);
					view.Invalidate(leftLineUpper);
					view.Invalidate(leftLineBottom);
				}

				if (!onlyMargins)
				{
					rect.Inflate(k, k);

					view.Invalidate(rect);
				}
			}

			if (!foreGround)
				return;

			Invalidate(RectangleToClient(upperLineMid));
			Invalidate(RectangleToClient(rightLineMid));
			Invalidate(RectangleToClient(lowerLineMid));
			Invalidate(RectangleToClient(leftLineMid));
			Invalidate(RectangleToClient(workingToolTipRect));

			//Invalido l'area delle linee di allineamento solo se è premuto il mouse.
			if (Capture)
			{
				Invalidate(RectangleToClient(upperLineLeft));
				Invalidate(RectangleToClient(upperLineRight));
				Invalidate(RectangleToClient(rightLineUpper));
				Invalidate(RectangleToClient(rightLineBottom));
				Invalidate(RectangleToClient(lowerLineLeft));
				Invalidate(RectangleToClient(lowerLineRight));
				Invalidate(RectangleToClient(leftLineUpper));
				Invalidate(RectangleToClient(leftLineBottom));
			}

			if (!onlyMargins)
				Invalidate(RectangleToClient(rect));
		}

		//-----------------------------------------------------------------------------
		internal bool CanChangeEnablePropertyEditor()
		{
			return true;
			/*IWindowWrapper current = selections?.MainSelection?.GetCurrentWindow();
			if (current == null)
				return true;
			bool x = !(current is MTileDialog) && !(current is MTilePanel);
			return x ? 
				x : true;
				(containerHighlighter != null ? containerHighlighter.IsInEdit(current) :*/
		}

		//-----------------------------------------------------------------------------
		private void InitReferencedComponents()
		{
			Action<Sources, List<ReferenceableComponent>> loadReferencedComponents = new Action<Sources, List<ReferenceableComponent>>(
					(Sources customizationSources, List<ReferenceableComponent> referencedComponents)
					=>
					{
						NamespaceDeclaration nsDecl = EasyBuilderSerializer.GetNamespaceDeclaration(customizationSources.CustomizationInfos.EbDesignerCompilationUnit);

						foreach (TypeDeclaration type in nsDecl.Members)
						{
							foreach (var attributeSection in type.Attributes)
							{
								foreach (ICSharpCode.NRefactory.CSharp.Attribute att in attributeSection.Attributes)
								{
									if (att.Type.AstTypeToString() != typeof(ReferenceDeclarationAttribute).Name)   //att.Name
										continue;

									ReferenceDeclarationAttribute declAttr = ReferenceDeclarationAttribute.Create(att);
									if (declAttr == null)
										continue;

									ReferenceableComponent refComponent = new ReferenceableComponent(
										declAttr,
										customizationSources.Namespace
										);

									referencedComponents.Add(refComponent);
								}
							}
						}
					}
					);

			ComponentDeclarator = new ComponentDeclarator(Sources, new Dictionary<Type, Func<TypeDeclaration, NameSpace>>(), loadReferencedComponents);
		}
		/// <summary>
		/// Opens the EasyBuilder environment.
		/// </summary>
		//-----------------------------------------------------------------------------
		public void Show(
			INameSpace ns,
			MDocument document,
			DocumentView view,
			DocumentController controller,
			DocumentControllers controllers,
			DockPanel hostingPanel,
			bool newDocument
			)
		{
			Control.CheckForIllegalCrossThreadCalls = false;
			Cursor.Current = Cursors.WaitCursor;
			SuspendDirtyChanges = true;
			try
			{
				this.newDocument = newDocument;
				this.documentNamespace = document.Namespace;
				this.customizationNamespace = ns;
				if (RunAsEasyStudioDesigner)
					CppInfo.StartParsing();
				using (var splash = EBSplash.StartSplash(Properties.Resources.Splash))
				{
                    splash.SetMessage(Resources.InitEasyBuilder);

					controlsManager = new ControlsManager(this);
					IEasyBuilderApp currentEasyBuilderApp = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp;

					splash.SetMessage(Resources.InitSources);
					if (!runAsEasyStudioDesigner)
					{
						var sources = EBLicenseManager.GenerateCsproj
							? new ControllerCsprojSources(currentEasyBuilderApp.ApplicationName, currentEasyBuilderApp.ModuleName, currentEasyBuilderApp.ApplicationType)
							: new ControllerSources(currentEasyBuilderApp.ApplicationName, currentEasyBuilderApp.ModuleName, currentEasyBuilderApp.ApplicationType);
						InitSources(sources);

						InitReferencedComponents();
					}
                    //metto da parte il controller da ripristinare qualora debba rollbackare la modifica
                    this.controllers = controllers;
					this.Controller = controller as DocumentController;

                    //dal namespace di documento capisco se si tratta di un nuovo documento non ancora salvato
                    //tengo da parte l'informazione perché poi il namespace mi viene sostituito con quello scelto dall'utente
                   if (ControllerSources != null)
						ControllerSources.Init(ns, view, document, Controller);

                    EventsManager = new EventsManager(this, Sources);
					LocalizationManager = new LocalizationManager(Sources);

					SelectionService.SelectionChanged += new EventHandler(SelectionService_SelectionChanged);
					SelectionService.SelectionChanging += new EventHandler(SelectionService_SelectionChanging);

					splash.SetMessage(Resources.InitObjectModel);
					InitViewAndDocument(view, document);

					Debug.Assert(ns != null);

					this.Site = new TBSite(this, null, this, NameSolverStrings.EasyStudioDesigner);

					//ControlsManager ascolta gli eventi relativi al ciclo di vita dei component
					//per effettuare e operazioni dovute in caso di add o remove di un component.
					controlsManager.Site = new TBSite(controlsManager, null, this, "ControlsManager");
					if (ComponentChangeService != null)
					{
						controlsManager.ListenToComponentEvents(ComponentChangeService);
						if (!RunAsEasyStudioDesigner)
						{
							LocalizationManager.ListenToComponentEvents(ComponentChangeService);
							LocalizationManager.LocalizationChanged += new EventHandler<EventArgs>(localizationManager_LocalizationChanged);
						}
						EventsManager.ListenToComponentEvents(ComponentChangeService);
						ComponentChangeService.ComponentChanged += new ComponentChangedEventHandler(ComponentChanged);
						ComponentChangeService.ComponentAdded += new ComponentEventHandler(ComponentAdded);
						ComponentChangeService.ComponentRemoved += new ComponentEventHandler(ComponentRemoved);
					}

					view.EnableKeepAlive();

					InitializeFormEditor(hostingPanel, splash);
                    Show();
                    ExternalAPI.SetParent(Handle, this.view.FrameHandle);
					splash.SetMessage(Resources.CreateCustomControls);
					Left = 0;
					Top = 0;
					UpdateViewSize();
					//in partenza non devo chiedere di salvare (pulisco eventuali
					//SetDirty(true) scatenati da eventi nelle istruzioni eseguite  fino a qui

					//controlli dummy per problemi di refresh
					dummy = new MParsedStatic(this.view, "EBDummyField", "StringStatic", Point.Empty, false);
					dummy.Size = new Size(Width, 1);
					dummy.BackColor = Color.Transparent;
					dummy.ForeColor = Color.Transparent;

					if (jsonEditorConnector != null && !string.IsNullOrEmpty(jsonEditorConnector.InitialJsonFile))
						OpenJsonForm(jsonEditorConnector.InitialJsonFile);
					Focus();
					if (!formEditorValid)
						mainForm?.HostedControl?.ExitFromCustomization();

				}
			}
			catch (EBFileNotFoundException ex)
			{
				MessageBox.Show(
					this,
					string.Format(Resources.EasyBuilderSourceFileNotFoundCannotEditThisStandardization, ex.MetaDataFile),
					Resources.EasyBuilder,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information
					);
				AskAndSave(false);
				return;
			}
			catch (NoSourcesOrDatException)
			{
                var uiService = GetService(typeof(IUIService)) as IUIService;
                uiService.ShowModalMessageBox(
                    ()
                    =>
                    MessageBox.Show(
                        this,
                        String.Format(Resources.NoSourcesAvailableCannotEdit, Resources.EasyBuilder),
                        Resources.EasyBuilder,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                        )
                    );

                OnRestartDocument(new RestartEventArgs(NameSpace.Empty, document.Namespace, RestartAction.RestartAndLoadAll));
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				OnRestartDocument(new RestartEventArgs(NameSpace.Empty, document.Namespace, RestartAction.RestartAndLoadAll));
			}

			finally
			{
				Cursor.Current = Cursors.Default;
				SuspendDirtyChanges = false;
                if (Controller != null && Controller.View != null)
                    Controller.View.Visible = true;
            }
		}

        //-----------------------------------------------------------------------------
        internal void HighlightContainer(TreeNode node)
		{
			if (node == null || containerHighlighter == null)
				return;
			Rectangle rect = containerHighlighter.GetRectFromContainer(node);
            if (rect == Rectangle.Empty)
                return;

			containerHighlighter.HighlightRect(RectangleToClient(rect), this);
			InvalidateEditor();
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		internal override void InitSources(Sources aSources)
		{
			ControllerSources controllerSources = sources = aSources as ControllerSources;
			if (controllerSources != null)
				controllerSources.CodeChanged += Sources_CodeChanged;

			Sources.CodeMethodEdit += Sources_CodeMethodEdit;
		}

		//-----------------------------------------------------------------------------
		void Sources_CodeMethodEdit(object sender, CodeMethodEditorEventArgs e)
		{
			OnOpenCodeEditor(e);
		}

		//-----------------------------------------------------------------------------
		void Sources_CodeChanged(object sender, CodeChangedEventArgs e)
		{
			SetDirty(true);
		}


		//-----------------------------------------------------------------------------
		void localizationManager_LocalizationChanged(object sender, EventArgs e)
		{
			mainForm.HostedControl.PopulateLocalizationStrings();
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);

			ShowHelp(null, RunAsEasyStudioDesigner);
			hevent.Handled = true;
		}

		//-----------------------------------------------------------------------------
		void SelectionService_SelectionChanging(object sender, EventArgs e)
		{
			NotifySelectionChanges(true);
		}

		//-----------------------------------------------------------------------------
		internal void SelectionService_SelectionChanged(object sender, EventArgs e)
		{
			IWindowWrapper wrapper = SelectionService.PrimarySelection as IWindowWrapper;
			if (SelectionService.SelectionCount > 0)
				SetCurrentWindow(wrapper, false);//può essere anche nullo
			else
				SetCurrentWindow(null, false);

			NotifySelectionChanges(false);

			//Se è stato selezionato un IWindowWrapper allora invalido
			//l'editor per ridisegnare i bordi della selezione degli oggetti.
			//Se non è un IWindowWrapper allora non invalido nulla perchè non è stato selezionato un oggetto
			//che il form editor sa disegnare e gestire.
			if (wrapper != null)
				InvalidateEditor();
		}

		//-----------------------------------------------------------------------------
		private bool NotifySelectionChanges(bool isChanging)
		{
			/*if (selections != null)
				selections.ClearSelections();
			if (SelectionService.SelectionCount == 0)
				return true;
                */
			foreach (var sel in SelectionService.GetSelectedComponents())
			{
				IWindowWrapper w = sel as IWindowWrapper;
				if (w == null)
					continue;
				if (containerHighlighter != null)
				{
					IWindowWrapper tile = (w is MTileDialog) ? w : (w.Parent is MTileDialog ? w.Parent : w);
					if (isChanging)
						containerHighlighter.SelectionChanging(tile);
					else
						containerHighlighter.SelectionChanged(tile);

				}
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		void Parent_SetFocus(object sender, EasyBuilderEventArgs e)
		{
			Focus();
		}

		//-----------------------------------------------------------------------------
		private void Parent_SizeChanged(object sender, EasyBuilderEventArgs e)
		{
			UpdateViewSize();
		}

		//-----------------------------------------------------------------------------
		private void Parent_ScrollChanged(object sender, EasyBuilderEventArgs e)
		{
			selections.AlignHighlightingRectangleToCurrentWindow();
			if (dummy != null)
				dummy.Location = Point.Empty;

            if (zIndexManager.Editing)
                Invalidate();
		}

		//-----------------------------------------------------------------------------
		private void UpdateViewSize()
		{
			if (view == null)
				return;

			if (RunAsEasyStudioDesigner)
			{
				Width = view.Size.Width - (view.VerticalScrollBar ? System.Windows.Forms.SystemInformation.VerticalScrollBarWidth : 0);
				Height = view.Size.Height - (view.HorizontalScrollBar ? System.Windows.Forms.SystemInformation.HorizontalScrollBarHeight : 0);
			}
			else
			{
				Width = Math.Max(view.Rectangle.Width, view.Size.Width) - (view.VerticalScrollBar ? System.Windows.Forms.SystemInformation.VerticalScrollBarWidth : 0);
				Height = Math.Max(view.FrameArea.Height, view.Size.Height) - (view.HorizontalScrollBar ? System.Windows.Forms.SystemInformation.HorizontalScrollBarHeight : 0);
			}

			if (dummy != null)
				dummy.Size = new Size(Width, 1);

			selections.AlignHighlightingRectangleToCurrentWindow();

			Invalidate();
		}

		//-----------------------------------------------------------------------------
		private void InitializeContextMenu()
		{
			this.ContextMenuStrip = cmsFormEditorContextMenu;
			this.ContextMenuStrip.Opening += new CancelEventHandler(ContextMenuStrip_Opening);
			tsDeleteItem.Click += new EventHandler(TsDeleteItem_Click);
			tsProperties.Click += new EventHandler(TsProperties_Click);

			tsCutItem.Click += new EventHandler(TsCutItem_Click);
			tsCopyItem.Click += new EventHandler(TsCopyItem_Click);
			tsPasteItem.Click += new EventHandler(TsPasteItem_Click);

			tsEditTile.Visible = !RunAsEasyStudioDesigner;
			tsEditTile.Click += TsEditTile_Click;
			tsPromoteItem.Visible = RunAsEasyStudioDesigner;
			tsPromoteItem.Click += TsPromoteItem_Click;
		}

		//-----------------------------------------------------------------------------
		void TsPromoteItem_Click(object sender, EventArgs e)
		{
			PromoteControls();
		}

		//-----------------------------------------------------------------------------
		internal void PromoteControls()
		{
			
			List<string> ids = new List<string>();
			using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))
			{
				if (mainForm == null)
					return;

				MainFormJsonEditor jsonEditor = (mainForm.HostedControl as MainFormJsonEditor);

				Cursor = Cursors.WaitCursor;
				if (jsonEditor?.JsonCodeControl == null)
					return;
				List<GenericWindowWrapper> ctrls = new List<GenericWindowWrapper>();
				foreach (var item in selections.Items)
				{
					IWindowWrapper bww = item.GetCurrentWindow();
					ids.Add(bww.Id);
					GenericWindowWrapper w = bww as GenericWindowWrapper;
					if (w == null)
						continue;
					ctrls.Add(w);
				}
				string json = jsonEditorConnector.Json;
				if (JsonProcessor.PromoteGenericControls(ref json, ctrls.ToArray()))
				{
					jsonEditorConnector.Json = json;
					RefreshWrappers(true);
				}
				Cursor = Cursors.Default;
			}
			List<BaseWindowWrapper> components = new List<BaseWindowWrapper>();
			foreach (var id in ids)
			{
				components.AddRange(view.GetChildrenByIdOrName(id, ""));
			}
			UpdateObjectViewModel(Controller);
			SelectionService.SetSelectedComponents(components);
		}

		//-----------------------------------------------------------------------------
		private void TsEditTile_Click(object sender, EventArgs e)
		{
			OnMouseDownOn(null, true);
		}

		//-----------------------------------------------------------------------------
		private void TsPasteItem_Click(object sender, EventArgs e)
		{
			Paste();
		}

		//-----------------------------------------------------------------------------
		private void TsCopyItem_Click(object sender, EventArgs e)
		{
			Copy();
		}

		//-----------------------------------------------------------------------------
		private void TsCutItem_Click(object sender, EventArgs e)
		{
			Cut();
		}

		//-----------------------------------------------------------------------------
		void TsProperties_Click(object sender, EventArgs e)
		{
			OnOpenProperties();
		}

		//-----------------------------------------------------------------------------
		internal void AddDbt(string tableName = "", MDBTObject dbt = null)
		{
			MDBTObject newDbt = null;
			IUIService uiSvc = this.GetService(typeof(IUIService)) as IUIService;
			if (uiSvc != null)
			{
				DBTEditor editor = new DBTEditor(Controller.Document, tableName, dbt);
				if (uiSvc.ShowDialog(editor) == DialogResult.OK)
					newDbt = editor.DataManager;
			}

			if (newDbt != null)
			{
				RefreshDocumentClass();
			}
		}

		//-----------------------------------------------------------------------------
		internal override void AddHotLink(AddHotLinkEventArgs e)
		{
			MHotLink hotLink = null;
			IUIService uiSvc = this.GetService(typeof(IUIService)) as IUIService;
			if (uiSvc == null)
				return;
			switch (e.Request)
			{
				case AddHotLinkEventArgs.RequestType.FromTable:
					{
						DataManagerEditor editor = new DataManagerEditor(e.Source);
						editor.ExistName = (name) => { return document.GetHotLink(name) != null; };
						if (uiSvc.ShowDialog(editor) == DialogResult.OK)
						{
							hotLink = new MHotLink(editor.TableName, editor.DataManagerName, document, false);
							NameSpace ns = new NameSpace(hotLink.Namespace.FullNameSpace);
							if (CustomizationContext.CustomizationContextInstance != null)
							{
								ns.Application = CustomizationContext.CustomizationContextInstance.CurrentApplication;
								ns.Module = CustomizationContext.CustomizationContextInstance.CurrentModule;
								ns.Library = NameSolverStrings.DynamicLibraryName;
								hotLink.PublicationNamespace = ns.FullNameSpace;
								hotLink.AddChangedProperty("PublicationNamespace");
							}
						}
						break;
					}
				case AddHotLinkEventArgs.RequestType.FromTemplate:
					{
						Func<string, bool> nameValidator = new Func<string, bool>(
							(string hklName) => document.GetHotLink(hklName) == null
							);
						HotLinkWrapperEditor editor = new HotLinkWrapperEditor(nameValidator, e.Source);
						if (uiSvc.ShowDialog(editor) == DialogResult.OK)
							hotLink = new MHotLink(editor.HotLinkNamespace, editor.HotLinkName, document, false);
						break;
					}
				case AddHotLinkEventArgs.RequestType.FromDocument:
					if (!string.IsNullOrWhiteSpace(e.Source))
						hotLink = new MHotLink(NameSpace.Empty, e.Source, document, true);
					break;
			}

			if (hotLink != null)
			{
				Controller.Document.Add(hotLink);
				hotLink.CallCreateComponents();
				hotLink.Searches.Initialize();
				RefreshDocumentClass();
				if (HotLinkAdded != null)
					HotLinkAdded(this, EventArgs.Empty);
			}
		}

		//-----------------------------------------------------------------------------
		internal void AddDataManager(AddDataManagerEventArgs args)
		{
			IUIService uiSvc = this.GetService(typeof(IUIService)) as IUIService;
			if (uiSvc == null)
				return;

			MDataManager dataManager = null;

			switch (args.RequestType)
			{
				case AddDataManagerEventArgs.DataManagerRequestType.FromTable:
					DataManagerEditor editor = new DataManagerEditor(args.TableName);
					editor.ExistName = (name) => { return document.GetDataManager(name) != null; };
					if (uiSvc.ShowDialog(editor) == DialogResult.OK)
					{
						dataManager = new MDataManager(editor.TableName, editor.DataManagerName, null);
						Controller.Document.Add(dataManager);
						dataManager.CallCreateComponents();
						RefreshDocumentClass();
					}
					return;
				default:
					break;
			}
		}

		//-----------------------------------------------------------------------------
		internal override void DeclareComponent(DeclareComponentEventArgs e)
		{
			ComponentDeclarator.ExecuteRequest(e.Request);
			UpdateController();
			UpdateObjectViewModel(Controller);
			SetDirty(true);
		}

		//-----------------------------------------------------------------------------
		internal void RefreshRecords()
		{
			if (Controller == null || Controller.Document.Master == null)
				return;

			((MSqlRecord)Controller.Document.Master.Record).RefreshFields();

			foreach (IComponent component in Controller.Document.Components)
			{
				IDataManager dataManager = component as IDataManager;
				if (dataManager == null)
					continue;

				((MSqlRecord)dataManager.Record).RefreshFields();
				// in caso di dbt faccio refresh anche sull'old
				MDBTObject dbt = component as MDBTObject;
				if (dbt != null)
					((MSqlRecord)dbt.OldRecord).RefreshFields();
			}
			RefreshDocumentClass();
		}

		//-----------------------------------------------------------------------------
		internal void RefreshObjectModel()
		{
			DesignerCurrentStatus oldStatus = new DesignerCurrentStatus();
			if (Controller != null && Controller.View != null)
				Controller.View.SaveCurrentStatus(oldStatus);

			//Da chiamare prima della Save in modo da sganciare l' oggetto correntemente
			//puntato da FormEditor/PropertyGrid affinche` la ricompilazione e la
			//conseguente re-istanziazione degli oggetti (che fa scattare la OnComponentChanged)
			//non li affligga mentre hanno in canna oggetti non piu` buoni.
			SelectObject(null);
			UpdateController();
			SetDirty(true);

			if (Controller != null && Controller.View != null)
				Controller.View.ApplyCurrentStatus(oldStatus);
		}

		/// <summary>
		/// Internal use.
		/// </summary>
		//-----------------------------------------------------------------------------
		public void RefreshDocumentClass()
		{
			if (Sources != null)
			{
				UpdateController();
				SetDirty(true);
			}
		}

		//-----------------------------------------------------------------------------
		internal void RefreshDocumentClassAndSelectObject(IComponent item)
		{
			string path = GetComponentPath(item);//metto da parte il percorso perche' la refreshdocumentclass mi ricrea tutti gli oggetti
			RefreshDocumentClass();
			SelectComponentromPath(path);
		}

		//-----------------------------------------------------------------------------
		private static string GetComponentPath(IComponent item)
		{
			return ReflectionUtils.GetComponentFullPath(item);
		}

		//-----------------------------------------------------------------------------
		private void SelectComponentromPath(string path)
		{
			IComponent cmp = ReflectionUtils.GetComponentFromPath(Controller, path); //recupero il nuovo oggetto corrispondente al path
																					 //lo seleziono e apro la finestra delle proprieta`
			ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
			if (selectionService != null)
				selectionService.SetSelectedComponents(new Object[] { cmp });

			IWindowWrapper w = cmp as IWindowWrapper;
			if (w != null)
				selections.Add(cmp as IWindowWrapper, true);
			else
				ClearSelections();
		}

		//-----------------------------------------------------------------------------
		void TsDeleteItem_Click(object sender, EventArgs e)
		{
			TryDeleteEasyBuilderComponents(selections.Components);
		}

		//-----------------------------------------------------------------------------
		internal void TryDeleteEasyBuilderComponents(IWindowWrapper[] components)
		{
            if (MessageBox.Show(Resources.AreYouSureDeleteItem, Resources.DeleteItemCaption, MessageBoxButtons.YesNo) == DialogResult.No)
                return;

			List<EasyBuilderComponent> ebComponents = new List<EasyBuilderComponent>();
			foreach (var item in components)
				if (item is EasyBuilderComponent)
					ebComponents.Add(item as EasyBuilderComponent);

			TryDeleteEasyBuilderComponents(ebComponents);
            Invalidate();
		}

		//-----------------------------------------------------------------------------
		internal void TryDeleteEasyBuilderComponents(List<EasyBuilderComponent> components)
		{

			string whyNot;
			StringBuilder whyNots = new StringBuilder();
			foreach (EasyBuilderComponent item in components)
				if (!CanDeleteEasyBuilderComponent(item, out whyNot))
				{
					if (whyNots.Length > 0)
						whyNots.Append(Environment.NewLine);
					whyNots.Append(whyNot);
				}

			if (whyNots.Length > 0)
			{
				MessageBox.Show(this, whyNots.ToString(), Resources.DeleteItemCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			foreach (var item in components)
				DeleteEasyBuilderComponent(item);
		}

		//-----------------------------------------------------------------------------
		internal bool CanDeleteEasyBuilderComponent(EasyBuilderComponent ebComponent, out string whyNot)
		{
			whyNot = string.Empty;
			if (ebComponent == null)
			{
				whyNot = "This is not a EasyBuilderComponent";
				return false;
			}

            IContainer container = ebComponent as IContainer;
            if (container != null && container.Components.Count > 0 && container is BaseWindowWrapper)
            {
                whyNot = string.Format(Resources.ObjectNotEmpty, ebComponent.SerializedName);
                return false;
            }

			string referencedByList = ebComponent.GetReferencedByList();
			if (!string.IsNullOrEmpty(referencedByList))
			{
				StringBuilder msg = new StringBuilder();
				msg.AppendFormat(Resources.CannotDeleteObject, ebComponent.SerializedName);
				msg.Append("\n");
				msg.Append(referencedByList);
				msg.Append(Resources.DeleteReferencingObjects);
				msg.Append("\n");
				whyNot = msg.ToString();
				return false;
			}

            if (ebComponent.ChangedEventsCount > 0)
            {
                StringBuilder msg = new StringBuilder();
                msg.AppendFormat(Resources.CannotDeleteObject, ebComponent.SerializedName);
                msg.Append("\n");
                msg.Append(ebComponent.ChangedEvents.ToString());
                msg.Append(Resources.DeleteReferencingObjects);
                msg.Append("\n");
                whyNot = msg.ToString();
                return false;
            }


			if (ebComponent.CanBeDeleted)
				return true;
			whyNot = string.Format(Resources.CannotDeleteHasCodeBehind, ebComponent.SerializedName);
			return false;
		}

		//-----------------------------------------------------------------------------
		internal void DeleteEasyBuilderComponent(EasyBuilderComponent ebComponent)
		{
			DeleteObjectEventArgs args = new DeleteObjectEventArgs(ebComponent);
			try
			{
                RemoveControlLocalizations(ebComponent, sources);

                // i referenced component non possono rientrare nel meccanismo standard perchè
                // non appartengono alla serializzazione del grappolo di classi del controller
                // ma hanno una loro grammatica specifica nel ns addizionale e inoltre non hanno
                // oggetti visivi da selezionare e aggiornare
                ReferenceableComponent refComponent = ebComponent as ReferenceableComponent;
				if (refComponent != null)
				{
					/*bool removeDeclaration = SafeMessageBox.Show(
						this,
						string.Format(Resources.DeleteDeclarationToo, refComponent.Description),
						NameSolverStrings.EasyStudioDesigner,
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2
						)
						== DialogResult.Yes;*/

					ComponentDeclarator.Remove(refComponent, true);
					RefreshObjectModel();
					OnComponentDeleted(args);
					return;
				}

				IWindowWrapper window = ebComponent as IWindowWrapper;
				if (RunAsEasyStudioDesigner && window != null)
					DeleteWindowDescription(window.Handle);

				//check for dynamic hotlink.xml and if any, delete	
				MHotLink hotlink = ebComponent as MHotLink;
				if (hotlink != null)
					serializationAddOnService.RemoveHotLink(hotlink);

				BaseWindowWrapper baseWrapper = ebComponent as BaseWindowWrapper;
				if (baseWrapper != null && !RunAsEasyStudioDesigner)
					View.RemoveLayoutObjectOn(baseWrapper.Namespace);

				IContainer container = ebComponent?.Site?.Container as IContainer;
				container?.Remove(ebComponent);

				if (sources != null)
                {
                    EasyBuilderSerializer.RemoveClass(sources.CustomizationInfos.EbDesignerCompilationUnit, ebComponent);
                }

				selections.RemoveSelection(window);
				SelectionService.SetSelectedComponents(null);
				ebComponent.Dispose();

				if (Sources != null && container != null)
				{
					if ((container is IWindowWrapper))
					{
						UpdateObjectViewModel(ebComponent);
					}
					else
						RefreshDocumentClass();
				}

			}
			catch (Exception) { }

			OnComponentDeleted(args);

            SetDirty(true);
		}

        //-----------------------------------------------------------------------------
        private static void RemoveControlLocalizations(IComponent component, Sources sources)
        {
            Debug.Assert(component != null);
            Debug.Assert(component.Site != null);
            Debug.Assert(sources != null);
            Debug.Assert(sources.Localization != null);

            if (component is IWindowWrapperContainer container)
            {
                foreach (IComponent childComponent in container.Components)
                {
                    RemoveControlLocalizations(childComponent, sources);
                }
            }

            sources?.Localization.RemoveControlLocalization(component.Site.Name);
        }

        //-----------------------------------------------------------------------------
        void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			Point p = MousePosition;

			IWindowWrapper tempWrapper = GetChildFromPos(p);
			if (tempWrapper == null)
			{
				e.Cancel = true;
				return;
			}
			bool hasCodeBehind = false;
			if (tempWrapper is BaseWindowWrapper)
				hasCodeBehind = ((BaseWindowWrapper)tempWrapper).HasCodeBehind;
			else if (tempWrapper is MBodyEditColumn)
				hasCodeBehind = ((MBodyEditColumn)tempWrapper).HasCodeBehind;
			tsDeleteItem.Enabled = !hasCodeBehind;

			toolStripSeparator2.Visible = tsCutItem.Visible = tsPasteItem.Visible = tsCopyItem.Visible = RunAsEasyStudioDesigner;

			tsPasteItem.Enabled = Clipboard.ContainsText(TextDataFormat.UnicodeText);

			tsCutItem.Enabled = tsCopyItem.Enabled = !(tempWrapper is MEasyStudioPanel);

			if (!RunAsEasyStudioDesigner && containerHighlighter != null)
				tsEditTile.Visible = (SelectionService.PrimarySelection is MTileDialog) && (!containerHighlighter.IsTheUniqueTile(view, SelectionService.PrimarySelection));

			tsPromoteItem.Enabled = false;

			MainFormJsonEditor jsonEditor = (mainForm.HostedControl as MainFormJsonEditor);
			if (jsonEditor != null && jsonEditor.JsonCodeControl != null)
			{
				foreach (var sel in selections.Items)
				{
					if (sel.GetCurrentWindow() is GenericWindowWrapper)
					{
						tsPromoteItem.Enabled = true;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Compila al volo il codice del controller e delle classi collegate e ne istanzia uno
		/// </summary>
		//-----------------------------------------------------------------------------
		internal override bool Build(bool debug, string filePath, bool generateSources = true)
		{
            if (generateSources && this.controller != null)
            {
                SourcesSerializer.GenerateClass(Sources.CustomizationInfos.EbDesignerCompilationUnit, Controller);

                if (!filePath.IsNullOrEmpty())
                {
                    using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
                    {
                        ControllerSources?.CreateDelta(this.controller);
                    }
                }
            }

            EBCompilerResults results = sourcesBuilder.Build(Sources, debug, filePath);

			if (results.Errors.Count == 0)
			{
				if (!filePath.IsNullOrEmpty() && this.controller != null)
				{
					SourcesSerializer.GenerateClass(sources.CustomizationInfos.EbDesignerCompilationUnit, document);
					results = sourcesBuilder.Build(Sources, debug);
				}
				CreateControllerSafe(string.Empty, view.Handle, results);
				if (this.controller != null)
					RefreshIntellisense();

				OnCreateControllerCompleted(EventArgs.Empty);
				return true;
			}
            if (mainForm != null)
			    mainForm.HostedControl.CollectAndSaveCodeChanges(true);
			return false;
		}

		//-----------------------------------------------------------------------------
		internal void RefreshIntellisense()
		{
			sources.InitializeProjectContent(customizationNamespace.Leaf);
			//eliminare ciò che è stato rinominato od eliminato
			//inserire ciò che ho appena buildato, ma che non è ancora su fileSystem
		}

		//-----------------------------------------------------------------------------
		private void AddReferenceToComponents()
		{
			if (ControllerSources == null)
				return;
			
			//List<EasyBuilderComponent> componentsToUpdate = new List<EasyBuilderComponent>();
			//List<EasyBuilderComponent> currentComponents;
			
			foreach (MethodDeclaration method in ControllerSources.GetUserMethods())
			{
				ControllerSources.GetComponentsToUpdate(method.Name, Controller, null);
				//foreach (EasyBuilderComponent item in currentComponents)
				//{
				//	if (!componentsToUpdate.Contains(item))
				//		componentsToUpdate.Add(item);
				//}
			}
		}

		//-----------------------------------------------------------------------------
		private void CreateControllerSafe(string filePath, IntPtr viewPtr, EBCompilerResults results)
		{
			DocumentControllers controllers = new DocumentControllers();
			if (InvokeRequired)
			{
				this.Invoke((Action)delegate { CreateControllerSafe(filePath, viewPtr, results); });
				return;
			}
			using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
			{
				if (string.IsNullOrEmpty(filePath))
					controllers.LoadAssembliesForEdit(document.Namespace, results.CompiledAssembly, document.GetDocumentPtr(), viewPtr);
				else
					controllers.LoadAssembliesForEdit(document.Namespace, filePath, document.GetDocumentPtr(), viewPtr);

				if (controllers.Count <= 0)
					return;

				Controller = controllers[0] as DocumentController;
				if (view == null)
					return;

				//ho rigenerato il controller: devo ricreare tutte i controlli customizzati
				//altrimenti mi perdo qualche pezzo quando poi risalverò successivamente
				//(infatti, siccome le tab non muoiono, non verrebbe chiamata al CreateComponents a seguito della
				try
				{
					view.KeepTabsAlive = true;

					//controller.CallCreateComponents();//creo i wrapper per gli oggetti customizzati
					view.SwitchVisibility(Settings.Default.ShowHiddelFields);   //visualizzo i campi nascosti per poterli modificare

					controller.CreateComponents();
					document.CallCreateComponents();
					view.CallCreateComponents();

					view.CreateWrappers(HandlesToSkip);  //creo i wrapper per gli oggetti NON customizzati

					// la AdjustSite va fatta prima di bindare il data model in modo che i 
					// sites vengano sistemati sull'istanza corretta degli oggetti del data model
					TBSite.AdjustSites(this);

					if (!RunAsEasyStudioDesigner)
						ControllerSources?.GenerateFieldDeclarations(view);
					view.KeepTabsAlive = false;

					// ora aggiorno il nuovo controllo con le reference attuali
					if (ComponentDeclarator != null)
						ComponentDeclarator.AdjustAllReferencedByOn(controller);

				}
				catch (Exception exc)
				{
					controllers.LogExceptionsToFile(Sources?.Namespace, exc);
				}
			}

		}

		//-----------------------------------------------------------------------------
		private void InitializeFormEditor(DockPanel hostingPanel, IUserFeedback log)
		{
			log.SetMessage(Resources.InitSources);

			//Causa un crash sulla creazione del nuovo documento in modo random (piu frequente in debug)
			hostingPanel.Refresh();

			log.SetMessage(Resources.InitController);
			if (RunAsEasyStudioDesigner)
			{
				view.KeepTabsAlive = true;
				IntPtr handle = view.SendMessage(ExternalAPI.WM_COMMAND, Commands.CmdCreateJsonEditor, IntPtr.Zero);
				if (handle != IntPtr.Zero)
				{
					GCHandle gch = (GCHandle)handle;
					jsonEditorConnector = gch.Target as IJsonEditor;
					if (jsonEditorConnector != null)
						DirtyChanged += jsonEditorConnector.OnFormEditorDirtyChanged;
					gch.Free();
				}
			}
			else
			{
				containerHighlighter = new ContainerEditingHighlighter();
				TemplateService = new TemplatesService();
				TemplateService.TemplateAdded += TemplateService_TemplateAdded;
				TemplateService.RemovingTemplate += TemplateService_RemovingTemplate;

				//La init references non deve segnalare che il codice è cambiato per non causare
				//ricompilazioni inutili in apertura della customizzazione
				//bool added = Sources.CustomizationInfos.RefreshUsings();
				//if (added)
				//	Sources.OnCodeChanged();
			}

			log.SetMessage(Resources.InitMainForm);
			CreateMainForm(hostingPanel);

			log.Show(false);

			if (Controller == null)
			{
				Build(true, string.Empty);
				log.Show(true);
			}
			else
			{
                log.Show(true);

				//Aggiorniamo l'object model, questa operazione e' fatta di default nella 
				//Build controller, ma in questo caso noi abbiamo gia' un controller valido e non 
				//viene ne' ricaricato ne' buildato.
				//Inoltre creiamo tutti i wrapper per la customizzazione di cui stiamo entrando in modifica.
				log.SetMessage(Resources.BuildModel);
				if (Controller.View.CreateWrappers(HandlesToSkip) && !RunAsEasyStudioDesigner)
					ControllerSources?.GenerateFieldDeclarations(view);

				Controller.View.SwitchVisibility(Settings.Default.ShowHiddelFields);//visualizzo i campi nascosti per poterli modificare
																					// la AdjustSites va fatta prima della bind per poter sistemare correttamente i sites
																					// nelle istanze degli oggetti del data model corretti
				TBSite.AdjustSites(this);
                Results result = null;
                if (!RunAsEasyStudioDesigner && Sources != null)
                {
                    result = SourcesChangesSeeker.SeekChanges(this, CalculateOutputFileFullName(Sources.Namespace));
                    formEditorValid = !result.Cancel;
                    // ricompila la modifica dei sorgenti
                    if (result != null && result.ExecuteBuild)
                        Build(true, result.Path, false);
                    // questa compilazione è quella che si occupa dell'integrazione
                    Build(true, string.Empty, true);
                }

            }

			//Va chiamata dopo InitializeFormEditor perchè li dentro avviene la AdjustSites che mette
			//a posto i site usati per recuperare il component dato il suo fullpath
			//SourceVisitor visitor = new SourceVisitor(Controller, ControllerSources.CustomizationInfos.EbDesignerCompilationUnit);
			//Sources?.CustomizationInfos.EbDesignerCompilationUnit.AcceptVisitor(visitor, null);
		}

        /// <remarks/>
        public static void RestoreThreadStaticController(DocumentController controller)
        {
            var controllerStaticField = controller.GetType().GetField("controller", BindingFlags.Public | BindingFlags.Static);
            controllerStaticField.SetValue(controller, controller);
        }

        //-----------------------------------------------------------------------------
        private void TemplateService_RemovingTemplate(object sender, TemplatesServiceEventArgs e)
		{
			mainForm.HostedControl.RefreshTemplates();
		}

		//-----------------------------------------------------------------------------
		private void TemplateService_TemplateAdded(object sender, TemplatesServiceEventArgs e)
		{
			mainForm.HostedControl.RefreshTemplates();
		}

		//-----------------------------------------------------------------------------
		internal void RenameTemplate(string oldName, string newName)
		{
			if (templateService != null)
				templateService.RenameTemplate(oldName, newName);
		}

		//-----------------------------------------------------------------------------
		internal bool DeleteTemplate(string name)
		{
			return templateService != null ? templateService.DeleteTemplate(name) : false;
		}

		//-----------------------------------------------------------------------------
		private void SetHasCodeBehind(ComponentCollection components, bool set)
		{
			foreach (IComponent cmp in components)
			{
				if (cmp is BaseWindowWrapper)
				{
					((BaseWindowWrapper)cmp).HasCodeBehind = set;
					foreach (EasyBuilderComponentExtender ext in ((BaseWindowWrapper)cmp).Extensions)
					{
						ext.HasCodeBehind = set;
					}
					if (cmp is WindowWrapperContainer)
					{
						SetHasCodeBehind(((WindowWrapperContainer)cmp).Components, set);
					}
					else if (cmp is MBodyEdit)
					{
						foreach (MBodyEditColumn col in ((MBodyEdit)cmp).ColumnsCollection)
							col.HasCodeBehind = set;
					}
				}

			}
		}

		//-----------------------------------------------------------------------------
		private void CreateMainForm(DockPanel hostingPanel)
		{
			if (!RunAsEasyStudioDesigner)
			{
				mainForm = TBDockContent<MainFormEasyStudio>.CreateDockablePane
									(
										hostingPanel,
										WeifenLuo.WinFormsUI.Docking.DockState.DockTop,
										Resources.EasyBuilderIcon,
										hostingPanel,
										this
									) as ITBDockContent<MainForm>;
			}
			else
			{
				mainForm = TBDockContent<MainFormJsonEditor>.CreateDockablePane
									(
										hostingPanel,
										WeifenLuo.WinFormsUI.Docking.DockState.DockTop,
										Resources.EasyBuilderIcon,
										hostingPanel,
										this
									);
			}

			DockContent main = mainForm as DockContent;
			if (main == null)
				return;

			main.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
			main.FormClosed += new FormClosedEventHandler(MainForm_FormClosed);

		}

		//-----------------------------------------------------------------------------
		void ComponentRemoved(object sender, ComponentEventArgs e)
		{
			//non chiamare la SetDirty qui: chiamarla solo nelle azioni di editing del FormManager
		}

		//-----------------------------------------------------------------------------
		void ComponentAdded(object sender, ComponentEventArgs e)
		{
			//non chiamare la SetDirty qui: chiamarla solo nelle azioni di editing del FormManager
		}

		//-----------------------------------------------------------------------------
		void ComponentChanged(object sender, ComponentChangedEventArgs e)
		{
			//se siamo nel DocOutline, gestisco in modo diverso: update code editor e selection nel code del component
			DocOutlineProperties windowProperties = e.Component as DocOutlineProperties;
			if (windowProperties != null)
			{
				mainForm.HostedControl.UpdateDocOutline(null);
				UpdateJsonCodeSelection(windowProperties.Id);
				return;
			}

			IWindowWrapper windowWrapper = e.Component as IWindowWrapper;
			
			if (windowWrapper == null)
			{
				EasyBuilderComponentExtender extender = e.Component as EasyBuilderComponentExtender;
				if (extender == null)
					return;
				windowWrapper = extender.ParentComponent as IWindowWrapper;

				if (windowWrapper == null)
					return;
			}

			if (RunAsEasyStudioDesigner)
			{
				UpdateWindowDescription(windowWrapper, e.Member.Name, e.NewValue);
			}
			//non chiamare la SetDirty qui: chiamarla solo nelle azioni di editing del FormManager

			UpdateChangedRectangle(windowWrapper, e);

			OnSelectedObjectUpdated(e);
		}

		/// <summary>
		/// Verifica se sono state cambiate le proprietà Location o Size del control 
		/// e scatena l'aggiornamento a video del rettangolo di evidenziazione.
		/// Altrimenti ridisegna solo la cornice rossa di selezione.
		/// </summary>
		/// <param name="windowWrapper"></param>
		/// <param name="e"></param>
		/// <remarks>
		/// Se è la property grid ad aver sollevato l'evento allora le coordinate
		/// mi arrivano in spazio parent del control modificato, quindi, siccome tutta
		/// la logica di disegno del form editor ragiona in coordinate screen, devo
		/// riportarle in coordinate screen.
		/// </remarks>
		//-----------------------------------------------------------------------------
		private void UpdateChangedRectangle(IWindowWrapper windowWrapper, ComponentChangedEventArgs e)
		{
			Selection sel = selections.GetSelection(windowWrapper);
			if (sel == null || windowWrapper.Parent == null)
				return;

			Rectangle rectangle = sel.HighlightingRectangle;

			bool locationChanged = false;
			bool sizeChanged = false;

			//Se sono cambiate le proprietà Location o Size allora sposto anche il rettangolo
			//di evidenziazione
			if (e.Member.Name == ReflectionUtils.GetPropertyName(() => new Control().Location))
			{
				Point newLoc = (Point)e.NewValue;
				//Traduco indietro in coordinate screen la location.
				windowWrapper.Parent.ClientToScreen(ref newLoc);
				if (newLoc != rectangle.Location)
				{
					rectangle.Location = newLoc;
					locationChanged = true;
				}
			}

			if (
				e.Member.Name == ReflectionUtils.GetPropertyName(() => new Control().Size) &&
				rectangle.Size != (Size)e.NewValue
				)
			{
				rectangle.Size = (Size)e.NewValue;
				sizeChanged = true;
			}

			if (locationChanged || sizeChanged)
				sel.HighlightingRectangle = rectangle;

			Invalidate(false, true, true, sel.HighlightingRectangle, sel.TooltipRectangle);
		}

		//-----------------------------------------------------------------------------
		void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Dispose();
		}

		//-----------------------------------------------------------------------------
		void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (askAndSaveOnExit)
				e.Cancel = !AskAndSave(false);
		}

		/// <summary>
		/// Chiede all'utente se vuole salvare le modifiche effettuate prima di uscire dal documento
		/// </summary>
		/// <param name="closingDocument">specifica se la chiusura è stata forzata manualmente dall'utente</param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		public bool AskAndSave(bool closingDocument)
		{
			if (RunAsEasyStudioDesigner)
			{
				if (!IsDirty || SaveJson(true, false))
				{
					if (!closingDocument)
						document.PostMessageUM(ExternalAPI.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
					return true;
				}
				return false;
			}
			//Se non c'è niente da salvare non salvo
			if (!IsDirty)
			{
				//restarto il documento solamente se è un'azione scatentata dalla chiusura della main form fatta dall'utente
				if (!closingDocument)
				{
					RestartEventArgs ev = new RestartEventArgs(customizationNamespace, documentNamespace, RestartAction.RestartAndLoadAll);
					OnRestartDocument(ev);
				}

				return true;
			}

			//Metto da parte il namespace del documento per poterne fare un restart alla fine.
			//Siamo in chusura: il documento va ri-lanciato sia che le modifiche siano state salvate
			//sia che non siano state salvate.
			//Nel secondo caso serve ri-lanciare il documento per eliminare eventuali modifiche che ho fatto,
			//per esempio spostando dei control, ma che non ho salvato.
			//Se non ri-lancio il documento all'uscita del form editor queste non vengono scaricate ed il risultato è che mi rimane un
			//documento con le modifiche che ho fatto e non ho salvato. In più non vengono ricaricate le altre customizzazioni che
			//erano state scaricate all'entrata in modifica.
			INameSpace documentToBeRestartedNamespace = document.Namespace;

			//Salvo la customizzazione
			bool result = true;
			if (EBLicenseManager.CanISave)
				result = Save(true, true);

			//Se necessario restarto il documento
			if (result)
			{
				if (closingDocument)
					SetDirty(false);
				else
					OnRestartDocument(new RestartEventArgs(NameSpace.Empty, documentToBeRestartedNamespace, RestartAction.RestartAndLoadAll));
			}

			return result;
		}

		/// <summary>
		/// Effettua il salvataggio della customizzazione
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		internal override bool Save(bool askForOptions)
		{
			DesignerCurrentStatus oldStatus = new DesignerCurrentStatus();
			if (Controller != null && Controller.View != null)
				Controller.View.SaveCurrentStatus(oldStatus);

			//Da chiamare prima della Save in modo da sganciare l' oggetto correntemente
			//puntato da FormEditor/PropertyGrid affinche` la ricompilazione e la
			//conseguente re-istanziazione degli oggetti (che fa scattare la OnComponentChanged)
			//non li affligga mentre hanno in canna oggetti non piu` buoni.
			SelectObject(null);
			bool result = Save(askForOptions, false);

			if (Controller != null && Controller.View != null)
				Controller.View.ApplyCurrentStatus(oldStatus);
			return result;
		}

        ///<remarks />
		//-----------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			DoPaint(e.Graphics);
		}

		//-----------------------------------------------------------------------------
		private void DoPaint(Graphics g)
		{
			if (view == null)
				return;
			try
			{
                WindowWrapperContainer wndContainer = selections?.Parent;
                if (wndContainer == null)
                {
                    view.Invalidate();
                    view.UpdateWindow();
                }
                else
                {
                    wndContainer.Invalidate();
                    wndContainer.UpdateWindow();
                }

                if (zIndexManager.Editing)
					zIndexManager.DrawZIndexMarkers(g, view);
				else
				{
					if (!startSelectionClientPoint.IsEmpty && !endSelectionClientPoint.IsEmpty)
					{
						Rectangle r = CalculateSelectionRect();
						r.Inflate(-1, -1);
						g.DrawRectangle(Pens.Red, r);
					}
					selections.DoPaint(g, editingMode);
				}
			}
			catch
			{
			}
		}


		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		protected override CreateParams CreateParams
		{
			//trucco per gestire la trasparenza del controllo
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT
				return cp;
			}
		}

		//-----------------------------------------------------------------------------
		internal ContainerEditingHighlighter ContainerHighlighter
		{
			get { return containerHighlighter; }

			set { containerHighlighter = value; }
		}

		internal TemplatesService TemplateService
		{
			get
			{
				return templateService;
			}

			set
			{
				templateService = value;
			}
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			//trucco per gestire la trasparenza del controllo
			//non chiamare la base!!!
		}

		//--------------------------------------------------------------------------------
		private void AddTemplateFromToolbox(string name, IWindowWrapper parentWindow, Point screenPoint)
		{
			if (templateService == null)
				return;

			EasyStudioTemplate template = templateService[name];
			if (template == null || !template.IsValid)
				return;

			Type rootType = template.ViewModelRootType;
			if (rootType == null)
				return;

			IWindowWrapperContainer container = parentWindow as IWindowWrapperContainer;
			IDesignerTarget target = container as IDesignerTarget;
			IWindowWrapper rootWrapper = null;
			if (target == null || target.CanDropTarget(rootType))
			{
				template.NeedCreateWindow += Template_NeedCreateWindow;
				rootWrapper = template.ApplyViewModel(container, screenPoint);
				template.NeedCreateWindow -= Template_NeedCreateWindow;
				// l'integrazione con il layout la chiamo solo alla fine di tutto 
				// in modo da avere un unica passata già ben composta
				if (rootWrapper != null && rootWrapper.Parent != null)
				{
					View.LayoutChangedFor(rootWrapper.Parent.Namespace);
				}
				SetDirty(true);
				View.RequestRelayout();
			}
		}

		//--------------------------------------------------------------------------------
		private void Template_NeedCreateWindow(object sender, EasyStudioTemplateWndCreateEventArgs e)
		{
			e.Wrapper = CreateWindowWrapper(e.Info.ControlType, e.Info.ControlClass, e.Info.Caption, e.Info.ScreenLocation, e.Info.Parent);
			if (e.Wrapper == null)
				return;

			IDesignerTarget target = e.Wrapper.Parent as IDesignerTarget;
			if (target != null)
				target.AfterTargetDrop(e.Wrapper.GetType());
		}

		//--------------------------------------------------------------------------------
		private void FormEditor_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Effect == DragDropEffects.None)//non faccio l'and bitwise perchè None = 0;
				return;

			SetDirty(true);

			TbToolBoxItem draggedObject = (TbToolBoxItem)e.Data.GetData(typeof(TbToolBoxItem));

			//se è un oggetto droppato dalla toolbox, lo aggiungo e lo seleziono
			if (draggedObject != null)
			{
				if (templateService != null && draggedObject.ControlType == typeof(EasyStudioTemplate))
				{
					Point screenPoint = new Point(e.X, e.Y);
					AddTemplateFromToolbox(draggedObject.Caption, GetChildFromPos(screenPoint), screenPoint);
					return;
				}

				IWindowWrapper newWrapper = AddControlFromToolbox(e, draggedObject);
				if (newWrapper == null)
					return;

				SelectObject(newWrapper);
				Focus();
				if (RunAsEasyStudioDesigner)
				{
					AddWindowDescription(newWrapper.Handle, newWrapper.Parent.Handle);

					if (IsAutomaticPosition(newWrapper))
					{
						MEasyStudioPanel panel = newWrapper.Parent as MEasyStudioPanel;
						BaseWindowWrapper bww = newWrapper as BaseWindowWrapper;

						string anchor = "COL1";
						if (panel.TileDialogSize == ETileDialogSize.Wide && newWrapper.Location.X > panel.Size.Width / 2)
							anchor = "COL2";
						bww.Anchor = anchor;
						OnComponentPropertyChanged(bww, "Anchor", "", anchor);

					}
				}
				SetDirty(true);

				return;
			}

			//altrimenti se vengo dal datamodel, lo aggiungo e basta, senza selezionarlo in questo modo se
			//devo droppare tanti control, non mi viene tutte le volte spostato il tree dell'object model
			AddControlFromDataModel(e);

			//deseleziono eventuali altri oggetti
			SelectObject(null);
		}

		//-----------------------------------------------------------------------------
		private bool IsAutomaticPosition(IWindowWrapper newWrapper)
		{
			MEasyStudioPanel panel = newWrapper.Parent as MEasyStudioPanel;
			BaseWindowWrapper bww = newWrapper as BaseWindowWrapper;
			return panel != null &&
				panel.PanelType == EPanelType.Tile &&
				bww != null &&
				!(bww is MBodyEdit) &&
				!(bww is MPropertyGrid) &&
				!(bww is WindowWrapperContainer);
		}

		//-----------------------------------------------------------------------------
		private void FormEditor_DragOver(object sender, DragEventArgs e)
		{
			Point screenPoint = new Point(e.X, e.Y);


            //Se sto droppando un oggetto dal datamodel, è possibile farlo solamente per dbt e dataobj 
            //dbt e dataobj hanno DataBinding diverso da nullo
            //oppure per hotlink, che ha databinding nullo
            DataModelDropObject dataModelDrop = (DataModelDropObject)e.Data.GetData(typeof(DataModelDropObject));

            //recupero il controllo associato al punto cliccato
            IWindowWrapper topMostChild = GetChildFromPos(screenPoint, dataModelDrop == null);
            IDesignerTarget target = topMostChild as IDesignerTarget;

			if (dataModelDrop != null && target != null)
			{
				e.Effect = DragDropEffects.None;

				if (dataModelDrop.DataBinding != null && target.CanDropData(dataModelDrop.DataBinding))
				{
					e.Effect = DragDropEffects.Copy;
					return;
				}

				if (dataModelDrop.Component is MHotLink && target.CanDropTarget(dataModelDrop.Component.GetType()))
				{
					MHotLink hotLink = dataModelDrop.Component as MHotLink;
					MParsedControl ctrl = target as MParsedControl;
					string error = string.Empty;
					if (
							ctrl?.DataBinding != null && hotLink.CanBeAttached((DataType)ctrl.CompatibleType, ctrl.MaxLength, ref error)
						)
					{

						e.Effect = DragDropEffects.Copy;
						return;
					}
				}
			}

			//Se non trovo child significa che il mouse sta gravitando direttamente sopra alla view.
			if (topMostChild == null)
				topMostChild = Controller.View as DocumentView;

			//Se sto droppando un oggetto dalla toolBox :
			TbToolBoxItem draggedObject = (TbToolBoxItem)e.Data.GetData(typeof(TbToolBoxItem));
			if (draggedObject == null)
				return;
			
			if (ImEditingOnlyThisTile)
			{
				while (topMostChild != null)
				{
					target = topMostChild as IDesignerTarget;

					if (target != null && target.CanDropTarget(draggedObject.ControlType))
					{
						e.Effect = (target is MTileGroup && ImEditingOnlyThisTile) ?
							DragDropEffects.None : DragDropEffects.Copy;
						return;
					}
					topMostChild = topMostChild.Parent;
				}
				e.Effect = DragDropEffects.None;
			}
			else
				e.Effect = target != null && target.CanDropTarget(draggedObject.ControlType)
				? e.Effect = DragDropEffects.Copy
				: e.Effect = DragDropEffects.None;
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
		
			if (e.KeyCode == Keys.ControlKey && zIndexManager.Editing)
			{
				zIndexManager.ShowArrows = !zIndexManager.ShowArrows;
				InvalidateEditor();
			}

			if (e.Control)
			{
				switch (e.KeyCode)
				{
					case Keys.Left:
						if (e.Shift)//Ctrl + Shift + Left Arrow allinea a sinistra
							PerformAlignAction(Selections.Action.AlignLeft);
						break;
					case Keys.Up:
						if (e.Shift)//Ctrl + Shift + Up Arrow allinea a sinistra
							PerformAlignAction(Selections.Action.AlignTop);
						break;
					case Keys.Right:
						if (e.Shift)//Ctrl + Shift + Right Arrow allinea a sinistra
							PerformAlignAction(Selections.Action.AlignRight);
						break;
					case Keys.Down: 
						if (e.Shift)//Ctrl + Shift + Down Arrow allinea a sinistra
							PerformAlignAction(Selections.Action.AlignBottom);
						break;
					case Keys.F9: //Ctrl + Shift + F9 effettua l'allineamento orizzontale //Ctrl + F9 effettua l'allineamento verticale
						PerformAlignAction(e.Shift? Selections.Action.CenterHorizontally : Selections.Action.CenterVertically) ;
						return; 
					case Keys.F10: OnRequestedSave(e);								return; //Ctrl + F10 effettua il salvataggio della customizzazione
					case Keys.C: Copy();											return; //Ctrl + C copia i controlli
					case Keys.D: OnRequestedObjectModel(e);							return; //Ctrl + D va sul panel Document Model	//ex onrequestedDOCUMENTmodelreturn;	
					case Keys.E: PerformAlignAction(Selections.Action.SameSize);	return; //Ctrl + E imposta la size(dimensione) dei selezionati uguale per tutti
					case Keys.H: PerformAlignAction(Selections.Action.SameHeight);  return; //Ctrl + H imposta l'altezza dei selezionati uguale per tutti
					case Keys.M: OnRequestedOpenCodeEditor(e);  					return; //Ctrl + M va sul panel MethodOutline
					case Keys.O: OnRequestedOptions(e);								return; //Ctrl + O apre le options
					case Keys.S: Save(false);										return;	//Ctrl + S fa scattare il Save
					case Keys.V: Paste();											return;	//Ctrl + V incolla i controlli
					case Keys.W: PerformAlignAction(Selections.Action.SameWidth);	return; //Ctrl + W imposta la larghezza dei selezionati uguale per tutti
					case Keys.X: Cut();												return;	//Ctrl + X taglia i controlli
					case Keys.Y: Redo();											return; //Ctrl + Y Redo
					case Keys.Z: Undo();											return;	//Ctrl + Z Undo
					default: break;
				}			
			}

			if (selections.Count == 0)
				return;

			int xOffset = 0, yOffset = 0;
			switch (e.KeyValue)
			{
				//La nuova posizione del controllo è la location modificata
				//di tot pixel nella direzione della freccia premuta.
				case 40://Down
					yOffset += arrowKeysMovementOffset;
					break;
				case 37://Left
					xOffset -= arrowKeysMovementOffset;
					break;
				case 39://Right
					xOffset += arrowKeysMovementOffset;
					break;
				case 38://Up
					yOffset -= arrowKeysMovementOffset;
					break;
				case 46://Cancel
					{
						TryDeleteEasyBuilderComponents(selections.Components);
                        return;
					}
				default:
					return;
			}

			numberOfFollowingKeyDown++;

			//Se il numero di keyDown consecutivi (tasto premuto) è maggiore della mia soglia AND
			//se l'offset per lo spostamento non è ancora superiore dell'offset massimo che mi sono dato
			//allora raddoppio l'offset dello spostamento.
			if (
				numberOfFollowingKeyDown > followingKeyDownThreshold &&
				arrowKeysMovementOffset < arrowKeysMovementMaxOffset
				)
			{
				arrowKeysMovementOffset *= 2;
				//Se l'offset dello spostamento ha superato l'offset massimo che mi sono dato allora lo limito
				//proprio all'offset massimo.
				if (arrowKeysMovementOffset > arrowKeysMovementMaxOffset)
					arrowKeysMovementOffset = arrowKeysMovementMaxOffset;
			}

			editingMode = GetEditingMode(e);
			if (selections.Count != 0)
			{
				if (RunAsEasyStudioDesigner)
				{
					Point pt = new Point(xOffset, yOffset);
					if (selections.MainSelectedWindow.Parent != null)
					{
						//la funzione di mapping lavora solo con numeri positivi
						//in EasyStudio devo spostarmi di unità logiche, non pixel
						((WindowWrapperContainer)selections.MainSelectedWindow.Parent).ToPixels(ref pt);
					}
					selections.KeyMove(editingMode, pt.X, pt.Y);
				}
				else
				{
					selections.KeyMove(editingMode, xOffset, yOffset);
				}
			}

		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			switch (e.KeyValue)
			{
				case 115://F4
					if (selections.Count != 0)
						OnOpenProperties();
					break;
				case 40://Down
				case 37://Left
				case 39://Right
				case 38://Up
					numberOfFollowingKeyDown = 0;
					arrowKeysMovementOffset = initialArrowKeysMovementOffset;
					FinalizeMoveAction(false);
					break;
				case 27://Esc
					mainForm.Close();
					break;
			}
		}


		//--------------------------------------------------------------------------------
		internal void ProcessMouseMoveAction(Point newScreenPosition)
		{
			if (lastDrawPosition == newScreenPosition)
				return;
			long currentTickCount = DateTime.Now.Ticks;
			if (currentTickCount - lastDrawTickCount < delta)
				return;

			lastDrawPosition = newScreenPosition;
			if (zIndexManager.Editing)
			{
				zIndexManager.ProcessMoveAction(newScreenPosition, GetChildFromPos(newScreenPosition) as BaseWindowWrapper);
				return;
			}
			if (!startSelectionClientPoint.IsEmpty)
			{
				Rectangle r = CalculateSelectionRect();
				InvalidateEditor(r);
				endSelectionClientPoint = PointToClient(newScreenPosition);
				r = CalculateSelectionRect();
				InvalidateEditor(r);
				return;
			}
			selections.DoResizeOrMove(editingMode, newScreenPosition, false);

			this.lastDrawTickCount = currentTickCount;

		}

		//--------------------------------------------------------------------------------
		private Rectangle CalculateSelectionRect()
		{
			Point topLeft = new Point(Math.Min(startSelectionClientPoint.X, endSelectionClientPoint.X), Math.Min(startSelectionClientPoint.Y, endSelectionClientPoint.Y));
			Point bottomRigth = new Point(Math.Max(startSelectionClientPoint.X, endSelectionClientPoint.X), Math.Max(startSelectionClientPoint.Y, endSelectionClientPoint.Y));
			return new Rectangle(topLeft, new Size(bottomRigth.X - topLeft.X, bottomRigth.Y - topLeft.Y));
		}

		//--------------------------------------------------------------------------------
		private void FinalizeMoveAction(bool copyActive)
		{
			if (zIndexManager.Editing)
			{
				zIndexManager.FinalizeMoveAction();
				return;
			}
			if (!startSelectionClientPoint.IsEmpty && !endSelectionClientPoint.IsEmpty)
			{
				Rectangle r = CalculateSelectionRect();
				if (r.Width > 0 && r.Height > 0)
				{
					InvalidateEditor(r);
					ClearSelections();
					SelectWindowsInRect(RectangleToScreen(r));
					SelectionService.SetSelectedComponents(selections.Components);
				}
				startSelectionClientPoint = endSelectionClientPoint = Point.Empty;
				return;
			}
			startSelectionClientPoint = endSelectionClientPoint = Point.Empty;
			JsonUpdateSession updateSession = null;
			if (copyActive && jsonEditorConnector != null)
				updateSession = new JsonUpdateSession(jsonEditorConnector.Json);

			List<BaseWindowWrapper> newComponents = new List<BaseWindowWrapper>();
			foreach (Selection sel in selections.Items)
			{
				IWindowWrapper currentWindow = sel.GetCurrentWindow();
				if (sel.Modified && currentWindow != null && editingMode != EditingMode.None)
				{
					WindowWrapperContainer parentWindow = (WindowWrapperContainer)currentWindow.Parent;
					if (parentWindow != null)
					{
						if (copyActive)
						{
							Point clientLocation = sel.HighlightingRectangle.Location;
							parentWindow.ScreenToClient(ref clientLocation);
							parentWindow.ToLogicalUnits(ref clientLocation);
							updateSession?.Clone(currentWindow.Id, parentWindow.Id, clientLocation);
						}
						else
						{
							//Se il controllo è stato creato dal form editor allora lo aggiungo alla collezione di components del parent.
							if (!parentWindow.HasControl(currentWindow.Handle))
								AddControlInEditingMode(parentWindow, currentWindow as BaseWindowWrapper);

                            UpdateComponentRectangle(parentWindow, currentWindow, sel.HighlightingRectangle);
						}
					}

					//Se non sono ancora dirty e il rettangolo finale è diverso da quello del control di partenza allora imposto 
					//il dirty
					if (!IsDirty)
						SetDirty(true);


				}
				sel.EndDrag();
                Invalidate(false, true, true, sel.HighlightingRectangle, sel.TooltipRectangle);
            }

            if (updateSession != null)
			{
				jsonEditorConnector.Json = updateSession.Json;
				RefreshWrappers(true);
			}
			if (newComponents.Count > 0)
			{
				SelectionService.SetSelectedComponents(newComponents);
			}
			editingMode = EditingMode.None;

            //MView wrapperView = View as MView;
            //if (wrapperView != null)
            //    wrapperView.SuspendLayout = false;
        }
		//-----------------------------------------------------------------------------
		private void Copy()
		{
			if (jsonEditorConnector == null)
				return;
			JsonProcessor.CopyControls(jsonEditorConnector.Json, selections.Components);
		}
		//-----------------------------------------------------------------------------
		private void Cut()
		{
			if (jsonEditorConnector == null)
				return;
			if (JsonProcessor.CopyControls(jsonEditorConnector.Json, selections.Components))
				TryDeleteEasyBuilderComponents(selections.Components);

		}
		//-----------------------------------------------------------------------------
		private void Paste()
		{
			if (selections.Count != 1 || jsonEditorConnector == null)
				return;
			IWindowWrapper current = selections.MainSelection.GetCurrentWindow();
			if (current == null)
				return;
			WindowWrapperContainer parentWindow = current is WindowWrapperContainer ? (WindowWrapperContainer)current : current.Parent as WindowWrapperContainer;
			if (parentWindow == null)
				return;

			string newJson = JsonProcessor.PasteControls(jsonEditorConnector.Json, parentWindow.Id);
			if (!string.IsNullOrEmpty(newJson))
			{
				jsonEditorConnector.Json = newJson;
				RefreshWrappers(true);
				/*List<BaseWindowWrapper> components = View.GetChildrenByIdOrName(id, "");

				SelectionService.SetSelectedComponents(components);*/
			}
			/*
			List<BaseWindowWrapper> newComponents = new List<BaseWindowWrapper>();
			try
			{
				foreach (var currentWindow in data.List)
				{
					if (currentWindow == null || currentWindow.IsDisposed)
						continue;

					Point loc = Point.Empty;
					//la finestra potrebbe appartenere ad un altro thread, dirotto sul thread giusto la chiamata
					data.Context.Send((SendOrPostCallback) =>
					{
						loc = currentWindow.Location;

					}, null);

					loc.Offset(10, 10);
					BaseWindowWrapper copy = (BaseWindowWrapper)CreateWindowWrapper(currentWindow.GetType(), string.Empty, string.Empty, loc, parentWindow);

					System.Attribute[] attribs = new System.Attribute[2];
					attribs[0] = new BrowsableAttribute(true);
					attribs[1] = new ReadOnlyAttribute(false);

					PropertyDescriptorCollection sourceProperties = currentWindow.GetProperties(attribs);
					PropertyDescriptorCollection targetProperties = copy.GetProperties(attribs);

					foreach (PropertyDescriptor sourceProp in sourceProperties)
					{
						//queste proprietà non vanno copiate
						if (NonCopyableProp(sourceProp.Name))
							continue;
						object o = null;
						//la finestra potrebbe appartenere ad un altro thread, dirotto sul thread giusto la chiamata
						data.Context.Send((SendOrPostCallback) =>
						{
							o = sourceProp.GetValue(currentWindow);

						}, null);

						PropertyDescriptor targetProp = targetProperties.Find(sourceProp.Name, false);

						targetProp.SetValue(copy, o);
					}
					string id = currentWindow.Id, name = currentWindow.Name;
					data.Context.Send((SendOrPostCallback) =>
						{
							copy.CreateUniqueNameAndId(parentWindow, name, id);

						}, null);

					if (RunAsEasyStudioDesigner)
					{
						AddWindowDescription(copy.Handle);
					}
					newComponents.Add(copy);
					SetDirty(true);
				}
				if (data.DeleteOrigin)
				{
					data.Context.Post((SendOrPostCallback) =>
					{
						data.Editor.TryDeleteEasyBuilderComponents(data.List.ToArray());

					}, null);

				}
				if (newComponents.Count > 0)
				{
					SelectionService.SetSelectedComponents(newComponents);
				}
					catch (Exception ex)
			{
				MessageBox.Show(mainForm, ex.Message, null);
			}*/
		}
		
		//-----------------------------------------------------------------------------
		private bool NonCopyableProp(string propName)
		{
			return propName == "Id" ||
				propName == "Name" ||
				propName == "LocationLU" ||
				propName == "TabOrder" ||
				propName == "StateButton";

		}
		//-----------------------------------------------------------------------------
		private void SelectWindowsInRect(Rectangle rectangle)
		{
			foreach (IWindowWrapper child in selectionContainer.Components)
			{
				Rectangle r = child.Rectangle;
				if (rectangle.IntersectsWith(r))
					selections.Add(child, false);
			}
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			//Non tengo conto del fatto che l'utente possa aver invertito tasto
			//dx e sx del mouse da pannello di controllo perchè .NET, a prescindere dalle impostazioni,
			//fa sempre arrivare i bottoni del mouse come fossero a default.

			Point screenPoint = MousePosition;

			Capture = true;

			if (view == null)
				return;

			IWindowWrapper child = null;

			//Se ho fatto click su uno dei gripper allora il control selezionato
			//rimane quello che è attualmente, parent compreso.
			if ((child = selections.GetWindow(screenPoint)) == null)
			{
				//altrimenti recupero il controllo associato al punto cliccato
				child = GetChildFromPos(screenPoint);
			}

			//Nell'EasyStudioDesigner la view non deve rispondere a nulla
			if (child == view && RunAsEasyStudioDesigner)
			{
				Capture = false;
				return;
			}

			//mando il click al controllo sottostante se necessario
			IDesignerTarget target = child as IDesignerTarget;
			if (target != null && target.MouseDownTarget)
				target.OnMouseDown(screenPoint);

			if (!selections.AmIWorkingOnSelectedControl(child, screenPoint))
			{
				//Se non si è fatto clic su nessun controllo (child == null) allora azzera la selezione,
				//altrimenti aggiorna la selezione.
				SelectObject(child, screenPoint);
			}
			else
			{
				Selection sel = selections.GetSelection(child);
				if (sel != null)
				{
					selections.MakeActive(sel);

					//Invalido solo i margini per disegnare le linee che aiutano nell'allineamento.
					Invalidate(true, false, true, sel.HighlightingRectangle, sel.TooltipRectangle);
				}
			}

			if (containerHighlighter != null)
				OnMouseDownOn(e, false);

            //MView wrapperView = View as MView;
            //if (wrapperView != null)
            //    wrapperView.SuspendLayout = true;

            editingMode = selections.GetAcceptableEditingMode(screenPoint);
			if (editingMode != EditingMode.None)
			{
				SetCursor(e.Button);
                selections.InitDrag(screenPoint);
			}
			else
			{
				IWindowWrapper w = child == null ? View : child;

				startSelectionClientPoint = PointToClient(screenPoint);
				selectionContainer = w is IWindowWrapperContainer ? (IWindowWrapperContainer)w : w.Parent;

			}
			Focus();

			if (zIndexManager.Editing)
			{
				EasyBuilderControl main = selections.MainSelectedWindow as EasyBuilderControl;
				if (main != null && main.Parent != null)
					zIndexManager.CurrentWindow = main;
			}
		}

		//-----------------------------------------------------------------------------
		private void OnMouseDownOn(MouseEventArgs e, bool UniqueTileORSaveExitEvent)
		{
			bool ImEditingOnlyThisTile_before = ImEditingOnlyThisTile;
			containerHighlighter.OnMouseDownOn(SelectionService.GetSelectedComponents(), e, UniqueTileORSaveExitEvent);
			if (ImEditingOnlyThisTile_before != ImEditingOnlyThisTile)
			{
				tsEditTile.Text = ImEditingOnlyThisTile ?
					EditAllTilesString : EditOnlyThisTileString;
				mainForm.HostedControl.ChangeEnablePropertyEditor();
				mainForm.HostedControl.ChangeFilterViewTree(ImEditingOnlyThisTile);
			}

		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			try
			{
				if (!Capture)
					return;

                MView wrapperView = View as MView;
                if (wrapperView != null)
                    wrapperView.SuspendLayout = true;

                ProcessMouseMoveAction(MousePosition);

               wrapperView = View as MView;
                if (wrapperView != null)
                    wrapperView.SuspendLayout = false;
            }
			finally
			{
				SetCursor(e.Button);
			}
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			FinalizeMoveAction(IsCopyActive());

			Capture = false;
		}

		//--------------------------------------------------------------------------------
		private IWindowWrapper GetChildFromPos(Point screenPoint, bool bodyEditColumnSelectionAllowed = true)
		{
			IList<IWindowWrapper> children = new List<IWindowWrapper>();
			view.GetChildrenFromPos(screenPoint, Handle, children);
			children.Add(view);

			IWindowWrapper child = SelectionHelper.CalculateSelected(children, this);
			IWindowWrapper realChild = CalculateRealChild(screenPoint, ref child);

            if (!bodyEditColumnSelectionAllowed && realChild is MBodyEditColumn)
                realChild = ((MBodyEditColumn)realChild).BodyEdit;

            return realChild;

        }

		//--------------------------------------------------------------------------------
		private IWindowWrapper CalculateRealChild(Point screenPoint, ref IWindowWrapper child)
		{
			//se child ==null significa che ho cliccato in un area fuori dalla tile editata al momento
			if (child == null && SelectionService.PrimarySelection != null)
			{
				if (SelectionService.PrimarySelection is MTileDialog)
					child = SelectionService.PrimarySelection as IWindowWrapper;
				else if (SelectionService.PrimarySelection is MParsedControl)
				{
					if ((SelectionService.PrimarySelection as MParsedControl).Parent is MTileDialog)
						child = (SelectionService.PrimarySelection as MParsedControl).Parent as IWindowWrapper;
				}
			}
			IWindowWrapper realChild = null;
			if (child != null && child.Parent != null)
			{
				realChild = (child is MTileDialog) ?
					child : (child.Parent is MTileDialog ? child.Parent : null);
				if (realChild != null && !realChild.Rectangle.Contains(screenPoint))
					return child.Parent;
			}
			return child;
		}

		//--------------------------------------------------------------------------------
		internal bool CanBeSelected(IWindowWrapper wrapper)
		{
			if (containerHighlighter != null && containerHighlighter.ImEditingOnlyThisTile)
				return containerHighlighter.CanBeSelected(wrapper);
			return true;
		}

		//--------------------------------------------------------------------------------
		private bool UpdateComponentRectangle(IWindowWrapperContainer parent, IWindowWrapper currentWindow, Rectangle newRect)
		{
            BaseWindowWrapper wrapper = currentWindow as BaseWindowWrapper;
            wrapper.EndCreation = true;
			Rectangle oldRect = currentWindow.Rectangle;
			if (oldRect == newRect)
				return false;

			IComponent currentWindowAsIComponent = currentWindow as IComponent;
			if (oldRect.Location != newRect.Location)
			{
				OnComponentPropertyChanging(currentWindowAsIComponent, "Location");

				//Porto le coordinate in spazio client per assegnarle e lanciare l'evento di component changed.
				//In questo modo il comportamento è allineato a quello che tiene la property grid.
				Point clientNewLocation = newRect.Location;
				parent.ScreenToClient(ref clientNewLocation);
				currentWindow.Location = clientNewLocation;

				Point clientOldLocation = oldRect.Location;
				parent.ScreenToClient(ref clientOldLocation);

				OnComponentPropertyChanged(currentWindowAsIComponent, "Location", clientOldLocation, clientNewLocation);
			}
			if (oldRect.Size != newRect.Size)
			{
				OnComponentPropertyChanging(currentWindowAsIComponent, "Size");

				currentWindow.Size = newRect.Size;

				OnComponentPropertyChanged(currentWindowAsIComponent, "Size", oldRect.Size, newRect.Size);
			}
			return true;
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		internal virtual void OnComponentPropertyChanging(IComponent component, string changingPropertyName)
		{
			PropertyDescriptorCollection propDescs = TypeDescriptor.GetProperties(component.GetType());
			if (propDescs == null || propDescs.Count == 0)
				return;

			PropertyDescriptor pDesc = propDescs[changingPropertyName];
			if (pDesc == null)
				return;

			ComponentChangeService.OnComponentChanging(component, pDesc);
		}

		//-----------------------------------------------------------------------------
		internal virtual void OnComponentPropertyChanged(
			IComponent component,
			string changingPropertyName,
			object oldValue,
			object newValue
			)
		{
			PropertyDescriptorCollection propDescs = TypeDescriptor.GetProperties(component.GetType());
			if (propDescs.Count == 0)
				return;

			PropertyDescriptor pDesc = propDescs[changingPropertyName];
			if (pDesc == null)
				return;

			ComponentChangeService.OnComponentChanged(component, pDesc, oldValue, newValue);


			EasyBuilderComponent ebComponent = component as EasyBuilderComponent;
			if (ebComponent == null)
				return;
			ebComponent.IsChanged = true;

            while (ebComponent.ParentComponent != null)
			{
				EasyBuilderComponent temp = ebComponent.ParentComponent;
				temp.IsChanged = true;
				ebComponent = ebComponent.ParentComponent;
			}
		}


		//--------------------------------------------------------------------------------
		private IWindowWrapper AddControlFromDataModel(DragEventArgs e)
		{
			DataModelDropObject dropObject = (DataModelDropObject)e.Data.GetData(typeof(DataModelDropObject));
			if (dropObject == null)
				return null;

			try
			{
				if (dropObject.Component is MHotLink)
				{
					//recupero il controllo associato al punto cliccato
					MParsedControl ctrl = GetChildFromPos(new Point(e.X, e.Y)) as MParsedControl;
					if (ctrl == null)
						return null;
					MHotLink old = ctrl.HotLink;
					ctrl.HotLink = (MHotLink)dropObject.Component;
					//Segnalo che è cambiato il componente affinchè venga serializzato
					OnComponentPropertyChanged(ctrl, ReflectionUtils.GetPropertyName(() => ctrl.HotLink), old, ctrl.HotLink);

					return null;

				}
				Point screenpoint = new Point(e.X, e.Y);
				IWindowWrapper droppedOn = GetChildFromPos(screenpoint, false);
				if (droppedOn == null)
					droppedOn = Controller.View as DocumentView;

                IWindowWrapper wrapper = null;
				//se ho droppato su un databinding consumer, allora non devo creare un controllo
				//ma assegnare il databinding al consumer
				if (droppedOn is IDataBindingConsumer)
				{
					wrapper = droppedOn;
				}
				else
				{
					string controlClass = string.Empty;
					Type type = MParsedControl.GetDefaultControlType(dropObject.DataBinding.DataType, dropObject.DataBinding.IsDataReadOnly, ref controlClass);

                    IDesignerTarget target = droppedOn as IDesignerTarget;

                    if (target != null && !target.CanDropTarget(type))
                        return null;

                    wrapper = CreateWindowWrapper(type, controlClass, dropObject.SourceNodeName, screenpoint);//screen coordinates
				}

				if (wrapper is IDataBindingConsumer consumer)
                {
                    if (consumer.CanAutoFillFromDataBinding)
                    {
                        consumer.AutoFillFromDataBinding(dropObject.DataBinding.Clone() as IDataBinding, false);//Clono perchè dropObject verrà disposato

                        if (consumer is MBodyEdit bodyEdit)
                        {
                            //Fix an. 26620: dico che sono cambiati tutti i titoli delle colonne in modo che il localization manager aggiunga i titoli come stringe localizzabili
                            foreach (var column in bodyEdit.ColumnsCollection)
                            {
                                OnComponentPropertyChanged(column, ReflectionUtils.GetPropertyName(() => column.ColumnTitle), string.Empty, column.ColumnTitle);
                            }
                        }
                    }
                    else
                    {
                        consumer.DataBinding = dropObject.DataBinding.Clone() as IDataBinding;//Clono perchè dropObject verrà disposato
                    }
                    OnComponentPropertyChanged(wrapper as IComponent, EasyBuilderSerializer.DataBindingPropertyName, null, consumer?.DataBinding);
                }

				return wrapper;
			}
			finally
			{
				dropObject?.Dispose();
			}
		}

		//--------------------------------------------------------------------------------
		private IWindowWrapper AddControlFromToolbox(DragEventArgs e, TbToolBoxItem draggedObject)
		{
			Point location = new Point(e.X, e.Y);

			IDesignerTarget target = null;
			IWindowWrapper wrapper = GetChildFromPos(location);
			while (wrapper != null)
			{
				target = wrapper as IDesignerTarget;

				if (target != null && target.CanDropTarget(draggedObject.ControlType))
				{
					target = wrapper as IDesignerTarget;
					break;
				}
				wrapper = wrapper.Parent;
			}

			if (target != null && target.CanUpdateTarget(draggedObject.ControlType))
			{
				target.UpdateTargetFromDrop(draggedObject.ControlType);
				return wrapper;
			}

			//Se sto droppando una tab sulla view, creo un tabber vuoto prima, così non sono obbligato 
			//a fare l'operazione in due step
			IWindowWrapperContainer parentContainer = GetParentContainer(location);
			if (draggedObject.ControlType == typeof(MTab) && parentContainer.GetType().IsSubclassOf(typeof(DocumentView)))
				CreateWindowWrapper(typeof(MTabber), string.Empty, string.Empty, location);

			parentContainer = target as IWindowWrapperContainer;
			IWindowWrapper wrap = null;
			if (parentContainer == null)
				wrap = CreateWindowWrapper(draggedObject.ControlType, string.Empty, string.Empty, location);
			else
				wrap = CreateWindowWrapper(draggedObject.ControlType, string.Empty, string.Empty, location, parentContainer);

			target?.AfterTargetDrop(draggedObject.ControlType);

			// integro il layout solo dalla toolbox perche' gli oggetti coinvolti
			// possono arrivarre solo da lì
			if (wrap != null && wrap.Parent != null)
			{
				WindowWrapperContainer parent = wrap.Parent as WindowWrapperContainer;
				if (parent != null)
				{
					View.LayoutChangedFor(parent.Namespace);
					UpdateObjectViewModel(parent);
				}
			}
			return wrap;
		}

		//--------------------------------------------------------------------------------
		private IWindowWrapper CreateWindowWrapper(
			Type type,
			string controlClass,
			string controlCaption,
			Point screenLocation)
		{
			IWindowWrapperContainer parentContainer = GetParentContainer(screenLocation);
			if (parentContainer == null)
				return null;
			return CreateWindowWrapper(type, controlClass, controlCaption, screenLocation, parentContainer);
		}

		//--------------------------------------------------------------------------------
		private IWindowWrapper CreateWindowWrapper(
			Type type,
			string controlClass,
			string controlCaption,
			Point clientLocation,
			IWindowWrapperContainer parentContainer)
		{
			try
			{
				parentContainer.ScreenToClient(ref clientLocation);

				//creazione dinamica del wrapper: deve avere un costruttore con almeno tre parametri,
				//gli altri vengono valorizzati col valore nullo (caso della label che prende una location,
				//che tanto in questo contesto non serve per crearla (non devo trovarla, 
				//parto proprio da una ben precisa e mi ci devo appiccicare)

				int index = 1;
				//il name del control sarà basato sulla caption più indice numerico, la caption però non deve essere modificata
				string controlName = controlCaption;
				if (string.IsNullOrEmpty(controlName))
					controlName = string.Empty;//per esd
				else
					while (parentContainer.HasComponent(controlName))
						controlName = controlCaption + (index++).ToString();

				object[] parms = null;
				ConstructorInfo[] ctors = type.GetConstructors();
				foreach (ConstructorInfo ctor in ctors)
				{
					int nParams = ctor.GetParameters().Length;
					if (nParams < 3)
						continue;
					parms = new object[nParams];
					parms[0] = parentContainer;
					parms[1] = controlName;
					parms[2] = controlClass;
					parms[3] = clientLocation;

					//se ci sono ulteriri parametri, li riempio col valore null
					for (int i = 4; i < nParams; i++)
						parms[i] = null;
					break;
				}

				//Creo l'istanza dell'oggetto via reflection specificando il type
				BaseWindowWrapper wrapper = (BaseWindowWrapper)System.Activator.CreateInstance(type, parms);
				//inizialmente messo come controllo, non funziona con oggetti a creazione ritardata come la tab
				//if (wrapper.Handle.Equals(IntPtr.Zero))
				//	return null;

				//va fatta prima di cambiarne la size perché serve il Site
				AddControlInEditingMode(parentContainer, wrapper);

				//Essendo creato un wrapper con una CreateInstance  di cui non sappiamo nulla se non il tipo, 
				//l'unico modo per fare una "post initialization" del wrapper è avere un metodo virtuale che ogni tipo
				//di control può reimplementare per farcire un oggetto appena creato con modifiche valide solamente 
				//quando siamo in design
				IDesignerTarget target = wrapper as IDesignerTarget;
				if (target != null)
					target.OnDesignerControlCreated();

				if (target == null || (target.DesignerMovable == EditingMode.All))
					AdjustWrapperSizeToAvoidIntersections(parentContainer, wrapper);

				//Dico che sono cambiati location e size affinchè il controllo venga serializzato.
				OnComponentPropertyChanged(wrapper, ReflectionUtils.GetPropertyName(() => wrapper.Location), Point.Empty, wrapper.Location);
				OnComponentPropertyChanged(wrapper, ReflectionUtils.GetPropertyName(() => wrapper.Size), Size.Empty, wrapper.Size);

				if (!controlCaption.IsNullOrEmpty())
				{
                    //se si tratta di un parsed control e mi e stata passata una caption
                    //la imposto e ne forzo la serializzazione
                    if (wrapper is MParsedControl parsedControl)
                    {
                        parsedControl.Caption = controlCaption;
                        string captionPropertyName = ReflectionUtils.GetPropertyName(() => parsedControl.Caption);
                        //Segnalo che è cambiato il componente affinchè LocalizationManager possa aggiornare le risorse con la
                        //localizzaizone della proprietà Text.
                        OnComponentPropertyChanged(parsedControl, captionPropertyName, string.Empty, parsedControl.Caption);

                    }
                }

                wrapper.EndCreation = true;
				return wrapper;
			}
			catch (Exception e)
			{
				Debug.Fail(e.ToString());
				return null;
			}
		}

		//--------------------------------------------------------------------------------
		private IWindowWrapperContainer GetParentContainer(Point screenLocation)
		{
			IWindowWrapperContainer parentContainer = null;
			IWindowWrapper dropTarget = GetChildFromPos(screenLocation);
			IWindowWrapperContainer dropTargetContainer = dropTarget as IWindowWrapperContainer;
			parentContainer = dropTarget == null ? Controller.View
												: (
														dropTargetContainer != null ?
														dropTargetContainer :
														dropTarget.Parent
													);

			return parentContainer;
		}

		/// <summary>
		/// determina se vi sono intersezioni fra il rettangolo della finestra che si sta droppando ed eventuali contenitori fratelli.
		/// //in caso affermativo, ridimensiona la finestra in modo da evitare sovrapposizioni, tenendone fermo il top left
		/// </summary>
		/// <param name="parentContainer"></param>
		/// <param name="wrapper"></param>
		//--------------------------------------------------------------------------------
		private static void AdjustWrapperSizeToAvoidIntersections(IWindowWrapperContainer parentContainer, BaseWindowWrapper wrapper)
		{
			Rectangle winRect = wrapper.Rectangle;
			foreach (IComponent item in parentContainer.Components)
			{
				WindowWrapperContainer container = item as WindowWrapperContainer;
				if (container == null || container == wrapper) //posso sovrappormi a me stesso, ignoro
					continue;

				Rectangle siblingContainerRectangle = container.Rectangle;
				if (!winRect.IntersectsWith(siblingContainerRectangle))
					continue;//nessuna intersezione, ignoro
				siblingContainerRectangle.Intersect(winRect);

				//calcolo l'area rimanente in caso di 'taglio' per orizzontale e per verticale
				int remainingWidth = siblingContainerRectangle.Left - winRect.Left - 1;
				int remainingHeight = siblingContainerRectangle.Top - winRect.Top - 1;
				int remainingAreaAfterWidthCut = remainingWidth * winRect.Height;
				int remainingAreaAfterHeightCut = remainingHeight * winRect.Width;

				//taglio in modo da massimizzare l'area rimanente
				if (remainingAreaAfterWidthCut > remainingAreaAfterHeightCut)
					winRect.Width = remainingWidth;
				else
					winRect.Height = remainingHeight;
			}
			wrapper.Size = winRect.Size;
		}

		//--------------------------------------------------------------------------------
		private void SetCursor(MouseButtons mb)
		{
			Point p = MousePosition;

			if (zIndexManager.Editing)
			{
				zIndexManager.SetCursor(p);
				return;
			}
			if (selections.SetCursor(p, mb))
				return;

			Cursor = Cursors.Default;

		}

		//--------------------------------------------------------------------------------
		private static EditingMode GetEditingMode(KeyEventArgs k)
		{
			bool shiftPressed = k.Shift;
			switch (k.KeyValue)
			{
				case 38://Up
				case 40://Down
					{
						if (!(shiftPressed = k.Shift) && !k.Control)
							return EditingMode.Moving;

						return shiftPressed ? EditingMode.ResizingMidTop : EditingMode.ResizingMidBottom;
					}
				case 37://Left
				case 39://Right
					{
						if (!(shiftPressed = k.Shift) && !k.Control)
							return EditingMode.Moving;

						return shiftPressed ? EditingMode.ResizingMidRight : EditingMode.ResizingMidLeft;
					}
				default:
					return EditingMode.None;
			}
		}

		//--------------------------------------------------------------------------------
		private void SelectObject(IWindowWrapper wrapper)
		{
			Point point = Point.Empty;
			SelectObject(wrapper, point);
		}

		//--------------------------------------------------------------------------------
		private void SelectObject(IWindowWrapper wrapper, Point screenPoint)
		{
			if (wrapper == null)
				return;

			//Nel caso io abbia cliccato su una linguetta del tabber, il wrapper è il tabber stesso
			//ma per comodità d'uso, recupero la vera tab cliccata e seleziono quella (e non il tabber)
			MTabber tabber = wrapper as MTabber;
			if (tabber != null)
			{
				MTab tab = tabber.GetTabByPoint(screenPoint);
				if (tab != null)
					wrapper = tab as IWindowWrapper;
			}

			MTileManager tileManager = wrapper as MTileManager;
			if (tileManager != null)
			{
				MTileGroup tileGroup = tileManager.GetTabByPoint(screenPoint);
				if (tileGroup != null)
					wrapper = tileGroup as IWindowWrapper;
			}

			SetCurrentWindow(wrapper, IsMultiSelectionSalvatoreQuasimode());
			OnSelectedObjectChanged(new SelectedObjectEventArgs(wrapper));

			SelectionService.SetSelectedComponents(selections.Components);
		}

		//-----------------------------------------------------------------------------
		internal string CallAddItem(string directoryPath)
		{
			string filePath = mainForm.HostedControl.AddItem(null, directoryPath, true);
			return Path.GetFileNameWithoutExtension(filePath);
		}

		//--------------------------------------------------------------------------------
		private void AddControlInEditingMode(IWindowWrapperContainer parentWindow, EasyBuilderControl control)
		{
			parentWindow.Add(control);
		}

		//--------------------------------------------------------------------------------
		internal void UpdateObjectViewModel(IComponent component)
		{
			if (mainForm == null || !CanUpdateObjectModel)
				return;
			mainForm.HostedControl.UpdateObjectViewModel(component);
		}

		//---------------------------------------------------------------------
		private void SelectFileInTree(string file)
		{
			mainForm.HostedControl.SelectFileInTree(file);
		}

		//--------------------------------------------------------------------------------
		private void RemoveControl(IWindowWrapperContainer parentWindow, EasyBuilderComponent control)
		{
			try
			{
				//Rimuove il control dall'array di components e scatena la distruzione dell'oggetto
				parentWindow.Remove(control);
				control.Dispose();
			}
			catch { }
		}

		//--------------------------------------------------------------------------------
		private bool Save(bool askForOptions, bool leavingCustomization)
		{
			if (RunAsEasyStudioDesigner)
				return SaveJson(false, askForOptions);

			NameSpace ns = Sources == null ? null : Sources.Namespace;
			if (!EBLicenseManager.CanISave)
			{
				IUIService uiSvc = GetService(typeof(IUIService)) as IUIService;
				uiSvc?.ShowMessage(
					Resources.FunctionalityNotAllowedWithCurrentActivation,
					NameSolverStrings.EasyStudioDesigner,
					MessageBoxButtons.OK
					);

				return false;
			}
			try
			{
				DialogResult res = DialogResult.Cancel;

				bool existing = CustomizationInfos.ExistCustomizationName(ns, BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
				//se sto creando un nuovo documento, di default pubblico la customizzazione, o sono in edit di un serverdoc creato con EB
				bool publish = newDocument || BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationType == ApplicationType.Standardization;
				bool isActive = newDocument; //se sto inserendo un nuovo documento, la prima è la sua customizzazione di default
											 //Se il path della customizzazione esiste, allora è già stata salvata e non chiedo niente, 
											 //altrimenti chiedo il nome della customizzazione
                bool saveForWeb = true;    // TODOWEB mettere a true quando serve a noi

				if (existing)
				{
					//l'utente ha chiesto di salvare oppure sto inserendo un nuovo documento: non chiedo nulla, la risposta implicita è YES
					if (newDocument && !askForOptions)
						res = DialogResult.Yes;
					else
					{
						if (askForOptions)
							res = SaveCustomization.SaveExistingCustomization(this, Resources.SaveCustomization, Resources.SaveChanges, ref ns, ref publish, out isActive, ref saveForWeb);
						else
						{
							res = DialogResult.Yes;
							isActive = BaseCustomizationContext.CustomizationContextInstance.IsActiveDocument(ns);
						}
					}
				}
				else
				{
					if (newDocument)//nuovo documento: non devo chiedere ulteriori informazioni, sono già implicite nel nome documento
						res = DialogResult.Yes;
					else//l'applicazione è in chiusura: chiedo all'utente se voglio salvare ed il nome della customizzazione
						res = SaveCustomization.SaveNewCustomization(this, Resources.SaveCustomization, Resources.SaveChanges, ref ns, ref publish, out isActive, saveForWeb);
				}

                switch (res)
				{
					case DialogResult.Cancel:
						return false;
                    case DialogResult.Yes:
                        Cursor.Current = Cursors.WaitCursor;

                        //crea/aggiorna il json
                        if (saveForWeb)
                        {
                            NameSpace nsForJson = new NameSpace(Sources?.Namespace);
                            nsForJson.Application = BaseCustomizationContext.CustomizationContextInstance.CurrentApplication;
                            nsForJson.Module = BaseCustomizationContext.CustomizationContextInstance.CurrentModule;
                            SerializationAddOnService ser = (SerializationAddOnService)view?.Site.GetService(typeof(SerializationAddOnService));
                            bool bResSerializeToJson = (bool)ser?.GenerateJson(view, nsForJson);
                        }

                        NameSpace old = Sources?.Namespace;
                        if (Sources != null)
							Sources.Namespace = ns;

						if (old?.FullNameSpace != ns?.FullNameSpace)
						{
							bool openCodeEditor =  mainForm.HostedControl.UpdateCodeEditor(sources);
							if (!mainForm.HostedControl.CollectAndSaveCodeChanges(openCodeEditor))
								return false;
						}
						else if (!mainForm.HostedControl.CollectAndSaveCodeChanges(false))
							return false;

						if (Controller == null)
							return false;
						//generazione file xml per dynamic hotlink, solo se proprietà Published == true
						serializationAddOnService.SerializePublishedHotLinks(Controller.Document, BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName, BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName);

                        //Aggiornamento masterTable=true per la buona gestione del TBGUID
                        serializationAddOnService.UpdateDatabaseObjects(Controller.Document);

                        string userFilePath = CalculateOutputFileFullName(ns);
                        bool result = Build(true, userFilePath);
                        if (!result || Controller == null)
                            return false;

						AddReferenceToComponents();

                        //la modifica è pubblica? allora sposto la dll dalla cartella utente alla common
                        //e aggiungo il file appena creato alla customizzazione corrente
                        if (publish)
                            BaseCustomizationContext.CustomizationContextInstance.PublishDocument(Path.GetDirectoryName(userFilePath), Path.GetFileNameWithoutExtension(userFilePath), CUtility.GetUser(), isActive);
                        else
                            BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(userFilePath, true, isActive, publish ? string.Empty : CUtility.GetUser(), document.Namespace.ToString());

                        string referencedPath = PathFinderWrapper.GetEasyStudioReferenceAssembliesPath();
                        foreach (AssemblyName an in Controller.GetType().Assembly.GetReferencedAssemblies())
                        {
							Assembly asm = AssembliesLoader.Load(an);

                            if (asm.Location != null && asm.Location.StartsWith(referencedPath, StringComparison.InvariantCultureIgnoreCase))
                                BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(asm.Location);
                        }
                        SetDirty(false);
                        //aggiornamento del controller nella lista del documento

                        return true;
                    default:
						// se ho una nuova customizzazione devo ricordarmi di farne la dispose
						// altrimenti rimane in memoria perchè non fa ancora parte dell'array
						// dei controllers. 
						if (leavingCustomization && Controller != null && !controllers.Contains(Controller))
							Controller = null;
						return true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.ToString());
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		internal static string CalculateOutputFileFullName(NameSpace ns)
        {
            return BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppAssemblyFullName(ns, CUtility.GetUser(), BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
        }

        //--------------------------------------------------------------------------------
        private bool SaveJson(bool askForSaving, bool askForFile)
		{
			if (jsonEditorConnector == null)
				return false;
			if (askForSaving)
			{
				DialogResult res = MessageBox.Show(this, Resources.SaveChanges, Resources.EasyBuilder, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
				if (res == DialogResult.Cancel)
					return false;
				if (res == DialogResult.No)
				{
					SetDirty(false);					
					return true;
				}
			}

			// se sto salvando qlcs per DocOutline, non devo passare dal c++ (jsonConnector)
			string newjsonSerialized = mainForm.HostedControl.SaveDocOutlineSerialized();
			if(newjsonSerialized == Resources.UnauthorizedAccessreadOnlyFile)
			{
				SetDirty(true);
				return false;
			}

			if (newjsonSerialized.IsNullOrEmpty() && !jsonEditorConnector.SaveJson())
				return false;

			SetDirty(false);
			return true;
		}


		//--------------------------------------------------------------------------------
		internal void SetJsonEditorCode(string newText)
		{
			if (newText != null)
				jsonEditorConnector.UpdateFromSourceCode(newText); //prima era jsonEditorConnector.Json = newText;
			//ma l'assegnazione del text farebbe scattare UpdateFromSourceCode e UpdateSourceCode. la seconda non la voglio
		}    

		//--------------------------------------------------------------------------------
		private void UpdateWindowDescription(IWindowWrapper wrapper, string propertyName, object propertyValue)
		{
			if (jsonEditorConnector == null)
				return;

			BaseWindowWrapper bwWrapper = wrapper as BaseWindowWrapper;
			string id = bwWrapper!=null ? bwWrapper.Id : String.Empty; //se non è un BaseWindowWrapper, non dovrbbe avere nè Anchor, nè PanelType
			string json = jsonEditorConnector.Json;

			switch (propertyName)	
			{
				case ("Anchor"):
					if (IsTheLastSelection(wrapper))
						RefreshJsonAndWrappers(id, json);
					break;
				case ("PanelType"):
					if (JsonProcessor.ChangePanelType(ref json, (EPanelType)propertyValue))
						RefreshJsonAndWrappers(id, json);
					break;
				case ("TabOrder"):
					jsonEditorConnector.UpdateTabOrder(wrapper.Handle);
					RefreshWrappers(true);
					break;
				case ("Items"):
					MPropertyGrid propGrid = wrapper as MPropertyGrid;
					propGrid?.UpdateAllPropertyGrid();   //description e order				
					jsonEditorConnector.UpdateWindow(wrapper.Handle);
					break;
				default:
					jsonEditorConnector.UpdateWindow(wrapper.Handle); 	break;
			}
			SetDirty(true);
		}

		//--------------------------------------------------------------------------------
		private bool IsTheLastSelection(IWindowWrapper wrapper)
		{
			int count = selections.Count;
			List<Selection> sels = new List<Selection>();
			sels.AddRange(selections.Items);
			for (int i = 0; i < count; i++)
			{
				if (sels[i].GetCurrentWindow().Equals(wrapper))
					return (i == count - 1);
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		private void RefreshJsonAndWrappers(string id, string json)
		{
			jsonEditorConnector.Json = json;//assegno lo stesso json, solo per forzare il refresh della finestra
			RefreshWrappers(true);
			List<BaseWindowWrapper> components = View.GetChildrenByIdOrName(id, "");
			SelectionService.SetSelectedComponents(components);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		//--------------------------------------------------------------------------------
		public void UpdateJsonCodeSelection(string id)
		{
			MainFormJsonEditor jsonEditor = mainForm?.HostedControl as MainFormJsonEditor;
			jsonEditor?.JsonCodeControl?.SelectCode(id);
		}


		//--------------------------------------------------------------------------------
		private void AddWindowDescription(IntPtr hwnd, IntPtr hwndParent)
		{
			if (jsonEditorConnector == null)
				return;

			jsonEditorConnector.AddWindow(hwnd, hwndParent);
			SetDirty(true);
		}

		//--------------------------------------------------------------------------------
		private void DeleteWindowDescription(IntPtr hwnd)
		{
			if (jsonEditorConnector == null)
				return;

			jsonEditorConnector.DeleteWindow(hwnd);
			RefreshWrappers(true);
			SetDirty(true);
		}

		/// <summary>
		/// Adds a component to the children components list with the given name.
		/// </summary>
		/// <param name="component">The component to be added</param>
		/// <param name="name">The name to give to the component</param>
		/// <seealso cref="System.ComponentModel.IComponent"/>
		//-----------------------------------------------------------------------------
		public void Add(IComponent component, string name)
		{
			componentsList.Add(component);
		}

		/// <summary>
		/// Adds a component to the children components list.
		/// </summary>
		/// <param name="component">The component to be added</param>
		/// <seealso cref="System.ComponentModel.IComponent"/>
		//-----------------------------------------------------------------------------
		public void Add(IComponent component)
		{
			Add(component, null);
		}

		/// <summary>
		/// Removes a component from the children components list.
		/// </summary>
		/// <param name="component">The component to remove.</param>
		/// <seealso cref="System.ComponentModel.IComponent"/>
		//-----------------------------------------------------------------------------
		public void Remove(IComponent component)
		{
			componentsList.Remove(component);
		}

		//--------------------------------------------------------------------------------
		internal void UpdateController(bool updateWithoutUserMethods = true)
		{
			//Qui devo prendere il codice originale di eventuali codeeditori aperti (l'ultimo che ha buildato)
			if (updateWithoutUserMethods)
				mainForm.HostedControl.CollectOriginalMethodBodys();

			Build(true, string.Empty);
			
			//e qui devo ripristinare il tutto;
			if (updateWithoutUserMethods)
				mainForm.HostedControl.CollectAndSaveCodeChanges(false);
		}

		//--------------------------------------------------------------------------------
		internal IWindowWrapper GetCurrentWindow(IntPtr hCurrentWindow)
		{
			return view?.GetControl(hCurrentWindow);
		}

		//-----------------------------------------------------------------------------
		private bool IsMultiSelectionSalvatoreQuasimode()
		{
			return ModifierKeys.HasFlag(Keys.Control) && !ModifierKeys.HasFlag(Keys.Alt) && !ModifierKeys.HasFlag(Keys.Shift);
		}

		//-----------------------------------------------------------------------------
		internal bool IsCopyActive()
		{
			return ModifierKeys.HasFlag(Keys.Control) && !ModifierKeys.HasFlag(Keys.Alt) && ModifierKeys.HasFlag(Keys.Shift);
		}

		//--------------------------------------------------------------------------------
		internal void SetCurrentWindow(IWindowWrapper current, bool add)
		{
			if (current == null || selections == null)
				return;

			//Se mi arriva un current il cui handle corrisponde a quello del windowWrapper
			//già selezionato allora ritorno senza fare nulla.
			if (selections.GetSelection(current) != null)
				return;

			UpdateJsonCodeSelection(current.Id);

			//attivo il controllo
			IDesignerTarget target = current as IDesignerTarget;
			target?.Activate();

			selections.Add(current, !add);

			//per adesso ha senso solo per la tab: se la attivo, crea i wrappers ai figli
			WindowWrapperContainer wrapper = current as WindowWrapperContainer;
			if (wrapper != null)
			{
				bool somethingCreated = false;
				using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
				{
					somethingCreated = wrapper.CreateWrappers(HandlesToSkip);
					wrapper.SwitchVisibility(Settings.Default.ShowHiddelFields);//visualizzo i campi nascosti per poterli modificare
				}
				if (somethingCreated)
				{
					ControllerSources?.GenerateFieldDeclarations(wrapper);
					UpdateObjectViewModel(wrapper);

				}
			}
		}

		/// <summary>
		/// Internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public override object GetService(Type serviceType)
		{
			if (serviceType == typeof(IModelRoot))
				return Controller;

			if (serviceType == typeof(DocumentController))
				return Controller;

			if (serviceType == typeof(Framework.TBApplicationWrapper.MDocument))
				return Controller.Document;
			if (serviceType == typeof(FormEditor))
				return this;
			if (serviceType == typeof(SerializationAddOnService))
				return serializationAddOnService;

			return base.GetService(serviceType);
		}

		//-----------------------------------------------------------------------------
		internal void PerformAlignAction(Selections.Action action)
		{
			selections.PerformAction(action);
			InvalidateEditor(); //fix: dopo l'align la mainSelection non veniva più disegnata
		}

		///<remarks />
		//---------------------------------------------------------------------
		public override void SetDirty(bool dirty)
		{
			mainForm?.HostedControl?.EnableCodeEditorButton();

			if (SuspendDirtyChanges)
				return;

			if (IsDirty != dirty)
			{
				IsDirty = dirty;
				OnDirtyChanged(new DirtyChangedEventArgs(IsDirty, sources?.Namespace));
			}
		}

		///<remarks />
		//---------------------------------------------------------------------
		internal void CreateJsonForm(string file, UI.AddItem.ItemType type, bool isFromDocOutline = false, AdditemComboOption aico = null)
		{
			if (!isFromDocOutline && (!AskAndSave(true)))
					return;

			string path = Path.GetDirectoryName(file);
			if (!Directory.Exists(path) && path != null)
				Directory.CreateDirectory(path);
			
			using (StreamWriter sw = new StreamWriter(file, false, Encoding.UTF8))
			{
				sw.Write(JsonProcessor.CreateJsonForm(Path.GetFileNameWithoutExtension(file), type, aico));
			}
		}

		///<remarks />
		//---------------------------------------------------------------------
		internal void CreateHJson(string file)
		{
			string hjsonpath = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + NameSolverStrings.HjsonExtension);
			if (File.Exists(hjsonpath))
				return;
			string strName = Path.GetFileNameWithoutExtension(hjsonpath);
			string hJson = "#pragma once\r\n#include\t<TbNameSolver\\TBResourcesMap.h>\r\n#define ";
			hJson += strName + "\tGET_IDD( ";
			hJson += strName + ", ";
			hJson += CUtility.GetJsonContext(file) + " )\r\n";
			using (StreamWriter sw = new StreamWriter(hjsonpath, false, Encoding.UTF8))
			{
				sw.Write(hJson);
			}
		}

		///<remarks />
		//---------------------------------------------------------------------
		internal bool CloseJsonForm()
		{
			if (jsonEditorConnector == null || string.IsNullOrEmpty(currentJsonFile))
				return false;

			if (!AskAndSave(true))
				return false;

			using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
			{
				SetHasCodeBehind(view.Components, true);
				if (!jsonEditorConnector.CloseJson(currentJsonFile, CurrentJsonIsDocOutline))
				{
					return false;
				}
				view.ClearComponents();

			}
			UpdateClearSetDirty(false); //aggiorno la finestra del View Model, SelectionService, selection e SetDirty(setDirty)	
			currentJsonFile = string.Empty;
			return true;
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		internal void Undo()
		{
			if (jsonEditorConnector == null)
				return;
			string oldCode = string.Empty;
			using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
			{
				 oldCode = jsonEditorConnector.Undo();
			}
			mainForm.HostedControl.UpdateDocOutline(oldCode);
			RefreshWrappers(true);
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		internal void Redo()
		{
			if (jsonEditorConnector == null)
				return;
			string newCode = string.Empty;
			using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
			{
				newCode = jsonEditorConnector.Redo();
			}
			mainForm.HostedControl.UpdateDocOutline(newCode);
			RefreshWrappers(true);
		}

		///<remarks />
		//---------------------------------------------------------------------
		internal void OpenJsonForm(string file)
		{
			if (jsonEditorConnector == null || file == currentJsonFile)
				return;
			currentJsonFile = file;

			if (!AskAndSave(true))
				return;
			using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
			{
				//ripristino lo stato che avevo tolto allo step successivo (vedi qualche riga più avanti) per permettere la corretta distruzione degli oggetti
				SetHasCodeBehind(view.Components, true);
				//pulisco gli oggetti correnti
				view.ClearComponents();

				//creo la dialog a partire dal tbjson selezionato
				CurrentJsonIsDocOutline = mainForm.HostedControl.OpenDocOutlineIfNeeded(new JsonFormSelectedEventArgs(file, null));
				bool successfullOpening = jsonEditorConnector.OpenJson(file, CurrentJsonIsDocOutline);
				if (!successfullOpening)
				{
					MessageBox.Show(string.Format( Resources.CannotOpenThisFile, file));
					CloseJsonForm();
					return;
				}
				//creo i nuovi wrappers
				view.CreateWrappers(HandlesToSkip);

				view.SwitchVisibility(Settings.Default.ShowHiddelFields);//visualizzo i campi nascosti per poterli modificare
				view.CallCreateComponents();

				//designer json: le finestre nascono da json, quindi tutti i wrapper sono creati attorno 
				//a finestre già esistenti, ossia con HasCodeBehind a true
				//ma questo mi impedisce di editarne molte proprietà, nel caso di editor va rimosso
				SetHasCodeBehind(view.Components, false);

				// la AdjustSites va fatta prima della bind per poter sistemare correttamente i sites
				// nelle istanze degli oggetti del data model corretti
				TBSite.AdjustSites(this);
			}
			UpdateClearSetDirty(false);	//aggiorno la finestra del View Model, SelectionService, selection e SetDirty(setDirty)	
			SelectFileInTree(file);		//aggiorno il Json tree
		}

		///<remarks />
		//---------------------------------------------------------------------
		internal void RefreshWrappers(bool setDirty = false)
		{
			using (SuspendObjectModelUpdate upd = new SuspendObjectModelUpdate(this))//per sospendere l'update dell'object model
			{
				//ripristino lo stato che avevo tolto allo step successivo (vedi qualche riga più avanti) per permettere la corretta distruzione degli oggetti
				SetHasCodeBehind(view.Components, true);
				//pulisco gli oggetti correnti
				view.ClearComponents();
				//creo i nuovi wrappers
				view.CreateWrappers(HandlesToSkip);
				view.SwitchVisibility(Settings.Default.ShowHiddelFields);//visualizzo i campi nascosti per poterli modificare
														//designer json: le finestre nascono da json, quindi tutti i wrapper sono creati attorno a finestre già esistenti, 
														//ossia con HasCodeBehind a true, ma questo mi impedisce di editarne molte proprietà, nel caso di editor va rimosso
				SetHasCodeBehind(view.Components, false);

				// la AdjustSites va fatta prima della bind per poter sistemare correttamente i sites
				// nelle istanze degli oggetti del data model corretti
				TBSite.AdjustSites(this);
			}
			UpdateClearSetDirty(setDirty); //aggiorno la finestra del View Model, SelectionService, selection e SetDirty(setDirty)	
		}

		//---------------------------------------------------------------------
		private void UpdateClearSetDirty(bool setDirty)
		{
			UpdateObjectViewModel(Controller);
			SelectionService.SetSelectedComponents(null);
			ClearSelections();
			SetDirty(setDirty);
		}

		///<remarks />
		//---------------------------------------------------------------------
		internal bool DeleteJsonForm(string file)
		{
			if (jsonEditorConnector == null)
				return false;
			if (DialogResult.Yes != MessageBox.Show(
				this,
				string.Format(Resources.AreYouSureDeleteItem, file),
				Resources.EasyBuilder,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Exclamation
				))
				return false;
			try
			{

				if (Directory.Exists(file))
				{
					var c = Directory.GetFiles(file);
					if (c.Length > 0)
					{
						if (DialogResult.Yes != MessageBox.Show(
							   this,
							   string.Format(Resources.FolderNotEmptyDeleteFilesAnyway, file),
							   Resources.EasyBuilder,
							   MessageBoxButtons.YesNo,
							   MessageBoxIcon.Exclamation
							   ))
							return false;

						string[] files = Directory.GetFiles(file);
						foreach (string item in files)
						{
							File.SetAttributes(item, FileAttributes.Normal);
							File.Delete(item);
							string hjsonCorr = Path.ChangeExtension(item, NameSolverStrings.HjsonExtension);
							if (File.Exists(hjsonCorr))
								File.Delete(hjsonCorr);
							if (item.CompareNoCase(currentJsonFile))
								CloseJsonForm();
						}
					}
					Directory.Delete(file);
					return true;
				}

				File.Delete(file);
				string hjson = Path.ChangeExtension(file, NameSolverStrings.HjsonExtension);
				if (File.Exists(hjson))
					File.Delete(hjson);

				return (file.CompareNoCase(currentJsonFile))
					? CloseJsonForm()
					: true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return false;

		}

		//-----------------------------------------------------------------------------
		internal void ClearSelections()
		{
			selections.ClearSelections();
		}

		//-----------------------------------------------------------------------------
		internal void SaveAsTemplate(WindowWrapperContainer window)
		{
			if (TemplateService != null)
				TemplateService.GenerateTemplate(window, true);
		}
	}

	/// <summary>
	/// Stores all information about the status of the EasyBuilder designer.
	/// </summary>
	//-----------------------------------------------------------------------------
	[Serializable]
	public class DesignerCurrentStatus : Dictionary<string, IDesignerCurrentStatusObject>, IDesignerCurrentStatus
	{
	};

	/// <summary>
	/// The core object for all customization operations.
	/// It allows the user to modify the user interface of a document, it exposes all
	/// widget useful to create a customization (Toolbox for dragging new controls,
	/// property grid to inspect customization objects, methods outline to manage all
	/// source code added by the user and so on).
	/// </summary>

	/// <summary>
	/// Helper for all drag and drop operations concerning object model objects.
	/// </summary>
	/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.Model.IDataBinding"/>
	/// <seealso cref="System.ComponentModel.IComponent"/>
	//=============================================================================
	internal class DataModelDropObject : IDisposable
	{
		IDataBinding dataBinding;
		IComponent component;
		string sourceNodeName;

		/// <summary>
		/// Gets the name of the object model tree node where the drag started
		/// </summary>
		//-----------------------------------------------------------------------------
		public string SourceNodeName { get { return sourceNodeName; } }

		/// <summary>
		/// Gets the dragged IComponent .
		/// </summary>
		//-----------------------------------------------------------------------------
		public IComponent Component { get { return component; } }

		/// <summary>
		/// Get the dragged data source.
		/// </summary>
		//-----------------------------------------------------------------------------
		public IDataBinding DataBinding { get { return dataBinding; } }

		/// <summary>
		/// Initializes a new instance of DataModelDropObject.
		/// </summary>
		/// <param name="component">The dragged component</param>
		/// <param name="sourceNodeName">The object model tree node where the drag started</param>
		//-----------------------------------------------------------------------------
		public DataModelDropObject(IComponent component, string sourceNodeName)
		{
			this.component = component;
			this.sourceNodeName = sourceNodeName;

			MDataObj data = component as MDataObj;
			if (data != null)
			{
				MSqlRecord record = data.Site.Container as MSqlRecord;
				if (record != null && record.Site != null && record.Site.Container != null)
				{
					IDataManager dataObject = record.Site.Container as IDataManager;
					dataBinding = new FieldDataBinding(data, dataObject);
				}
				return;
			}
			MSqlRecordItem dataItem = component as MSqlRecordItem;
			if (dataItem != null)
			{

				MSqlRecord record = dataItem.Record as MSqlRecord;
				if (record != null && record.Site != null && record.Site.Container != null)
				{
					IDataManager dataObject = record.Site.Container as IDataManager;
					dataBinding = new FieldDataBinding((MDataObj)dataItem.DataObj, dataObject);
				}
				return;
			}
			MDBTSlaveBuffered dbt = component as MDBTSlaveBuffered;
			if (dbt != null)
			{
				dataBinding = new DBTDataBinding(dbt);
				return;
			}
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		///<remarks />
		//-----------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				IDisposable disposable = dataBinding as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
					dataBinding = null;
				}
			}
		}
	}


}
