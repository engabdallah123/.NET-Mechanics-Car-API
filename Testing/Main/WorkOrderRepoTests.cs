using Domain.DTO.Part;
using Domain.DTO.Repair_Task;
using Domain.DTO.Work_Order;
using Domain.Enum;
using Domain.Model;
using FakeItEasy;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Services.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing.Main
{
    public class WorkOrderRepoTests
    {
        [Fact]
        public async Task CreateWorkOrderInSlotAsync_ShouldBeCreated()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

                Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;

           
            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
               
                
            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

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
              //  partDTO = part,
                taskDTO = task
            };
            await taskRepo.CreateRepairTask(taskWithPart);
            var taskId = context.RepairTasks.First().Id;

            var dto = new AddWorkOrderDTO
            {
                SlotId = slot.Id,
                CustomerId = customer.Id,
                TechnicianId = technical.Id,
                VehicleId = vehicle.Id,
                RepairTasks = new List<AddRepairTaskToOrderDTO>
                {
                    new AddRepairTaskToOrderDTO
                    {
                        RepairTaskId = taskId,
                        
                    }
                }
            };

            // Act
             await workOrderRepo.CreateWorkOrderInSlotAsync(dto);
            var createdOrder = context.WorkOrders.First();

            // Assert
            Assert.NotNull(createdOrder);
            Assert.Equal(dto.CustomerId, createdOrder.CustomerId);
            Assert.Equal(dto.VehicleId, createdOrder.VehicleId);
            Assert.Equal(dto.TechnicianId, createdOrder.TechnicianId);

            // Slot should now be unavailable and linked to WorkOrder
            var updatedSlot = await context.ScheduleSlots.FirstAsync(s => s.Id == slot.Id);
            Assert.False(updatedSlot.IsAvailable);
            Assert.Equal(createdOrder.Id, updatedSlot.WorkOrderId);

            // Check that the repair tasks are linked correctly
            var linkedTasks = await context.WorkOrderRepairTasks
                .Where(t => t.WorkOrderId == createdOrder.Id)
                .ToListAsync();

            Assert.Equal(1, linkedTasks.Count);
            Assert.Contains(linkedTasks, t => t.RepairTaskId == taskId);     
        }

        [Fact]
        public async Task ExtendWorkOrderAsync_ShouldExtendWorkOrder_WhenEnoughSlotsAvailable()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

            Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;


            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
                

            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

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

            var dto = new AddWorkOrderDTO
            {
                SlotId = slot.Id,
                CustomerId = customer.Id,
                TechnicianId = technical.Id,
                VehicleId = vehicle.Id,
                RepairTasks = new List<AddRepairTaskToOrderDTO>
                {
                    new AddRepairTaskToOrderDTO
                    {
                        RepairTaskId = taskId,

                    }
                },
                
            };
            await workOrderRepo.CreateWorkOrderInSlotAsync(dto);
            var workOrder = context.WorkOrders.First();
            var initialEndTime = workOrder.EndTime;
            // Act: extend by 2 more slots
            var result = await workOrderRepo.ExtendWorkOrderAsync(workOrder.Id, additionalSlots: 2);

            // Assert
            Assert.True(result);
            // Check that WorkOrder end time extended correctly (30 minutes more)
            var updatedWorkOrder = await context.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
            Assert.Equal(initialEndTime.AddMinutes(15 * 2), updatedWorkOrder.EndTime);

            // Check that 3 slots are now linked to the WorkOrder
            var linkedSlots = await context.ScheduleSlots
                .Where(s => s.WorkOrderId == workOrder.Id)
                .ToListAsync();

            Assert.Equal(3, linkedSlots.Count);
            Assert.All(linkedSlots, s => Assert.False(s.IsAvailable));
        }

        [Fact]
        public async Task UpdateWorkOrderStatusesAutomaticAsync_ShouldUpdateStatusesCorrectly()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

            Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;


            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
               

            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

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

            var now = DateTime.Now;
            // WorkOrders with different times
            var workOrderPending = new WorkOrder
            {
                CustomerId = customerId,
                VehicleId = vehicleId,
                TechnicianId = userId,
                StartTime = now.AddHours(1),  // يبدأ بعد ساعة
                EndTime = now.AddHours(2),
                Status = WorkOrderStatus.Pending
            };
            var workOrderInProgress = new WorkOrder
            {
                CustomerId =customerId,
                VehicleId = vehicleId,
                TechnicianId = userId,
                StartTime = now.AddMinutes(-30), // بدأ قبل 30 دقيقة
                EndTime = now.AddMinutes(30),    // ينتهي بعد 30 دقيقة
                Status = WorkOrderStatus.Pending
            };
            var workOrderCompleted = new WorkOrder
            {
                CustomerId = customerId,
                VehicleId = vehicleId,
                TechnicianId = userId,
                StartTime = now.AddHours(-2),   // بدأ قبل ساعتين
                EndTime = now.AddHours(-1),     // انتهى قبل ساعة
                Status = WorkOrderStatus.InProgress
            };
            context.WorkOrders.AddRange(workOrderPending, workOrderInProgress, workOrderCompleted);
            await context.SaveChangesAsync();

            // Act
            await workOrderRepo.UpdateWorkOrderStatusesAutomaticAsync();

            // Assert
            var updatedOrders = await context.WorkOrders.ToListAsync();

            var pending = updatedOrders.First(o => o.Id == workOrderPending.Id);
            var inProgress = updatedOrders.First(o => o.Id == workOrderInProgress.Id);
            var completed = updatedOrders.First(o => o.Id == workOrderCompleted.Id);

            Assert.Equal(WorkOrderStatus.Pending, pending.Status);
            Assert.Equal(WorkOrderStatus.InProgress, inProgress.Status);
            Assert.Equal(WorkOrderStatus.Completed, completed.Status);

        }

        [Fact]
        public async Task UpdateWorkOrderStatusManualAsync_ShouldUpdate_WhenAuthorized()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

            Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;


            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
               

            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

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
                //partDTO = part,
                taskDTO = task
            };
            await taskRepo.CreateRepairTask(taskWithPart);
            var taskId = context.RepairTasks.First().Id;

            var workOrder = new WorkOrder
            {
                CustomerId = customerId,
                VehicleId = vehicleId,
                TechnicianId = userId,
                Status = WorkOrderStatus.Pending
            };
            context.WorkOrders.Add(workOrder);
            await context.SaveChangesAsync();

            // Act
            var result = await workOrderRepo.UpdateWorkOrderStatusManualAsync(workOrder.Id, userId, WorkOrderStatus.InProgress);

            // Assert
            Assert.True(result);
            var updatedOrder = await context.WorkOrders.FirstAsync(w => w.Id == workOrder.Id);
            Assert.Equal(WorkOrderStatus.InProgress, updatedOrder.Status);
        }
        [Fact]
        public async Task UpdateWorkOrderStatusManualAsync_ShouldThrowUnauthorized_WhenWrongTechnician()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

            Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;


            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
                

            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

            var technical2 = new AppUser
            {
                UserName = "Test2",
            };
            context.AppUsers.Add(technical2);
            await context.SaveChangesAsync();
            var userId2 = technical2.Id;

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
                //partDTO = part,
                taskDTO = task
            };
            await taskRepo.CreateRepairTask(taskWithPart);
            var taskId = context.RepairTasks.First().Id;

            var workOrder = new WorkOrder
            {
                CustomerId = customerId,
                VehicleId = vehicleId,
                TechnicianId = userId,
                Status = WorkOrderStatus.Pending
            };
            context.WorkOrders.Add(workOrder);
            await context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await workOrderRepo.UpdateWorkOrderStatusManualAsync(workOrder.Id, userId2, WorkOrderStatus.InProgress);
            });
        }

        [Fact]
        public async Task UpdateWorkOrderStatusManualAsync_ShouldThrowInvalidOperation_WhenInvalidStatus()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

            Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;


            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
               

            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

            var technical2 = new AppUser
            {
                UserName = "Test2",
            };
            context.AppUsers.Add(technical2);
            await context.SaveChangesAsync();
            var userId2 = technical2.Id;

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
                //partDTO = part,
                taskDTO = task
            };
            await taskRepo.CreateRepairTask(taskWithPart);
            var taskId = context.RepairTasks.First().Id;

            var workOrder = new WorkOrder
            {
                CustomerId = customerId,
                VehicleId = vehicleId,
                TechnicianId = userId,
                Status = WorkOrderStatus.Pending
            };
            context.WorkOrders.Add(workOrder);
            await context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await workOrderRepo.UpdateWorkOrderStatusManualAsync(workOrder.Id, userId, (WorkOrderStatus)999);
            });
        }

        [Fact]
        public async Task GetWorkOrderDetailsAsync_ShouldReturnCorrectDTO()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

            Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;


            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
                

            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

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
                //partDTO = part,
                taskDTO = task
            };
            await taskRepo.CreateRepairTask(taskWithPart);
            var taskId = context.RepairTasks.First().Id;

            var dto = new AddWorkOrderDTO
            {
                SlotId = slot.Id,
                CustomerId = customer.Id,
                TechnicianId = technical.Id,
                VehicleId = vehicle.Id,
                RepairTasks = new List<AddRepairTaskToOrderDTO>
                {
                    new AddRepairTaskToOrderDTO
                    {
                        RepairTaskId = taskId,
                    }
                }
            };

             await workOrderRepo.CreateWorkOrderInSlotAsync(dto);
            var createdOrder = context.WorkOrders.First();

            // Act
            var dtoReturned = await workOrderRepo.GetWorkOrderDetailsAsync(createdOrder.Id);

            // Assert
            Assert.NotNull(dtoReturned);
            Assert.Equal(15, dtoReturned.Duration); // 9:15 - 9:00 = 15 minutes
            Assert.Equal("Pending", dtoReturned.Status);
            Assert.Equal("Test", dtoReturned.TechnicianName);
            Assert.Equal("Station 1", dtoReturned.WorkStationName);

            // Vehicle
            Assert.Equal(vehicle.Id, dtoReturned.Vehicle.Id);
            Assert.Equal("QWE123", dtoReturned.Vehicle.LicensePlate);
            Assert.Equal("Abdallah", dtoReturned.Vehicle.CustomerName);


            // Repair Tasks
            Assert.Single(dtoReturned.RepairTasks);
            var taskDto = dtoReturned.RepairTasks.First();
            Assert.Equal("Tair Rotation", taskDto.Name);
            Assert.Single(taskDto.Parts);
            var partDto = taskDto.Parts.First();
            Assert.Equal("Valve", partDto.Name);
        }

        [Fact]
        public async Task UpdateTechnicianForWorkOrderAsync_WithNewTechnician_ShouldUpdateSuccessfully()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

            Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;


            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
    

            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

            var technical2 = new AppUser
            {
                UserName = "Test2",
            };
            context.AppUsers.Add(technical2);
            await context.SaveChangesAsync();
            var userId2 = technical2.Id;

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
                //partDTO = part,
                taskDTO = task
            };
            await taskRepo.CreateRepairTask(taskWithPart);
            var taskId = context.RepairTasks.First().Id;

            var dto = new AddWorkOrderDTO
            {
                SlotId = slot.Id,
                CustomerId = customer.Id,
                TechnicianId = technical.Id,
                VehicleId = vehicle.Id,
                RepairTasks = new List<AddRepairTaskToOrderDTO>
                {
                    new AddRepairTaskToOrderDTO
                    {
                        RepairTaskId = taskId,
                    }
                }
            };

            await workOrderRepo.CreateWorkOrderInSlotAsync(dto);
            var createdOrder = context.WorkOrders.First();
            // Act
           // await workOrderRepo.UpdateTechnicianForWorkOrderAsync(createdOrder, userId2);

            // Assert
            var updated = await context.WorkOrders.FindAsync(createdOrder.Id);
            Assert.Equal(userId2, updated.TechnicianId);
        }

        [Fact]
        public async Task AddRepairTasksToWorkOrderAsync_ShouldAddNewTasks()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var workOrderRepo = new WorkOrderRepo(context);
            var taskRepo = new RepairTaskRepo(context);
            var partRepo = new PartRepo(context);

            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            context.WorkStations.AddRange(
                      new WorkStation { Name = "Station 1", Code = "ASD" },
                      new WorkStation { Name = "Station 2", Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            var generatedDay = await SchRepo.GenerateDayScheduleAsync(testDate);

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            var slot = await context.ScheduleSlots
                   .Include(s => s.WorkStation)
                    .FirstOrDefaultAsync(s => s.ScheduleDayId == generatedDay.Id && s.IsAvailable);

            Assert.NotNull(slot); // Sure it is available

            var customer = new Customer
            {
                Name = "Abdallah",
                Email = "qqq@m.com",
                PhoneNumber = "1234567890"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var customerId = customer.Id;


            var vehicle = new Vehicle
            {
                CustomerId = customerId,
                LicensePlate = "QWE123",
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            var vehicleId = vehicle.Id;

            var model = new VehicleModel
            {
                Name = "Sunny",
           

            };
            context.VehicleModels.Add(model);
            await context.SaveChangesAsync();

            var technical = new AppUser
            {
                UserName = "Test",
            };
            context.AppUsers.Add(technical);
            await context.SaveChangesAsync();
            var userId = technical.Id;

            var technical2 = new AppUser
            {
                UserName = "Test2",
            };
            context.AppUsers.Add(technical2);
            await context.SaveChangesAsync();
            var userId2 = technical2.Id;

            var task1 = new AddRepairTaskDTO
            {
                Name = "Tair Rotation",
                Description = "Description",
                Duration = 30,
                LaborCost = 40,
            };
            await taskRepo.AddTask(task1);
            var task1Id = context.RepairTasks.First().Id;

            var part1 = new AddPartDTO
            {
                Name = "Valve",
                Quantity = 2,
                UnitPrice = 20

            };
            await partRepo.AddPart(part1, task1Id);

            var dto = new AddWorkOrderDTO
            {
                SlotId = slot.Id,
                CustomerId = customer.Id,
                TechnicianId = technical.Id,
                VehicleId = vehicle.Id,
                RepairTasks = new List<AddRepairTaskToOrderDTO>
                {
                    new AddRepairTaskToOrderDTO
                    {
                        RepairTaskId = task1Id,
                    }
                }
            };

            await workOrderRepo.CreateWorkOrderInSlotAsync(dto);
            var createdOrder = context.WorkOrders.First();

            var task2 = new AddRepairTaskDTO
            {
                Name = "Air Condition",
                Description = "Description2",
                Duration = 20,
                LaborCost = 100,
            };
            await taskRepo.AddTask(task2);
            var task2Id = context.RepairTasks.Where(x => x.Name == "Air Condition").Select(x => x.Id).FirstOrDefault();

            var part2 = new AddPartDTO
            {
                Name = "Air",
                Quantity = 1,
                UnitPrice = 50

            };
            await partRepo.AddPart(part2, task2Id);

            var task3 = new AddRepairTaskDTO
            {
                Name = "task 3",
                Description = "Description3",
                Duration = 40,
                LaborCost = 300,
            };
            await taskRepo.AddTask(task3);
            var task3Id = context.RepairTasks.Where(x => x.Name == "task 3").Select(x => x.Id).FirstOrDefault();

            var part3 = new AddPartDTO
            {
                Name = "part3",
                Quantity = 4,
                UnitPrice = 100

            };
            await partRepo.AddPart(part3,task3Id);


            // Act
            var taskIds = new List<int> { task2Id,task3Id };
            await workOrderRepo.AddRepairTasksToWorkOrderAsync(createdOrder.Id, taskIds);

            // Assert
            var updated = await context.WorkOrders
                .FirstAsync(w => w.Id == createdOrder.Id);

            Assert.Equal(3, updated.WorkOrderRepairTasks.Count);
        }
    }
 }

