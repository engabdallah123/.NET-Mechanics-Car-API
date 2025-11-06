using Domain.DTO.Part;
using Domain.DTO.Repair_Task;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.IReposetory;
using Services.Repository;

namespace CarMaintenance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartController : ControllerBase
    {
        private readonly IPartRepo partRepo;

        public PartController(IPartRepo partRepo)
        {
            this.partRepo = partRepo;
        }
        [HttpPost]
        public async Task<IActionResult> AddPart([FromQuery] int taskId,AddPartDTO partDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest("Data Is Required");
            await partRepo.AddPart(partDTO,taskId);

            return Ok(new { message = "Part added successfully" });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePart(int id)
        {
            await partRepo.DeletePart(id);
            return Ok(new { message = "Part deleted successfully" });
        }

    }
}
