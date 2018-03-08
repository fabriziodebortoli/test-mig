using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using TaskBuilderNetCore.EasyStudio.Serializers;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;

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
