using Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.IReposetory;

namespace CarMaintenance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleModelController : ControllerBase
    {
        private readonly IVehicleModelRepo _vehicleModelRepo;

        public VehicleModelController(IVehicleModelRepo vehicleModelRepo)
        {
            _vehicleModelRepo = vehicleModelRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllModels()
        {
            var models = await _vehicleModelRepo.GetAllModelsAsync();
            return Ok(models);
        }
        [HttpGet("ModelsByMake/{id}")]
        public async Task<IActionResult> GetModelsByVehcile(int id)
        {
            var models = await _vehicleModelRepo.GetModelsByMakeId(id);
            return Ok(models);
        }

        [HttpPost]
        public async Task<IActionResult> AddModel([FromBody] AddVehicleModelDTO modelDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _vehicleModelRepo.AddModelAsync(modelDTO);
            return Ok(new { Message = "Added model successfuly" });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateModel(int id, [FromBody] VehicleModelDTO modelDTO)
        {
            await _vehicleModelRepo.UpdateModelAsync(id, modelDTO);
            return Ok(new { message = "Vehicle model updated successfully." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteModel(int id)
        {
            await _vehicleModelRepo.DeleteModelAsync(id);
            return Ok(new { message = "Vehicle model deleted successfully." });
        }
    }
}

