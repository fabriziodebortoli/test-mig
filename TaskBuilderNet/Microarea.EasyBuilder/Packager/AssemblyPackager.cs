using System;
using System.IO;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	internal partial class AssemblyPackager : ThemedForm
	{
		private CancellationTokenSource token;
		private Task task;
		private bool cloeseOnEnd;

		internal enum ProcedureBehaviour { Unattended, AttendedLeaveDialog, AttendedCloseDialog }
		//--------------------------------------------------------------------------------
		private AssemblyPackager()
		{
			InitializeComponent();
		}
		//--------------------------------------------------------------------------------
		private static void Message(AssemblyPackager dialog, string message, params object[] args)
		{
			if (dialog == null)
				return;
			message = string.Format(message, args);
			dialog.Message(message);
		}
		//--------------------------------------------------------------------------------
		private void Message(string message)
		{
			BeginInvoke((Action)delegate { textBoxMessage.AppendText(message); textBoxMessage.AppendText(Environment.NewLine); });
		}
		//--------------------------------------------------------------------------------
		internal static void Build(ProcedureBehaviour behaviour, string sourcesPath, IEasyBuilderApp app)
		{
			AssemblyPackager dialog = null;
			if (behaviour != ProcedureBehaviour.Unattended)
			{
				dialog = new AssemblyPackager();
				dialog.cloeseOnEnd = behaviour == ProcedureBehaviour.AttendedCloseDialog;
				dialog.HandleCreated += (object sender, EventArgs args) => 
				{
					dialog.token = new CancellationTokenSource();
					dialog.task = TaskProcedure(dialog.token, sourcesPath, app, dialog);
				};
				dialog.FormClosing += (object sender, FormClosingEventArgs args) => 
				{
					bool ended = dialog.task.IsCompleted || dialog.task.IsCanceled || dialog.task.IsFaulted;
					if (!ended && DialogResult.Yes != MessageBox.Show(dialog, Microarea.EasyBuilder.Properties.Resources.AreYouSureToStopTheProcedure, "EasyStudio", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						args.Cancel = true;
						return;
					}

					dialog.token.Cancel(); 
					dialog.task.Wait(); 
				};
				dialog.ShowDialog();

			}
			else
			{
				Task t = TaskProcedure(new CancellationTokenSource(), sourcesPath, app, dialog);
				t.Wait();
			}
		}

		//--------------------------------------------------------------------------------
		private static Task TaskProcedure(
			CancellationTokenSource tokenSource, 
			string sourcesPath,
			IEasyBuilderApp app, 
			AssemblyPackager dialog)
		{
			//TODO MATTEO rivedere dopo il refactoring per il code dom.
			return Task.Factory.StartNew(new Action(() => { ;}));
			//CancellationToken ct = tokenSource.Token;
			//return Task.Factory.StartNew((Action)delegate
			//        {
			//            Message(dialog, Microarea.EasyBuilder.Properties.Resources.ProcedureStarted);
			//            string outputAssemblyFullPath = GetEBModuleAssemblyPath(app.ApplicationName, app.ModuleName);
			//            List<string> sourceCodes = new List<string>();
			//            List<NameSpace> modules = new List<NameSpace>();
			//            CodeDomProvider compiler = null;
			//            try
			//            {
			//                CompilerParameters parameters = new CompilerParameters();
			//                parameters.IncludeDebugInformation = false;
			//                parameters.GenerateInMemory = false;
			//                parameters.OutputAssembly = outputAssemblyFullPath;
			//                ScriptingLanguage language = ScriptingLanguage.CSharp;
			//                List<string> metadataFiles = new List<string>();
			//                metadataFiles.AddRange(Directory.GetFiles(sourcesPath, '*' + NameSolverStrings.EbsExtension));
			//                foreach (string metaDataFile in metadataFiles)
			//                {
			//                    if (ct.IsCancellationRequested)
			//                        goto cancelled;

			//                    Message(dialog, Microarea.EasyBuilder.Properties.Resources.ReadingModuleInformationFromFile0, metaDataFile);
			//                    using (Sources sources = Sources.CreateSourcesFromFiles(metaDataFile, app.ApplicationType))
			//                    {
			//                        //aggiungo solo le dll di documento, quella di modulo non deve generare link
			//                        if (sources is ControllerSources)
			//                            modules.Add(sources.Namespace);
									
			//                        sourceCodes.Add(sources.GetAllCode(false));

			//                        if (compiler == null)
			//                        {
			//                            compiler = sources.CreateCodeDomProvider();
			//                            sourceCodes.Add(sources.GetAssemblyInfoCode());
			//                            language = sources.ScriptingLanguage;
			//                        }
			//                        else
			//                        {
			//                            if (language != sources.ScriptingLanguage)
			//                                throw new ApplicationException(Microarea.EasyBuilder.Properties.Resources.CannotGroupEasyBuilderStandardizations);
			//                        }

			//                        foreach (ReflectionProjectContent content in sources.Projects)
			//                            if (!parameters.ReferencedAssemblies.Contains(content.AssemblyLocation))
			//                                parameters.ReferencedAssemblies.Add(content.AssemblyLocation);
			//                    }
			//                }
			//                //aggiungo tutti i .cs o .vb che trovo nella cartella sources
			//                foreach (string sourceFile in Directory.GetFiles(sourcesPath, '*' + CustomizationInfos.GetSourceExtension(language)))
			//                {
			//                    if (ct.IsCancellationRequested)
			//                        goto cancelled;
			//                    string ebsFile = Path.ChangeExtension(sourceFile, NameSolverStrings.EbsExtension);
			//                    //se il cs e' stato prodotto da un ebs, lo salto, altrimenti mi trovo un duplicato
			//                    if (metadataFiles.ContainsNoCase(ebsFile))
			//                        continue;
			//                    Message(dialog, Microarea.EasyBuilder.Properties.Resources.AddingSourceFile0, sourceFile);
			//                    sourceCodes.Add(File.ReadAllText(sourceFile));

			//                }
			//                if (ct.IsCancellationRequested)
			//                    goto cancelled;

			//                List<string> tempFiles = new List<string>();
			//                foreach (string resxFile in Directory.GetFiles(sourcesPath, "*.resx"))
			//                {
			//                    if (ct.IsCancellationRequested)
			//                        goto cancelled;

			//                    Message(dialog, Microarea.EasyBuilder.Properties.Resources.ReadingResourceInformationFromFile0, resxFile);
			//                    string resourceFile = Path.Combine
			//                        (
			//                        Path.GetTempPath(),
			//                        Path.GetFileNameWithoutExtension(resxFile) + LocalizationSources.ResourceExtension
			//                        );

			//                    using (ResourceWriter writer = new ResourceWriter(resourceFile))
			//                    {
			//                        using (ResXResourceReader reader = new ResXResourceReader(resxFile))
			//                            foreach (DictionaryEntry d in reader)
			//                                writer.AddResource((string)d.Key, d.Value);
			//                    }
			//                    parameters.EmbeddedResources.Add(resourceFile);
			//                    tempFiles.Add(resourceFile);
			//                }
			//                if (ct.IsCancellationRequested)
			//                    goto cancelled;

			//                Message(dialog, Microarea.EasyBuilder.Properties.Resources.CompilingAssembly0, outputAssemblyFullPath);

			//                CompilerResults results = compiler.CompileAssemblyFromSource(parameters, sourceCodes.ToArray());//compilo in memoria perché non sono in modalità debug
			//                if (results.Errors.HasErrors)
			//                {
			//                    Message(dialog, Microarea.EasyBuilder.Properties.Resources.SomeErrorsOccurredDuringModuleCompilation);
			//                    foreach (CompilerError error in results.Errors)
			//                        Message(dialog, error.ToString());
			//                    goto end;
			//                }
			//                Message(dialog, Microarea.EasyBuilder.Properties.Resources.DeletingTemporaryFiles);

			//                foreach (NameSpace ns in modules)
			//                {
			//                    EBLink std = new EBLink();
			//                    std.Application = app.ApplicationName;
			//                    std.Module = app.ModuleName;
			//                    string dllPath = BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppAssemblyFullName(ApplicationType.Standardization, ns, "");
			//                    string ebModulePath = Path.ChangeExtension(dllPath, EBLink.ebLinkExt);
			//                    std.Save(ebModulePath);
			//                    BaseCustomizationContext.CustomizationContextInstance.AddToEasyBuilderAppCustomizationList(app, ebModulePath);
			//                    BaseCustomizationContext.CustomizationContextInstance.RemoveFromCustomListAndFromFileSystem(app, dllPath);

			//                }
			//                BaseCustomizationContext.CustomizationContextInstance.AddToEasyBuilderAppCustomizationList(app, outputAssemblyFullPath);
			//                foreach (string tmp in tempFiles)
			//                    File.Delete(tmp);
			//                Message(dialog, Microarea.EasyBuilder.Properties.Resources.ProcedureTerminated);
			//                goto end;
			//            cancelled:
			//                Message(dialog, Microarea.EasyBuilder.Properties.Resources.ProcedureCancelled);
			//            end:
			//                ;
			//            }
			//            catch (Exception ex)
			//            {
			//                Message(dialog, ex.Message);
			//            }
			//            finally
			//            {
			//                if (compiler != null)
			//                    compiler.Dispose();
			//                if (dialog != null)
			//                {
			//                    dialog.BeginInvoke((Action)delegate
			//                    {
			//                        if (dialog.cloeseOnEnd)
			//                        {
			//                            dialog.Close();
			//                        }
			//                        else
			//                        {
			//                            dialog.buttonCancel.Text = Microarea.EasyBuilder.Properties.Resources.Close1;
			//                            dialog.buttonCancel.DialogResult = DialogResult.OK;
			//                        }
			//                    });
			//                }
			//            }
						
			//        }, ct);
		}

		//--------------------------------------------------------------------------------
		internal static string GetEBModuleAssemblyPath(string appName, string modName)
		{
			return Path.Combine(
				BasePathFinder.BasePathFinderInstance.GetApplicationModuleObjectsPath(appName, modName),
				string.Concat(appName, '.', modName, ".dll")
				);
		}
		
	}
}
