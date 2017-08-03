namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IRole
    {
        string RoleName { get; set; }
        string Description { get; set; }
        string ParentRoleName { get; set; }
        bool Disabled { get; set; }
    }
}