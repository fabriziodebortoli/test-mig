using System;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System.IO;
using System.Diagnostics;

namespace Microarea.MenuManager
{
    /// <summary>
    /// Class that hosts a CEF browser and is dockable
    /// </summary>
    //============================================================================
    class BrowserDockableForm : GenericDockableForm
    {
        protected CCEFBrowserWrapper wrapper;

        //--------------------------------------------------------------------------------
        public BrowserDockableForm()
        {
            ITheme theme = DefaultTheme.GetTheme();
            BackColor = theme.GetThemeElementColor("LoginBackGroundColor");  //TODOLUCA
                                                                             //BackColor = System.Drawing.Color.Lavender;
                                                                             //ForeColor = System.Drawing.Color.Navy;

            wrapper = new CCEFBrowserWrapper();

            wrapper.BrowserReady += OnBrowserReady;
            wrapper.BrowserClosing += BrowserClosing;
        }

        //--------------------------------------------------------------------------------
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            wrapper.Create(Handle, "");
        }

        //--------------------------------------------------------------------------------
        void BrowserClosing(object sender, EventArgs e)
        {
            if (InvokeRequired && IsHandleCreated)
            {
                BeginInvoke((Action)delegate ()
                {
                    Close();
                });
            }
        }

        //--------------------------------------------------------------------------------
        public virtual void OnBrowserReady(object sender, EventArgs e)
        {
            wrapper.AdjustPosition(Width, Height);
        }

        //--------------------------------------------------------------------------------
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (wrapper != null)
            {
                wrapper.AdjustPosition(Width, Height);
            }

        }

        //--------------------------------------------------------------------------------
        protected override void OnFormClosed(System.Windows.Forms.FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (wrapper != null)
                wrapper.Dispose();
        }

        //--------------------------------------------------------------------------------
        public void SetCookie(string url, string name, string value)
        {
            if (wrapper != null)
                wrapper.SetCookie(url, name, value);
        }

        //--------------------------------------------------------------------------------
        public void Navigate(string url)
        {
            if (wrapper != null)
                wrapper.Navigate(url);
        }

        //--------------------------------------------------------------------------------
        public void Reload()
        {
            if (wrapper != null)
                wrapper.Reload();
        }
    }

    /// <summary>
    /// Class that hosts a CEF browser and displays the app menu
    /// </summary>
    //============================================================================
    class GenericBrowserDockableForm : BrowserDockableForm
    {
        string url;
        public GenericBrowserDockableForm(string url)
        {
            this.url = url;
        }

        public override void OnBrowserReady(object sender, EventArgs e)
        {
            base.OnBrowserReady(sender, e);
            Navigate(url);
        }
    }

    /// <summary>
    /// Class that hosts a CEF browser and displays the app menu
    /// </summary>
    //============================================================================
    class MenuBrowserDockableForm : BrowserDockableForm
    {
        bool newMenu = false;
        int newMenuLocalPort = 0;

        public string ITokenforLogin = null;
        public string TbLinkToOpen = null;

        private string authToken;
        public MenuBrowserDockableForm(string authToken, bool newMenu, int newMenuLocalPort)
        {
            this.Icon = MenuManagerStrings.MenuMngForm;
            this.newMenu = newMenu;
            this.authToken = authToken;
            this.newMenuLocalPort = newMenuLocalPort;
        }

        //--------------------------------------------------------------------------------
        protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            e.Cancel = true;
            DockPanel.FindForm().Close();
        }

        //--------------------------------------------------------------------------------
        public override void OnBrowserReady(object sender, EventArgs e)
        {
            base.OnBrowserReady(sender, e);
            string sUrl = "";

            sUrl = "http://localhost/tb/menu/";
            if (ITokenforLogin != null)
            {
                sUrl = sUrl + "loginhost.html?tk=" + ITokenforLogin + "&tblink=" + TbLinkToOpen;
            }
            else
            {

                //TODOLUCA apogeo
                if (!newMenu)
                {
                    if (!authToken.IsNullOrEmpty())
                    {
                        sUrl = sUrl + InstallationData.BrandLoader.GetBrandedStringBySourceString("MenuPage");
                    }
                    else
                        sUrl = sUrl + "loginhost.html";
                }
                else
                {
                    if (newMenuLocalPort > 0)
                        sUrl = "http://localhost:" + newMenuLocalPort.ToString();
                    else
                        sUrl = Path.Combine(PathFinder.BasePathFinderInstance.WebFrameworkRootUrl, NameSolverStrings.M4Client, "index.html");
                }
            }

            SetCookie("http://localhost", "authtoken", authToken);
            Navigate(sUrl);
        }
    }

    //============================================================================
    class RunDocumentEventArgs : EventArgs
    {
        public string Namespace { get; set; }
        public string Arguments { get; set; }

        public RunDocumentEventArgs(string ns, string args)
        {
            this.Namespace = ns;
            this.Arguments = args;
        }
    }
}
