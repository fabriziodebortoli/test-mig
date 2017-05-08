using System;
using TaskBuilderNetCore.Documents.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.Documents.Model
{
    /// <summary>
    /// It contains context from caller user including identity, parameters or aux objects
    /// </summary>
    //====================================================================================    
    public class CallerContext : ICallerContext
    {
        INameSpace nameSpace;
        string authToken;
        string company;

        //-----------------------------------------------------------------------------------------------------
        public INameSpace NameSpace
        {
            get
            {
                return nameSpace;
            }

            set
            {
                nameSpace = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public string AuthToken
        {
            get
            {
                return authToken;
            }

            set
            {
                authToken = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public string Company
        {
            get
            {
                return company;
            }

            set
            {
                company = value;
            }
        }

        //-----------------------------------------------------------------------------------------------------
        public bool IsSameIdentity(ICallerContext context)
        {
            return NameSpace.FullNameSpace == context.NameSpace.FullNameSpace &&
                AuthToken == context.AuthToken &&
                Company == context.Company;
        }
    }
}