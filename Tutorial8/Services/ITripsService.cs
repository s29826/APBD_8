using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<List<TripDTO>> GetTrip(int id);
    Task AddClientToTrip(int idClient, int idTrip);
    Task DeleteClientFromTrip(int idClient, int idTrip);
}