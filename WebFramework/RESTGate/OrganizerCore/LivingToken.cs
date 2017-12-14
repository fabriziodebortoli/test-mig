
namespace RESTGate.OrganizerCore
{
    //================================================================================
    public class LivingToken
    {
        string token;
        string company;

        public string Token { get { return this.token; } set { this.token = value; } }
        public string Company { get { return this.company; } set { this.company = value; } }

        //--------------------------------------------------------------------------------
        public LivingToken(string token, string company)
        {
            this.token = token;
            this.company = company;
        }
    }
}