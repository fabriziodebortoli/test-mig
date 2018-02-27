using System;

namespace TaskBuilderNetCore.EasyStudio.Serializers
{
    //====================================================================
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultSerializerAttribute : Attribute
    {
        Type serializerType;

        //---------------------------------------------------------------
        public Type SerializerType
        {
            get { return serializerType; }
        }

        //---------------------------------------------------------------
        public DefaultSerializerAttribute(Type serializerType)
        {
            this.serializerType = serializerType;
        }
    }
}
