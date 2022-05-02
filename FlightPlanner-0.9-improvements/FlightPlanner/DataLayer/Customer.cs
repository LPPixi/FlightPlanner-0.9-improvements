using System;

namespace FlightPlanner.DataLayer
{
    public class Customer
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string City { get; set; }

        public Customer(int id, string lastName, DateTime birthday, string city)
        {
            Id = id;
            LastName = lastName;
            Birthday = birthday;
            City = city;
        }

        public Customer()
        {
        }

        public override string ToString()
        {
            return $"Id: {Id}\nLast Name: {LastName}\nBirthday: {Birthday.Date.ToString("dd.MM.yyyy")}\nCity: {City}";
        }
    }
}