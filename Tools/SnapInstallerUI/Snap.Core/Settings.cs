using Microarea.Snap.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Microarea.Snap.IO;

namespace Microarea.Snap.Core
{
    public class Settings : ISettings
    {
        //Inserisco TaskBuilderFramework (e TaskBuilder.Net per la parte Mago.net) anziche` i nomi delle applicazioni Mago4 e Mago.net per evitare che,
        //in un futuro dove questo strumento potrebbe essere usato anche da ValoreStudio, Abitat o altri, appaiano i nomi Microarea anche per essi.
        //Effetto collaterale di questa scelta: su una macchina dove sia installato sia Mago4 che ValoreStudio allora SnapInstaller lavora per entrambi
        //i prodotti nella cartella TaskBuilderFramework.
        //L'effetto collaterale e` ancora piu` limitato se pensiamo che, oltre al nome TaskBuilderFramework, la cartella verra` creata anche per versione
        //di SnapInstaller. Quindi il problema si presenterebbe per Mago4 e ValoreStudio installati su una stessa macchina e con la stessa identica versione.
        readonly static string workingFolderPath = Path.Combine(Path.GetTempPath(), "SnapInstaller", "TaskBuilderFramework", "working");
        readonly static string logsFolderPath = Path.Combine(Path.GetTempPath(), "SnapInstaller", "TaskBuilderFramework", "logs");
        internal readonly static string ConfigRootFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SnapInstaller", "TaskBuilderFramework");

        string productInstanceFolder;

        public event EventHandler<EventArgs> ProductInstanceFolderChanged;
        protected virtual void OnProductInstanceFolderChanged()
        {
            ProductInstanceFolderChanged?.Invoke(this, EventArgs.Empty);
        }

        public string SnapPackagesRegistryFolder
        {
            get; set;
        }

        public string WorkingFolder
        {
            get; set;
        } = workingFolderPath;

        public string LogsFolder
        {
            get; set;
        } = logsFolderPath;

        public string ProductInstanceFolder
        {
            get { return this.productInstanceFolder; }
            set
            {
                this.productInstanceFolder = value;
                OnProductInstanceFolderChanged();
            }
        }

        public bool UseTransactions
        {
            get;
            set;
        } = true;
    }
}
