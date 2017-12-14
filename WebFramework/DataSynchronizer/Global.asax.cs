using Microarea.TaskBuilderNet.Core.NameSolver;
using System;
using System.IO;
using System.Reflection;

namespace DataSynchronizer
{
    public class Global : System.Web.HttpApplication
    {
        public static Assembly iMagoStudioDll = null;
    
        void Application_Start(object sender, EventArgs e)
        {
            //module path ImagoSTudio
            string path = Path.Combine(Path.GetDirectoryName(BasePathFinder.BasePathFinderInstance.GetModuleConfigFullName("ERP","IMagoStudio")), "Files", "iMagoStudioRuntimeProxy.dll");

            if(File.Exists(path))
            {
                bool bFound = false;
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < loadedAssemblies.Length; i++)
                {
                    if (loadedAssemblies[i].Location == path)
                    {
                        bFound = true;
                        break;
                    }
                }
                if(!bFound)
                    iMagoStudioDll = Assembly.LoadFrom(path);
            }
                
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown
            Microarea.WebServices.DataSynchronizer.DataSynchronizerApplication.ReleaseAllResources();
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            try
            {
                Exception excp = sender as Exception;
                if (excp != null)
                    Microarea.WebServices.DataSynchronizer.DataSynchronizerApplication.DSEngine.WriteErrorLog
                        ($"{Environment.NewLine}Message: {excp.Message}{Environment.NewLine}InnerMessage: {excp.InnerException?.Message}{Environment.NewLine}StakTrace: {excp.StackTrace}");
            }
            catch{ }
        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
