using FlightPlanner.DataLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace FlightPlanner
{
    class CustomerDataMapper
    {
        public string ConnectionString { get; set; }

        public CustomerDataMapper(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private Customer ParseRecord(IDataReader reader)
        {
            Customer customer = new Customer();

            customer.Id = reader.GetInt32(0);
            customer.LastName = reader.GetString(1);
            customer.Birthday = reader.GetDateTime(2);
            customer.City = reader.GetString(3);

            return customer;
        }
        
        private int ExecuteCustomerNonQuery(string command)
        {
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                IDbCommand updateCustomerCommand = databaseConnection.CreateCommand();
                updateCustomerCommand.CommandText = command;

                Console.WriteLine(updateCustomerCommand.CommandText);

                databaseConnection.Open();

                return updateCustomerCommand.ExecuteNonQuery();
            }
        }
        public int Create(Customer customer)
        {
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                IDbCommand createCustomerCommand = databaseConnection.CreateCommand();
                createCustomerCommand.CommandText =
                    $"INSERT INTO Customer VALUES({customer.Id}, '{customer.LastName}', '{customer.Birthday.Date}', '{customer.City}');";

                databaseConnection.Open();

                return createCustomerCommand.ExecuteNonQuery();
            }
        }

        public List<Customer> ReadCustomers()
        {
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                IDbCommand readCustomersCommand = databaseConnection.CreateCommand();

                readCustomersCommand.CommandText =
                    $"select * from Customer;";

                databaseConnection.Open();

                IDataReader reader = readCustomersCommand.ExecuteReader();

                List<Customer> customers = new List<Customer>();
                while (reader.Read())
                {
                    Customer customer = ParseRecord(reader);
                    customers.Add(customer);
                }

                return customers;
            }
        }

        public Customer ReadCustomerById(int id)
        {
            using (DbConnection databaseConnection = new SqlConnection(this.ConnectionString))
            {
                IDbCommand readCustomerCommand = databaseConnection.CreateCommand();

                readCustomerCommand.CommandText = $"select * from Customer where Id={id}";

                databaseConnection.Open();

                IDataReader reader = readCustomerCommand.ExecuteReader();
                
                Customer customer = null;
                while (reader.Read())
                {
                    customer = ParseRecord(reader);
                }
                return customer;
            }
        }


        public int UpdateLastName(int id, string newLastName) => ExecuteCustomerNonQuery($"update Customer set LastName = '{newLastName}' where Customer.Id = {id};");

        public int UpdateBirthdate(int id, DateTime BirthDate) => ExecuteCustomerNonQuery($"update Customer set LastName = CAST('{BirthDate.Date}' as DATE) where Customer.Id = {id};");

        public int UpdateCity(int id, int newCityName) => ExecuteCustomerNonQuery($"update Customer set City = '{newCityName}' where Customer.Id = {id};");

        public int DeleteById(int id) => ExecuteCustomerNonQuery($"delete from Customer where Customer.Id = {id}");
    }
}
