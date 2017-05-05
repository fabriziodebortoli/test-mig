using Microarea.Common.Generic;
using System;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    public class NameSpaceAttribute : Attribute
    {
        INameSpace nameSpace;
        public NameSpaceAttribute(string nameSpace)
        {
            this.nameSpace = new NameSpace(nameSpace);
        }

        public INameSpace NameSpace
        {
            get
            {
                return nameSpace;
            }

            set
            {
                nameSpace = value;
            }
        }
    }
}