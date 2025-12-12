using CustomerAI.Core.DTOs;
using CustomerAI.Core.Entities;
using CustomerAI.Data.Context;
using CustomerAI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerAI.Services.Concrete
{
    public class CustomerService : ICustomerService
    {
        private readonly CustomerAiDbContext _context;

        public CustomerService(CustomerAiDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDto> AddAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Sector = dto.Sector,
                City = dto.City,
                MembershipDate = DateTime.Now 
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Sector = customer.Sector,
                City = customer.City
            };
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Sector = c.Sector,
                    City = c.City
                })
                .ToListAsync();

            return customers;
        }

        public async Task<CustomerDto> GetByIdAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null) return null;

            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Sector = customer.Sector,
                City = customer.City
            };
        }
    }
}