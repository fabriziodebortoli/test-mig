using System;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Core.ApplicationsWinUI.EnumsViewer
{
 	/// <summary>
	/// Manager for automatic management of Enums Viewer tool
	/// </summary>
	//============================================================================
    public class EnumsViewerManager
    {
        #region Data Members

            private static	EnumsDialog enumsViewer = null;
            public static event EventHandler Closing;        
        #endregion


        #region Construction Distruction And Initialization

        /// <summary>
	    /// Default Constructor
	    /// </summary>
	    //-----------------------------------------------------------------------------
        static EnumsViewerManager()
        {
            enumsViewer = null;
        }

        #endregion

        #region Management Methods

        //---------------------------------------------------------------------------------------
        static public bool IsClosed () 
        {
            lock (typeof(EnumsViewerManager))
            {
                return enumsViewer == null;
            }
        }

        //---------------------------------------------------------------------------------------
        static public IntPtr GetWindowHandle()
        {
            return enumsViewer == null ? IntPtr.Zero : enumsViewer.Handle;
        }

        //---------------------------------------------------------------------------------------
        static public bool Open(string culture, string installation, bool withUI = true)
        {
            Cursor.Current = Cursors.WaitCursor;
	        if (IsClosed())
	        {
		        Settings enumsViewerSetting = new Settings(installation);
		        enumsViewerSetting.Culture = culture;
                if (withUI)
                {
                    enumsViewerSetting.AskSaveSettings = false;
                    enumsViewerSetting.SaveSettings = true;
                    enumsViewer = new EnumsDialog(enumsViewerSetting);
                    enumsViewer.FormClosing += new FormClosingEventHandler(EnumsViewer_FormClosing);
                }
	        }

            if (withUI)
            {
                enumsViewer.Show();
                enumsViewer.TopMost = false;
            }
            Cursor.Current = Cursors.Default;
	        
            return true;
        }


       //---------------------------------------------------------------------------------------
        static public string CreateXml(string installation, string culture)
        {
            Open(culture, installation, false);
            string error;
            if (!InitCulture(culture, out error))
            {
                Close();
                return CreateErrorXml(error);
            }
            
            Enums enums = new Enums();
            string result = string.Empty;
            try
            {
                enums.LoadXml();

                using (MemoryStream stream = new MemoryStream())
			    {
                    enums.Tags.SaveXml(stream, true , true);
                    result = System.Text.UTF8Encoding.UTF8.GetString(stream.ToArray());
			    }
            }
            catch (EnumsException e)
            {
                result = CreateErrorXml(e.Message);
            }

            Close();
            return result;
        }

        //---------------------------------------------------------------------
        static private string CreateErrorXml(string error)
        {
            return string.Concat("<?xml version='1.0' standalone='true'?><", EnumsXml.Element.Enums, "><ErrorMessage>", error, "</ErrorMessage></", EnumsXml.Element.Enums, ">");
        }

        /// <summary>
        /// Culture and language initialization
        /// </summary>
        //---------------------------------------------------------------------
        static internal bool InitCulture(string culture, out string error)
        {
            error = string.Empty;
            try
            {
                if (culture != null && culture != string.Empty)
                {
                    DictionaryFunctions.SetCultureInfo(culture, culture);
                    return true;
                }
                if (InstallationData.ServerConnectionInfo != null)
                {
                    DictionaryFunctions.SetCultureInfo
                        (
                        InstallationData.ServerConnectionInfo.PreferredLanguage,
                        InstallationData.ServerConnectionInfo.ApplicationLanguage
                        );
                }
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        //---------------------------------------------------------------------------------------
        static public bool Close()
        {
			

	        if (IsClosed())
		        return true;

			try
			{
				if (enumsViewer != null)
					enumsViewer.Exit();

				if (enumsViewer != null && enumsViewer.IsDisposed)
					enumsViewer = null;

			}
			catch 
			{ 
				return false; 
			}

	        return true;
        }

        #endregion

        #region Event Handlers
        
        //---------------------------------------------------------------------------------------
        static public void EnumsViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Closing != null)
                Closing(null,null);
            enumsViewer = null;
        }
        #endregion

    };
}
