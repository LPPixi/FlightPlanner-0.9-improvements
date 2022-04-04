using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightPlanner
{
    class CustomerDataMapper
    {
        public string ConnectionString { get; set; }
        
        public CustomerDataMapper(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public int UpdateLastName(int id, string newName)
        {
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                IDbCommand updateCustomerCommand = databaseConnection.CreateCommand();
                updateCustomerCommand.CommandText =
                    $"update Customer set LastName = '{newName}' where Customer.Id = {id};";

                // Console.WriteLine NICHT an dieser Stelle in einem professionellen Programm verwenden, 
                // Methode soll auch bei GUI Anwendungen funktionieren
                Console.WriteLine(updateCustomerCommand.CommandText);

                databaseConnection.Open();

                int rowCount = updateCustomerCommand.ExecuteNonQuery();
                return rowCount;
            }
        }
    }
}
