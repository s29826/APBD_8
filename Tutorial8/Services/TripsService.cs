using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var dict = new Dictionary<int, TripDTO>();
        
        //Pobieram wszstkie informacje o wycieczce oraz za pomocą JOIN także o krajach do niej przypisanych
        string command = "SELECT Trip.IdTrip, Trip.Name, Description, DateFrom, DateTo, MaxPeople, C.Name AS C\n" +
                         "FROM Trip\n" +
                         "JOIN s29826.Country_Trip CT on Trip.IdTrip = CT.IdTrip\n" +
                         "JOIN s29826.Country C on CT.IdCountry = C.IdCountry";
        
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

    public async Task<List<TripMainDTO>> GetTrip(int clientId)
    {
        var dict = new Dictionary<int, TripMainDTO>();

        //Pobieram wszystkie informację o wycieczce, a także o krajach, które odwiedzi, ale także informację o rejestracji
        //klienta na wycieczkę oraz szukam odpowieniego ID klienta, stąd tyle JOINów
        string sql = "SELECT Trip.IdTrip, Trip.Name,  Description, DateFrom, DateTo, MaxPeople, C2.Name AS C, RegisteredAt, PaymentDate\n" +
                     "FROM Trip\n" +
                     "JOIN s29826.Client_Trip CT on Trip.IdTrip = CT.IdTrip\n" +
                     "JOIN s29826.Client C on C.IdClient = CT.IdClient\n" +
                     "JOIN s29826.Country_Trip T on Trip.IdTrip = T.IdTrip\n" +
                     "JOIN s29826.Country C2 on C2.IdCountry = T.IdCountry\n" +
                     "WHERE C.IdClient = @id";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@id", clientId);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    string country = reader.GetString(reader.GetOrdinal("C"));

                    if (!dict.TryGetValue(tripId, out var dto))
                    {
                        dto = new TripMainDTO()
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

    public async Task<bool> DoesTripExist(int tripId)
    {
        //Zwracam liczbę wycieczek, aby sprawdzić, czy taka istnieje dla danego klienta
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Client_Trip\n" +
                     "WHERE IdClient = @id";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@id", tripId);
            
            await conn.OpenAsync();
            
            var result = await cmd.ExecuteScalarAsync();
            int val = Convert.ToInt32(result);
            
            
            return val > 0;
        }
    }

    public async Task<bool> IsTripFull(int tripId)
    {
       await using var conn = new SqlConnection(_connectionString);
       await conn.OpenAsync();

       //Pobieram aktualną liczbę osób zapisanych na daną wycieczkę
       await using var cmdCounter = new SqlCommand("SELECT COUNT(*)\n" +
                                                   "FROM Client_Trip\n" +
                                                   "WHERE IdTrip = @id", conn);
       cmdCounter.Parameters.AddWithValue("@id", tripId);
       int counter = Convert.ToInt32(await cmdCounter.ExecuteScalarAsync());

       //Pobieram maksymalną dopuszczalną liczbę osób na daną wycieczkę
       await using var cmdFull = new SqlCommand("SELECT MaxPeople\n" +
                                                "FROM Trip\n" +
                                                "WHERE IdTrip = @id", conn);
       cmdFull.Parameters.AddWithValue("@id", tripId);
       int full = Convert.ToInt32(await cmdFull.ExecuteScalarAsync());


       return counter >= full;
    }

    public async Task<bool> DoesRegistartionExist(int clientId, int tripId)
    {
        //Zwracam liczbę rejestracji, aby sprawdzić, czy taka istnieje
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Client_Trip\n" +
                     "WHERE IdTrip = @idTrip AND IdClient = @idClient;";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@idTrip", tripId);
            cmd.Parameters.AddWithValue("@idClient", clientId);

            await conn.OpenAsync();

            int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            
            
            return result > 0;
        }
    }

    public async Task DeleteClientFromTrip(int idClient, int idTrip)
    {
        //Usuwam klienta z danej wycieczki uzywając ID przekazanych w argumentach
        string sql = "DELETE FROM Client_Trip\n" +
                     "WHERE IdClient = @idClient AND IdTrip = @idTrip";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@idClient", idClient);
            cmd.Parameters.AddWithValue("@idTrip", idTrip);

            await conn.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
        }
    }
}