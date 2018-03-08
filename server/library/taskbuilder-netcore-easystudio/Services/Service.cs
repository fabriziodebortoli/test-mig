using Microarea.Common.NameSolver;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.EasyStudio.Serializers;

namespace TaskBuilderNetCore.EasyStudio.Services
{
    //====================================================================
    public class Service<T>
    {
        private T service;

        //---------------------------------------------------------------
        public T Obj { get => service; set => service = value; }

        //---------------------------------------------------------------
        public static implicit operator T(Service<T> value) => value.Obj;

        //---------------------------------------------------------------
        public static implicit operator Service<T>(T value) => new Service<T> { Obj = value };
    }

    //====================================================================
    public class Service : Component, IService
    {
        //---------------------------------------------------------------
        IServiceManager services;
        ISerializer serializer;
        IDiagnosticProvider diagnostic;

        public IDiagnosticProvider Diagnostic { get => diagnostic; set => diagnostic = value; }

        //---------------------------------------------------------------
        public IServiceManager Services { get => services; set => services = value; }
        //---------------------------------------------------------------
        public ISerializer Serializer
        {
            get
            {
                if (serializer == null)
                    Serializer = DefaultSerializer;

                return serializer;
            }
            set
            {
                serializer = value;
                if (serializer != null)
                {
                    Serializer ser = serializer as Serializer;
                    if (ser != null && ser.PathFinder == null)
                        ser.PathFinder = PathFinder;
                }
            }
        }

        //---------------------------------------------------------------
        public PathFinder PathFinder { get => Services.PathFinder; }

        //---------------------------------------------------------------
        public Service()
        {
        }          
    }
}
