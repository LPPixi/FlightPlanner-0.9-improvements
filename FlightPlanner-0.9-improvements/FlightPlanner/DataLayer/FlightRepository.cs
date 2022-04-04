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
    // Implement operations that affect several tables (e.g. deleting a flight affects also the Booking table)
    class FlightRepository
    {
        private BookingDataMapper bookingDataMapper;
        private FlightDataMapper flightDataMapper;
        // TODO: add other data mappers
        string ConnectionString { get; set; }

        public FlightRepository(string connectionString)
        {
            this.ConnectionString = connectionString;
            bookingDataMapper = new BookingDataMapper(this.ConnectionString);
            flightDataMapper = new FlightDataMapper(this.ConnectionString);
        }

        public void DeleteFlight(int id)
        {
            int rowCount = Int32.MinValue;
            try
            {
                // FK_Booking_Flight uses "on delete no action"
                // FK_PilotRoster_Flight uses "ON DELETE CASCADE"
                rowCount = bookingDataMapper.DeleteByFlightId(id);
                rowCount = flightDataMapper.Delete(id);
            }
            catch (DbException dbEx) // TODO: review and improve exception handling
            {
                // TODO: log to log file
                throw new InvalidOperationException("Flight could not be deleted!", dbEx);
            }
            catch (Exception)
            {
                // TODO: log to log file
                throw;
            }

            if (rowCount < 1)
            {
                throw new InvalidOperationException("The specified flight could not be deleted.");
            }
        }

    }
}
