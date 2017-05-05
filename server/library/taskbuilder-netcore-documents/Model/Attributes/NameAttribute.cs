using System;

namespace TaskBuilderNetCore.Documents.Model
{
    public class NameAttribute : Attribute
    {
        string name;
        public NameAttribute(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
    }
}