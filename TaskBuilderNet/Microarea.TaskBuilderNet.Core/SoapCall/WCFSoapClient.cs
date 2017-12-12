using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Core.SoapCall
{
	
	//================================================================================
	public class ServiceClientCache
	{
		private static Dictionary<string, ProxyFactoryCache> factories = new Dictionary<string, ProxyFactoryCache>();
		private static ReaderWriterLock lockObject = new ReaderWriterLock();
		private static Assembly ServiceAssembly = null;
		public const string AssemblyNamespace = "Microarea.Library.WCFGeneratedFunctions";
		public static string ServiceAssemblyFolder = Functions.GetExecutingAssemblyFolderPath();
		//------------------------------------------------------------------
		public static Assembly GetServicesAssembly()
		{
			if (ServiceAssembly != null)
				return ServiceAssembly;
			
			lock (typeof(ServiceClientCache))
			{
				if (ServiceAssembly != null)
					return ServiceAssembly;
				
				string path = GetServicesAssemblyPath();
				if (File.Exists(path))
				{
					ServiceAssembly = Assembly.LoadFrom(path);
					return ServiceAssembly;
				}
			}
			return null;
		}


		//------------------------------------------------------------------
		public static string GetServicesAssemblyPath()
		{
			return Path.Combine(ServiceAssemblyFolder, "TbDynamicWCF.dll");

		}
		//--------------------------------------------------------------------------------
		public static DynamicProxy GetClientProxy(string url, TimeSpan timeout, bool forceAssemblyCreation)
		{
			return GetClientProxy(null, null, url, timeout, forceAssemblyCreation);
		}
		//--------------------------------------------------------------------------------
		public static DynamicProxy GetClientProxy(Binding mexBinding, Binding communicationBinding, string url, TimeSpan timeout, bool forceAssemblyCreation)
		{
			ProxyFactoryCache cache = GetFactory(mexBinding, url, forceAssemblyCreation);
			DynamicProxy proxy = cache.GetProxy(communicationBinding == null ? typeof(BasicHttpBinding) : communicationBinding.GetType(), timeout);
			if (proxy != null)
				return proxy;

			DynamicProxyException exception = new DynamicProxyException(
					SoapCallStrings.GenerationError);
			exception.MetadataImportErrors = cache.ImportErrors;
			exception.CodeGenerationErrors = cache.CodeGenerationErrors;
			exception.CompilationErrors = cache.CompilerErrors;
			throw exception;
		}

		//--------------------------------------------------------------------------------
		private static ProxyFactoryCache GetFactory(Binding mexBinding, string url, bool forceAssemblyGeneration)
		{
			ProxyFactoryCache factory;
			if (!forceAssemblyGeneration)
			{
				try
				{
					lockObject.AcquireReaderLock(Timeout.Infinite);
					if (factories.TryGetValue(url, out factory))
						return factory;
				}
				finally
				{
					lockObject.ReleaseReaderLock();
				}
			}
			try
			{
				lockObject.AcquireWriterLock(Timeout.Infinite);
				if (!forceAssemblyGeneration && factories.TryGetValue(url, out factory))
					return factory;

				factory = new ProxyFactoryCache(mexBinding, url, forceAssemblyGeneration);
				factories[url] = factory;
			}
			finally
			{
				lockObject.ReleaseWriterLock();
			}
			return factory;
		}
	}

	//================================================================================
	internal class ProxyFactoryCache : DynamicProxyFactory
	{
		private Dictionary<Type, DynamicProxy> services = new Dictionary<Type, DynamicProxy>();
		private ReaderWriterLock lockObject = new ReaderWriterLock();

		//--------------------------------------------------------------------------------
		public ProxyFactoryCache(Binding mexBinding, string url, bool forceAssemblyGeneration)
			: base(mexBinding, url, forceAssemblyGeneration)
		{

		}
		//--------------------------------------------------------------------------------
		internal DynamicProxy GetProxy(Type bindingType, TimeSpan timeout)
		{
			return GetCachedProxy(bindingType, timeout);
		}
		//--------------------------------------------------------------------------------
		internal DynamicProxy GetCachedProxy(Type bindingType, TimeSpan timeout)
		{
			DynamicProxy proxy;

			try
			{
				lockObject.AcquireReaderLock(Timeout.Infinite);
				if (services.TryGetValue(bindingType, out proxy))
					goto verifyProxy;
			}
			finally
			{
				lockObject.ReleaseReaderLock();
			}
			try
			{
				lockObject.AcquireWriterLock(Timeout.Infinite);

				if (services.TryGetValue(bindingType, out proxy))
					goto verifyProxy;

				foreach (ServiceEndpoint ep in Endpoints)
					if (ep.Binding.GetType() == bindingType)
					{
						proxy = CreateProxy(ep, timeout);
						break;
					}

				services[bindingType] = proxy;
			}
			finally
			{
				lockObject.ReleaseWriterLock();
			}
verifyProxy:
			return VerifiedProxy(proxy, bindingType, timeout);
		}

		//--------------------------------------------------------------------------------
		private DynamicProxy VerifiedProxy(DynamicProxy proxy, Type bindingType, TimeSpan timeout)
		{
			if (proxy == null || proxy.Valid)
				return proxy;

			try { proxy.Close(); } catch {}
			
			try
			{
				lockObject.AcquireWriterLock(Timeout.Infinite);
				if (services.ContainsKey(bindingType))
					services.Remove(bindingType);
			}
			finally
			{
				lockObject.ReleaseWriterLock();
			}
			return GetCachedProxy(bindingType, timeout);
		}
	}
}
