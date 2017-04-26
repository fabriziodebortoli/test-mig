namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    interface IInstance
    {
        string Id { get; }
        string InstanceName { get; }
        bool Disabled { get; }
    }
}
