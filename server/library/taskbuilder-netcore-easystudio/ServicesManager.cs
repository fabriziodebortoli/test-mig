using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using TaskBuilderNetCore.EasyStudio.Serializers;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;
using System.Reflection;

namespace TaskBuilderNetCore.EasyStudio
{
    //====================================================================
    public class ServicesManager : List<IService>, IServiceManager
    {
        PathFinder pathFinder;

        //---------------------------------------------------------------
        public PathFinder PathFinder
        {
            get
            {
                return pathFinder;
            }

        }
        
        //---------------------------------------------------------------
        public ServicesManager()
        {
            pathFinder = PathFinder.PathFinderInstance;

        }

        //---------------------------------------------------------------
        private IService GetService(Type serviceType)
        {
            foreach (IService service in this)
            {
                if (service.GetType() == serviceType)
                   return service;
            }

            return CreateService(serviceType);
        }

        //---------------------------------------------------------------
        public IService GetService(string name)
        {
            foreach (IService service in this)
            {
                if (string.Compare(service.Name, name) == 0)
                    return service;
            }

            Type serviceType = GetServiceType(name);
            return serviceType == null ? null : CreateService(serviceType);
        }

        //---------------------------------------------------------------
        private Type GetServiceType(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (!typeof(IService).IsAssignableFrom(type))
                    continue;

                string serviceName = Component.GetNameAttributeFrom(type);
                if (string.Compare(serviceName, name, true) == 0)
                    return type;
            }
            return null;
        }

        //---------------------------------------------------------------
        public T GetService<T>()
        {
            return (T) GetService(typeof(T));
        }

        //---------------------------------------------------------------
        private IService CreateService(Type serviceType)
        {
            IService service = Activator.CreateInstance(serviceType) as IService;
            if (service == null)
                return null;

            service.Services = this;
            service.Diagnostic = new DiagnosticProvider(string.Concat(NameSolverStrings.EasyStudio, ": ", service.Name));

            Add(service);
            return service;
        }
    }
}
