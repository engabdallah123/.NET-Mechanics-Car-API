using Domain.DTO.Part;
using Domain.DTO.Repair_Task;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Services.IReposetory;
using Services.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing.Main
{
    public class RepairTaskRepoTests
    {
        [Fact]
        public async Task CreateRepairTask_ShouldCreateTaskWithPartsByTransaction_ByAddTaskAndAddPartShouldAndCreateRepairTask_ShouldTheCountBe2()
        {
            var context = new InMemoryDBContext();
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var task = new AddRepairTaskDTO
            {
                Name = "Tair Rotation",
                Description = "Description",
                Duration = 30,
                LaborCost = 40,
            };
            await taskRepo.AddTask(task);
            var taskId = context.RepairTasks.First().Id;

            var part = new AddPartDTO
            {
                Name = "Valve",
                Quantity = 2,
                UnitPrice = 20
                
            };
            await partRepo.AddPart(part,taskId);

            // Act
            var taskWithPart = new AddRepairTaskWithPartDTO
            {
               // Parts = part,
                taskDTO = task
            };
            await taskRepo.CreateRepairTask(taskWithPart);

            // Assert
            Assert.Equal(2, context.Parts.Count());
            Assert.Equal(2, context.RepairTasks.Count());
            Assert.Equal(context.RepairTasks.First().Id, context.Parts.First().TaskId);
        }

        [Fact]
        public async Task UpdateRepairTask_ShouldUpdateTaskWithPartsByTransaction()
        {
            var context = new InMemoryDBContext();
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var task = new AddRepairTaskDTO
            {
                Name = "Tair Rotation",
                Description = "Description",
                Duration = 30,
                LaborCost = 40,
            };

            var part = new AddPartDTO
            {
                Name = "Valve",
                Quantity = 2,
                UnitPrice = 20

            };

            var taskWithPart = new AddRepairTaskWithPartDTO
            {
               // partDTO = part,
                taskDTO = task
            };
            await taskRepo.CreateRepairTask(taskWithPart);
            var taskId = context.RepairTasks.First().Id;
            var Updatetask = new AddRepairTaskDTO{ LaborCost = 70 };
            var Updatepart = new AddPartDTO{ Quantity = 3 };
            var updateTaskWithPart = new AddRepairTaskWithPartDTO
            {
               // partDTO = part,
                taskDTO = task
            };
            // Act
            await taskRepo.UpdateRepairTask(taskId, updateTaskWithPart);

            // Assert
            var updatedRepairTask = await context.RepairTasks
                                .Include(v => v.Parts)
                                .FirstAsync(v => v.Id == taskId);
            var updatedPart = await context.Parts.Where(p => p.TaskId == taskId).FirstAsync();
            Assert.Equal(70, updatedRepairTask.LaborCost);
            Assert.Equal(30, updatedRepairTask.EstimatedDuration); // unchanged
            Assert.Equal("Tair Rotation", updatedRepairTask.Name); // unchanged
           

            Assert.Equal(3, updatedPart.Quantity); 
            Assert.Equal("Valve", updatedPart.Name); // unchanged 
   
        }
        [Fact]
        public async Task GetAllRepairTask_ShouldReturnAllTasksWithParts()
        {
            var context = new InMemoryDBContext();
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var task1 = new AddRepairTaskDTO
            {
                Name = "Tair Rotation",
                Description = "Descriptionc one",
                Duration = 30,
                LaborCost = 40,
            };

            var part1 = new AddPartDTO
            {
                Name = "Valve",
                Quantity = 2,
                UnitPrice = 20

            };
            var taskWithPart1 = new AddRepairTaskWithPartDTO
            {
               // partDTO = part1,
                taskDTO = task1
            };
           
            var task2 = new AddRepairTaskDTO
            {
                Name = "Spark Plug Replacement",
                Description = "Description tow",
                Duration = 60,
                LaborCost = 100,
            };

            var part2 = new AddPartDTO
            {
                Name = "Spark Plug",
                Quantity = 3,
                UnitPrice = 90

            };
            var taskWithPart2 = new AddRepairTaskWithPartDTO
            {
               // partDTO = part2,
                taskDTO = task2
            };
            await taskRepo.CreateRepairTask(taskWithPart1);
            await taskRepo.CreateRepairTask(taskWithPart2);

            // Act 
          var result = await taskRepo.GetAllRepairTask();

            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Description == "Descriptionc one");
            Assert.Contains(result, c => c.Parts.Any(p => p.Quantity == part2.Quantity));
        }
    }
}
