
using Microsoft.Data.SqlClient;
using Probny_Kolos.Models.DTOs;

namespace Probny_Kolos.Repositories;

public class AnimalsRepository : IAnimalsRepository
{
    private readonly IConfiguration _configuration;
    public AnimalsRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> DoesAnimalExist(int id)
    {
        var query = "SELECT 1 FROM Animal WHERE ID = @ID";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesOwnerExist(int id)
    {
        var query = "SELECT 1 FROM Owner WHERE ID = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesProcedureExist(int id)
    {
        var query = "SELECT 1 FROM [Procedure] WHERE ID = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<AnimalDetails> GetAnimal(int idAnimal)
    {
        AnimalDetails animalDetails = null;

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
            {
                connection.Open();

                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = @"
                    SELECT 
                        A.ID AS Animal_ID,
                        A.Name AS Animal_Name,
                        A.Type AS Animal_Type,
                        A.AdmissionDate AS Animal_AdmissionDate,
                        O.ID AS Owner_ID,
                        O.FirstName AS Owner_FirstName,
                        O.LastName AS Owner_LastName,
                        P.Name AS Procedure_Name,
                        P.Description AS Procedure_Description,
                        PA.Date AS Procedure_Date
                    FROM 
                        Animal A
                    JOIN 
                        Owner O ON A.Owner_ID = O.ID
                    JOIN 
                        Procedure_Animal PA ON A.ID = PA.Animal_ID
                    JOIN 
                        [Procedure] P ON PA.Procedure_ID = P.ID
                    WHERE 
                        A.ID = @AnimalID";

                command.Parameters.AddWithValue("@AnimalID", idAnimal);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        animalDetails = new AnimalDetails
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("Animal_ID")),
                            Name = reader.GetString(reader.GetOrdinal("Animal_Name")),
                            Type = reader.GetString(reader.GetOrdinal("Animal_Type")),
                            AdmissionDate = reader.GetDateTime(reader.GetOrdinal("Animal_AdmissionDate")),
                            Owner = new OwnerDetails
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("Owner_ID")),
                                FirstName = reader.GetString(reader.GetOrdinal("Owner_FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("Owner_LastName"))
                            },
                            Procedures = new List<ProcedureDetails>()
                        };

                        do
                        {
                            animalDetails.Procedures.Add(new ProcedureDetails
                            {
                                Name = reader.GetString(reader.GetOrdinal("Procedure_Name")),
                                Description = reader.GetString(reader.GetOrdinal("Procedure_Description")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Procedure_Date"))
                            });
                        } while (reader.Read());
                    }
                }
            }

            if (animalDetails == null)
            {
                throw new Exception();
            }

            return animalDetails;
    }

    public async Task AddNewAnimalWithProcedures(NewAnimalWithProcedures newAnimalWithProcedures)
    {
        var insert = @"INSERT INTO Animal VALUES(@Name, @Type, @AdmissionDate, @OwnerId);
					   SELECT @@IDENTITY AS ID;";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
	    
        command.Connection = connection;
        command.CommandText = insert;
        
        command.Parameters.AddWithValue("@Name", newAnimalWithProcedures.Name);
        command.Parameters.AddWithValue("@Type", newAnimalWithProcedures.Type);
        command.Parameters.AddWithValue("@AdmissionDate", newAnimalWithProcedures.AdmissionDate);
        command.Parameters.AddWithValue("@OwnerId", newAnimalWithProcedures.OwnerId);
        
        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
	    
        try
        {
            var id = await command.ExecuteScalarAsync();
    
            foreach (var procedure in newAnimalWithProcedures.Procedures)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Procedure_Animal VALUES(@ProcedureId, @AnimalId, @Date)";
                command.Parameters.AddWithValue("@ProcedureId", procedure.ProcedureId);
                command.Parameters.AddWithValue("@AnimalId", id);
                command.Parameters.AddWithValue("@Date", procedure.Date);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}