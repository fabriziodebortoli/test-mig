namespace Microarea.Snap.IO
{
    public interface IFolder : IFileSystemElement
    {
        IFile[] GetFiles(string searchPattern);
        IFile[] GetFiles(IFolder rootFolder, string searchPattern);
        IFolder[] GetFolders(string searchPattern);
        IFolder[] GetFolders(IFolder rootFolder, string searchPattern);
        void Create();
        void Delete(IFolder rootFolder);
        void Delete();
        bool IsEmpty(IFolder rootFolder);
        bool IsEmpty();
        IFolder ParentFolder { get; }
        void CopyTo(IFolder destination);
    }
}