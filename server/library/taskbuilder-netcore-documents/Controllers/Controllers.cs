using System;
using System.Collections.Generic;
using System.Reflection;
using TaskBuilderNetCore.Documents.Controllers.Interfaces;

namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    public class Controllers : List<IController>
    {
        public T GetController<T>(string name = "")
        {
            var info = typeof(T).GetTypeInfo();
            foreach (IController controller in this)
            {
                if ((info.IsAssignableFrom(controller.GetType()) && (string.IsNullOrEmpty(name) || controller.Name.CompareTo(name) == 0)))
                    return (T)controller;
            }
            return default(T);
        }
    }
}
