using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
    //====================================================================
    public interface IServiceManager
    {
        IService GetService(Type serviceType);
    }
}
