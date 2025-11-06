using Domain.DTO;
using Domain.DTO.Customer;
using Domain.DTO.Vehicle;
using Domain.Model;
using Services.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing.Main
{
    public class CustomerRepoTests
    {
        [Fact]
        public async Task AddCustomer_ShouldAddCustomer()
        {
            // Arrange
            var context = new InMemoryDBContext();
            var repo = new CustomerRepo(context);
            

            // Act
            var customerDto = new CustomerDTO
            {
                Name = "Abdallah Ebrahim",
                Email = "eng@gmail.com",
                Phone = "01000000000"

            };
           await repo.AddCustomer(customerDto);

            // Assert
            Assert.Equal(1, context.Customers.Count());
        }
        [Fact]
        public async Task AddCustomerWithVehicleAsync_ShouldAddCustomerWithVehicleAndModel()
        {
            // Arrange
            var context = new InMemoryDBContext();
            var repo = new CustomerRepo(context);
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

                LicensePlate = "",

            };
           // await vehicleRepo.AddVehicleAsync(vehicle,modelId);
            var vehicleId = context.Vehicles.First().Id;

            var custWithVehicle = new AddCustomerWithVehicleDTO
            {
                CustomerName = "Abdallah Ebrahim",
                Email = "eng@gmail.com",
                Phone = "01000000000",

            };
            // Act
            await repo.AddCustomerWithVehicleAsync(custWithVehicle);

            // Assert

            Assert.Single(context.Customers);
            Assert.Single(context.Vehicles);
            Assert.Single(context.VehicleModels);
            Assert.Equal(context.Customers.First().Id, context.Vehicles.First().CustomerId);



        }

        [Fact]
        public async Task RemoveCustomer_ShouldRemoveCustomer()
        {
            // Arrange
            var context = new InMemoryDBContext();
            var repo = new CustomerRepo(context);

            // Act
            var customer = new CustomerDTO
            {
                Name = "Abdallah Ebrahim",
                Email = "eee@kk.com",
                Phone = "01000000000"
            };

            await repo.AddCustomer(customer);
            var customerId = context.Customers.First().Id;
            await repo.DeleteCustomer(customerId);

            // Assert
            Assert.Empty(context.Customers);

        }
        [Fact]
        public async Task UpdateCustomer_ShouldUpdateCustomer()
        {
            // Arrange
            var context = new InMemoryDBContext();
            var repo = new CustomerRepo(context);
            // Act
            var customer = new CustomerDTO
            {
                Name = "Abdallah Ebrahim",
                Email = "ss@ff.com",
                Phone = "01000000000",
            };
           await repo.AddCustomer(customer);
            var customerId = context.Customers.First().Id;
            var updatedCustomer = new CustomerDTO
            {

                Name = "Updated Name",

            };
           // await repo.UpdateCustomer(customerId, updatedCustomer);

            // Assert
            var updatedCustomerId = context.Customers.Find(customerId); // Retrieve the updated customer from the context
            Assert.Equal("Updated Name", updatedCustomerId.Name);
            Assert.Equal("ss@ff.com", updatedCustomerId.Email); // unchanged
            Assert.Equal("01000000000", updatedCustomerId.PhoneNumber); // unchanged
        }
        [Fact]
        public async Task GetCustomerById_ShouldReturnCustomer()
        {
            // Arrange
            var context = new InMemoryDBContext();
            var repo = new CustomerRepo(context);

            var customer = new CustomerDTO
            {
                Name = "Abdallah",
                Email = "ww@jj.com",
                Phone = "01000000000"
            };   
           await repo.AddCustomer(customer);
           

            // Act
            var result = await repo.GetCustomerById(context.Customers.First().Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Abdallah", result.Name);
            Assert.Equal("ww@jj.com", result.Email);
            Assert.Equal("01000000000", result.Phone);
        }
        [Fact]
        public async Task GetAllCustomer_ShouldReturnAllCustomers()
        {
            // Arrange
            var context = new InMemoryDBContext();
            var repo = new CustomerRepo(context);

            var customer1 = new CustomerDTO
            {
                Name = "Abdallah",
                Email = "ww@jj.com",
                Phone = "01000000000"
            };   
            var customer2 = new CustomerDTO
            {
                Name = "Ebrahim",
                Email = "ww@com",
                Phone = "02000000000"
            };   
           await repo.AddCustomer(customer1);
           await repo.AddCustomer(customer2);


            // Act
            var result =await repo.GetAllCustomer();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Name == "Abdallah");
            Assert.Contains(result, c => c.Email == "ww@com");
        }

    }
}
