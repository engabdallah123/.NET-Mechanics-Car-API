using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.IReposetory;
using Services.Repository;

namespace CarMaintenance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepo scheduleRepo;

        public ScheduleController(IScheduleRepo scheduleRepo)
        {
            this.scheduleRepo = scheduleRepo;
        }
        [HttpGet("today")]
        public async Task<IActionResult> GetOrCreateScheduleForToday()
        {
            try
            {
                var result = await scheduleRepo.GetOrCreateScheduleForTodayAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("todayWithOrder")]
        public async Task<IActionResult> GetScheduleForTodayWithOrder(DateTime date)
        {
            try
            {
                var result = await scheduleRepo.GetScheduleDayWithWorkOrdersAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        

        [HttpGet("stations")]
        public async Task<IActionResult> GetTodaySchedule()
        {
            try
            {
                var result = await scheduleRepo.GetTodayScheduleAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("reschedule")]
        public async Task<IActionResult> ReScheduleWorkOrder([FromBody] ReScheduleRequest request)
        {
            try
            {
                var result = await scheduleRepo.ReScheduleWorkOrderAsync(
                    request.WorkOrderId,
                    request.NewSlotIds,
                    request.NewTechnicianId
                );

                return Ok(new { message = "Work order rescheduled successfully", success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    // DTO for reschedule request
    public class ReScheduleRequest
    {
        public int WorkOrderId { get; set; }
        public List<int> NewSlotIds { get; set; } = new();
        public string? NewTechnicianId { get; set; }
    }

}

