using Microarea.Snap.Services.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    internal class RegistryService : IRegistryService, ILogger
    {
        const string guidKeyPath = @"SOFTWARE\Microarea\Mago4\E51B08A3-8D02-44BE-B3BC-85144A6C7EBA";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string RetrieveProductInstallationPath()
        {
            try
            {
                using (var localMachineKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                using (var guidKey = localMachineKey.OpenSubKey(guidKeyPath, true))
                {
                    if (guidKey == null)
                    {
                        return null;
                    }

                    var installDirObj = guidKey.GetValue("InstallDir");
                    if (installDirObj != null)
                    {
                        return installDirObj.ToString();
                    }

                    return null;
                }
            }
            catch (Exception exc)
            {
                this.LogError(Resources.ErrorReadingProductInstallationPath, exc);
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public string[] RetrieveInstalledDictionaries()
        {
            try
            {
                using (var localMachineKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                using (var guidKey = localMachineKey.OpenSubKey(guidKeyPath, true))
                {
                    if (guidKey == null)
                    {
                        return null;
                    }

                    var dictionariesObj = guidKey.GetValue("Dictionaries");
                    if (dictionariesObj == null)
                    {
                        return new string[] { };
                    }

                    var dictionaries = dictionariesObj
                        .ToString()
                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    return dictionaries;
                }
            }
            catch (Exception exc)
            {
                this.LogError(Resources.ErrorReadingInstalledDictionaries, exc);
                return null;
            }
        }
    }
}
