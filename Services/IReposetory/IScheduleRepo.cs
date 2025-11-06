using Domain.DTO.Schedule_Day;
using Domain.DTO.Work_Station;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IReposetory
{
    public interface IScheduleRepo
    {
        Task<ScheduleDayDTO> GetOrCreateScheduleForTodayAsync();
       Task<ScheduleDayDTO> GetScheduleDayWithWorkOrdersAsync(DateTime date);
        Task<ScheduleDay> GenerateDayScheduleAsync(DateTime date);
        Task<List<WorkStationScheduleDTO>> GetTodayScheduleAsync();
        Task<bool> ReScheduleWorkOrderAsync(int workOrderId, List<int> newSlotIds, string? newTechnicianId = null);
    }
}
