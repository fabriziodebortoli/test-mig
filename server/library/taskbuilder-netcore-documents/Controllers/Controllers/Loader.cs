using System;
using System.Runtime;
using TaskBuilderNetCore.Documents.Interfaces;
using TaskBuilderNetCore.Documents.Model;
using System.Runtime.Loader;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Collections.Generic;
using TaskBuilderNetCore.Interfaces;
using System.Linq;

namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    [Name("Loader"), Description("It manages document loading process.")]
    public class Loader : Controller, ILoader
    {
        IBasePathFinder pathFinder;
        List<Assembly> loadedAssemblies;
        AssemblyLoader assemblyLoader;

        //-----------------------------------------------------------------------------------------------------
        public Loader(IBasePathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
            loadedAssemblies = new List<Assembly>();
            assemblyLoader = new AssemblyLoader();
        }


        // si limita ad identificare l'assembly e a caricarlo
        // se cambiamo strategia di caricamento (vd. DocumentObjects.xml, il codice va qui)
        //-----------------------------------------------------------------------------------------------------
        private Assembly LoadAssemblyFromNamespace(INameSpace nameSpace)
        {
            if (nameSpace.NameSpaceType.Type != NameSpaceObjectType.Document)
                return null;
            string tbApplicationPath = AppContext.BaseDirectory;
            string assemblyName = System.IO.Path.Combine(tbApplicationPath, nameSpace.Application + "." + nameSpace.Module + "." + nameSpace.Library + NameSolverStrings.DllExtension);
            // controllo sono già caricato in memoria
            Assembly assembly = GetLoadedAssembly(assemblyName);
            if (assembly != null)
                return assembly;

            try
            {
                assembly = assemblyLoader.LoadFromAssemblyPath(assemblyName);
                if (assembly != null)
                    loadedAssemblies.Add(assembly);
            }
            catch (Exception e)
            {
                return null;
            }
            return assembly;
        }

        //-----------------------------------------------------------------------------------------------------
        private Assembly GetLoadedAssembly(string assemblyName)
        {
            foreach (Assembly assembly in loadedAssemblies)
            {
                if (assembly.FullName == assemblyName)
                    return assembly;
            }
            return null;
        }


        //-----------------------------------------------------------------------------------------------------
        public Type GetDocument(INameSpace nameSpace)
        {
            Assembly loadedAssembly = LoadAssemblyFromNamespace(nameSpace);

            if (loadedAssembly == null)
                return null;

            TypeInfo info = typeof(Document).GetTypeInfo();
            foreach (Type t in loadedAssembly.GetTypes())
            {
                if (info.IsAssignableFrom(t))
                {
                    NameSpaceAttribute nameAttr = t.GetTypeInfo().GetCustomAttribute<NameSpaceAttribute>() as NameSpaceAttribute;
                    if (nameAttr == null)
                        continue;

                    if (nameAttr.NameSpace.FullNameSpace == nameSpace.FullNameSpace)
                        return t;
                }
            }
            return null;
        }
    }

    //====================================================================================    
    internal class AssemblyLoader : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            /* var deps = DependencyContext.Default;
             var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
             var assembly = Assembly.Load(new AssemblyName(res.First().Name));*/
            string fullName = System.IO.Path.Combine(AppContext.BaseDirectory, String.Concat(assemblyName.Name, NameSolverStrings.DllExtension));
            Assembly assembly = Assembly.Load(new AssemblyName(fullName));
            
            return assembly;
        }
    }
}
