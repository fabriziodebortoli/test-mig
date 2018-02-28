using Microarea.Common.NameSolver;
using System;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
    //====================================================================
    public interface IServiceManager
    {
        PathFinder PathFinder { get; }
        IService GetService(Type serviceType);
    }
}
