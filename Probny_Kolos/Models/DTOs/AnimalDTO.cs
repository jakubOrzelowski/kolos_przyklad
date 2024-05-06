namespace Probny_Kolos.Models.DTOs;

public class AnimalDetails
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public DateTime AdmissionDate { get; set; }
    public OwnerDetails Owner { get; set; }
    public List<ProcedureDetails> Procedures { get; set; }
}

public class OwnerDetails
{
    public int ID { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class ProcedureDetails
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
}