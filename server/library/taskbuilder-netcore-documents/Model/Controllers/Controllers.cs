using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
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
