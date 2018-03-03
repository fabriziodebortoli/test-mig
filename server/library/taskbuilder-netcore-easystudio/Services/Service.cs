using TaskBuilderNetCore.EasyStudio.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Services
{
    //====================================================================
    public class Service : Component, IService
    {
        //---------------------------------------------------------------
        IServiceManager services;
        ISerializer serializer;

        //---------------------------------------------------------------
        public IServiceManager Services { get => services; set => services = value; }
        //---------------------------------------------------------------
        public ISerializer Serializer
        {
            get
            {
                if (serializer == null)
                    serializer = DefaultSerializer;

                return serializer;
            }
            set => serializer = value;
        }
    }
}
