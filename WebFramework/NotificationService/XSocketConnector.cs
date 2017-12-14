using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace NotificationService
{
  //  internal class XSocketConnector
  //  {
  //      //COPIATO e modificato da $/TaskBuilder/TaskBuilder/TaskBuilderNet/Microarea.TaskBuilderNet.Core/WebSocket

  //      //TENERE ALLINEATO CON QUELLO IN
  //      //\TaskBuilder\Framework\TbGenLibManaged\Main.h
        
  //      static IXSocketServerContainer s_oContainer;
  //      static int Port;
  //      static string Url;
  //      public static string ControllerUrl { get; private set; }
  //      const string ControllerPathToken = "/NotificationController";
        
  //      /// <summary>
  //      /// Starts the XSocket engine
  //      /// </summary>
  //      public static void Start()
  //      {
  //          lock (typeof(XSocketConnector))
  //          {
  //              if (s_oContainer != null)
  //              {
  //                  Debug.Fail("XSocketConnector::InitializeXSocket: IXSocketServerContainer already initilalized");
  //                  return;
  //              }

  //              //needs to obtain a free port without conflicting with other conturrent processes (i.e. terminal server)
  //              //so I use a mutex
  //              string mutexName = "Global\\XSocket" + BasePathFinder.BasePathFinderInstance.Installation;
  //              bool mutexWasCreated;
  //              Mutex menuMutex = new Mutex(true, mutexName, out mutexWasCreated);
  //              try
  //              {
  //                  // If this thread does not own the named mutex, it requests it by calling WaitOne.
  //                  if (!mutexWasCreated)
  //                      menuMutex.WaitOne(Timeout.Infinite, false);
  //                  // StartupLog
  //                  //mutex has to have no security, so it can be shared by all sessions of terminal services
  //                  Functions.SetNoSecurityOnMutex(menuMutex);

  //                  //if (!ConfigurationManager.AppSettings.AllKeys.Contains<string>("XSockets.PluginFilter"))
  //                  //{
  //                  //    string sLocation = Assembly.GetExecutingAssembly().Location;
  //                  //    string sDir = Path.GetDirectoryName(sLocation);

  //                  //    string sConfigPath = sDir + Path.DirectorySeparatorChar + "TBIISXSocket.dll.config";

  //                  //    using (AppConfig.Change(sConfigPath))
  //                  //    {
  //                  //        Composable.ClearPluginFilters();
  //                  //        Composable.AddPluginFilter("XSockets.*.dll");
  //                  //    }
  //                  //}
  //                  //Composable.AddPluginFilter("Microarea.TaskBuilderNet.Core.dll");

  //                  //List of IConfigurationSettings
  //                  var myCustomConfigs = new List<IConfigurationSetting>();
  //                  //Add one configuration
  //                  Port = TbLoaderClientInterface.GetNewTbLoaderPort(4502);
  //                  Url = string.Concat("ws://127.0.0.1:", Port);
  //                  ControllerUrl = string.Concat(Url, ControllerPathToken);
  //                  myCustomConfigs.Add(new ConfigurationSetting(Url));
                    
  //                  using (var container = XSockets.Plugin.Framework.Composable.GetExport<XSockets.Core.Common.Socket.IXSocketServerContainer>())
  //                  {
  //                      container.StartServers(true, false, myCustomConfigs);
  //                      foreach (var server in container.Servers)
  //                          Console.WriteLine(server.ConfigurationSetting.Endpoint);
  //                  }
  //              }
  //              catch (Exception)
  //              {
  //                  // se il lancio di XSocket fallisce l'exe continua a girare senza problemi, E.G. 
  //                  // lancio da ClickOnceDeployer
  //              }
  //              finally
  //              {
  //                  if (menuMutex != null)
  //                      menuMutex.ReleaseMutex();
  //              }
  //          }
  //      }

  //      /// <summary>
  //      /// Stop Xsocket engine.
  //      /// </summary>
  //      public static void Stop()
  //      {
  //          lock (typeof(XSocketConnector))
  //          {
  //              if (s_oContainer != null)
  //              {
  //                  s_oContainer.StopServers();
  //                  s_oContainer.Dispose();
  //                  s_oContainer = null;
  //              }
  //          }
  //      }

  //      public static void SendBBFormNotify(string BBUserName, int formId)
  //      {
  //          ClientPool clientPool = ClientPool.GetInstance(ControllerUrl, "*");
  //          clientPool.Send(new { BBUserName, formId }, "SendForm");
  //      }

  //      public static void SendBBMileStoneNotify(string BBUserName, string mileStoneMessage) 
  //      {
  //          ClientPool clientPool = ClientPool.GetInstance(ControllerUrl, "*");
  //          clientPool.Send(new { BBUserName, mileStoneMessage }, "SendMileStone");
  //      }
		
		//public static void SendMessage(string BBUserName, string message) 
  //      {
  //          ClientPool clientPool = ClientPool.GetInstance(ControllerUrl, "*");
		//	clientPool.Send(new { BBUserName, message }, "SendMessage");
  //      }

		////public static void SendIGenericNotify(string BBUserName, IGenericNotify notify)
		////{
		////	ClientPool clientPool = ClientPool.GetInstance(ControllerUrl, "*");
		////	clientPool.Send(new { BBUserName, notify }, "SendIGenericNotify");
		////}

		//public static void SendIGenericNotify(string BBUserName, GenericNotify notify)
		//{
		//	ClientPool clientPool = ClientPool.GetInstance(ControllerUrl, "*");
			
		//	//string jSonNotify = notify.Serialize();
			
		//	//var deserializedNotify = jSonNotify.Deserialize<GenericNotify>();

		//	//clientPool.Send(new { BBUserName, jSonNotify = deserializedNotify.Serialize() }, "SendIGenericNotify");

		//	clientPool.Send(new { BBUserName, notify}, "SendIGenericNotify");
		//}

  //  }

    public abstract class AppConfig : IDisposable
    {
        public static AppConfig Change(string path)
        {
            return new ChangeAppConfig(path);
        }

        public abstract void Dispose();

        private class ChangeAppConfig : AppConfig
        {
            private readonly string oldConfig =
                AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();

            private bool disposedValue;

            public ChangeAppConfig(string path)
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
                ResetConfigMechanism();
            }

            public override void Dispose()
            {
                if (!disposedValue)
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", oldConfig);
                    ResetConfigMechanism();


                    disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }

            private static void ResetConfigMechanism()
            {
                typeof(ConfigurationManager)
                    .GetField("s_initState", BindingFlags.NonPublic |
                                             BindingFlags.Static)
                    .SetValue(null, 0);

                typeof(System.Configuration.ConfigurationManager)
                    .GetField("s_configSystem", BindingFlags.NonPublic |
                                                BindingFlags.Static)
                    .SetValue(null, null);

                typeof(ConfigurationManager)
                    .Assembly.GetTypes()
                    .Where(x => x.FullName ==
                                "System.Configuration.ClientConfigPaths")
                    .First()
                    .GetField("s_current", BindingFlags.NonPublic |
                                           BindingFlags.Static)
                    .SetValue(null, null);
            }
        }
    }
}