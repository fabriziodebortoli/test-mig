using System;
using System.Reflection;
using TaskBuilderNetCore.EasyStudio;
using escli.Controllers;
using System.Linq;

namespace escli
{
    //=========================================================
    public class Engine
    {
        ServicesManager serviceManager = new ServicesManager();
        //=========================================================
        internal class Strings
        {
            internal const string EmptyRequest = "Empty request in first parameter! Please specify a service and an action!";
            internal const string MissingService = "First request parameter: Missing service name!";
            internal const string MissingAction = "First request parameter: Missing action name!";
            internal const string InvalidServiceName = "Service name does not correspond to a service!";
        }

        const string requestSeparator = "/";

        //-----------------------------------------------------
        public Engine()
        {
        }

        //-----------------------------------------------------
        private Tuple<string, string> CalculateRequest(string request)
        {
            string[] tokens = request.Split(requestSeparator);
            if (tokens.Length < 3)
                return null;
            return new Tuple<string, string>(tokens[1], tokens[2]);
        }

        //-----------------------------------------------------
        public void Execute(string[] args)
        {
            string requestParam = args[0];
            if (string.IsNullOrEmpty(requestParam))
            {
                Console.WriteLine(Strings.EmptyRequest);
                return;
            }

            string[] arParams = new string[args.Length - 1];
            for (int i = 1; i < args.Length; i++)
                arParams[i - 1] = args[i];

            Tuple<string, string> request = CalculateRequest(requestParam);
            Execute(request.Item1, request.Item2, arParams);
        }

        //-----------------------------------------------------
        private void Execute(string controllerName, string action, string[] arParams)
        {
            if (string.IsNullOrEmpty(controllerName))
            {
                Console.WriteLine(Strings.MissingService);
                return;
            }

            if (string.IsNullOrEmpty(action))
            {
                Console.WriteLine(Strings.MissingAction);
                return;
            }
            IController controller = GetController(controllerName);
            if (controller == null)
            {
                Console.WriteLine(Strings.InvalidServiceName);
                return;
            }
            if (!controller.ExecuteRequest(action, arParams))
                Console.WriteLine(controller.Diagnostic.ToString());
        }

        //-----------------------------------------------------
        private IController GetController(string name)
        {
            IController controller = null;
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly == null)
                return null;

            foreach (Type type in assembly.GetTypes())
            {
                if (!typeof(IController).IsAssignableFrom(type))
                    continue;

                var attribute = type.GetTypeInfo().GetCustomAttributes(typeof(RouteAttribute), true).FirstOrDefault() as RouteAttribute;
                string controllerName = attribute?.Name;
                if (string.Compare(controllerName, name, true) == 0)
                {
                    controller = Activator.CreateInstance(type, serviceManager) as IController;
                    break;
                }
            }
            return controller;
        }
    }
}
