using System;
using System.Collections.Generic;
using System.Globalization;

namespace RESTGate.OrganizerCore
{
    // This class is the Commitments cache. It's used by RESTGate to hold commitments

    //================================================================================
    public sealed class OrganizerCache
    {
        // singleton implementation

        private static OrganizerCache instance;
        private static readonly Object sync = new object();
        
        // models

        List<OrganizerCompanyCache> companies;

        public List<OrganizerCompanyCache> Companies { get { return this.companies; } }

        // singleton
        //--------------------------------------------------------------------------------
        public static OrganizerCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (sync)
                    {
                        if (instance == null)
                            instance = new OrganizerCache();
                    }
                }

                return instance;
            }
        }

        //--------------------------------------------------------------------------------
        private OrganizerCache()
        {
            this.companies = new List<OrganizerCompanyCache>();
        }

        //--------------------------------------------------------------------------------
        private void PopulateDataModels()
        {
            foreach (OrganizerCompanyCache companyCache in this.companies)
            {
                companyCache.Commitments = OrganizerDataHelper.ReadCommitments(companyCache.ConnectionString);
                companyCache.Workers = OrganizerDataHelper.ReadWorkers(companyCache.ConnectionString);
            }
        }

        //--------------------------------------------------------------------------------
        public bool ReloadData()
        {
            bool val = true;

            try
            {
                PopulateDataModels();
            }
            catch (Exception)
            {
                val = false;
            }

            return val;
        }

        //--------------------------------------------------------------------------------
        public void AddIfNotExists(OrganizerCompanyCache companyCache)
        {
            if (this.companies.Find(p=>p.Company.Equals(companyCache.Company)) != null)
            {
                return;
            }

            companyCache.Commitments = OrganizerDataHelper.ReadCommitments(companyCache.ConnectionString);
            companyCache.Workers = OrganizerDataHelper.ReadWorkers(companyCache.ConnectionString);

            this.companies.Add(companyCache);
        }

        //--------------------------------------------------------------------------------
        public bool AddCommitment(string company, OM_Commitments commitment)
        {
            OrganizerCompanyCache companyCache = SelectCompany(company);
            companyCache.Commitments.Add(commitment);
            OrganizerDataHelper.SaveEntity<OM_Commitments>(commitment, companyCache.ConnectionString);
            return true;
        }

        //--------------------------------------------------------------------------------
        private OrganizerCompanyCache SelectCompany(string companyName)
        {
            OrganizerCompanyCache company = this.companies.Find(
                p => p.Company.Equals(companyName, StringComparison.InvariantCultureIgnoreCase) &&
                !String.IsNullOrEmpty(p.Company));

            if (company == null)
                return new OrganizerCompanyCache(String.Empty, String.Empty);

            return company;
        }

        //--------------------------------------------------------------------------------
        public OrganizerCompanyCache GetCompany(string companyName)
        {
            return SelectCompany(companyName);
        }
    }
}