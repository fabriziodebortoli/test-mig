using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using System;

namespace Microarea.TaskBuilderNet.Core.EasyStudioServer.Serializers
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
    }
}
