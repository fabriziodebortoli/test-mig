using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using TaskBuilderNetCore.EasyStudio.Serializers;
using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace TaskBuilderNetCore.EasyStudio
{
    //====================================================================
    public class ServicesManager : List<IService>
    {
		PathFinder pathFinder;

		//---------------------------------------------------------------
		public ServicesManager()
        {
			pathFinder = new PathFinder("", "");

		}

        private static ServicesManager servicesManagerInstance;
        private static readonly object staticLock = new object();

        public static ServicesManager ServicesManagerInstance
        {
            get
            {
                lock (staticLock)
                {
                    if (servicesManagerInstance == null)
                        servicesManagerInstance = new ServicesManager();

                    return servicesManagerInstance;
                }
            }
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

                Add(instance);
                return instance;
            }

            return null;
        }
    }
 }
