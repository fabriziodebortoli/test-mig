using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.ApplicationsWinUI.EnumsViewer
{
    /// <summary>
    /// Form to show information
    /// </summary>
    //============================================================================
    public partial class EnumsViewerAppInfo : Form
    {
        /// <summary>
        /// Utility Class to Manage HashTable items
        /// </summary>
        //============================================================================
        private class AppInfoItem
        {
            #region Data Members

            internal string Application     = string.Empty;
            internal int    FirstTagDefined = 0;
            internal int    LastTagDefined  = 0;
            internal  bool  Activated       = false;

            #endregion

            #region Properties

			internal string ActivatedMessage { get { return Activated ? ApplicationsWinUIStrings.Activated : ApplicationsWinUIStrings.NotActivated; } }
            
            #endregion
        }

        #region Data Members

        private IEnumsViewerAppInfo info = null;
        private Hashtable applicationsInfo = new Hashtable();

        #endregion

        #region Constructors, Destructors and Initializations

        /// <summary>
        /// Construction
        /// </summary>
        //-----------------------------------------------------------------------------
       public EnumsViewerAppInfo(IEnumsViewerAppInfo info)
       {
            this.info = info;
            if (this.info == null)
                return;

            InitializeComponent();
            
            LoadAppInfo();
            LoadAppInfoListBox();
        }
  
        /// <summary>
        /// Calculates Application Info
        /// </summary>
        //-----------------------------------------------------------------------------
        public void LoadAppInfo ()
        {
            if (info == null || info.EnumsDefined == null)
            {
                Debug.Assert(false);
                return;
            }

            Cursor = Cursors.WaitCursor;

            foreach (EnumTag tag in info.EnumsDefined.Tags)
            {
                foreach (EnumItem item in tag.EnumItems)
                {
                    string appName = item.OwnerModule.ParentApplicationInfo.Name;

                    // application name and title are calculated once
                    AppInfoItem appInfoItem = null;
                    if (applicationsInfo.ContainsKey(appName))
                        appInfoItem = (AppInfoItem) applicationsInfo[appName];
                    else
                    {
                        appInfoItem = new AppInfoItem();

                        string brandMenuTitle = info.BrandLoader.GetApplicationBrandMenuTitle(appName);
                        appInfoItem.Application = brandMenuTitle.IsNullOrEmpty() ? appName : brandMenuTitle;
                    }

                    // first tag value
                    if (appInfoItem.FirstTagDefined == 0)
                        appInfoItem.FirstTagDefined = tag.Value;

                    // last tag value
                    if (appInfoItem.LastTagDefined < tag.Value)
                        appInfoItem.LastTagDefined = tag.Value;

                    // activation state is calculated only when not yet determined
                    if (!appInfoItem.Activated)
                    {
                        string moduleKey = tag.OwnerModule.ParentApplicationInfo.Name + NameSpace.TokenSeparator + tag.OwnerModule.Name;
                        moduleKey = moduleKey.ToLower();

                        if (info.ActivationCache.Contains(moduleKey))
                            appInfoItem.Activated = true;
                    }

                    applicationsInfo[appName] = appInfoItem;
                }
            }
            
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Loads Application Info into List Box
        /// </summary>
        //-----------------------------------------------------------------------------
        public void LoadAppInfoListBox()
        {
            LbxAppInfo.Items.Clear();

            foreach (AppInfoItem item in applicationsInfo.Values)
            {
                string enumsDefined = item.FirstTagDefined.ToString() + "-" + item.LastTagDefined.ToString();
                ListViewItem lbxItem = new ListViewItem(new string[] {  item.Application, 
                                                           enumsDefined, 
                                                           item.ActivatedMessage 
                                                         },
                                                  -1);
                LbxAppInfo.Items.Add(lbxItem);
            }
          
            LbxAppInfo.Update ();
        }

        #endregion

        #region Graphical Events

        private void lnkClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }

    /// <summary>
    /// Interface to communicate with the dialog
    /// </summary>
    //============================================================================
    public class IEnumsViewerAppInfo
    {
        #region Data Members

        internal Enums EnumsDefined = null;
        internal BrandLoader BrandLoader = null;
        internal ArrayList ActivationCache = null;

        #endregion
    };
}