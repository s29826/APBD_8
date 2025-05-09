using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<int> CreateClient([FromBody] ClientDTO dto);
    Task<bool> DoesClientExist(int clientId);
    Task AddClientToTrip(int idClient, int idTrip);
}