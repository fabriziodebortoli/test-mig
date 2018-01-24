using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class DescriptionAttribute : Attribute
    {
        string description;
        //-----------------------------------------------------------------------------------------------------
        public string Description { get => description; set => description = value; }
        //-----------------------------------------------------------------------------------------------------
        public DescriptionAttribute(string description)
        {
            this.Description = description;
        }

    }
}
