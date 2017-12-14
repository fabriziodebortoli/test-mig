using Alphaleonis.Win32.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Microarea.Snap.IO
{
    public class Transaction : ITransaction
    {
        readonly object lockTicket = new object();
        readonly static object staticLockTicket = new object();

        static ITransaction _instance;

        public static ITransaction Instance
        {
            get
            {
                lock (staticLockTicket)
                {
                    if (_instance == null)
                    {
                        _instance = new Transaction();
                    }
                    return _instance;
                }
            }
        }

        protected Transaction()
        {

        }
        public KernelTransaction Current { get; private set; }
        public void Start()
        {
            lock (lockTicket)
            {
                if (Current != null)
                {
                    throw new InvalidOperationException("A transaction is already pending, commit it first");
                }

#warning Dare nome alla transazione?
                Current = new KernelTransaction();
            }
        }

        public void Commit()
        {
            lock (lockTicket)
            {
                if (Current == null)
                {
                    throw new InvalidOperationException("Start a transaction first");
                }

                Current.Commit();
                Current.Dispose();
                Current = null;
            }
        }

        public void Rollback()
        {
            lock (lockTicket)
            {
                if (Current == null)
                {
                    throw new InvalidOperationException("Start a transaction first");
                }

                Current.Rollback();
                Current.Dispose();
                Current = null;
            }
        }
    }
}
