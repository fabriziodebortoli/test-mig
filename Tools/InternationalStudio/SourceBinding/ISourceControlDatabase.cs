using System;
namespace Microarea.Tools.TBLocalizer.SourceBinding
{
    public interface ISourceControlDatabase : IDisposable
    {
        bool CheckInFile(string file, string localPath);
		bool RemoveFile(string file);
        bool CheckOutFile(string file, string localPath);
        ISourceControlItem CreateProject(string path, string comment, bool recursive);
        ISourceControlItem GetItem(string aPath);
        ISourceControlItemCollection GetItems(string aPath);
        bool IsOpen { get; }
        string LastError { get; }
        bool Open(string iniPath);
        bool Open(string iniPath, string userName, string password);
    }
}
