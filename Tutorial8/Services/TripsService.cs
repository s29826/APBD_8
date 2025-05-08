using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var dict = new Dictionary<int, TripDTO>();
        
        string command = "SELECT Trip.IdTrip, Trip.Name, Description, DateFrom, DateTo, MaxPeople, C.Name AS C\nFROM Trip\nJOIN s29826.Country_Trip CT on Trip.IdTrip = CT.IdTrip\nJOIN s29826.Country C on CT.IdCountry = C.IdCountry";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    string country = reader.GetString(reader.GetOrdinal("C"));

                    if (!dict.TryGetValue(idOrdinal, out var dto))
                    {
                        dto = new TripDTO()
                        {
                            Id = idOrdinal,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            Countries = new List<CountryDTO>()
                        };
                        dict.Add(idOrdinal, dto);
                    }
                    dto.Countries.Add(new CountryDTO {Name = country});
                }

            }

        }
        

        return dict.Values.ToList();
    }

    public async Task<List<TripDTO>> GetTrip(int id)
    {
        var dict = new Dictionary<int, TripDTO>();

        string sql = "SELECT Trip.IdTrip, Trip.Name,  Description, DateFrom, DateTo, MaxPeople, C2.Name AS C, RegisteredAt, PaymentDate\nFROM Trip\nJOIN s29826.Client_Trip CT on Trip.IdTrip = CT.IdTrip\nJOIN s29826.Client C on C.IdClient = CT.IdClient\nJOIN s29826.Country_Trip T on Trip.IdTrip = T.IdTrip\nJOIN s29826.Country C2 on C2.IdCountry = T.IdCountry\nWHERE C.IdClient = @id";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    string country = reader.GetString(reader.GetOrdinal("C"));

                    if (!dict.TryGetValue(tripId, out var dto))
                    {
                        dto = new TripDTO()
                        {
                            Id = tripId,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            RegisteredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
                            PaymentDate = reader.GetInt32(reader.GetOrdinal("PaymentDate")),
                            Countries = new List<CountryDTO>()
                        };
                        dict.Add(tripId, dto);
                    }
                    dto.Countries.Add(new CountryDTO {Name = country});
                }
            }
        }


        return dict.Values.ToList();
    }

    public async Task AddClientToTrip(int idClient, int idTrip)
    {
        string sql = "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)\nVALUES (@IdClient, @IdTrip, CONVERT(int, GETDATE()), CONVERT(int, GETDATE()))";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@IdClient", idClient);
            cmd.Parameters.AddWithValue("@IdTrip", idTrip);

            await conn.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
        }
    }
}