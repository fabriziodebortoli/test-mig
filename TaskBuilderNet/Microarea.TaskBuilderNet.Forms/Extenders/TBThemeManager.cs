using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.Themes
{
    //=============================================================================================
    internal static class DesignDependencyLoader
    {
        //-------------------------------------------------------------------------
        internal static string DesignAssembliesPath { get { return Path.Combine(System.Environment.GetEnvironmentVariable("TBAppsBin"), "Debug"); } }
        internal static string AssembliesPath
        {
            get
            {
                if (IsInDesigner)
                    return DesignAssembliesPath;

                 return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        //-------------------------------------------------------------------------
        internal static bool IsInDesigner { get { return !BasePathFinder.IsInitialized; } }
        internal static bool EnableResolving
        {
            set
            {
                if (!IsInDesigner)
                    return;

                if (value == true)
                    AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
                else
                    AssembliesLoader.Load(Path.Combine(DesignAssembliesPath, "Telerik.WinControls.UI.Design.dll"));
            }
        }

        //-------------------------------------------------------------------------
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!args.Name.StartsWith("Telerik", StringComparison.OrdinalIgnoreCase))
                return AssembliesLoader.Load(args.Name);

            string fileName = args.Name.Split(',')[0]; ;
            return AssembliesLoader.Load(Path.Combine(DesignAssembliesPath, fileName + ".dll"));
        }
    }

    //=============================================================================================
    public static class TBThemeManager
    {
        private static ITBThemeProvider theme;
        //-------------------------------------------------------------------------
        public static ITBThemeProvider Theme 
        {
            get
            {
                if (theme == null)
                    Theme = Load("Microarea.TaskBuilderNet.Themes.DefaultTheme");

                return theme;
            }

            set 
            {
                theme = value;
            } 
       }

 
        //-------------------------------------------------------------------------
        public static ITBThemeProvider Load(string themeName)
        {
            string fileName = Path.Combine(DesignDependencyLoader.AssembliesPath, themeName + ".dll");          

            if (!File.Exists(fileName))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot load theme {0}, {1} file not found!", themeName, fileName));

            DesignDependencyLoader.EnableResolving = true;

            try
            {
                Assembly asm = AssembliesLoader.Load(fileName);

                foreach (Type t in asm.GetTypes())
			    {
                    if (typeof(ITBThemeProvider).IsAssignableFrom(t) && t.FullName == themeName)
                    {
                        theme = Activator.CreateInstance(t) as ITBThemeProvider;
                        break;
                    }      
                }
            }
            catch (Exception e)
            {
                 throw new ApplicationException (string.Format(CultureInfo.InvariantCulture, "Cannot load theme {0}, due to the following error {1}", themeName, e.ToString()));
            }

            DesignDependencyLoader.EnableResolving = false;
            return theme;
        }
    }
}
