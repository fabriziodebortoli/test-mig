using System;
using TaskBuilderNetCore.Documents.Interfaces;
using TaskBuilderNetCore.Documents.Model;
using System.Runtime.Loader;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using System.Collections.Generic;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{

    [Name("Loader"), Description("It manages document loading process.")]
    public class Loader : Controller, ILoader
    {
        IBasePathFinder pathFinder;
        List<Assembly> loadedAssemblies;

        public Loader(IBasePathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
            loadedAssemblies = new List<Assembly>();
        }


        // si limita ad identificare l'assembly e a caricarlo
        // se cambiamo strategia di caricamento (vd. DocumentObjects.xml, il codice va qui)
        private Assembly LoadAssemblyFromNamespace(INameSpace nameSpace)
        {
            if (nameSpace.NameSpaceType.Type != NameSpaceObjectType.Document)
                return null;
            string tbApplicationPath = string.Empty;
            string exePath  = @"C:\Dev_Next\Standard\Applications\TbNetCoreLoader\bin\Debug\netcoreapp1.1.1\win10-x64\";
            string assemblyName = System.IO.Path.Combine(exePath, nameSpace.Application + "." + nameSpace.Module + "." + nameSpace.Library + NameSolverStrings.DllExtension);
            // controllo sono già caricato in memoria
            Assembly assembly = GetLoadedAssembly(assemblyName);
            if (assembly != null)
                return assembly;

            try
            {
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyName);
                if (assembly != null)
                    loadedAssemblies.Add(assembly);
            }
            catch (Exception e)
            {
                return null;
            }
            return assembly;
        }

        private Assembly GetLoadedAssembly(string assemblyName)
        {
            foreach (Assembly assembly in loadedAssemblies)
            {
                if (assembly.FullName == assemblyName)
                    return assembly;
            }
            return null;
        }

    
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
}
