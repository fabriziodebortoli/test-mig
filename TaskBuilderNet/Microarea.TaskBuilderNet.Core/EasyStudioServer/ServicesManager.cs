using Microarea.TaskBuilderNet.Core.EasyStudioServer.Services;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer
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
                if (instance.Serializer != null)
                    instance.Serializer.PathFinder = pathFinder;

				Add(instance);
                return instance;
            }

            return null;
        }
    }
 }
