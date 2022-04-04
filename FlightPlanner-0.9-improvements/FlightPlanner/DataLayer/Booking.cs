using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightPlanner.DataLayer
{
    class Booking 
    {

        public Booking()
        {

        }

        public Booking(int flightId, int customerId, int seats, int travelClass, decimal price)
        {
            FlightId = flightId;
            CustomerId = customerId;
            Seats = seats;
            TravelClass = travelClass;
            Price = price;
        }

        public int FlightId { set; get; } // foreign key
        public int CustomerId { set; get; } // foreign key
        public int Seats { set; get; }
        public int TravelClass { set; get; }
        public decimal Price { set; get; }
    }
}
