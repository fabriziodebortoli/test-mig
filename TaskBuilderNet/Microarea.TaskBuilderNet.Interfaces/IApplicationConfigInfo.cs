
namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	/// <summary>
	/// Corrispettivo del file Application.config che decrive un'applicazione
	/// per TaskBuilder.
	/// </summary>
	public interface IApplicationConfigInfo
	{
		string DbSignature { get; }
		string HelpModule { get; }
		string Icon { get; }
		string Name { get; }
		bool Parse();
		bool Save();
		string Type { get; }
		string Uuid { get; }
		string Version { get; set; }
		bool Visible { get; }
		string WelcomeBmp { get; }
	}
}
