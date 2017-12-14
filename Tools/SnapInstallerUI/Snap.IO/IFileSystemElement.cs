namespace Microarea.Snap.IO
{
    public interface IFileSystemElement
    {
        bool Exists { get; }
        string FullName { get; }
        bool IsPathRooted { get; }
        string Name { get; }
    }
}