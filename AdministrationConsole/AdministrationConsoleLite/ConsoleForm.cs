using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console
{
	/// <summary>
	/// ConsoleForm
	/// Form che contiene gli oggetti Tree, e i due panel delle working area
	/// della console. La form viene aggiunta alla Console.cs
	/// </summary>
	//=========================================================================
	public partial class ConsoleForm : System.Windows.Forms.Form
	{
		private static string nameSpacePlugInTreeNode = "Microarea.Console.Core.PlugIns.PlugInTreeNode";
		
		private ArrayList plugIns;
		private DiagnosticViewer	diagnosticViewer = new DiagnosticViewer();
        private Diagnostic			diagnostic = new Diagnostic("MicroareaConsole.ConsoleForm");
		
		public delegate void Errors(Diagnostic diagnostic);
		public event Errors OnErrors;

		public delegate void SelectedTreeNode(string nameOfAssembly, System.EventArgs e);
		public event SelectedTreeNode OnSelectedTreeNode;

		public ArrayList PlugIns { get { return plugIns; } set { plugIns = value;} }
        public Diagnostic Diagnostic { get { return diagnostic; } }

		#region Costruttore - Carico il panel della working area (il bottom non è visibile) e il tree
		/// <summary>
		/// Costruttore
		/// Disabilito il panel in fondo allo schermo
		/// I plugIn che necessitano di usarlo, se lo dovranno mettere a true (tanto lo ricevono nella Load)
		/// </summary>
		//---------------------------------------------------------------------
		public ConsoleForm()
		{
			InitializeComponent();
			plugInBottomWorkingArea.Enabled = false;
			plugInBottomWorkingArea.Visible = false;
			splitterHorizontal.TabStop		= false;
			splitterVertical.TabStop		= false;
		}
		#endregion

		#region FindPlugIn - Ricerca nell'array dei PlugIns caricati, il plugIn identificato dal tipo
		/// <summary>
		/// FindPlugIn
		/// Trova, nell'array dei plugIns già caricati, il plugIn identificato dal suo tipo
		/// </summary>
		/// <param name="assemblyType"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private object FindPlugIn(Type assemblyType)
		{
			if (plugIns == null) 
				return null;

			foreach (object plugin in plugIns)
				if (plugin.GetType() == assemblyType)
					return plugin;

			return null;
		}
		#endregion
		
		#region Funzioni di Select sul Tree di Console

		#region consoleTree_AfterSelect - Richimata ogni volta che si seleziona un nodo del tree
		/// <summary>
		/// consoleTree_AfterSelect
		/// Risponde ogni volta che si seleziona un nodo del tree della console.
		/// Viene chiamata la funzione OnAfterSelectConsoleTree nel plugIn al quale il nodo appartiene (se esiste). 
		/// Se la funzione non esiste nel plugIn, non fa nulla
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void consoleTree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			//Nodo selezionato
			if (e.Node == null) 
				return;

			PlugInTreeNode selectedNode = (PlugInTreeNode)e.Node;
			if (selectedNode == null)
				return;
			
			//Tipologia del nodo
			Type nodeType = e.Node.GetType();
			
			if ((String.Compare(nodeType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0) ||
		     (String.Compare(nodeType.BaseType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0))
			{
				if (OnSelectedTreeNode != null)
					OnSelectedTreeNode(selectedNode.AssemblyName, new EventArgs());

				Type type = selectedNode.AssemblyType;
				//Cerca nella collection dei plugIns caricati dalla console, quello a cui il nodo appartiene
				if (type == null)
					return;

				Object plugInLoaded = FindPlugIn(type);
				if (plugInLoaded == null)
					return;
				
				//se il plugIn è stato caricato invoca il metodo, passandogli i parametri opportuni
				if (plugInLoaded != null)
				{
					Type  assemblyType = plugInLoaded.GetType();
					MethodInfo actionPlugIn = assemblyType.GetMethod("OnAfterSelectConsoleTree");

					if (actionPlugIn == null && plugInWorkingArea != null)
						plugInWorkingArea.Controls.Clear();

					if (actionPlugIn != null)
					{
						Object[] parameters = new Object[2];
						parameters[0] = sender;
						parameters[1] = e;
						try
						{
							actionPlugIn.Invoke(plugInLoaded,parameters);
						}
						catch(TargetException targetExc)
						{
							string message = String.Format(Strings.PlugInMethodExecuting, "OnAfterSelectConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage = String.Format("{0} (consoleTree_AfterSelect)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(Strings.Description, targetExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, targetExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(ArgumentException argumentExc)
						{
							string message = String.Format(Strings.PlugInMethodExecuting, "OnAfterSelectConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage = String.Format("{0} (consoleTree_AfterSelect)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(Strings.Description, argumentExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, argumentExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(TargetInvocationException eTargetInvocationException)
						{
							string message = String.Format(Strings.PlugInMethodExecuting, "OnAfterSelectConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage = String.Format("{0} (consoleTree_AfterSelect)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(Strings.Description, eTargetInvocationException.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, eTargetInvocationException.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(TargetParameterCountException parameterCountExc)
						{
							string message = String.Format(Strings.PlugInMethodExecuting, "OnAfterSelectConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage = String.Format("{0} (consoleTree_AfterSelect)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(Strings.Description, parameterCountExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, parameterCountExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(System.MethodAccessException methodAccessExc)
						{
							string message = String.Format(Strings.PlugInMethodExecuting, "OnAfterSelectConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage = String.Format("{0} (consoleTree_AfterSelect)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(Strings.Description, methodAccessExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, methodAccessExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
					}
				}
			}
		}

		#endregion

		#region consoleTree_MouseDown - Intercetta l'evento di MouseDown (per attach del Context menu)
		/// <summary>
		/// consoleTree_MouseDown
		/// Intercetta l'evento di MouseDown sul tree e, se presente, attacca al nodo selezionato il contextMenu 
		/// precedentemente impostato per quel nodo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void consoleTree_MouseDown(object sender, MouseEventArgs e)
		{
            TreeView localTree = (TreeView)sender;

            PlugInTreeNode nodeToSelectIfNull =
                (PlugInTreeNode)localTree.Nodes[0].FirstNode == null
                ? (PlugInTreeNode)localTree.Nodes[0]
                : (PlugInTreeNode)localTree.Nodes[0].FirstNode;

            //se ho cliccato in una zona fuori dal tree seleziono come nodo di destinazione il primo nodo del tree
            localTree.SelectedNode =
                ((PlugInTreeNode)localTree.GetNodeAt(e.X, e.Y) != null)
                ? (PlugInTreeNode)localTree.GetNodeAt(e.X, e.Y)
                : nodeToSelectIfNull;

            consoleTree.ContextMenuStrip = null;
            consoleTree.ContextMenu = null;

            PlugInTreeNode selected = (PlugInTreeNode)localTree.SelectedNode;

            if (selected != null)
            {
                Type nodeType = selected.GetType();
                if ((String.Compare(nodeType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0) ||
                    (String.Compare(nodeType.BaseType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0))
                {
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            break;
                        case MouseButtons.Right:
                            if (selected.ContextMenuStrip != null)
                                consoleTree.ContextMenuStrip = selected.ContextMenuStrip;
                            else if (selected.ContextMenu != null)
                                consoleTree.ContextMenu = selected.ContextMenu;
                            break;
                        case MouseButtons.Middle:
                        case MouseButtons.None:
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                consoleTree.ContextMenu = null;

		}
		#endregion

		#region consoleTree_DoubleClick - Intercetta l'evento di DoubleClick sul Tree 
		/// <summary>
		/// consoleTree_DoubleClick
		/// Intercetta l'evento di DoubleClick del tree e, se presente, carica il metodo 
		/// OnAfterDoubleClickConsoleTree presente nel plugIn del nodo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void consoleTree_DoubleClick(object sender, System.EventArgs e)
		{
			TreeView selectedTree = (TreeView)sender;
			if (selectedTree == null)
				return;

			PlugInTreeNode selectedNode	= (PlugInTreeNode)(selectedTree.SelectedNode);
			if (selectedNode == null) 
				return;

			Type nodeType = selectedNode.GetType();

			if ((String.Compare(nodeType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0) ||
			 (String.Compare(nodeType.BaseType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0))
			{
				if (OnSelectedTreeNode != null)
					OnSelectedTreeNode(selectedNode.AssemblyName, new EventArgs());

				Type type = selectedNode.AssemblyType;
				if (type == null)
					return;
				//Cerca nella collection dei plugIns caricati dalla console, quello a cui il nodo appartiene
				Object plugInLoaded = FindPlugIn(type);

				//se il plugIn è stato caricato invoca il metodo, passandogli i parametri opportuni
				if (plugInLoaded != null)
				{
					Type  assemblyType		= plugInLoaded.GetType();
					MethodInfo actionPlugIn = assemblyType.GetMethod("OnAfterDoubleClickConsoleTree");
	
					if (actionPlugIn != null)
					{
						Object[] parameters = new Object[2];
						parameters[0] = sender;
						parameters[1] = e;
						try
						{
							actionPlugIn.Invoke(plugInLoaded,parameters);
						}
						catch(TargetException targetExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterDoubleClickConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_DoubleClick)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, targetExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, targetExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
						}
						catch(ArgumentException argumentExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterDoubleClickConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_DoubleClick)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, argumentExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, argumentExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(TargetInvocationException eTargetInvocationException)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterDoubleClickConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_DoubleClick)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, eTargetInvocationException.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, eTargetInvocationException.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(TargetParameterCountException parameterCountExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterDoubleClickConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_DoubleClick)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, parameterCountExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, parameterCountExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(System.MethodAccessException methodAccessExc)
						{
							string message = String.Format(Strings.PlugInMethodExecuting, "OnAfterDoubleClickConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_DoubleClick)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(Strings.Description, methodAccessExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, methodAccessExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
					}
				}
			}
		}
		#endregion

		#region consoleTree_KeyDown - Intercetta la pressione del tasto CANC o DELETE sul Tree
		/// <summary>
		/// consoleTree_KeyDown
		/// Routine per la gestione del tasto CANC o DELETE intercettato sul tree
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void consoleTree_KeyDown(object sender, KeyEventArgs e)
		{
			TreeView selectedTree = (TreeView)sender;
			if (selectedTree == null)
				return;

			PlugInTreeNode selectedNode	= (PlugInTreeNode)selectedTree.SelectedNode;
			if (selectedNode == null) 
				return;

			Type nodeType = selectedNode.GetType();
			if ((String.Compare(nodeType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0) ||
				(String.Compare(nodeType.BaseType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0))
			{
				if (OnSelectedTreeNode != null)
					OnSelectedTreeNode(selectedNode.AssemblyName, new EventArgs());

				Type type = selectedNode.AssemblyType;
				if (type == null)
					return;
				//Cerca nella collection dei plugIns caricati dalla console, quello a cui il nodo appartiene
				Object plugInLoaded	= FindPlugIn(type);
				
				//se il plugIn è stato caricato invoca il metodo, passandogli i parametri opportuni
				if (plugInLoaded != null)
				{
					Type  assemblyType		= plugInLoaded.GetType();
					MethodInfo actionPlugIn = assemblyType.GetMethod("OnAfterKeyDownConsoleTree");
					
					if (actionPlugIn != null)
					{
						Object[] parameters = new Object[2];
						parameters[0]		= sender;
						parameters[1]		= e;
						try
						{
							actionPlugIn.Invoke(plugInLoaded,parameters);
						}
						catch(TargetException targetExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterKeyDownConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_KeyDown)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, targetExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, targetExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(ArgumentException argumentExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterKeyDownConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_KeyDown)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, argumentExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, argumentExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(TargetInvocationException eTargetInvocationException)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterKeyDownConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_KeyDown)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, eTargetInvocationException.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, eTargetInvocationException.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(TargetParameterCountException parameterCountExc)
						{
							string message = String.Format(Strings.PlugInMethodExecuting, "OnAfterKeyDownConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_KeyDown)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(Strings.Description, parameterCountExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, parameterCountExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(System.MethodAccessException methodAccessExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterKeyDownConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_KeyDown)", ConstStrings.ApplicationName);
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, methodAccessExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, methodAccessExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
					}
				}
			}
		}
		#endregion

		#region consoleTree_AfterExpand - Intercetta l'evento di AfterExpand sul tree
		/// <summary>
		/// consoleTree_AfterExpand
		/// Intercetta l'evento di AfterExpand del tree e, se presente, carica il metodo 
		/// OnAfterExpandConsoleTree presente nel plugIn del nodo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void consoleTree_AfterExpand(object sender, TreeViewEventArgs e)
		{
			TreeView selectedTree = (TreeView)sender;
			if (selectedTree == null || selectedTree.SelectedNode == null)
				return;
			
			PlugInTreeNode selectedNode	= (PlugInTreeNode)(selectedTree.SelectedNode);
			if (selectedNode == null) 
				return;

			Type nodeType = selectedNode.GetType();
			if ((String.Compare(nodeType.FullName, nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0) ||
				(String.Compare(nodeType.BaseType.FullName,	nameSpacePlugInTreeNode, true, CultureInfo.InvariantCulture) == 0))
			{
				if (OnSelectedTreeNode != null)
					OnSelectedTreeNode(selectedNode.AssemblyName, new EventArgs());

				Type type = selectedNode.AssemblyType;
				if (type == null)
					return;
				// Cerca nella collection dei plugIns caricati dalla console, quello a cui il nodo appartiene
				Object plugInLoaded = FindPlugIn(type);
				
				// se il plugIn è stato caricato invoca il metodo, passandogli i parametri opportuni
				if (plugInLoaded != null)
				{
					Type assemblyType		= plugInLoaded.GetType();
					MethodInfo actionPlugIn = assemblyType.GetMethod("OnAfterExpandConsoleTree");
					
					if (actionPlugIn != null)
					{
						Object[] parameters = new Object[2];
						parameters[0] = sender;
						parameters[1] = e;
						
						try
						{
							actionPlugIn.Invoke(plugInLoaded,parameters);
						}
						catch(TargetException targetExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterExpandConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_AfterExpand)", ConstStrings.ApplicationName);	
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, targetExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, targetExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(ArgumentException argumentExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterExpandConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_AfterExpand)", ConstStrings.ApplicationName);	
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, argumentExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, argumentExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(TargetInvocationException eTargetInvocationException)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterExpandConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_AfterExpand)", ConstStrings.ApplicationName);	
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, eTargetInvocationException.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, eTargetInvocationException.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(TargetParameterCountException parameterCountExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterExpandConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_AfterExpand)", ConstStrings.ApplicationName);	
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, parameterCountExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, parameterCountExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
						catch(System.MethodAccessException methodAccessExc)
						{
							string message				= String.Format(Strings.PlugInMethodExecuting, "OnAfterExpandConsoleTree", plugInLoaded.GetType().Name);
							string calledByMessage		= String.Format("{0} (consoleTree_AfterExpand)", ConstStrings.ApplicationName);	
							ExtendedInfo extendedInfo	= new ExtendedInfo();
							extendedInfo.Add(Strings.Description, methodAccessExc.Message);
							extendedInfo.Add(Strings.NodeSelected, selectedNode.Text);
							extendedInfo.Add(Strings.CalledBy, calledByMessage);
							extendedInfo.Add(Strings.Source, methodAccessExc.Source);
							Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							if (OnErrors != null)
								OnErrors(Diagnostic);
							Diagnostic.Clear();
						}
					}
				}
			}
		}
		#endregion

		#region consoleTree_BeforeSelect - Intercetto l'evento di BeforeSelect sul Tree
		/// <summary>
		/// consoleTree_BeforeSelect
		/// Intercetta l'evento di BeforeSelect del tree e, se presente, carica il metodo 
		/// OnBeforeSelectConsoleTree presente nel plugIn del nodo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void consoleTree_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			//c'è qualcosa nella working area? Se sì vado a vedere se è una PlugInsForm
			if (plugInWorkingArea.Controls.Count > 0)
			{
				Type plugInsForm = plugInWorkingArea.Controls[0].GetType().BaseType;
				
				if (plugInsForm != null && 
					string.Compare(plugInsForm.Name, "PlugInsForm", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					switch (((PlugInsForm)plugInWorkingArea.Controls[0]).State)
					{
						case StateEnums.Processing :
							e.Cancel = true; 
							break;
						case StateEnums.Editing :
							e.Cancel = !AskIfContinue(StateEnums.Editing); 
							break;
						case StateEnums.Waiting :
							e.Cancel = !AskIfContinue(StateEnums.Waiting); 
							break;
						case StateEnums.View :
						case StateEnums.None : 
							e.Cancel = false; 
							break;
					}
				}
			}
			else if (plugInBottomWorkingArea.Enabled && plugInBottomWorkingArea.Controls.Count > 0)
			{
				Type plugInsForm = plugInBottomWorkingArea.Controls[0].GetType().BaseType;
				
				if (plugInsForm != null && 
					string.Compare(plugInsForm.Name, "PlugInsForm", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					switch (((PlugInsForm)plugInBottomWorkingArea.Controls[0]).State)
					{
						case StateEnums.Processing : 
							e.Cancel = true; 
							break;
						case StateEnums.Editing : 
							e.Cancel = !AskIfContinue(StateEnums.Editing); 
							break;
						case StateEnums.Waiting :
							e.Cancel = !AskIfContinue(StateEnums.Waiting); 
							break;
						case StateEnums.View :
						case StateEnums.None : 
							e.Cancel = false; 
							break;
					}
				}
			}
		}

		/// <summary>
		/// AskIfContinue
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public bool AskIfContinue(StateEnums typeOfState)
		{
			DialogResult resultIfContinue =  MessageBox.Show
				(
					this,
					(typeOfState == StateEnums.Editing) ? Strings.AskIfQuitToFrom : Strings.AskIfQuitToAction,
					Strings.LblAskIfQuitToForm, 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question, 
					MessageBoxDefaultButton.Button2
				);
			
			return (resultIfContinue == DialogResult.Yes);
		}
		#endregion
		#endregion
	}
}