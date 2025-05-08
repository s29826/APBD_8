using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTrips();
            return Ok(trips);
        }

        [HttpGet("/api/clients/{id}/[controller]")]
        public async Task<IActionResult> GetTrip(int id)
        {
            // if( await DoesTripExist(id)){
            //  return NotFound();
            // }
            var trip = await _tripsService.GetTrip(id);
            if (trip.Count == 0)
            {
                return NotFound();
            }
            return Ok(trip);
        }

        [HttpPut("/api/clients/{clientId}/[controller]/{tripId}")]
        public async Task<IActionResult> AddClientToTrip(int clientId, int tripId)
        {
            await _tripsService.AddClientToTrip(clientId, tripId);
            return Ok();
        }

        [HttpDelete("/api/clients/{clientId}/[controller]/{tripId}")]
        public async Task<IActionResult> DeleteClientFromTrip(int clientId, int tripId)
        {
            await _tripsService.DeleteClientFromTrip(clientId, tripId);
            return Ok();
        }
    }
}
