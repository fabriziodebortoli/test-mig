namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface ILocalizer
	{
		string Translate(string baseString);
        void Build(string filepath, IBasePathFinder pathFinder);
	}
}
