using Domain.DTO.Schedule_Day;
using Domain.DTO.Schedule_Slot;
using Domain.DTO.Work_Station;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Services.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;

namespace Testing.Main
{
    public class ScheduleRepoTests
    {
        [Fact]
        public async Task GenerateDayScheduleAsync_IfTheDayIsNotExist_ShouldBeCreated()
        {
            // Arrang
            var context = new InMemoryDBContext();
            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context,fakeCache);
           

            context.WorkStations.AddRange(
                      new WorkStation {  Name = "Station 1",Code = "ASD" },
                      new WorkStation {  Name = "Station 2",Code = "NJH" }
                 );
            await context.SaveChangesAsync();
            var testDate = new DateTime(2025, 10, 20);

            // Act  
            var result = await SchRepo.GenerateDayScheduleAsync(testDate);


            // Assert
            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == testDate);

            Assert.NotNull(savedDay);                    
            Assert.Equal(testDate, savedDay.Date);         
            Assert.True(savedDay.Slots.Any());     
            Assert.All(savedDay.Slots, s => Assert.True(s.IsAvailable)); 
        }
        [Fact]
        public async Task GetOrCreateScheduleForTodayAsync_IfNoDayExists_ShouldCreateNewOne()
        {
            // Arrange
            var context = new InMemoryDBContext();
            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);




            context.WorkStations.AddRange(
                new WorkStation { Name = "Station A", Code = "A1" },
                new WorkStation {  Name = "Station B", Code = "B1" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await SchRepo.GetOrCreateScheduleForTodayAsync();

            // Assert
            var today = DateTime.Today;

            var savedDay = await context.ScheduleDays
                .Include(d => d.Slots)
                .FirstOrDefaultAsync(d => d.Date == today);

            Assert.NotNull(savedDay);                          // اليوم اتعمل
            Assert.Equal(today, savedDay.Date);                // التاريخ صح
            Assert.NotNull(result);                            // DTO رجع
            Assert.Equal(today, result.Date);                  // التاريخ في الـ DTO مظبوط
            Assert.NotEmpty(result.Stations);                  // فيه WorkStations
            Assert.All(result.Stations, s => Assert.NotEmpty(s.Slots)); // كل محطة فيها Slots
        }

        [Fact]
        public async Task GetOrCreateScheduleForTodayAsync_IfDayExists_ShouldNotCreateNewOne()
        {
            // Arrange
             var context = new InMemoryDBContext();
            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            var existingDay = new ScheduleDay
            {
                Date = DateTime.Today,
                Slots = new List<ScheduleSlot>()
                {
                 new ScheduleSlot
                 {
                    WorkStationId = 1,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(8, 15, 0),
                    IsAvailable = true
                 }
                }
            };

            context.WorkStations.Add(new WorkStation { Id = 1, Name = "Station X", Code = "X1" });
            context.ScheduleDays.Add(existingDay);

            await context.SaveChangesAsync();

            

            // Act
            var result = await SchRepo.GetOrCreateScheduleForTodayAsync();

            // Assert
            var count = await context.ScheduleDays.CountAsync();
            Assert.Equal(1, count); // ما اتعملش يوم جديد
            Assert.Equal(DateTime.Today, result.Date);
            Assert.Single(result.Stations); // فيه محطة واحدة زي ما أضفنا
        }
        [Fact]
        public async Task GetTodayScheduleAsync_ShouldReturnTodaySchedule()
        {
            var context = new InMemoryDBContext();
            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);

            var existingDay = new ScheduleDay
            {
                Date = DateTime.Today,
                Slots = new List<ScheduleSlot>()
                {
                 new ScheduleSlot
                 {
                    WorkStationId = 1,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(8, 15, 0),
                    IsAvailable = true
                 }
                }
            };

            var station = new WorkStation { Id = 1, Name = "Station X", Code = "X1" };
            context.WorkStations.Add(station);
            context.ScheduleDays.Add(existingDay);

            await context.SaveChangesAsync();

            // Act
            var result = await SchRepo.GetTodayScheduleAsync();

            // Assert
            Assert.Single(result); // 1 station
            Assert.Single(result[0].Slots); // 1 slot
            Assert.Equal(station.Name, result[0].WorkStationName);
            Assert.True(result[0].Slots[0].IsAvailable);
        }
        [Fact]
        public async Task ReScheduleWorkOrderAsync_ShouldMoveWorkOrder_ToNewSlots()
        {
            var context = new InMemoryDBContext();
            var fakeCache = A.Fake<IMemoryCache>();
            object outValue;
            A.CallTo(() => fakeCache.TryGetValue(A<object>._, out outValue))
                .Returns(false);
            var SchRepo = new ScheduleRepo(context, fakeCache);


            var day = new ScheduleDay { Date = DateTime.Today };
            var station = new WorkStation { Name = "Station X", Code = "X1" };
            context.WorkStations.Add(station);
            var slots = new List<ScheduleSlot>
            {
               new() {  ScheduleDay = day, StartTime = new(8,0,0), EndTime = new(8,15,0), IsAvailable = false ,WorkStationId = station.Id },
               new() {  ScheduleDay = day, StartTime = new(8,15,0), EndTime = new(8,30,0), IsAvailable = true ,WorkStationId = station.Id },
               new() {  ScheduleDay = day, StartTime = new(8,30,0), EndTime = new(8,45,0), IsAvailable = true , WorkStationId = station.Id},
            };

            var workOrder = new WorkOrder
            {
                TechnicianId = "Tech1",
                Slots = new List<ScheduleSlot> { slots[0] },
                StartTime = day.Date.Add(slots[0].StartTime),
                EndTime = day.Date.Add(slots[0].EndTime)
            };

            context.ScheduleDays.Add(day);
            context.ScheduleSlots.AddRange(slots);
            context.WorkOrders.Add(workOrder);
            await context.SaveChangesAsync();

            // Act
            var result = await SchRepo.ReScheduleWorkOrderAsync(1, new List<int> { 2, 3 }, "Tech2");

            // Assert
            Assert.True(result);
            var updatedOrder = await context.WorkOrders.Include(w => w.Slots).FirstAsync();
            Assert.Equal(2, updatedOrder.Slots.Count);          // بقى مرتبط بـ 2 slot
            Assert.All(updatedOrder.Slots, s => Assert.False(s.IsAvailable));  // مش متاحين
            Assert.Equal("Tech2", updatedOrder.TechnicianId);   
        }

    }
}
