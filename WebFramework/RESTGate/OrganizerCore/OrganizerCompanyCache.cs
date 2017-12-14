using System.Collections.Generic;

namespace RESTGate.OrganizerCore
{
    //================================================================================
    public class OrganizerCompanyCache
    {
        string company;
        string connectionString;
        List<OM_Commitments> commitments;
        List<OM_Workers> workers;

        public string Company { get { return this.company; } set { this.company = value; } }
        public string ConnectionString { get { return this.connectionString; } set { this.connectionString = value; } }
        public List<OM_Commitments> Commitments { get { return this.commitments; } set { this.commitments = value; } }
        public List<OM_Workers> Workers { get { return this.workers; } set { this.workers = value; } }

        //--------------------------------------------------------------------------------
        public OrganizerCompanyCache(string company, string connectionString)
        {
            this.company = company;
            this.connectionString = connectionString;
            this.commitments = new List<OM_Commitments>();
            this.workers = new List<OM_Workers>();
        }
    }
}