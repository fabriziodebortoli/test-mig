﻿using System;

namespace Microarea.AdminServer.Controllers.Helpers
{
    /// <summary>
    /// Contiene le informazioni da passare nell'header delle API per i controlli di sicurezza
    /// </summary>
    //================================================================================
    public class AuthorizationInfo
    {
        // a seconda della chiamata il tipo puo' essere JWT / APP
        string type;
        public string Type { get { return this.type; } set { this.type = value.ToUpperInvariant(); }}

        // se il tipo == JWT l'AppId e' empty
        public string AppId { get; set; }
        public string SecurityValue { get; set; }

        public const string TypeJwtName = "JWT";
        public const string TypeAppName = "APP";

        //-----------------------------------------------------------------------------	
        public bool IsJwtToken { get { return (string.Compare(Type, TypeJwtName, StringComparison.CurrentCultureIgnoreCase) == 0); } }
        public bool IsAppToken { get { return (string.Compare(Type, TypeAppName, StringComparison.CurrentCultureIgnoreCase) == 0); } }

        //-----------------------------------------------------------------------------	
        public AuthorizationInfo()
        {
        }

        //-----------------------------------------------------------------------------	
        public AuthorizationInfo(string type, string appId, string securityValue)
        {
            Type = type;
            AppId = appId;
            SecurityValue = securityValue;
        }
    }
}