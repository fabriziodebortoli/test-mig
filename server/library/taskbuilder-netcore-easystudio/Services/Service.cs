using System;
using System.Diagnostics;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Services
{
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
                    serializer = DefaultSerializer;

                return serializer;
            }
            set => serializer = value;
        }

        //---------------------------------------------------------------
        public Service()
        {
            diagnostic = new DiagnosticProvider(string.Concat(NameSolverStrings.EasyStudio, ": ", Name));
        }
            
    }
}
