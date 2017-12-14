using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.Snap.Services
{
    public interface ITransactionManager
    {
        void Start();
        void Commit();
        void Rollback();
    }
}
