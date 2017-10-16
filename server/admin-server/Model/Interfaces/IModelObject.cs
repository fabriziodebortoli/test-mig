﻿using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using System.Data;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IModelObject
    {
        OperationResult Save(BurgerData burgerData);
        IModelObject Fetch(IDataReader reader);
        string GetKey();
    }
}
  
