using Microsoft.AspNetCore.Mvc;
using Probny_Kolos.Repositories;
using Probny_Kolos.Models.DTOs;

namespace Probny_Kolos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WetClinicController : ControllerBase
    {
        private readonly IAnimalsRepository _animalsRepository;
        public WetClinicController(IAnimalsRepository animalsRepository)
        {
            _animalsRepository = animalsRepository;
        }

        [HttpGet("{idAnimal}")]
        public async Task<IActionResult> GetAnimal(int idAnimal)
        {
            if (!await _animalsRepository.DoesAnimalExist(idAnimal))
                return NotFound($"Animal with given ID - {idAnimal} doesn't exist");
            
            var animal = await _animalsRepository.GetAnimal(idAnimal);

            return Ok(animal);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddAnimal(NewAnimalWithProcedures newAnimalWithProcedures)
        {
            if (!await _animalsRepository.DoesOwnerExist(newAnimalWithProcedures.OwnerId))
                return NotFound($"Owner with given ID - {newAnimalWithProcedures.OwnerId} doesn't exist");

            foreach (var procedure in newAnimalWithProcedures.Procedures)
            {
                if (!await _animalsRepository.DoesProcedureExist(procedure.ProcedureId))
                    return NotFound($"Procedure with given ID - {procedure.ProcedureId} doesn't exist");
            }

            await _animalsRepository.AddNewAnimalWithProcedures(newAnimalWithProcedures);

            return Created(Request.Path.Value ?? "api/animals", newAnimalWithProcedures);
        }
        
    }
    
}
