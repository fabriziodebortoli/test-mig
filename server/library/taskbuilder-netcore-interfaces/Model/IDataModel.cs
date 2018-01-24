using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Model.Interfaces
{
    public interface IDataModel
    {
        bool LoadData();
        bool NewData();
        bool EditData();
        bool ClearData();
        bool SaveData();
        bool DeleteData();
    }
}
