using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<List<TripMainDTO>> GetTrip(int clientId);
    Task<bool> DoesTripExist(int tripId);
    Task<bool> IsTripFull(int tripId);
    Task<bool> DoesRegistartionExist(int clientId, int tripId);
    Task DeleteClientFromTrip(int idClient, int idTrip);
}