using System;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities
{
    public class LogCleaner
    {
        const int DAYS_TO_KEEP = 7;

        /// <summary>
        /// Cancella i log del SynchroConnector creati da più di 7 giorni
        /// </summary>
        //-------------------------------------------------------------------------
        public void PurgeSynchroConnectorLog(string companyName, out string message)
        {
            message = string.Empty;
            try
            {
                DirectoryInfo dirPath = new DirectoryInfo(Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompanyLogPath(companyName), NameSolverStrings.SynchroConnectorModule));

                foreach (FileInfo f in dirPath.GetFiles())
                {
                    if (f.CreationTime <= DateTime.Now.AddDays(-DAYS_TO_KEEP))
                        File.Delete(f.FullName);
                }
                message = $"SynchroConnectors logs created before {DateTime.Now.AddDays(-DAYS_TO_KEEP).ToString("D")} successfully deleted";
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
