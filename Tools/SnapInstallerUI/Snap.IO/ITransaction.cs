using Alphaleonis.Win32.Filesystem;

namespace Microarea.Snap.IO
{
    public interface ITransaction
    {
        KernelTransaction Current { get; }

        void Start();

        void Commit();
        void Rollback();
    }
}