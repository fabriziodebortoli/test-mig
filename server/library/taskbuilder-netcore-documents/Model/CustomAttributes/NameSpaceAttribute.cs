using System;
using System.Collections.Generic;
using System.Text;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class NameSpaceAttribute : Attribute
    {
        string nameSpace;
        //-----------------------------------------------------------------------------------------------------
        public string Namespace { get => nameSpace; set => nameSpace = value; }
        //-----------------------------------------------------------------------------------------------------
        public NameSpaceAttribute(string nameSpace)
        {
            this.Namespace = nameSpace;
        }

    }
}
