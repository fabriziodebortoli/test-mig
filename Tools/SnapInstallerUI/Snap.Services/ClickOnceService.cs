using Microarea.Snap.Core;
using Microarea.Snap.Core.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    internal class ClickOnceService : IClickOnceService , ILogger
    {
        ISettings settings;

        public ClickOnceService(ISettings settings)
        {
            this.settings = settings;
        }
        public void Deploy()
        {
            string clickOnceDeployerExePath = Path.Combine(
                        this.settings.ProductInstanceFolder,
                        "Apps",
                        "ClickOnceDeployer",
                        "ClickOnceDeployer.exe"
                        );
            this.LaunchProcess(
                clickOnceDeployerExePath,
                "Deploy",
                3600000
                );
        }

        public void UpdateDeployment()
        {
            string clickOnceDeployerExePath = Path.Combine(
                        this.settings.ProductInstanceFolder,
                        "Apps",
                        "ClickOnceDeployer",
                        "ClickOnceDeployer.exe"
                        );
            this.LaunchProcess(
                clickOnceDeployerExePath,
                "UpdateDeployment",
                3600000
                );
        }
    }
}
