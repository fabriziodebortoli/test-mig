using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.MVC
{
	/// <summary>
	/// It is the object responsible for loading customizations, dispatching
	/// document events and document life cycle events.
	/// It also logs all exceptions raised by controllers during their work.
	/// </summary>
	/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentController"/>
	//=============================================================================
	public class DocumentControllers : List<IDocumentController>, IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public const string DisabledCustFileExtension = "disabled";

		private IDictionary<DocumentController, string> controllerPaths = new Dictionary<DocumentController, string>();
		private IList<string> wrongAssembliesName = new List<string>();

		/// <summary>
		/// Gets a value indicating whether there is at least one controller
		/// attached to a document event or not.
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool HasEventManagerEvents
		{
			get
			{
				foreach (DocumentController controller in this)
				{
					if (controller == null)
						continue;
					if (controller.HasEventManagerEvents)
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Internal Use, returns the filesystem path of the controller
		/// </summary>
		//-----------------------------------------------------------------------------
		public string GetControllerPathByController(DocumentController controller)
		{
			string controllerPath = null;
			controllerPaths.TryGetValue(controller, out controllerPath);
			return controllerPath;
		}

		/// <summary>
		/// Gets the list of wrong assemblies names.
		/// </summary>
		/// <remarks>
		/// An assembly is wrong if it is not loadable due to file system troubles,
		/// if it has a bad format image or due to reflection exceptions during the
		/// creation of instances using types of the just loaded assemlby.
		/// </remarks>
		//-----------------------------------------------------------------------------
		public IList<string> WrongAssembliesName
		{
			get { return wrongAssembliesName; }
		}

		/// <summary>
		/// Gets a value indicating if there had been troubles loading customizations.
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool WereThereLoadingTroubles
		{
			get { return wrongAssembliesName.Count > 0; }
		}

		/// <summary>
		/// Dispatches a document life cycle event to all loaded customizations.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventManagerArgs"/>
		//-----------------------------------------------------------------------------
		public void DispatchEvent(ControllerEventManagerArgs args)
		{
			if (Count == 0)
				return;
			
			foreach (DocumentController controller in this)
			{
				if (controller == null || !controller.CanBeLoaded)
					continue;

				try
				{
					controller.ReactToManagedClientDocEvent(args);
				}
				catch (Exception exc)
				{
					Controller_ExceptionRaised(controller, new ExceptionRaisedEventArgs(exc));
				}
			}
		}
		/// <summary>
		/// Dispatches a report event to all loaded customizations.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventManagerArgs"/>
		//-----------------------------------------------------------------------------
		public void DispatchEvent(WoormEventArgs args)
		{
			if (Count == 0)
				return;

			foreach (DocumentController controller in this)
			{
				if (controller == null || !controller.CanBeLoaded)
					continue;

				try
				{
					controller.ReactToManagedClientDocEvent(args);
				}
				catch (Exception exc)
				{
					Controller_ExceptionRaised(controller, new ExceptionRaisedEventArgs(exc));
				}
			}
		}
		/// <summary>
		/// Dispatches a batch event to all loaded customizations.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ControllerEventManagerArgs"/>
		//-----------------------------------------------------------------------------
		public void DispatchEvent(BatchEventArgs args)
		{
			if (Count == 0)
				return;

			foreach (DocumentController controller in this)
			{
				if (controller == null || !controller.CanBeLoaded)
					continue;

				try
				{
					controller.ReactToManagedClientDocEvent(args);
				}
				catch (Exception exc)
				{
					Controller_ExceptionRaised(controller, new ExceptionRaisedEventArgs(exc));
				}
			}
		}
		/// <summary>
		/// Dispatches a lock document event to all loaded customizations.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ManagedClientDocEvent"/>
		//-----------------------------------------------------------------------------
		public bool DispatchEvent(ManagedClientDocEvent clientDocEvent, ControllerEventArgs e)
		{
			bool goOn = false;
			foreach (DocumentController controller in this)
			{
				if (controller == null || !controller.CanBeLoaded)
					continue;

				try
				{
					goOn = controller.ReactToManagedClientDocEvent(clientDocEvent, e);
				}
				catch (Exception exc)
				{
					Controller_ExceptionRaised(controller,new ExceptionRaisedEventArgs(exc));
				}
				if (!goOn)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Dispatches a document event to all loaded customizations.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ManagedClientDocEvent"/>
		//-----------------------------------------------------------------------------
		public bool Dispatch(ManagedClientDocEvent clientDocEvent)
		{
			if (Count == 0)
				return true;

			bool goOn = false;
			foreach (DocumentController controller in this)
			{
				if (controller == null || !controller.CanBeLoaded)
					continue;

				try
				{
					goOn = controller.ReactToManagedClientDocEvent(clientDocEvent, new ControllerEventArgs());
				}
				catch (Exception exc)
				{
					Controller_ExceptionRaised(controller, new ExceptionRaisedEventArgs(exc));
				}
				if (!goOn)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns the DocumentController identified by the given namespace if any,
		/// otherwise null.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentController"/>
		//-----------------------------------------------------------------------------
		public DocumentController GetControllerByName(string name, bool onlyCurrentContext = true)
		{
			if (String.IsNullOrWhiteSpace(name))
				return null;

			foreach (DocumentController controller in this)
			{
                // prima controllo il nome
                if (String.Compare(name, controller.Name, StringComparison.InvariantCulture) != 0)
                    continue;

                if (onlyCurrentContext)
                {
                    INameSpace ownerNamespace = BasePathFinder.BasePathFinderInstance.GetNamespaceFromPath(GetControllerPathByController(controller));

                    // non posso controllarlo
                    if (ownerNamespace == null)
                        return controller;

                    // non e' lo stesso contesto
                    if (string.Compare(ownerNamespace.Application, BaseCustomizationContext.CustomizationContextInstance.CurrentApplication) != 0 ||
                        string.Compare(ownerNamespace.Module, BaseCustomizationContext.CustomizationContextInstance.CurrentModule) != 0)
                        continue;
                }
                return controller;
            }

			return null;
		}

		/// <summary>
		/// Loads all customizations from the file system.
		/// </summary>
		/// <param name="docNamespace">The path where to load assemblies is calculated using the namespace of
		/// the given MDocument.</param>
		/// <param name="documentPtr">The handle to the document to be wrapped.</param>
		/// <param name="formPtr">The handle to the view to be wrapped.</param>
		/// <seealso cref="Microarea.Framework.TBApplicationWrapper.MDocument"/>
		//-----------------------------------------------------------------------------
		public void LoadEasyBuilderApps(INameSpace docNamespace, IntPtr documentPtr, IntPtr formPtr)
		{
			List<ControllerBag> controllersType = new List<ControllerBag>();
			foreach (string path in BasePathFinder.BasePathFinderInstance.GetEasyBuilderAppAssembliesPaths(docNamespace, CUtility.GetUser(), BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications))
			{ 
				LoadControllerTypesServerDocumentAtFirst(path, controllersType, docNamespace);
			}

			List<string> modulePaths = new List<string>();
			foreach (EBLink linkDll in BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppStandardizationLinks(docNamespace))
			{
				string path = AssemblyPackager.GetEBModuleAssemblyPath(linkDll.Application, linkDll.Module);
				if (!modulePaths.Contains(path))
					modulePaths.Add(path);
			}
			foreach (string path in modulePaths)
				LoadControllerTypesServerDocumentAtFirst(path, controllersType, docNamespace);
			CreateAndAddControllers(docNamespace, documentPtr, formPtr, controllersType);
   		}

		//-----------------------------------------------------------------------------
		private void CreateAndAddControllers(INameSpace docNamespace, IntPtr documentPtr, IntPtr formPtr, List<ControllerBag> controllersType)
		{
			foreach (ControllerBag controllerBag in controllersType)
				CreateAndAddController(docNamespace, documentPtr, formPtr, controllerBag.AssemblyPath, controllerBag.ControllerType);
		}

		//=====================================================================
		private class ControllerBag
		{
			internal Type ControllerType { get; set; }
			internal string AssemblyPath { get; set; }

			//-----------------------------------------------------------------
			public ControllerBag(Type controllerType, string assemblyPath)
			{
				this.ControllerType = controllerType;
				this.AssemblyPath = assemblyPath;
			}
		}

		//-----------------------------------------------------------------------------
		private void LoadControllerTypesServerDocumentAtFirst(
			string path,
			List<ControllerBag> controllerBags,
			INameSpace docNamespace
			)
		{
			LoadControllerTypesServerDocumentAtFirst(
				controllerBags,
				docNamespace,
				path
				);
		}

		//---------------------------------------------------------------------
		private void LoadControllerTypesServerDocumentAtFirst(
			List<ControllerBag> controllerBags,
			INameSpace docNamespace,
			string path
			)
        {
            Assembly asm = null;
            try
            {
                asm = AssembliesLoader.Load(path);
            }
            catch (Exception)
            {
                this.Controller_ExceptionRaised(
                    this,
                    new ExceptionRaisedEventArgs(
                        new Exception(String.Format("Assembly {0} not loaded", path))
                        )
                    );
                return;
            }

            if (!IsActivated(asm))
                return;

            LoadControllerBags(controllerBags, docNamespace, path, asm);
        }

        private static void LoadControllerBags(List<ControllerBag> controllerBags, INameSpace docNamespace, string path, Assembly asm)
        {
            NameSpace safeDocNs = ControllerSources.GetSafeSerializedNamespace(docNamespace.FullNameSpace);

            foreach (Type t in asm.GetTypes())
            {
                if (t.IsSubclassOf(typeof(DocumentController)))
                {
                    NameSpace ns = ControllerSources.GetSafeSerializedNamespace(t.FullName);

                    if (ns.Application == safeDocNs.Application && ns.Module == safeDocNs.Module && ns.Document == safeDocNs.Document)
                    {
                        controllerBags.Add(new ControllerBag(t, path));
                        break;//un solo controller per assembly
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        private bool IsActivated(Assembly asm)
		{
			string app, mod;
			if (!OwnerEasyBuilderAppAttribute.GetModuleInfo(asm, out app, out mod))
				return true;//se non ho l'attributo, lascio passare
			return CUtility.IsActivated(app, mod);
		}

		/// <summary>
		/// Loads a customization and a server document if exists.
		/// </summary>
		/// <param name="documentPtr">The handle of the document to be wrapped</param>
		/// <param name="formPtr">The handle of the document view to be wrapped</param>
		/// <param name="documentNameSpace">The namespace of the document to load</param>
		/// <param name="customizationNameSpace">The namespace of the controller</param>
		//-----------------------------------------------------------------------------
		public void LoadAssembliesForEdit(IntPtr documentPtr, IntPtr formPtr, INameSpace documentNameSpace,  INameSpace customizationNameSpace)
		{
			List<ControllerBag> orderedControllerTypeToLoad = new List<ControllerBag>();

			string pathRoot = BasePathFinder.BasePathFinderInstance.GetApplicationPath(documentNameSpace);
			string userPathRoot = String.Empty;


			foreach (IEasyBuilderApp app in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
			{
				if (!BaseCustomizationContext.CustomizationContextInstance.IsCurrentEasyBuilderAppAStandardization)
					userPathRoot = BasePathFinder.BasePathFinderInstance.GetCustomizationPath(documentNameSpace, CUtility.GetUser(), app);

				DirectoryInfo pathRootDirInfo = new DirectoryInfo(pathRoot);
				DirectoryInfo userPathRootDirInfo = String.IsNullOrWhiteSpace(userPathRoot) ? null : new DirectoryInfo(userPathRoot);

				string assemblyToLoadFullPath = null;
				FileInfo[] dllFileInfos = null;

				//Comincio a spazzolare nella cartella delle dll pubblicate per tutti gli utenti.
				bool startSearchingFromPublishedDlls = pathRootDirInfo.Exists && (dllFileInfos = pathRootDirInfo.GetFiles(NameSolverStrings.DllSearchCriteria)).Length > 0;
				if (!startSearchingFromPublishedDlls && userPathRootDirInfo != null && userPathRootDirInfo.Exists)
					//Se non ci sono dll pubblicate per tutti gli utenti allora vado a cercare SOLO nella cartella dell'utente corrente.
					dllFileInfos = userPathRootDirInfo.GetFiles(NameSolverStrings.DllSearchCriteria);

				if (dllFileInfos != null && dllFileInfos.Length > 0)
				{
					foreach (FileInfo fi in dllFileInfos)
					{
						//Se ho cominciato a cercare nella cartella pubblica allora, prima di caricare la dll fi,
						//guardo se esiste una dll fi per l'utente corrente.
						//Se esiste allora la carico, altrimenti carico quella pubblica.
						//Se invece ho cominciato a cercare dalla cartella dell'utente corrente allora significa che non c'è alcuna dll pubblicata
						//per cui sicuramente assemblyToLoadFullPath = pathRootDirInfo.FullName non esisterà su file system per cui
						//l'if(!File.Exists(assemblyToLoadFullPath) mi riassegna assemblyToLoadFullPath = fi.FullName che è il nome corretto per la dll
						//non pubblicata.
						assemblyToLoadFullPath = startSearchingFromPublishedDlls && userPathRootDirInfo != null
													? Path.Combine(userPathRootDirInfo.FullName, fi.Name)
													: pathRootDirInfo.FullName;

						if (!File.Exists(assemblyToLoadFullPath))
							assemblyToLoadFullPath = fi.FullName;

						Assembly asm = null;
						try
						{
							asm = AssembliesLoader.Load(assemblyToLoadFullPath);
						}
						catch (Exception exc)
						{
							this.Controller_ExceptionRaised(this, new ExceptionRaisedEventArgs(exc));
							continue;
						}
					}
				}

				//Carichiamo il controller modificando dalla cartella relativa all'utente corrente.
				string path = BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppAssemblyFullName(customizationNameSpace, CUtility.GetUser(), app);
				//Se non esiste allora entro in modifica della dll pubblicata per tutti gli utenti
				if (!File.Exists(path))
					path = BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppAssemblyFullName(customizationNameSpace, null, app);

				if (File.Exists(path))
				{
					Assembly asm = null;
					try
					{
						asm = AssembliesLoader.Load(path);
					}
					catch (Exception exc)
					{
						this.Controller_ExceptionRaised(this, new ExceptionRaisedEventArgs(exc));
					}

                    LoadControllerBags(orderedControllerTypeToLoad, documentNameSpace, path, asm);
                }
            }

            if (orderedControllerTypeToLoad.Count > 0)
                CreateAndAddControllers(documentNameSpace, documentPtr, formPtr, orderedControllerTypeToLoad);
        }

        //-----------------------------------------------------------------------------
        private Type GetDocumentControllerTypeFromAssembly(Assembly asm)
		{
			foreach (Type t in asm.GetTypes())
			{
				if (t.IsSubclassOf(typeof(DocumentController)))
					return t;
			}
			return null;
		}

		/// <summary>
		/// Loads a customization.
		/// </summary>
		/// <param name="documentNamespace">The document namespace</param>
		/// <param name="path">The path of the assembly containing customizations</param>
		/// <param name="documentPtr">The handle of the document to be wrapped</param>
		/// <param name="formPtr">The handle of the document view to be wrapped</param>
		//-----------------------------------------------------------------------------
		internal void LoadAssembliesForEdit(INameSpace documentNamespace, string path, IntPtr documentPtr, IntPtr formPtr)
		{
			Assembly asm = null;
			try
			{
				asm = AssembliesLoader.Load(path);
			}
			catch (Exception exc)
			{
				wrongAssembliesName.Add(path);
				this.Controller_ExceptionRaised(this, new ExceptionRaisedEventArgs(exc));
				return;
			}

			Load(documentNamespace, asm, documentPtr, formPtr, path);
		}

		/// <summary>
		/// Loads a customization.
		/// </summary>
		/// <param name="documentNamespace">The document namespace</param>
		/// <param name="asm">The assembly containing customizations</param>
		/// <param name="documentPtr">The handle of the document to be wrapped</param>
		/// <param name="formPtr">The handle of the document view to be wrapped</param>
		//-----------------------------------------------------------------------------
		public void LoadAssembliesForEdit(INameSpace documentNamespace, Assembly asm, IntPtr documentPtr, IntPtr formPtr)
		{
			Load(documentNamespace,asm, documentPtr, formPtr, String.Empty);
		}

		/// <summary>
		/// Loads all customizations contained in the given assembly.
		/// </summary>
		/// <param name="documentNamespace">The document namespace</param>
		/// <param name="asm">The assembly containing customizations</param>
		/// <param name="documentPtr">The handle of the document to be wrapped</param>
		/// <param name="formPtr">The handle of the document view to be wrapped</param>
		/// <param name="assemblyPath">The file system path of the assembly</param>
		/// 
		/// N.B.: Devo passarmi assemblyPath perchè non riesco a risalire alla location di asm attraverso
		/// la proprietà Codebase poichè, avendo caricato l'assembly da un array di byte in memoria,
		/// tale proprietà rimane nulla.
		//--------------------------------------------------------------------------------
		private void Load(INameSpace documentNamespace, Assembly asm, IntPtr documentPtr, IntPtr formPtr, string assemblyPath)
		{
			if (asm == null)
			{
				this.Controller_ExceptionRaised(this, new ExceptionRaisedEventArgs(new ArgumentNullException("asm")));
				return;
			}

			try
			{
				foreach (Type t in asm.GetTypes())
				{
					if (t.IsSubclassOf(typeof(DocumentController)))
					{
						CreateAndAddController(documentNamespace,documentPtr, formPtr, assemblyPath, t);
						break;//un solo controller per assembly
					}
				}
			}
			catch (Exception exc)
			{
				wrongAssembliesName.Add(assemblyPath);
				this.Controller_ExceptionRaised(this, new ExceptionRaisedEventArgs(exc));
			}
		}

		//--------------------------------------------------------------------------------
		private void CreateAndAddController(
			INameSpace docNamespace,
			IntPtr documentPtr,
			IntPtr formPtr,
			string assemblyPath,
			Type controllerType
			)
		{
			try
			{
				int firstDot = controllerType.Namespace.IndexOf(".");
				if (firstDot < 0)
					return;

				string type = controllerType.Namespace.Left(firstDot);
				if (type.IsNullOrEmpty())
				{
					Debug.Fail("Invalid Namespace");
					return;
				}

				DocumentController controller = (DocumentController)Activator.CreateInstance
					(
						controllerType,
						new NameSpace(string.Format("{0}.{1}", type, docNamespace.GetNameSpaceWithoutType())), 
						formPtr, 
						documentPtr
					);

                if (controller != null && controller.View != null)
                    controller.View.Visible = true;

				controller.ExceptionRaised += new EventHandler<ExceptionRaisedEventArgs>(Controller_ExceptionRaised);

				Add(controller);
				controllerPaths.Add(controller, assemblyPath);
			}
			catch (Exception exc)
			{
				wrongAssembliesName.Add(assemblyPath);
				this.Controller_ExceptionRaised(this, new ExceptionRaisedEventArgs(exc));
			}
		}

		/// <summary>
		/// Unloads a not working customization.
		/// A customization is "not working" if it causes exceptions during its life cycle.
		/// The customization is also disabled in order not to be loaded in the future. The
		/// assembly still remains on file system but the file extension is turned to ".disabled".
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.MVC.DocumentController"/>
		//-----------------------------------------------------------------------------
		public void UnloadAndDisableNotWorkingControllers(IList<DocumentController> controllers)
		{
			IList<string> notWorkingAssembliesPaths = new List<string>();
			foreach (DocumentController controller in controllers)
			{
				Remove(controller);
				notWorkingAssembliesPaths.Add(controllerPaths[controller]);
				controllerPaths.Remove(controller);
				try
				{
					controller.Dispose();
				}
				catch (Exception exc)
				{
					Controller_ExceptionRaised(this, new ExceptionRaisedEventArgs(exc));
				}
			}
			DisableNotWorkingAssemblies(notWorkingAssembliesPaths);
		}

		/// <summary>
		/// Disable a not working customization.
		/// A customization is "not working" if it causes exceptions during its life cycle.
		/// The assembly still remains on file system but the file extension is turned
		/// to ".disabled".
		/// </summary>
		//-----------------------------------------------------------------------------
		public void DisableNotWorkingAssemblies(IList<string> notWorkingAssemblies)
		{
			foreach (string asm in notWorkingAssemblies)
			{
				if (!File.Exists(asm))
					continue;

				try
				{
					//La disabilitazione consiste nel rinominare il file da .dll a .disabled.
					string disabledFile = Path.ChangeExtension(asm, DisabledCustFileExtension);
					if (File.Exists(disabledFile))
						File.Delete(disabledFile);
					File.Move(asm, disabledFile);
				}
				catch (Exception exc)
				{
					this.Controller_ExceptionRaised(this, new ExceptionRaisedEventArgs(exc));
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void Controller_ExceptionRaised(object sender, ExceptionRaisedEventArgs e)
		{
			LogExceptionsToFile(sender.GetType().Namespace, e.RaisedException);
		}

		/// <summary>
		/// Logs the given exception to a log file.
		/// </summary>
		//-----------------------------------------------------------------------------
		public void LogExceptionsToFile(string nameSpace, Exception e)
		{
			try
			{
                if (nameSpace == null)
                {
                    nameSpace = "<namespace is null>";
                }
				string exceptionMessage = "Null exception received";
				string exceptionStackTrace = "No stack trace available";
				if (e != null)
				{
					exceptionMessage = e.Message;
					exceptionStackTrace = e.StackTrace;
				}

				string errorMessage = string.Format
					(
					Resources.ExceptionStackTrace,
					DateTime.Now.ToString(@"yyyy-MM-ddTHH\:mm\:ss\:fffffffK"),
					nameSpace,
					exceptionMessage,
					exceptionStackTrace
					);

				lock (typeof(DocumentController))
				{
					string fileName = BasePathFinder.BasePathFinderInstance.GetCustomizationLogFullName();
					if (!Directory.Exists(Path.GetDirectoryName(fileName)))
						Directory.CreateDirectory(Path.GetDirectoryName(fileName));

					File.AppendAllText(
						fileName,
						errorMessage + "\r\n-------------------------------------\r\n"
						);
				}
			}
			catch (Exception)
			{ }
		}

		/// <summary>
		/// Releases all resources used by this instance.
		/// </summary>
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
				foreach (DocumentController controller in this)
				{
					controller.ExceptionRaised -= new EventHandler<ExceptionRaisedEventArgs>(Controller_ExceptionRaised);
					controller.Dispose();
				}
				wrongAssembliesName.Clear();
				controllerPaths.Clear();
			}
		}
	}
}
