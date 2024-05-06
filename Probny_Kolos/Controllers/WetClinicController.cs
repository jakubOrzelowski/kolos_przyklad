using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Probny_Kolos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WetClinicController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public WetClinicController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetAnimal(int idAnimal)
    {
        List<AnimalDetails> animalDetails = new List<AnimalDetails>();

        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();

        SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText =
            "SELECT \n    A.ID AS Animal_ID,\n    A.Name AS Animal_Name,\n    A.Type AS Animal_Type,\n    A.AdmissionDate AS Animal_AdmissionDate,\n    O.ID AS Owner_ID,\n    O.FirstName AS Owner_FirstName,\n    O.LastName AS Owner_LastName,\n    P.Name AS Procedure_Name,\n    P.Description AS Procedure_Description,\n    PA.Date AS Procedure_Date\nFROM \n    Animal A\nJOIN \n    Owner O ON A.Owner_ID = O.ID\nJOIN \n    Procedure_Animal PA ON A.ID = PA.Animal_ID\nJOIN \n    [Procedure] P ON PA.Procedure_ID = P.ID\nWHERE \n    A.ID = @AnimalID;\n";
        command.Parameters.AddWithValue("@AnimalID", idAnimal);

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                AnimalDetails details = new AnimalDetails
                {
                    Animal_ID = reader.GetInt32(reader.GetOrdinal("Animal_ID")),
                    Animal_Name = reader.GetString(reader.GetOrdinal("Animal_Name")),
                    Animal_Type = reader.GetString(reader.GetOrdinal("Animal_Type")),
                    Animal_AdmissionDate = reader.GetDateTime(reader.GetOrdinal("Animal_AdmissionDate")),
                    Owner_ID = reader.GetInt32(reader.GetOrdinal("Owner_ID")),
                    Owner_FirstName = reader.GetString(reader.GetOrdinal("Owner_FirstName")),
                    Owner_LastName = reader.GetString(reader.GetOrdinal("Owner_LastName")),
                    Procedure_Name = reader.GetString(reader.GetOrdinal("Procedure_Name")),
                    Procedure_Description = reader.GetString(reader.GetOrdinal("Procedure_Description")),
                    Procedure_Date = reader.GetDateTime(reader.GetOrdinal("Procedure_Date"))
                };
                animalDetails.Add(details);
            }
        }



        return Ok(animalDetails);
    }


    public class AnimalDetails
    {
        public int Animal_ID { get; set; }
        public string Animal_Name { get; set; }
        public string Animal_Type { get; set; }
        public DateTime Animal_AdmissionDate { get; set; }
        public int Owner_ID { get; set; }
        public string Owner_FirstName { get; set; }
        public string Owner_LastName { get; set; }
        public string Procedure_Name { get; set; }
        public string Procedure_Description { get; set; }
        public DateTime Procedure_Date { get; set; }
    }
}