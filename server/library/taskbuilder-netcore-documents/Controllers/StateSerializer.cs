using TaskBuilderNetCore.Documents.Model;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;
using TaskBuilderNetCore.Documents.Model.Interfaces;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Controllers
{
    [Name("StateSerializer"), Description("It serialize/unserialize document when needed")]
     //====================================================================================    
    public class StateSerializer : Controller, IStateSerializer
    {
        static readonly string stateDirectory = "SerializedStates";
        //-----------------------------------------------------------------------------------------------------
        public void Add(IDocument document)
        {
            DocumentStateSerializer documentStateSerializer = new DocumentStateSerializer();
            documentStateSerializer.Document = document;
            documentStateSerializer.StateSerializer = this;
            documentStateSerializer.Initialize(document.CallerContext, document.DocumentServices);
            document.Components.Add(documentStateSerializer);

            LoadState(document as IComponent);
        }

        //-----------------------------------------------------------------------------------------------------
        public void Remove(IDocument document)
        {
            for (int c = document.Components.Count - 1; c >= 0; c--)
            {
                var component = document.Components[c];
                if (component is IStateSerializer stateSerializer)
                {
                    document.Components.Remove(component);
                    break;
                }
            }
       }

        //-----------------------------------------------------------------------------------------------------
        private string GetFileName(ICallerContext callerContext, bool createPath = false)
        {
            string fileName = string.Concat(callerContext.Identity, DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss-fff"), ".json");
            string basePath = this.PathFinder.GetCustomAppContainerPath(callerContext.Company, callerContext.NameSpace);

            basePath = Path.Combine(basePath, stateDirectory);

            if (!this.PathFinder.FileSystemManager.ExistPath(basePath) && createPath)
                this.PathFinder.FileSystemManager.CreateFolder(basePath, createPath);

            return Path.Combine(basePath, fileName);
        }

        //-----------------------------------------------------------------------------------------------------
        public void LoadJson(IComponent component, string json)
        {
            // devo popolare l'oggetto che mi arriva e non 
            // deserializzare creando un nuovo oggetto. 
            // Potrebbero essere cambiati i componenti
            // (attivazione) e il documento assegnato
            // deve essere quello istanziato completo
            JsonConvert.PopulateObject(json, component);
        }

        // questo ad esempio carica lo stato da file system
        //-----------------------------------------------------------------------------------------------------
        public void LoadState(IComponent component)
        {
            try
            {
                string fileName = GetFileName(component.CallerContext);
                
                if (this.PathFinder.FileSystemManager.ExistFile(fileName))
                {
                    using (StreamReader r = new StreamReader(fileName))
                    {
                        LoadJson(component, r.ReadToEnd());
                        r.Close();
                    }
                }
            }
            catch (Exception e)
            {
                component.CallerContext.Diagnostic.SetError(e.Message);
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public string GetJson(IComponent component)
        {
            return JsonConvert.SerializeObject(component);
        }
        
        //-----------------------------------------------------------------------------------------------------
        public async Task<bool> SaveState(IComponent component)
        {
            await Task.Run(() => Save(component));
            return true;
        }
        
        //-----------------------------------------------------------------------------------------------------
        private void Save(IComponent component)
        {
            try
            {
                string fileName = GetFileName(component.CallerContext, true);
                if (!File.Exists(fileName))
                {
                    string json = GetJson(component);
                    using (FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        StreamWriter sw = new StreamWriter(file);
                        sw.WriteLine(json);
                        sw.Close();
                        file.Close();
                    }
                }
            }
            catch (Exception e)
            {
                // TODOBRUNA okkio a scrivere l'oggetto in async
                component.CallerContext.Diagnostic.SetError(e.Message);
             }
         }

        //-----------------------------------------------------------------------------------------------------
        internal void RemoveState(IComponent component)
        {
            string fileName = GetFileName(component.CallerContext);

            if (PathFinder.FileSystemManager.ExistFile(fileName))
                PathFinder.FileSystemManager.RemoveFile(fileName);
        }
    }

    // Poichè gli evet handler devono lavorare sullo stesso thread aggiungo l'oggetto
    // al documento come componente aggiuntivo
    //====================================================================================    
    public class DocumentStateSerializer : DocumentComponent, IController
    {
        IStateSerializer stateSerializer;

        //-----------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public string Name => StateSerializer.GetType().Name;

        //-----------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public IStateSerializer StateSerializer { get => stateSerializer; set => stateSerializer = value; }

        //-----------------------------------------------------------------------------------------------------
        protected override bool OnInitialize()
        {
            Document doc = Document as Document;

            doc.DocumentState.StateChanging += DocumentState_StateChanging;
            doc.DocumentState.DirtyChanged += DocumentState_DirtyChanged;

            stateSerializer.LoadState(this);
            return base.OnInitialize();
        }

        // ad esempio salvo prima del cambio di stato
        //-----------------------------------------------------------------------------------------------------
        private void DocumentState_StateChanging(object sender, System.EventArgs e)
        {
            stateSerializer.SaveState(sender as IComponent);
        }

        // ad esempio salvo ogni tot modifiche
        //-----------------------------------------------------------------------------------------------------
        private void DocumentState_DirtyChanged(object sender, StateChangedEventArgs e)
        {
            Document doc = sender as Document;
            if (doc != null && doc.DocumentState.Dirty > 10)
                stateSerializer.SaveState(doc);
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed && disposing)
            {
                var doc = Document as Document;

                doc.DocumentState.StateChanging -= DocumentState_StateChanging;
                doc.DocumentState.DirtyChanged -= DocumentState_DirtyChanged;
            }

            base.Dispose(disposing);
        }
    }
}
