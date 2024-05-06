

using Probny_Kolos.Models.DTOs;

namespace Probny_Kolos.Repositories;


public interface IAnimalsRepository
{
    Task<bool> DoesAnimalExist(int id);
    Task<bool> DoesOwnerExist(int id);
    Task<bool> DoesProcedureExist(int id);
    Task<AnimalDetails> GetAnimal(int id);
    Task AddNewAnimalWithProcedures(NewAnimalWithProcedures newAnimalWithProcedures);
}