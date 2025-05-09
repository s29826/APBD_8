using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ITripsService _tripsService;

    public TripsController(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }

    //Ten endpoint będzie pobierał wszystkie dostępne wycieczki wraz z ich podstawowymi informacjami
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _tripsService.GetTrips();
        
        
        return Ok(trips);
    }

    //Ten endpoint będzie pobierał wszystkie wycieczki powiązane z konkretnym klientem
    [HttpGet("/api/clients/{id}/[controller]")]
    public async Task<IActionResult> GetTrip(int id)
    {
        if(!await _tripsService.DoesTripExist(id)){
            return NotFound();
        }
        
        var trip = await _tripsService.GetTrip(id);
        
        if (trip.Count == 0)
        {
            return NotFound();
        }

        
        return Ok(trip);
    }

    //Ten endpoint usunie rejestrację klienta z wycieczki
    [HttpDelete("/api/clients/{clientId}/[controller]/{tripId}")]
    public async Task<IActionResult> DeleteClientFromTrip(int clientId, int tripId)
    {
        if (!await _tripsService.DoesRegistartionExist(clientId, tripId))
        {
            return BadRequest("Nie istnieje taka rejestracja");
        }
        
        await _tripsService.DeleteClientFromTrip(clientId, tripId);
        
        
        return Ok($"Klient o id = {clientId}, został usunięty z wycieczki o id = {tripId}");
    }
}