using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using TaskBuilderNetCore.EasyStudio.Serializers;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace TaskBuilderNetCore.EasyStudio
{
    //====================================================================
    public class ServicesManager : List<IService>, IServiceManager
    {
		PathFinder pathFinder;

		//---------------------------------------------------------------
		public ServicesManager()
        {
			pathFinder = new PathFinder("", "");

		} 

		public PathFinder PathFinder
		{
			get
			{
				return pathFinder;
			}

		}


		//---------------------------------------------------------------
		public IService GetService(Type serviceType)
        {
            foreach (IService service in this)
            {
                if (service.GetType() == serviceType)
                    return service;
            }

            IService instance = Activator.CreateInstance(serviceType) as IService;
            if (instance != null)
            {
                Serializer serializer = instance.Serializer as Serializer;
                if (serializer != null)
                    serializer.PathFinder = pathFinder;
                instance.Services = this;
                Add(instance);
                return instance;
            }

            return null;
        }
    }
 }
