using System.Data.Linq.Mapping;

namespace RESTGate.OrganizerCore
{
    [Table(Name = "OM_Workers")]
    public class OM_Workers
    {
        int workerId;
        string name;
        string lastName;

        [Column(IsPrimaryKey = true, Name = "WorkerId")]
        public int WorkerId { get { return this.workerId; } set { this.workerId = value; } }

        [Column(Name = "Name")]
        public string Name { get { return this.name; } set { this.name = value; } }

        [Column(Name = "LastName")]
        public string LastName { get { return this.lastName; } set { this.lastName = value; } }
    }
}