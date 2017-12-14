namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	public enum ItemType { Folder, File }

    public interface ISourceControlItem
    {
        ISourceControlItem Add(string local, string comment, bool isProject);
        void CheckIn(string local, string comment);
        void CheckOut(string local, string comment, bool updateLocal);
        void Delete();
        ISourceControlItemCollection GetItems();
        void GetLatestVersion(string local);
        bool IsCheckedOut { get; }
        bool IsCheckedOutToMe { get; }
        bool IsDifferent(string localPath);
        bool IsFolder { get; }
        string LocalPath { get; set; }
        string Name { get; }
        string Path { get; }
        void Rename(string newName);
        ItemType Type { get; }
        void UndoCheckOut(string local);
    }
}
