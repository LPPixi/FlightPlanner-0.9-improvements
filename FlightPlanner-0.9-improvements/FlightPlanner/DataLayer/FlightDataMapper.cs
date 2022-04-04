using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace FlightPlanner.DataLayer
{

    /// <summary>
    /// Data mapper design pattern: https://en.wikipedia.org/wiki/Data_mapper_pattern    
    /// </summary>
    class FlightDataMapper
    {
        public String ConnectionString { get; set; }

        public FlightDataMapper(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public List<Flight> ReadFlights()
        {
            // Ignore:
            // Using OleDbConnection instead of SqlConection, e.g. for Microsoft Access
            // DbConnection databaseConnection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\schule2000.mdb"); 
            List<Flight> flights = new List<Flight>();

            // Declare the variable as DbConnection not as SqlConnection so you can easier replace SQL Server by an Oracle Database Server, etc.
            // There is also a design principle: "Program to an interface (= base class OR C#-interface) not an implementation."
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                // erzeuge zuerst ein zur Sql Server Connection passendes Command, weise Select Befehl zu
                IDbCommand selectFlightCommand = databaseConnection.CreateCommand();
                
                // Aus Performance Gründen alle Datensätze auf einmal lesen, nicht die Methode Read() dieser Klasse verwenden.
                selectFlightCommand.CommandText = "select * from Flight";

                databaseConnection.Open();

                // erzeuge den DataReader (der das zeilenweise Auslesen erlaubt) aus dem Command
                IDataReader flightReader = selectFlightCommand.ExecuteReader();
                // gibt es nur einen Wert ist auch .ExecuteScalar möglich
                // für update, insert, ...  ist aCommand.ExecuteNonQuery  zuständig
                
                //jetzt in der Schleife durch das Ergebnis des Select Befehls
                while (flightReader.Read())
                {
                    // https://docs.microsoft.com/en-us/dotnet/api/system.data.idatarecord.item
                    Flight flight = new Flight();
                    flight.Id = flightReader.GetInt32(0); // Column 0 of table Flight
                    flight.Departure = flightReader.GetString(1);
                    flight.Destination = flightReader.GetValue(2).ToString();
                    flight.Duration = (int)flightReader["Duration"]; // Column 3 GetInt32(3)
                    flight.DepartureDate = flightReader.GetDateTime(4);
                    flight.PlaneId = flightReader.GetInt32(5);

                    flights.Add(flight);
                }

                return flights;
            }
            // finally
        }
        /// <summary>
        /// Read a single flight record.
        /// </summary>
        /// <param name="Id">The primary key of the flight record.</param>
        /// <returns>Returns an object that stores the flight record.</returns>
        public Flight Read(int Id)
        {
            throw new NotImplementedException();
        }


        public int Create(Flight flight)
        {
            // using = try/finally block, in the finally block the database connection is disposed (removed) in case
            // of an error and in case of no error
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                // erzeuge zuerst ein zur Sql Server Connection passendes Command, weise Select Befehl zu
                IDbCommand createFlightCommand = databaseConnection.CreateCommand();
                // INSERT INTO Flight VALUES(203, 'Berlin', 'Paris', 120, '20180202', 21);
                createFlightCommand.CommandText =
                   $"insert into Flight values ({flight.Id}, '{flight.Departure}', '{flight.Destination}', " +
                   $"{flight.Duration}, '{flight.DepartureDate.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}', " + 
                   $"{flight.PlaneId});";

                // Console.WriteLine NICHT an dieser Stelle in einem professionellen Programm verwenden, 
                // Methode soll auch bei GUI Anwendungen funktionieren 
                Console.WriteLine(createFlightCommand.CommandText);
                databaseConnection.Open();

                int rowCount = createFlightCommand.ExecuteNonQuery();
                return rowCount;

            }
        }

        public int Update(Flight flight)
        {
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                // erzeuge zuerst ein zur Sql Server Connection passendes Command, weise Select Befehl zu
                IDbCommand updateFlightCommand = databaseConnection.CreateCommand();
                updateFlightCommand.CommandText = 
                   $"update Flight set Departure = '{flight.Departure}', " +
                   $"Destination = '{flight.Destination}', " +
                   $"Duration = {flight.Duration}, " +
                   $"DepartureDate = '{flight.DepartureDate.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}', " +
                   $"PlaneId = {flight.PlaneId} " +
                   $"where Flight.Id = {flight.Id};";

                // Console.WriteLine NICHT an dieser Stelle in einem professionellen Programm verwenden, 
                // Methode soll auch bei GUI Anwendungen funktionieren
                Console.WriteLine(updateFlightCommand.CommandText);
                
                databaseConnection.Open();

                int rowCount = updateFlightCommand.ExecuteNonQuery();
                return rowCount;
            }
        }

        public int Delete(Flight flight)
        {
            return Delete(flight.Id);
        }

        public int Delete(int Id)
        {
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                // erzeuge zuerst ein zur Sql Server Connection passendes Command, weise Select Befehl zu
                IDbCommand deleteFlightCommand = databaseConnection.CreateCommand();
                deleteFlightCommand.CommandText =
                   $"delete from Flight where Flight.Id = {Id};";

                // Console.WriteLine NICHT an dieser Stelle in einem professionellen Programm verwenden, 
                // Methode soll auch bei GUI Anwendungen funktionieren
                Console.WriteLine(deleteFlightCommand.CommandText);

                databaseConnection.Open();

                int rowCount = deleteFlightCommand.ExecuteNonQuery();
                return rowCount;
            }
            
        }

    }
}
