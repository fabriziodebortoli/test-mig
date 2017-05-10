using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TaskBuilderNetCore.Documents.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class Controller : IController
    {
            //---------------------------------------------------------------
        virtual public string Name
        {
            get
            {
                var nameAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault() as NameAttribute;
                return nameAttribute == null ? null : nameAttribute.Name;
            }
        }

        //---------------------------------------------------------------
        virtual public string Description
        {
            get
            {
                var desAttribute = GetType().GetTypeInfo().GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as DescriptionAttribute;
                return desAttribute == null ? string.Empty : desAttribute.Description;
            }
        }
    }
}
