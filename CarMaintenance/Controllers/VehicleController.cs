using Domain.DTO.Vehicle;
using Microsoft.AspNetCore.Mvc;
using Services.IReposetory;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleRepo _vehicleRepo;

        public VehicleController(IVehicleRepo vehicleRepo)
        {
            _vehicleRepo = vehicleRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _vehicleRepo.GetAllVehiclesAsync();
            return Ok(vehicles);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicle = await _vehicleRepo.GetVehicleByIdAsync(id);
            if (vehicle == null)
                return NotFound(new { message = $"Vehicle with ID {id} not found" });

            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle([FromBody] AddVehicleDTO vehicleDto)
        {
            if (vehicleDto == null)
                return BadRequest("Vehicle data is required");

            await _vehicleRepo.AddVehicleAsync(vehicleDto);

            return Ok(new { message = "Vehicle added successfully and linked to model" });
        }


        //PUT: api/vehicle/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] AddVehicleDTO vehicleDto)
        {
            if (vehicleDto == null)
                return BadRequest("Vehicle data is required");

            await _vehicleRepo.UpdateVehicleAsync(id, vehicleDto);
            return Ok(new { message = "Vehicle updated successfully" });
        }

        // DELETE: api/vehicle/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            await _vehicleRepo.DeleteVehicleAsync(id);
            return Ok(new { message = "Vehicle deleted successfully" });
        }
    }
}
