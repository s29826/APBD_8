using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";

    public async Task<int> CreateClient([FromBody] ClientDTO dto)
    {
        //Dodaję klienta za pomocą danych przekazanych w argumencie oraz OUTPUTem zwracam jego nowe ID
        string sql = "INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)\n" +
                     "OUTPUT inserted.IdClient\n" +
                     "VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", dto.FirstName);
            cmd.Parameters.AddWithValue("@LastName", dto.LastName);
            cmd.Parameters.AddWithValue("@Email", dto.Email);
            cmd.Parameters.AddWithValue("@Telephone", dto.Telephone);
            cmd.Parameters.AddWithValue("@Pesel", dto.Pesel);

            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();
            
            
            return Convert.ToInt32(result);
        }
    }

    public async Task<bool> DoesClientExist(int clientId)
    {
        //Zwracam liczbę klientów, aby sprawdzić, czy taki istnieje
        string sql = "SELECT COUNT(*)\n" +
                     "FROM Client\n" +
                     "WHERE IdClient = @id";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@id", clientId);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            
            
            return Convert.ToInt32(result) > 0;
        }
    }

    public async Task AddClientToTrip(int idClient, int idTrip)
    {
        //Dodaję klienta do wycieczki przekazując w arguymentach jego ID oraz ID wycieczki, a datę rejestracji
        //oraz płatności konwertuję na Inta, ponieważ w takiej postaci są w bazie
        string sql = "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)\n" +
                     "VALUES (@IdClient, @IdTrip, CONVERT(int, GETDATE()), CONVERT(int, GETDATE()))";
        
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