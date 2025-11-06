using Domain.DTO;
using Domain.DTO.Customer;
using Domain.DTO.Vehicle;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Services.IReposetory;
using Services.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing.Main
{
    public class VehicleRepoTests
    {
        [Fact]
        public async Task AddVehicleAsync_ShouldAddedSuccessed()
        {
            // Arrange
            var context = new InMemoryDBContext();
            var vehicleRepo = new VehicleRepo(context);
            var repoCust = new CustomerRepo(context);
            var modelRepo = new VehicleModelRepo(context);



            var model = new AddVehicleModelDTO
            {
                Name = "Sunny"
            };
            await modelRepo.AddModelAsync(model);
            var modelId = context.VehicleModels.First().Id;


            var vehicle = new AddVehicleDTO
            {
                LicensePlate = "",

            };

            // Act
           // await vehicleRepo.AddVehicleAsync(vehicle, modelId);
            var vehicleId = context.Vehicles.First().Id;

            //  Assert
            Assert.Equal(1, context.Vehicles.Count());

        }
        [Fact]
        public async Task UpdateVehicleAsync_ShouldUpdated()
        {
            var context = new InMemoryDBContext();
            var vehicleRepo = new VehicleRepo(context);
            var modelRepo = new VehicleModelRepo(context);



            var model = new AddVehicleModelDTO
            {
                Name = "Sunny"
            };
            await modelRepo.AddModelAsync(model);
            var modelId = context.VehicleModels.First().Id;


            var vehicle = new AddVehicleDTO
            {
                LicensePlate = "ABC123",

            };

            // Act
//await vehicleRepo.AddVehicleAsync(vehicle, modelId);


            var updatedNameVehicle = new AddVehicleDTO
            {
                LicensePlate = "A123",
            };
            await vehicleRepo.UpdateVehicleAsync(context.Vehicles.First().Id, updatedNameVehicle);

            // Assert
            var updatedVehicle = await context.Vehicles
                                .Include(v => v.VehicleModel)
                                .FirstAsync(v => v.Id == context.Vehicles.First().Id);
            Assert.Equal("A123", updatedVehicle.LicensePlate);
            Assert.Equal("ABC123", updatedVehicle.LicensePlate); // unchanged

        }
        [Fact]
        public async Task DeleteVehicle_ShouldRemoveVehicle()
        {
            var context = new InMemoryDBContext();
            var repoVeh = new VehicleRepo(context);
            var modelRepo = new VehicleModelRepo(context);



            var model = new AddVehicleModelDTO
            {
                Name = "Sunny"
            };
            await modelRepo.AddModelAsync(model);
            var modelId = context.VehicleModels.First().Id;


            var vehicle = new AddVehicleDTO
            {
                LicensePlate = "ABC123"

            };
           

           // await repoVeh.AddVehicleAsync(vehicle, modelId);
            // remove
            var vehicleId = context.Vehicles.First().Id;
            await repoVeh.DeleteVehicleAsync(vehicleId);
            // Assert
            Assert.Empty(context.Vehicles);


        }
        [Fact]
        public async Task GetAllVehiclesAsync_ShouldReturnAllVehcles()
        {
            var context = new InMemoryDBContext();
            var vehicleRepo = new VehicleRepo(context);
            var modelRepo = new VehicleModelRepo(context);



            var model1 = new AddVehicleModelDTO
            {
                Name = "Sunny"
            };
            await modelRepo.AddModelAsync(model1);
            var modelId1 = context.VehicleModels.First().Id;


            var vehicle1 = new AddVehicleDTO
            {

                LicensePlate = "ABC123"

            };
          //  await vehicleRepo.AddVehicleAsync(vehicle1, modelId1);

            var model2 = new AddVehicleModelDTO
            {
                Name = "X6"
            };
            await modelRepo.AddModelAsync(model2);
            var modelId2 = context.VehicleModels.Where(c => c.Name == "X6").Select(c => c.Id).FirstOrDefault();


            var vehicle2 = new AddVehicleDTO
            {

                LicensePlate = "ANM123"

            };
          //  await vehicleRepo.AddVehicleAsync(vehicle2, modelId2);

            // Act
            var result = await vehicleRepo.GetAllVehiclesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

        }
    }
}
