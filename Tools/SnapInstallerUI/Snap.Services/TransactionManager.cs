using Microarea.Snap.Core;
using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.Snap.Services
{
    class TransactionManager : ITransactionManager
    {
        public void Start()
        {
            Transaction.Instance.Start();
        }
        public void Commit()
        {
            Transaction.Instance.Commit();
        }

        public void Rollback()
        {
            Transaction.Instance.Rollback();
        }
    }
}
