using Alphaleonis.Win32.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.IO
{
    internal static class TransactionTrait
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "this")]
        public static void WrapWithTransaction(this TransactionalFile @this, Action<KernelTransaction> action)
        {
            WrapWithTransactionInternal(action);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "this")]
        public static void WrapWithTransaction(this TransactionalFolder @this, Action<KernelTransaction> action)
        {
            WrapWithTransactionInternal(action);
        }

        private static void WrapWithTransactionInternal(Action<KernelTransaction> action)
        {
            bool commitMemento = false;
            var transaction = Transaction.Instance;
            var currentTransaction = transaction.Current;
            if (currentTransaction == null)
            {
                transaction.Start();
                currentTransaction = transaction.Current;
                commitMemento = true;
            }
            try
            {
                action(currentTransaction);

                if (commitMemento)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (commitMemento)
                {
                    transaction.Rollback();
                }

                throw;
            }
        }
    }
}
