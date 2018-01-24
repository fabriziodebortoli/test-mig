using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class NameAttribute : Attribute
    {
        string name;
        //-----------------------------------------------------------------------------------------------------
        public string Name { get => name; set => name = value; }
        //-----------------------------------------------------------------------------------------------------
        public NameAttribute(string name)
        {
            this.Name = name;
        }

    }
}
