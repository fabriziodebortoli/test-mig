using TaskBuilderNetCore.EasyStudio.Interfaces;
using System;

namespace TaskBuilderNetCore.EasyStudio.Serializers
{
    //====================================================================
    public class SerializerException : Exception
    {
        ISerializer serializer;

        //---------------------------------------------------------------
        public ISerializer Serializer
        {
            get
            {
                return serializer;
            }
        }

        //---------------------------------------------------------------
        public SerializerException(ISerializer ser, string message)
            :
            base(message)
        {
            this.serializer = ser;
        }

        //---------------------------------------------------------------
        public SerializerException(ISerializer ser, Exception inner)
            :
            base(inner.Message, inner)
        {
            this.serializer = ser;
        }
    }
}
