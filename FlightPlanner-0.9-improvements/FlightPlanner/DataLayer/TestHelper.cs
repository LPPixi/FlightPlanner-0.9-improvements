using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightPlanner.DataLayer
{
    class TestHelper
    {
        public static void InitializeDatabase(string connectionString)
        {
            string sqlBatch = string.Empty;

            string script = File.ReadAllText(@"..\..\DataLayer\FlightPlannerCreateDb.sql");
            script += "\nGO"; // make sure last batch is executed in the foreach loop

            using (DbConnection databaseConnection = new SqlConnection(connectionString))
            {
                IDbCommand command = databaseConnection.CreateCommand();

                databaseConnection.Open();
                // Server server = new Server(new ServerConnection(conn)); -- use SQL Server Management Objects (SMO) as alternative solution

                // A GO cannot be executed by ADO.NET
                // If a GO ist found in a script split the script at this position into smaller scripts ("batches") and remove the GO. 
                foreach (string line in script.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        // a GO was found, execute the batch
                        command.CommandText = sqlBatch;
                        command.ExecuteNonQuery();
                        sqlBatch = string.Empty;
                    }
                    else
                    {
                        // add next line to the batch
                        sqlBatch += line + "\n";
                    }
                }
            }
        }
    }
}
