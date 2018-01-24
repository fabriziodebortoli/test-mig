using System;
using System.Collections.Generic;
using TaskBuilderNetCore.Interfaces;
namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
    //====================================================================================    
    public interface ICustomizationContext
	{
        IList<IEasyStudioApp> EasyStudioApplications { get; }
        string CurrentApplication { get; set; }
        string CurrentModule { get; set; }
    }
}