using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using TaskBuilderNetCore.Data;

namespace Test
{

    public class Test
    {
        public static void TestConnection()
        {
            using (DBConnection conn = new DBConnection(Provider.DBType.SQLSERVER, "Server=localhost;Database=Company40; User ID=sa ;Password=;Trusted_Connection=TRUE;"))
            {
                conn.Open();

                using (DBCommand command = new DBCommand("Select * from MA_Items", conn))
                {
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("{0} {1} {2}",
                            reader.GetString(0), reader.GetString(1), reader.GetString(2));
                        }
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            TestConnection();
        }
    }
}
