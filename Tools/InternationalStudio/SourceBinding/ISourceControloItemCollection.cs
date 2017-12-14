namespace Microarea.Tools.TBLocalizer.SourceBinding
{
    public interface ISourceControlItemCollection
    {
        int Count { get; }
        System.Collections.IEnumerator GetEnumerator();
    }
}
