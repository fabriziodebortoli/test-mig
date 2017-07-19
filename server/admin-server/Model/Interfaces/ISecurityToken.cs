using Microarea.AdminServer.Library;
using System;

namespace Microarea.AdminServer.Model.Interfaces
{
    //================================================================================
    public interface ISecurityToken : IAdminModel
    {
        string AccountName { get; set; }
        TokenType TokenType { get; set; }
        string Token { get; set; }
        bool Expired { get; set; }
        DateTime ExpirationDate { get; set; }
    }

    //---------------------------------------------------------------------
    public enum TokenType { Undefined, API, Authentication }
}
