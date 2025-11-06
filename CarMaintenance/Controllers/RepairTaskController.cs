using Domain.DTO.Part;
using Domain.DTO.Repair_Task;
using Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.IReposetory;
using Services.Repository;

namespace CarMaintenance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairTaskController : ControllerBase
    {
        private readonly IRepairTaskRepo repairTaskRepo;

        public RepairTaskController(IRepairTaskRepo repairTaskRepo)
        {
            this.repairTaskRepo = repairTaskRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await repairTaskRepo.GetAllRepairTask();
            return Ok(tasks);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await repairTaskRepo.GetRepairTaskById(id);
            if (task == null)
                return NotFound(new { message = $"Task with ID {id} not found" });

            return Ok(task);
        }
        [HttpPost]
        public async Task<IActionResult> CreateRepairTask(AddRepairTaskWithPartDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Data Is Required");
            await repairTaskRepo.CreateRepairTask(request);

            return Ok(new { message = "Repair Task added successfully and its Parts" });
        }
        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateRepairTask(int taskId, AddRepairTaskWithPartDTO request)
        {
            
            await repairTaskRepo.UpdateRepairTask(taskId, request);
            return Ok(new { message = "Task updated successfully" });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            await repairTaskRepo.DeleteRepairTask(id);
            return Ok(new { message = "Task deleted successfully" });
        }

    }
}
