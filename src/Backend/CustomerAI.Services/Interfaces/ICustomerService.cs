using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomerAI.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerAI.Services.Interfaces
{
    public interface ICustomerService
    {

        Task<IEnumerable<CustomerDto>> GetAllAsync();

        Task<CustomerDto> AddAsync(CreateCustomerDto customerDto);

        Task<CustomerDto> GetByIdAsync(int id);
    }
}