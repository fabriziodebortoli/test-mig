using LightInject;
using Microarea.Snap.IO;
using System;

namespace Microarea.Snap.Core
{
    internal class IocFactory : IDisposable, IInversionOfControlFactory
    {
        readonly static object staticLockTicket = new object();
        static IocFactory instance;
        static bool disposing;
        internal static IocFactory Instance
        {
            get
            {
                lock (staticLockTicket)
                {
                    if (instance == null)
                    {
                        instance = new IocFactory();
                        instance.Init();
                    }
                    return instance;
                }
            }
        }

        PerContainerLifetime singletonSettings = new PerContainerLifetime();
        ServiceContainer container;

        protected ServiceContainer Container
        {
            get
            {
                return container;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool managed)
        {
            if (managed)
            {
                if (container != null)
                {
                    container.Dispose();
                    container = null;
                }
                if (singletonSettings != null)
                {
                    singletonSettings.Dispose();
                    singletonSettings = null;
                }
                lock (staticLockTicket)
                {
                    if (instance != null && !disposing)
                    {
                        disposing = true;
                        instance.Dispose();
                        instance = null;
                    }
                }
            }
        }

        protected virtual void Init()
        {
            container = new ServiceContainer();

            container.Register<string, IFile>((factory, arg) => new Microarea.Snap.IO.File(arg));
            container.Register<string, IFile>((factory, arg) => new Microarea.Snap.IO.TransactionalFile(arg), "transactional");
            container.Register<string, IFolder>((factory, arg) => new Microarea.Snap.IO.Folder(arg));
            container.Register<string, IFolder>((factory, arg) => new Microarea.Snap.IO.TransactionalFolder(arg), "transactional");
            container.Register<ISettingsLoader, SettingsLoader>();
            container.Register<ISettings>(
                factory
                =>
                {
                    var settingsLoader = factory.Create<SettingsLoader>();
                    return settingsLoader.Load();
                }, singletonSettings
                );
        }

        public virtual T GetInstance<T>()
        {
            return container.GetInstance<T>();
        }

        public virtual T GetInstance<T, TParam>(TParam param)
        {
            if (typeof(T) == typeof(IFile) || typeof(T) == typeof(IFolder))
            {
                var settings = this.GetInstance<ISettings>();
                if (settings.UseTransactions)
                {
                    return container.GetInstance<TParam, T>(param, "transactional");
                }
                else
                {
                    return container.GetInstance<TParam, T>(param);
                }
            }

            return container.GetInstance<T>();
        }
    }
}
