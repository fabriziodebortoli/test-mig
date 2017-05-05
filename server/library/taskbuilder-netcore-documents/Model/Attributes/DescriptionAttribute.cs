using System;

namespace TaskBuilderNetCore.Documents.Model
{
    public class DescriptionAttribute : Attribute
    {
        string description;

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
    }
}