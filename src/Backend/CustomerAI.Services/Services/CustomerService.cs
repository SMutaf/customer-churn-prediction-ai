using CustomerAI.Core.DTOs;
using CustomerAI.Core.Entities;
using CustomerAI.Data.Context;
using CustomerAI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CustomerAI.Services.Concrete
{
    public class CustomerService : ICustomerService
    {
        private readonly CustomerAiDbContext _context;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(CustomerAiDbContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CustomerDto> AddAsync(CreateCustomerDto dto)
        {
            _logger.LogInformation("Yeni müşteri ekleme süreci başladı. Email: {Email}, Sektör: {Sector}", dto.Email, dto.Sector);

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

            _logger.LogInformation("Müşteri başarıyla veritabanına kaydedildi. Yeni ID: {CustomerId}", customer.Id);

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
            _logger.LogInformation("Tüm müşteriler listeleniyor...");

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

            _logger.LogInformation("Müşteri listesi çekildi. Toplam {Count} kayıt bulundu.", customers.Count);

            return customers;
        }

        public async Task<CustomerDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Müşteri detayı aranıyor. ID: {CustomerId}", id);

            var customer = await _context.Customers.FindAsync(id);

            if (customer == null) {
                _logger.LogWarning("Müşteri bulunamadı! Aranan ID: {CustomerId}", id);
                return null;
            }

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