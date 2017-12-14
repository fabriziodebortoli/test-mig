using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microarea.Snap.Installer.UI
{
    public partial class MainForm : Form
    {
        ChromiumWebBrowser chromeBrowser;
        IResourcesService resourcesService;

        public MainForm(IResourcesService resourcesService)
        {
            this.resourcesService = resourcesService;

            InitializeComponent();

            InitializeChromium();
        }

        private void InitializeChromium()
        {
            var settings = new CefSettings
            {
                BrowserSubprocessPath = "CefSharp.BrowserSubprocess.exe",
                RemoteDebuggingPort = 8088,
#if DEBUG
                LogSeverity = LogSeverity.Verbose
#else
                LogSeverity = LogSeverity.Disable
#endif
            };

            if (!Cef.Initialize(settings))
            {
                throw new Exception("Failed to init view");
            }

            var startPage = "http://local/index.html";

            this.chromeBrowser = new ChromiumWebBrowser(startPage);

            resourcesService.InitResourceHandlers(chromeBrowser);
            //this.chromeBrowser.RegisterJsObject("model", this.model);
            //this.chromeBrowser.RegisterJsObject("mediator", this.webMediator);
            //this.chromeBrowser.MenuHandler = this.cefFactory.CreateContextMenuHandler();
            this.Controls.Add(chromeBrowser);
            this.chromeBrowser.Dock = DockStyle.Fill;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }
    }
}
