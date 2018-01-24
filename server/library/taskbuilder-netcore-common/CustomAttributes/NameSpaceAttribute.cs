using Microarea.Common.Generic;
using System;
using System.Collections.Generic;
using System.Text;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Common.CustomAttributes
{
    //====================================================================================    
    public class NameSpaceAttribute : Attribute
    {
        INameSpace nameSpace;
        //-----------------------------------------------------------------------------------------------------
        public INameSpace Namespace { get => nameSpace; set => nameSpace = value; }
        //-----------------------------------------------------------------------------------------------------
        public NameSpaceAttribute(string nameSpace)
        {
            this.Namespace = new NameSpace(nameSpace);
        }
        //-----------------------------------------------------------------------------------------------------
        public NameSpaceAttribute(INameSpace nameSpace)
        {
            this.Namespace = nameSpace;
        }
    }
}
