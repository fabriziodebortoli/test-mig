using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Interfaces
{
    //====================================================================================    
    public interface IApplicationSerializer : ISerializer
    {
        bool DeleteApplication(string applicationName);
        bool DeleteModule(string applicationName, string moduleName);
		bool ExistsApplication(string applicationName);
        bool ExistsModule(string applicationName, string moduleName);
        bool CreateApplication(string applicationName, ApplicationType type);
        bool CreateModule(string applicationName, string moduleName);
        bool RenameApplication(string oldName, string newName);
        bool RenameModule(string applicationName, string oldName, string newName);
    }
}