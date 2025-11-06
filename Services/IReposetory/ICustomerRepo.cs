using Domain.DTO.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IReposetory
{
    public interface ICustomerRepo
    {
        public Task<List<CustomerDTO>> GetAllCustomer();
        public Task<CustomerDTO> GetCustomerById(int id);
       public Task<GetCustomerWithVehicleDTO> GetCustomerWithVehicles(int id);
        Task<List<GetCustomerWithVehicleDTO>> GetAllCustomerWithVehicles();
        public Task AddCustomer(CustomerDTO customerDto);
       public Task AddCustomerWithVehicleAsync(AddCustomerWithVehicleDTO dto);
        public Task DeleteCustomer(int id);
        public Task UpdateCustomerWithVehicle(int id, AddCustomerWithVehicleDTO Dto);
    }
}
