using Domain.DTO.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.IReposetory;
using Services.Repository;

namespace CarMaintenance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepo customerRepo;

        public CustomerController(ICustomerRepo customerRepo)
        {
            this.customerRepo = customerRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            try
            {
                var customers = await customerRepo.GetAllCustomer();

                if (customers == null || !customers.Any())
                    return NotFound("No customers found.");

                return Ok(customers);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} while retrieving customers.");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            try
            {
                var customer = await customerRepo.GetCustomerById(id);
                if (customer == null)
                    return NotFound($"Customer with ID {id} not found.");
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} while retrieving customer with ID {id}.");
            }
        }
        [HttpGet("CustomerWithVecicle/{id}")]
        public async Task<IActionResult> GetCustomerWithVecicle(int id)
        {
            try
            {
                var customer = await customerRepo.GetCustomerWithVehicles(id);
                if (customer == null)
                    return NotFound($"Customer with ID {id} not found.");
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} while retrieving customer with ID {id}.");
            }
        }
        [HttpGet("AllCustomerWithVecicle")]
        public async Task<IActionResult> GetAllCustomerWithVecicle()
        {
            try
            {
                var customer = await customerRepo.GetAllCustomerWithVehicles();
                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} while retrieving customers.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerDTO customerDto)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    await customerRepo.AddCustomer(customerDto);
                    return Created();
                }
                return BadRequest("Invalid customer data.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} while adding a new customer.");
            }
        }
        [HttpPost("AddCustomerWithVehicle")]
        public async Task<IActionResult> AddCustomerWithVehicle([FromBody] AddCustomerWithVehicleDTO dto)
        {
            if (dto == null)
                return BadRequest("Data is required.");

            await customerRepo.AddCustomerWithVehicleAsync(dto);
            return Ok(new { message = "Customer, Vehicle, and Model added successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                await customerRepo.DeleteCustomer(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} while deleting customer with ID {id}.");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] AddCustomerWithVehicleDTO Dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await customerRepo.UpdateCustomerWithVehicle(id, Dto);
                    return NoContent();
                }
                return BadRequest("Invalid customer data.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} while updating customer with ID {id}.");
            }
        }
    }
}
