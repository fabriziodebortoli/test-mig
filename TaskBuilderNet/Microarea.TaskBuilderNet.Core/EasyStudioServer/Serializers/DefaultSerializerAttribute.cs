using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers
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
