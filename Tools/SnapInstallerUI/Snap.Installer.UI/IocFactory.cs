using LightInject;
using Microarea.Snap.Core;
using System;
using System.IO;
using System.Reflection;

namespace Microarea.Snap.Installer.UI
{
    internal class IocFactory : IDisposable
    {
        readonly static object staticLockTicket = new object();
        static IocFactory instance;
        static string asmPath;
        public static IocFactory Instance
        {
            get
            {
                lock (staticLockTicket)
                {
                    if (instance == null)
                    {
                        instance = new IocFactory();
                        instance.Init();

                        var executingAssembly = Assembly.GetExecutingAssembly();
                        asmPath = GetAssemblyPath(executingAssembly);


                    }
                    return instance;
                }
            }
        }

        private static string GetAssemblyPath(Assembly asm)
        {
            string codeBase = asm.CodeBase;
            UriBuilder uriBuilder = new UriBuilder(codeBase);
            string filePath = Uri.UnescapeDataString(uriBuilder.Path);
            return Path.GetDirectoryName(filePath); //converte da url a path di file system
        }

        PerContainerLifetime singleton = new PerContainerLifetime();
        ServiceContainer container;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool manager)
        {
            if (container != null)
            {
                container.Dispose();
                container = null;
            }
            if (singleton != null)
            {
                singleton.Dispose();
                singleton = null;
            }
        }

        protected virtual void Init()
        {
            container = new ServiceContainer();

            container.Register<MainForm>();
            container.Register<string, IResourcesService>((factory, arg) => new FileSystemResourcesService(Path.Combine(asmPath, "web")));
            //container.Register<string, IResourcesService>((factory, arg) => new ZipResourcesService(Path.Combine(asmPath, "web.zip")));
            container.RegisterFallback((type, s) => typeof(ISettings).IsAssignableFrom(type), request => Microarea.Snap.Core.IocFactory.Instance.Get<ISettings>());
        }

        public T Get<T>()
        {
            return container.GetInstance<T>();
        }
    }
}
