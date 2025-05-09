using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _iClientsService;
    private readonly ITripsService _iTripsService;

    public ClientsController(IClientsService clientsService, ITripsService iTripsService)
    {
        _iClientsService = clientsService;
        _iTripsService = iTripsService;
    }

    //Ten endpoint utworzy nowy rekord klienta
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }
        
        var newId = await _iClientsService.CreateClient(dto);
        
        
        return Ok($"Utworzono nowego klienta, jego ID: {newId}");
    }
    
    //Ten endpoint zarejestruje klienta na konkretną wycieczkę
    [HttpPut("{clientId}/trips/{tripId}")]
    public async Task<IActionResult> AddClientToTrip(int clientId, int tripId)
    {
        if (!await _iTripsService.DoesTripExist(tripId))
        {
            return NotFound($"Wycieczka o id = {tripId} nie istnieje!");
        }
        
        if (!await _iClientsService.DoesClientExist(clientId))
        {
            return NotFound($"Klient o id = {clientId} nie istnieje");
        }

        if (await _iTripsService.IsTripFull(tripId))
        {
            return StatusCode(StatusCodes.Status409Conflict, new { message = "Wycieczka jest pełna!"});
        }
        
        await _iClientsService.AddClientToTrip(clientId, tripId);
        
        
        return Ok("Klient zarejestrowany poprawnie!");
    }
}