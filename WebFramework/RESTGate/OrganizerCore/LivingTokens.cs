using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace RESTGate.OrganizerCore
{
    //================================================================================
    public sealed class LivingTokens
    {
        // singleton members

        private static LivingTokens instance;
        private static readonly Object sync = new object();

        // singleton implementation
        //--------------------------------------------------------------------------------
        public static LivingTokens Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (sync)
                    {
                        if (instance == null)
                            instance = new LivingTokens();
                    }
                }

                return instance;
            }
        }

        // model

        List<LivingToken> tokens;

        public List<LivingToken> Tokens { get { return this.tokens; } set { this.tokens = value; } }

        //--------------------------------------------------------------------------------
        private LivingTokens()
        {
            this.tokens = new List<LivingToken>();
        }

        //--------------------------------------------------------------------------------
        public string GetCompanyFromToken(string token)
        {
            LivingToken livingToken = this.tokens.Find(p => p.Token.Equals(token, StringComparison.InvariantCultureIgnoreCase)
                && !String.IsNullOrEmpty(p.Token));

            if (livingToken == null)
            {
                return String.Empty;
            }

            return livingToken.Company;
        }

        //--------------------------------------------------------------------------------
        public bool AddToken(string token)
        {
            if (this.tokens.Find(p=>p.Token.Equals(token, StringComparison.InvariantCultureIgnoreCase)
                 && !String.IsNullOrEmpty(p.Token)) != null)
            {
                // token already exists
                return true;
            }

            // get company from token and add to current list

            TokenValidator tokenValidator = new TokenValidator(token);
            bool isAuthenticated = tokenValidator.IsTokenValid;
            string companyName = tokenValidator.CompanyName;

            if (isAuthenticated)
            {
                this.tokens.Add(new LivingToken(token, companyName));
                OrganizerCache.Instance.AddIfNotExists(
                    new OrganizerCompanyCache(companyName, tokenValidator.ConnectionString));

                return true;
            }
            else
            {
                return false;
            }
        }

        //--------------------------------------------------------------------------------
        public void RemoveToken(string token)
        {
            LivingToken tok = this.tokens.Find(p=>p.Token.Equals(token, StringComparison.InvariantCultureIgnoreCase));
            
            if (tok == null)
                return;

            this.tokens.Remove(tok);
        }
    }
}