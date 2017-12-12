using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using Microarea.EasyBuilder.CodeCompletion;
using Microarea.EasyBuilder.CodeEditorProviders;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.EasyBuilder.UI
{
	//==================================================================================
	/// <remarks/>
	public partial class CodeEditorForm : DockContent, IEasyBuilderCodeEditor, IDirtyManager
	{
		private bool					disposing;
		private bool					dirty;
		private bool					suspendDirtyChanges;
		private int						currentGotoErrorLine;
		private string					originalMethodBody;
		private Timer					buildTimer = new Timer();
		private Timer					parseTimer = new Timer();
		private Sources					sources;
		private Editor					editor;
		private TextArea				textAreaControl;
		private ICSharpCode.NRefactory.Editor.IDocument	textEditorDocument;
		//Booleano che indica se chiudere la finestra ignorando eventuali cambiamenti non salvati.
		//Serve per chiudere il code editor form nel caso in cui venga cancellato il suo metodo
		//dalla finestra Methods outline.
		private bool					ignoreChangesOnFormClosing;
        private SourcesBuilder			sourcesBuilder;
		private FoldingManager			foldingManager;

		private const string UserMethodString = "User methods";
		private const string usingString = "using {0};\r\n";
		//Point che mi dà la posizione di default del primo metodo inserito all'interno del CodeEditor, altrimenti si metterebbe in (0;0)
		private Point defaultLocationFirstMethod = new Point(30, 4);

		internal event EventHandler<FoldingUpdatedEventArgs> FoldingUpdated;

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public event EventHandler RefreshPropertyGrid;


		/// <remarks/>
		//-----------------------------------------------------------------------------
		public event EventHandler RefreshIntellisense;

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public event EventHandler<DirtyChangedEventArgs> DirtyChanged;

		private List<MethodDeclaration> methodRemovedByUser = new List<MethodDeclaration>();
		private List<MethodDeclaration> originalMethodsList = new List<MethodDeclaration>();


		/// <summary>
		/// Gets or sets a value to ignore changes on form closing.
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IgnoreChangesOnFormClosing
		{
			get { return ignoreChangesOnFormClosing; }
			set { ignoreChangesOnFormClosing = value; }
		}

		/// <remarks/>
		//-------------------------------------------------------------------------------
		public bool IsDirty { get { return dirty; } }

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Suspends dirty changes
		/// </summary>
		public bool SuspendDirtyChanges { get { return this.suspendDirtyChanges; } set { this.suspendDirtyChanges = value; } }

		//-------------------------------------------------------------------------------
		/// <summary>
		/// Internal use
		/// </summary>
		public CodeEditorForm()
		{
			InitializeComponent();
			this.Text = UserMethodString;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public void OnFoldingUpdated(FoldingUpdatedEventArgs e)
		{
			FoldingUpdated?.Invoke(this, e);
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public void ReinitializeForm(MethodDeclaration method)
		{
			sources.CustomizationInfos.UserMethodsCode = textEditorDocument.Text;
			AddMethodToTextIfNotExisting(method);

			ParseAndUpdateSyntaxTree(textEditorDocument.Text);

			//richiediamo il method, potrebbe essere diverso dopo la parse
			method = sources.GetUpdatedUserMethod(method);
			SetCaretToMethod(method);
			PopulateMethodCombo(method);
		}

		//--------------------------------------------------------------------------------
		private void SetCaretToMethod(MethodDeclaration methodDeclaration)
		{
			if (methodDeclaration == null)
				return;

			BlockStatement tryBlock = Sources.GetUserMethodTryCatchContent(methodDeclaration);
			//se il metodo ha il suo trycatch, mi posiziono subito all'interno del tryblock, altrimenti all'inizio del metodo

			int line = tryBlock != null ? tryBlock.StartLocation.Line : methodDeclaration.StartLocation.Line;
			line = line == 0 ? defaultLocationFirstMethod.X : line;
			int column = tryBlock != null ? tryBlock.StartLocation.Column : methodDeclaration.StartLocation.Column;
			column = column == 0 ? defaultLocationFirstMethod.Y : column;
			codeEditor.TextEditor.ScrollTo(line, column);
			codeEditor.TextEditor.CaretOffset = textEditorDocument.GetOffset(line, column) + 1;
			codeEditor.TextEditor.Focus();
		}

		//--------------------------------------------------------------------------------
		private void PopulateMethodCombo(MethodDeclaration method = null)
		{
			MethodDeclaration previuosSelection = tsMethodsCombo?.ComboBox?.SelectedItem as MethodDeclaration;
			if (tsMethodsCombo?.ComboBox == null || sources == null)
				return;
			tsMethodsCombo.ComboBox.Items.Clear();
			tsMethodsCombo.ComboBox.DisplayMember = "Name";

			var userMethods = sources.GetUserMethods().OrderBy(x=>x.Name);
			tsMethodsCombo.ComboBox.Items.AddRange(userMethods.ToArray());
			
			//selezioniamo o il method che abbiamo passato, o l'ultimo selezionato, se non c'è è null
			MethodDeclaration toSelect = method ?? previuosSelection;
			if(toSelect != null)
				tsMethodsCombo.ComboBox.SelectedItem = sources.GetUpdatedUserMethod(toSelect);
		}

		//--------------------------------------------------------------------------------
		private void PopulateMethodCombo(List<MethodDeclaration> methods)
		{
			if (tsMethodsCombo?.ComboBox == null)
				return;
			MethodDeclaration previuosSelection = tsMethodsCombo.ComboBox.SelectedItem as MethodDeclaration;
			tsMethodsCombo.ComboBox.Items.Clear();
			tsMethodsCombo.ComboBox.Items.AddRange(methods.ToArray());
			tsMethodsCombo.ComboBox.SelectedItem = previuosSelection;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}

		/// <remarks/>
		//--------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			PopulateMethodCombo();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//--------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			this.disposing = true;
			base.Dispose(disposing);
			if (disposing)
			{
				this.FoldingUpdated -= CodeEditorForm_FoldingUpdated;

				components?.Dispose();

				if (sources != null)
				{
					sources.CodeChanged -= new EventHandler<CodeChangedEventArgs>(Sources_CodeChanged);
				}

				if (sources != null && sources.CustomizationInfos != null)
					sources.CustomizationInfos.UsingsChanged -= CustomizationInfos_UsingsChanged;

				if (sourcesBuilder != null)
				{
					sourcesBuilder.BuildCompleted -= new EventHandler<BuildEventArgs>(Sources_BuildCompleted);
					sourcesBuilder.Dispose();
					sourcesBuilder = null;
				}
				EventHandlers.RemoveEventHandlers(ref DirtyChanged);

				FoldingManager.Uninstall(foldingManager);
				if (textAreaControl != null)
				{
					textAreaControl.DragOver -= TextAreaControl_DragOver;
					textAreaControl.Drop -= TextAreaControl_Drop;
					textEditorDocument.TextChanged -= TextEditorDocument_TextChanged;
				}

				if (codeEditor != null)
					codeEditor.Dispose();
			}
			this.disposing = false;
		}

		/// <summary>
		/// Lancia su un thread separato la build del codice ogni secondo e mezzo (se il 
		/// codice è stato modificato)
		/// </summary>
		/// <param name="e"></param>
		//-------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			//fermo la build automatica
			buildTimer.Stop();
			parseTimer.Stop();

			base.OnFormClosing(e);

			if (!dirty || ignoreChangesOnFormClosing)
				return;

			DialogResult result = MessageBox.Show(this, Resources.SaveChanges, Resources.SaveCustomization, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
			switch (result)
			{
				case DialogResult.Yes:
					if (!Save())
						e.Cancel = true;
					break;
				case DialogResult.No:
					List<MethodDeclaration> beforeClosingMethodsList = sources.GetUserMethods();
					sources.CustomizationInfos.UpdateUserMethodsCompilationUnit(originalMethodBody, sources.GetUserMethodsFilePath());
					sources.CustomizationInfos.UserMethodsCode = originalMethodBody;
					AlignMethodsFromCodeToSyntaxTree(originalMethodsList, beforeClosingMethodsList);
					RestoreEventHandlerAsOriginalMethodsList();
					break;
				default:
					e.Cancel = true;
					break;
			}
		}

		/// <summary>
		/// Internal use
		/// </summary>
		//-------------------------------------------------------------------------------
		public void Initialize(Editor editor, MethodDeclaration method = null)
		{
			this.editor = editor;
			this.sourcesBuilder = new SourcesBuilder();
			this.sources = editor.Sources;

			textAreaControl = codeEditor.TextEditor.TextArea;
			textEditorDocument = codeEditor.TextEditor.Document;

			codeEditor.InitializeEditor(sources, sources.GetUserMethodsFilePath());
			codeEditor.InitializeTextMarkerService();

			sources.CustomizationInfos.UsingsChanged += CustomizationInfos_UsingsChanged;

			SetOriginalUserMethods(method); //originalMethodBody and originalMethodsList

			AddMethodToTextIfNotExisting(method);

			this.sourcesBuilder.BuildCompleted += new EventHandler<BuildEventArgs>(Sources_BuildCompleted);
			this.sources.CodeChanged += new EventHandler<CodeChangedEventArgs>(Sources_CodeChanged);
			textAreaControl.DragOver += TextAreaControl_DragOver;
			textAreaControl.Drop += TextAreaControl_Drop;
			textEditorDocument.TextChanged += TextEditorDocument_TextChanged;

			foldingManager = FoldingManager.Install(textAreaControl);
			this.FoldingUpdated += CodeEditorForm_FoldingUpdated;

			UpdateFoldings(sources.CustomizationInfos.UserMethodsCompilationUnit);

			//Il document changed lo registro dopo gli altri perchè alla prima associazione del body del metodo
			//in realtà stiamo caricando o un metodo nuovo che non ha modifiche o uno presente nella dll che ancora
			//non è stato toccato, per cui non voglio che venga scatenato il changed prima

			//An.23927, aprendo il code editor non vuoto da property grid, non si posiziona sul metodo giusto
			SetCaretToMethod(method);
			PopulateMethodCombo(method);

			//Timer che fa scattare le build
			buildTimer.Interval = 2000;
			parseTimer.Interval = 500;
			buildTimer.Tick += new EventHandler(BuildTimer_Tick);
			parseTimer.Tick += ParseTimer_Tick;
		}

		//--------------------------------------------------------------------------------
		private void SetOriginalUserMethods(MethodDeclaration method)
		{
			textEditorDocument.Text = originalMethodBody = sources.GetUserMethodsFileCode();
			originalMethodsList = sources.GetUserMethods();
			if (method != null)
				originalMethodsList.Remove(method);
		}

		//--------------------------------------------------------------------------------
		private void CustomizationInfos_UsingsChanged(object sender, UsingsChanged e)
		{
			string toAddOrRemove = string.Format(usingString, e.UsingNamespace);
			int offset = textEditorDocument.IndexOf(toAddOrRemove, 0, textEditorDocument.Text.Length, StringComparison.InvariantCulture);

			if (offset < 0 && e.UsingsChangedType == UsingsChanged.UsingsChangedEnum.Added)
			{
				textEditorDocument.Insert(0, toAddOrRemove);
				return;
			}

			if (offset >= 0 && e.UsingsChangedType == UsingsChanged.UsingsChangedEnum.Removed)
			{
				textEditorDocument.Replace(offset, toAddOrRemove.Length, string.Empty);
				codeEditor.TextEditor.InvalidateVisual();
			}
		}

		//--------------------------------------------------------------------------------
		private void ParseTimer_Tick(object sender, EventArgs e)
		{
			parseTimer.Stop();

			string doc = textEditorDocument.Text;
			new Task(() =>
			{
				ParseAndUpdateSyntaxTree(doc);
			}).Start();

		}

		//--------------------------------------------------------------------------------
		private void CodeEditorForm_FoldingUpdated(object sender, FoldingUpdatedEventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.Invoke((Action)delegate { CodeEditorForm_FoldingUpdated(sender, e); });
				return;
			}

			foldingManager.UpdateFoldings(e.Foldings, e.FirstOffset);
		}

		//--------------------------------------------------------------------------------
		private void AddMethodToTextIfNotExisting(MethodDeclaration method)
		{
			if (method == null)
				return;

			if (textEditorDocument.Text != null && (textEditorDocument != null && textEditorDocument.Text.IndexOf(method.Name, StringComparison.Ordinal) >= 0))
				return;
			string newMethod = "\r\n";
			newMethod += AstFacilities.GetVisitedText(method);
			TypeDeclaration docControllerClass = EasyBuilderSerializer.FindClass(sources.CustomizationInfos.UserMethodsCompilationUnit, "DocumentController");
			if (docControllerClass == null)
			{
				Debug.Assert(false);
			}

			int offset = 0;
			try
			{
				offset = textEditorDocument.GetOffset(new TextLocation(docControllerClass.StartLocation.Line + 1, docControllerClass.StartLocation.Column + 1));
				textEditorDocument.Insert(offset, newMethod);
			}
			catch (Exception)
			{
				Debug.Assert(false, "Add method doesn't work");
				return;
			}
			SetDirty(true);

			//indentiamo il solo metodo appena aggiunto
			IDocumentLine line = textEditorDocument.GetLineByOffset(offset + newMethod.Length);
			textAreaControl.IndentationStrategy.IndentLines(codeEditor.TextEditor.Document, docControllerClass.StartLocation.Line + 1, line.LineNumber);
		}
	
		//--------------------------------------------------------------------------------
		/// <summary>
		/// stackoverflow: metodo che converte il dataobject wpf in quello winform, altrimenti viene data una interopexception
		/// </summary>
		/// <param name="dataObject"></param>
		/// <returns></returns>
		private IDataObject ConvertDataObject(System.Windows.IDataObject dataObject)
		{
			var oleConverterType = Type.GetType("System.Windows.DataObject+OleConverter, PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
			var oleConverter = typeof(System.Windows.DataObject).GetField("_innerData", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dataObject);
			return (DataObject)oleConverterType?.GetProperty("OleDataObject").GetValue(oleConverter, null);
		}

		//--------------------------------------------------------------------------------
		private void TextAreaControl_Drop(object sender, System.Windows.DragEventArgs e)
		{
			System.Windows.Point point = new System.Windows.Point(MousePosition.X, MousePosition.Y);
			string result = String.Empty;
			System.Windows.Forms.IDataObject dataObject = ConvertDataObject(e.Data);
		
			DataModelDropObject dataModelDropObject = dataObject.GetData(typeof(DataModelDropObject)) as DataModelDropObject;
			if (dataModelDropObject != null)
			{
				result = ReflectionUtils.GetComponentFullPath(dataModelDropObject.Component);
				if (String.IsNullOrWhiteSpace(result))
				{
					ReferenceableComponent refrencedComponent = dataModelDropObject.Component as ReferenceableComponent;
					if (refrencedComponent != null)
						result = refrencedComponent.MainClass;
				}
			}
			else
			{
				EnumsDropObject enumsDropObject = dataObject.GetData(typeof(EnumsDropObject)) as EnumsDropObject;
				if (enumsDropObject != null)
				{
					result = enumsDropObject.EnumSourceCodeString;
				}
			}

			if (result != null && result.Length == 0)
				return;

			int offset = GetCaretOffsetFromMousePosition(point);
			if (offset < 0)
				return;
			textAreaControl.Caret.Offset = offset;
			textEditorDocument.Insert(offset, result); //textAreaControl.InsertString(result);

			ParseAndUpdateSyntaxTree(textEditorDocument.Text);
			this.Activate();
		}

		//--------------------------------------------------------------------------------
		private int GetCaretOffsetFromMousePosition(System.Windows.Point point)
		{
			System.Windows.Point point2 = codeEditor.TextEditor.PointFromScreen(point);
			TextViewPosition? tempPosition = codeEditor.TextEditor.GetPositionFromPoint(point2);
			if (tempPosition != null)
			{
				TextViewPosition position = tempPosition.GetValueOrDefault();
				return textEditorDocument.GetOffset(position.Line, position.Column);
			}

			return -1;
		}

		//--------------------------------------------------------------------------------
		private void TextAreaControl_DragOver(object sender, System.Windows.DragEventArgs e)
		{
			
			System.Windows.Forms.IDataObject dataObject = ConvertDataObject(e.Data);
			DataModelDropObject dataModelDropObject = dataObject.GetData(typeof(DataModelDropObject)) as DataModelDropObject;
			if (dataModelDropObject != null)
			{
				e.Effects = System.Windows.DragDropEffects.Copy;
				return;
			}

			EnumsDropObject enumsDropObject = dataObject.GetData(typeof(EnumsDropObject)) as EnumsDropObject;
			if (enumsDropObject != null)
			{
				e.Effects = System.Windows.DragDropEffects.Copy;
				return;
			}
			e.Effects = System.Windows.DragDropEffects.None;
		}

		/// <summary>
		/// Ad ogni document change(scatendato dall'utente che digita codice nella form)
		/// viene resettato il timer che impedisce la build mentre si 
		/// sta scrivendo.
		/// Passato il secondo e mezzo la build viene nuovamente attivata
		/// </summary>
		//--------------------------------------------------------------------------------
		private void TextEditorDocument_TextChanged(object sender, TextChangeEventArgs e)
		{
			buildTimer.Stop();
			buildTimer.Start();

			parseTimer.Stop();
			parseTimer.Start();

			SetDirty(true);
		}

		//-------------------------------------------------------------------------------
		void Sources_CodeChanged(object sender, CodeChangedEventArgs e)
		{
			//if (e.ChangeType == ChangeType.MethodAdded || e.ChangeType == ChangeType.MethodRemoved)
			//{
			//	e.Method = sources.GetUpdatedUserMethod(e.Method);
			//}
		}


		/// <summary>
		/// Scatenato quando è passato un tot dall'ultima modifica al codice
		/// </summary>
		//--------------------------------------------------------------------------------
		void BuildTimer_Tick(object sender, EventArgs e)
		{
			buildTimer.Stop();

			string doc =  textEditorDocument.Text;
            var controller = editor.GetService(typeof(MVC.DocumentController)) as MVC.DocumentController;
            SourcesSerializer.GenerateClass(sources.CustomizationInfos.EbDesignerCompilationUnit, controller);
            new Task(() =>
			{
				Build(doc, false);
			}).Start();
		}

		/// <summary>
		/// Travasa il codice inserito dall'utente e non ancora salvato nei sorgenti, in vista di una successiva build
		/// </summary>
		//--------------------------------------------------------------------------------
		public bool CollectAndSaveCodeChanges()
		{
            return Save();
		}

		/// <summary>
		/// Travasa il codice originale (l'ultimo che ha compilato)nei sorgenti, in vista di una successiva build safe
		/// </summary>
		//--------------------------------------------------------------------------------
		public void CollectOriginalMethodBody()
		{
			if (!dirty)
				return;

			buildTimer.Stop();
			parseTimer.Stop();

			sources.CustomizationInfos.UpdateUserMethodsCompilationUnit(originalMethodBody, sources.GetUserMethodsFilePath());
			originalMethodsList = sources.GetUserMethods();
		}

		/// <summary>
		/// Saves the content of the codeeditor form
		/// </summary>
		//--------------------------------------------------------------------------------
		public bool Save()
		{
			//da quando si possono modificare i cs da esterno, mi serve una build in più per consentire o meno il save
			buildTimer.Stop();
			parseTimer.Stop();

			IModelRoot modelRoot = editor.GetService(typeof(IModelRoot)) as IModelRoot;

			//TODOLUCA //TODOROBY AdjustReferencedBy va fatta su tutti i metodi
			foreach (MethodDeclaration currentMethod in sources.GetUserMethods())
			{
				editor.ComponentDeclarator.AdjustReferencedBy(currentMethod.Name, AstFacilities.GetVisitedText(currentMethod.Body), modelRoot);
			}

			EBCompilerResults buildResult = Build(textEditorDocument.Text, true);
			if (buildResult.Errors.HasErrors)
				return false;

			if (!dirty)           //se non ho errori di build e non è dirty esco
				return true;

			//se non ci sono errori, aggiorno originalMethodBody e originalMethodsList
			originalMethodBody = textEditorDocument.Text;
			originalMethodsList = sources.GetUserMethods();

			foreach (MethodDeclaration currentMethod in sources.GetUserMethods())
			{
				editor.ComponentDeclarator.AdjustReferencedBy(currentMethod.Name, buildResult.CompiledAssembly);
			}

			ParseAndUpdateSyntaxTree(textEditorDocument.Text);
		
			this.Text = UserMethodString;

			SetDirty(false);

			//ho salvato, quindi cancello la lista di metodi cancellati dall'utente, perchè sono stati salvati
			methodRemovedByUser.Clear();
			return true;
		}

		//--------------------------------------------------------------------------------
		private void UpdateFoldings(SyntaxTree syntaxTree )
		{
			int firstErrorOffset = -1;

			//snapshot freezato del documento, almeno non rompe le scatole per il fatto che non è thread safe
			try
			{
				ICSharpCode.NRefactory.Editor.IDocument document = new ReadOnlyDocument(textEditorDocument.CreateSnapshot());
				IEnumerable<NewFolding> newFoldings = FoldingFacilities.CreateNewFoldings(syntaxTree, document);
				newFoldings = newFoldings.OrderBy(f => f.StartOffset);

				OnFoldingUpdated(new FoldingUpdatedEventArgs(newFoldings, firstErrorOffset));
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		//--------------------------------------------------------------------------------
		private void ParseAndUpdateSyntaxTree(string methodBody)
		{
			try
			{
				CSharpParser parser = new CSharpParser();
				SyntaxTree syntaxTree = parser.Parse(new StringReader(methodBody), sources.GetUserMethodsFilePath());
				if (syntaxTree.Errors.Count > 0)
					return;

				List<MethodDeclaration> oldMethods = sources.GetUserMethods();

				sources.CustomizationInfos.UpdateUserMethodsCompilationUnit(syntaxTree);
		
				sources.UpdateProjectContent(syntaxTree);

				UpdateFoldings(syntaxTree);

				AlignMethodsFromCodeToSyntaxTree(sources.GetUserMethods(), oldMethods);
			}
			catch
			{

			}
		}
		
		//--------------------------------------------------------------------------------
		private void RestoreEventHandlerAsOriginalMethodsList()
		{
			IModelRoot modelRoot = editor.GetService(typeof(IModelRoot)) as IModelRoot;
			foreach (MethodDeclaration item in originalMethodsList)
			{
				//dati i metodi originali, riaggiungo gli Eventhandler se non sono già presenti
				//i += che non servono sono già stati tolti dalla AlignMethodsFromCodeToSyntaxTree
				sources.RestoreEventHandlerRegistration(modelRoot, item);
			}
		}

		//--------------------------------------------------------------------------------
		private void AlignMethodsFromCodeToSyntaxTree(List<MethodDeclaration> newMethods, List<MethodDeclaration> oldMethods)
		{
			if (this.InvokeRequired)
			{
				this.Invoke((Action)delegate{ AlignMethodsFromCodeToSyntaxTree(newMethods, oldMethods); });
				return;
			}
		
			//aggiorna gli eventinfo per rimozione +=
			List<MethodDeclaration> methodsToRemove = FindMethodsToRemove(oldMethods, newMethods);
			IModelRoot modelRoot = editor.GetService(typeof(IModelRoot)) as IModelRoot;

			foreach (var item in methodsToRemove)
			{
				sources.RemoveEventHandlerRegistration(modelRoot, item);
				methodRemovedByUser.Add(item); //questa lista serve in caso di undo. se ripristino il method body devo ripristinare anche gli event handler che ho tolto
			}

			//ripopola la tendina dei metodi
			if (newMethods.Count != oldMethods.Count || methodsToRemove.Count > 0)
			{
				OnRefreshPropertyGrid(null);
				PopulateMethodCombo(newMethods);
			}

		}

		//--------------------------------------------------------------------------------
		private List<MethodDeclaration> FindMethodsToRemove(List<MethodDeclaration> oldMethods, List<MethodDeclaration> newMethods)
		{
			List<MethodDeclaration> methodsToRemove = new List<MethodDeclaration>();
			foreach (var method in oldMethods)
			{
				if (!newMethods.Exists(m => m.IsEqualTo(method)))
					methodsToRemove.Add(method);
			}

			return methodsToRemove;
		}

		//--------------------------------------------------------------------------------
		private EBCompilerResults Build(string methodBody, bool signalCodeChanged)
		{
			if (disposing || IsDisposed)
				return null;

			//travaso nei sorgenti il testo attuale della form
			sources.CustomizationInfos.UserMethodsCode = methodBody;
            EBCompilerResults results = sourcesBuilder.Build(sources, false, string.Empty);

			return results;
		}

		/// <summary>
		/// Popola la griglia di result effettuando una nuova build temporanea
		/// </summary>
		//--------------------------------------------------------------------------------
		public void PopulateErrors()
		{
			ParseAndUpdateSyntaxTree(textEditorDocument.Text);
			PopulateErrors(Build(textEditorDocument.Text, false));
		}

		/// <summary>
		/// Popola la griglia di result della build appena effettuata
		/// </summary>
		/// <param name="result"></param>
		//--------------------------------------------------------------------------------
		private void PopulateErrors(EBCompilerResults result)
		{
			try
			{
				ClearAllControls();

				if (result == null || result.Errors.Count <= 0)
					return;

				foreach (EBCompilerError error in result.Errors)
				{
					int newRow = dgBuildResult.Rows.Add();

					Image bitmap = (!error.IsWarning) ? ImageLists.ErrorList.Images[0] : ImageLists.ErrorList.Images[1];
					dgBuildResult.Rows[newRow].Cells[0].Value = bitmap;
					dgBuildResult.Rows[newRow].Cells[1].Value = error.ErrorText;
					dgBuildResult.Rows[newRow].Cells[2].Value = error.ErrorNumber;
					dgBuildResult.Rows[newRow].Cells[3].Value = error.Line;
					dgBuildResult.Rows[newRow].Cells[4].Value = error.Column;
					dgBuildResult.AutoResizeRow(newRow);
					MarkError(error.Line, error.Column);

					//Non faccio vedere più di 100 errori
					if (newRow > 100)
						break;
				}

				dgBuildResult.ClearSelection();
			}
			catch
			{
			}
		}

		//--------------------------------------------------------------------------------
		private void DgBuildResult_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			int row = e.RowIndex;
			currentGotoErrorLine = row;
			GotoError(row);
		}

		/// <summary>
		/// Inserisce nella text area della finestra di codice un segnaposto che segnala la 
		/// posizione dell'errore individuato da riga e colonna
		/// </summary>
		/// <param name="line"></param>
		/// <param name="column"></param>
		//--------------------------------------------------------------------------------
		public void MarkError(int line, int column)
		{

			if (line < 0 || line >= textEditorDocument.LineCount)
				return;

			try
			{
				codeEditor.MarkError(line, column);
			}
			catch { }
		}

		//--------------------------------------------------------------------------------
		private void GotoError(int row)
		{
			if (
				row < 0 ||
				dgBuildResult.Rows.Count <= row ||
				dgBuildResult.Rows[row].Cells[3].Value == null ||
				dgBuildResult.Rows[row].Cells[4].Value == null
				)
				return;

			int line = int.Parse(dgBuildResult.Rows[row].Cells[3].Value.ToString());
			int column = int.Parse(dgBuildResult.Rows[row].Cells[4].Value.ToString());

			line--;
			column--;

			if (line < 0 || line >= textEditorDocument.LineCount)
				return;

			try
			{ 
				codeEditor.TextEditor.ScrollTo(line, column);
				codeEditor.TextEditor.CaretOffset = textEditorDocument.GetOffset(line, column);
				codeEditor.TextEditor.Focus();

			}
			catch { }
		}

		//--------------------------------------------------------------------------			
		private void GotoNextError()
		{
			if (dgBuildResult.Rows.Count <= 0)
				return;

			try
			{
				int row = ++currentGotoErrorLine;

				//se ho raggiunto l'ultima riga, ricomincio da zero
				if (row > dgBuildResult.Rows.Count - 1)
					row = currentGotoErrorLine = 0;

				GotoError(row);

				dgBuildResult.Rows[row].Selected = true;
				dgBuildResult.FirstDisplayedScrollingRowIndex = row;
			}
			catch
			{
			}
		}

		/// <summary>
		/// Al termine di una build viene aggiornato sia la result grid che lo status della build
		/// </summary>
		//--------------------------------------------------------------------------------
		void Sources_BuildCompleted(object sender, BuildEventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new Action(() => Sources_BuildCompleted(sender, e)));
				return;
			}

			if (IsDisposed || !IsHandleCreated)
				return;
	
			PopulateErrors(e.Results);
		}

		/// <summary>
		/// Pulisce e refresha tutti i control della finestra in vista di una nuova build
		/// </summary>
		//--------------------------------------------------------------------------------
		private void ClearAllControls()
		{
			codeEditor.RemoveAllErrors();
			dgBuildResult.Rows.Clear();
			currentGotoErrorLine = 0;
		}

		/// <remarks/>
		//--------------------------------------------------------------------------			
		private void DoKeyDown(KeyEventArgs e)
		{
			if (e.KeyValue == (int)Keys.F8) //F8, GotoNext Error
				GotoNextError();
		}
		
		//--------------------------------------------------------------------------------
		private void tsFormatCode_Click(object sender, EventArgs e)
		{
			ICSharpCode.AvalonEdit.Editing.Selection selection = textAreaControl.Selection;

			if (selection.Length == 0)
				textAreaControl.IndentationStrategy.IndentLines(codeEditor.TextEditor.Document, 0, textEditorDocument.LineCount - 1); 
			else
				textAreaControl.IndentationStrategy.IndentLines(codeEditor.TextEditor.Document, selection.StartPosition.Line, selection.EndPosition.Line); 
		}

		//--------------------------------------------------------------------------------
		private void TsSave_Click(object sender, EventArgs e)
		{
			Save();
			OnRefreshIntellisense(null);
		}

		//--------------------------------------------------------------------------------
		private void TsExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <remarks/>
		//--------------------------------------------------------------------------------
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Control && e.KeyCode == Keys.F10)
			{
				Save();
				return;
			}

			if (e.Control && e.KeyCode == Keys.F4)
			{
				Close();
			}
		}

		//--------------------------------------------------------------------------------
		private void tsComment_Click(object sender, EventArgs e)
		{
			codeEditor.CommentCode();
		}

		//--------------------------------------------------------------------------------
		private void tsUncomment_Click(object sender, EventArgs e)
		{
			codeEditor.UnCommentCode();
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		internal void OnDirtyChanged(DirtyChangedEventArgs e)
		{
			DirtyChanged?.Invoke(this, e);
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		internal void OnRefreshPropertyGrid(EventArgs e)
		{
			RefreshPropertyGrid?.Invoke(this, e);
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		internal void OnRefreshIntellisense(EventArgs e)
		{
			RefreshIntellisense?.Invoke(this, e);
		}

		/// <remarks/>
		//--------------------------------------------------------------------------------
		public void SetDirty(bool dirty)
		{
			if (suspendDirtyChanges)
				return;

			if (this.dirty == dirty)
				return;

			this.dirty = dirty;
			tsSave.Enabled = dirty;

			this.Text = UserMethodString + (dirty ? "*" : "");

			if (dirty)
				OnDirtyChanged(new DirtyChangedEventArgs(dirty));
		}

		//-----------------------------------------------------------------------------
		private void tsMethodsCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			MethodDeclaration meth = tsMethodsCombo?.ComboBox?.SelectedItem as MethodDeclaration;
			if (meth == null)
				return;

			SetCaretToMethod(meth);
		}

		//-----------------------------------------------------------------------------
		private void tsShowLineNumbers_Click(object sender, EventArgs e)
		{
			codeEditor.TextEditor.ShowLineNumbers = !codeEditor.TextEditor.ShowLineNumbers;
		}
	}

}


