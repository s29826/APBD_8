using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _iClientsService;

    public ClientsController(IClientsService clientsService)
    {
        _iClientsService = clientsService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] ClientDTO dto)
    {
        var client = await _iClientsService.CreateClient(dto);
        return Created();
    }
}