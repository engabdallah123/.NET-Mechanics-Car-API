using Data;
using Domain.DTO;
using Domain.DTO.Customer;
using Domain.DTO.Vehicle;
using Domain.DTO.Vehicle_Model;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Services.IReposetory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Repository
{
    public class CustomerRepo : ICustomerRepo
    {
        private readonly DataContext db;

        public CustomerRepo(DataContext db)
        {
            this.db = db;
        }
        public async Task<List<CustomerDTO>> GetAllCustomer()
        {
            var customers = await db.Customers.Select(c => new CustomerDTO
            {
                Name = c.Name,
                Phone = c.PhoneNumber,
                Email = c.Email
            }).ToListAsync();
            return customers;
        }
        public async Task<CustomerDTO> GetCustomerById(int id)
        {
            var customer = await db.Customers
                .Where(c => c.Id == id)
                .Select(c => new CustomerDTO
                {
                    Name = c.Name,
                    Phone = c.PhoneNumber,
                    Email = c.Email
                })
                .FirstOrDefaultAsync();
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found.");

            return customer;
        }
        public async Task<GetCustomerWithVehicleDTO> GetCustomerWithVehicles(int id)
        {
            var customer = await db.Customers
                .Where(c => c.Id == id)
                .Select(c => new GetCustomerWithVehicleDTO
                {
                    CustomerName = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Vehicles = c.Vehicles.Select(v => new GetVehicleDTO
                    {
                        Id = v.Id,
                        LicensePlate = v.LicensePlate,
                        CustomerName = c.Name,
                        VehicleModel = new VehicleModelDTO
                        {
                            Name = v.VehicleModel.Name,
                            VehicleId = v.Id,
                            Year = new YearModelDTO
                            {
                                NameYear = v.VehicleModel.Year.YearName
                            },
                            Make = new MakeModelDTO
                            {
                                MakeName = v.VehicleModel.Make.Name
                            }
                        }
                    }).ToList()
                }).FirstOrDefaultAsync();
            return customer;
        }
        public async Task<List<GetCustomerWithVehicleDTO>> GetAllCustomerWithVehicles()
        {
            var customer = await db.Customers
                .Select(c => new GetCustomerWithVehicleDTO
                {
                    CustomerName = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    customerId = c.Id,
                    Vehicles = c.Vehicles.Select(v => new GetVehicleDTO
                    {
                        Id = v.Id,
                        LicensePlate = v.LicensePlate,
                        CustomerName = c.Name,
                        VehicleModel = new VehicleModelDTO
                        {
                            Name = v.VehicleModel.Name,
                            VehicleId = v.Id,
                            Year = new YearModelDTO
                            {
                                NameYear = v.VehicleModel.Year.YearName
                            },
                            Make = new MakeModelDTO
                            {
                                MakeName = v.VehicleModel.Make.Name
                            }
                        }
                    }).ToList()
                }).ToListAsync();
            return customer;
        }
        public async Task AddCustomer(CustomerDTO customerDto)
        {
            var customer = new Customer
            {
                Name = customerDto.Name,
                PhoneNumber = customerDto.Phone,
                Email = customerDto.Email
            };
            await db.Customers.AddAsync(customer);
            await db.SaveChangesAsync();
        }
        public async Task AddCustomerWithVehicleAsync(AddCustomerWithVehicleDTO dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {

                var customer = new Customer
                {
                    Name = dto.CustomerName,
                    Email = dto.Email,
                    PhoneNumber = dto.Phone
                };
                await db.Customers.AddAsync(customer);
                await db.SaveChangesAsync();

                var vehicles = dto.Vehiclse.Select(v => new Vehicle
                {
                    CustomerId = customer.Id,
                    ModelId = v.ModelId,
                    LicensePlate = v.LicensePlate,

                }).ToList();

                await db.Vehicles.AddRangeAsync(vehicles);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task DeleteCustomer(int id)
        {
            var vehicleIds = await db.Vehicles
             .Where(v => v.CustomerId == id)
             .Select(v => v.Id)
             .ToListAsync();

            // احذف WorkOrders أولًا على دفعات
            foreach (var vId in vehicleIds)
            {
                var workOrders = db.WorkOrders.Where(w => w.VehicleId == vId);
                db.WorkOrders.RemoveRange(workOrders);
                await db.SaveChangesAsync();
            }


            var vehicles = db.Vehicles.Where(v => v.CustomerId == id);
            db.Vehicles.RemoveRange(vehicles);
            await db.SaveChangesAsync();


            var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
            db.Customers.Remove(customer);
            await db.SaveChangesAsync();


        }


        public async Task UpdateCustomerWithVehicle(int id, AddCustomerWithVehicleDTO Dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var customer = await db.Customers.Where(c => c.Id == id).FirstOrDefaultAsync();
                if (customer == null)
                    throw new KeyNotFoundException($"Customer with ID {id} not found.");

                
                if (!string.IsNullOrWhiteSpace(Dto.CustomerName))
                    customer.Name = Dto.CustomerName;

                if (!string.IsNullOrWhiteSpace(Dto.Phone))
                    customer.PhoneNumber = Dto.Phone;

                if (!string.IsNullOrWhiteSpace(Dto.Email))
                    customer.Email = Dto.Email;


                if (Dto.Vehiclse != null && Dto.Vehiclse.Any())
                {
                    var newVehicles = Dto.Vehiclse
                        .Where(v => !string.IsNullOrWhiteSpace(v.LicensePlate)) 
                        .Select(v => new Vehicle
                        {
                            CustomerId = customer.Id,
                            ModelId = v.ModelId,
                            LicensePlate = v.LicensePlate
                        })
                        .ToList();

                    if (newVehicles.Any())
                        await db.Vehicles.AddRangeAsync(newVehicles);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();


            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


        }
    }
}
