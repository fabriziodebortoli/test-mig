using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.UI.WinControls.Splashes
{
    public class SplashManager
    {
        //---------------------------------------------------------------------
        public static void TemporarySplashForBrandLoading()
        {
            if (!InstallationData.ServerConnectionInfo.MasterSolutionName.IsNullOrEmpty())
                return;

            WaitingWindow ww = new WaitingWindow(String.Format(WinControlsStrings.LoadingConfiguration, InstallationData.InstallationName));
            ww.Show();

            BrandLoader.PreLoadMasterSolutionName();
            ww.Close();
            ww.Dispose();
        }
    }
}
