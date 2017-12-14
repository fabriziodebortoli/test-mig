using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.Snap.Services
{
    class NoTransactionManager : ITransactionManager
    {
        public NoTransactionManager()
        {

        }
        public void Start()
        {
            //Do nothing
        }

        public void Commit()
        {
            //Do nothing
        }

        public void Rollback()
        {
            //Do nothing
        }
    }
}
