using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace FlightPlanner.DataLayer
{
    class Flight
    {
        public int Id { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public int Duration { get; set; }
        public DateTime DepartureDate { get; set; }
        public int PlaneId { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Departure: {Departure}, Destination: {Destination}, " +
                $"Duration: {Duration}, DepartureDate: {DepartureDate}, PlaneId: {PlaneId}";
        }
    }
}
