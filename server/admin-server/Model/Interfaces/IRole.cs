using System;
using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface IRole : IAdminModel
    {
        string RoleName { get; set; }
        int RoleId { get; set; }
        string Description { get; set; }
        bool Disabled { get; set; }

    }
}