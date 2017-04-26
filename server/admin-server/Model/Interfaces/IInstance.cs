namespace Microarea.AdminServer.Model.Interfaces
{
    interface IInstance
    {
        string Id { get; }
        string Name { get; }
        bool Disabled { get; }
    }
}
