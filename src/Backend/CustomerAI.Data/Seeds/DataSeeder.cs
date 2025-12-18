using Bogus;
using CustomerAI.Core.Entities;
using CustomerAI.Core.Enums;
using CustomerAI.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerAI.Data.Seeds
{
    public class DataSeeder
    {
        private readonly CustomerAiDbContext _context;

        public DataSeeder(CustomerAiDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync(int count = 1000)
        {
            // veri tabanı doluysa işlem yapma
           // if (_context.Customers.Any()) return;

            var customers = new List<Customer>();

            // GRUP 1 MUTLU MÜŞTERİLER 
            var happyFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.Address.City())
                .RuleFor(c => c.Sector, f => f.PickRandom("Teknoloji", "Finans", "Eğitim"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(3)); // 3 yıl içinde üye olmuş

            for (int i = 0; i < count * 0.3; i++)
            {
                var customer = happyFaker.Generate();
                // sık sipariş, yeni tarih
                customer.Orders = GenerateOrders(customer.Id, 5, 2000, DateTime.Now.AddDays(-30));
                // pozitif etkileşim
                customer.Interactions = GenerateInteractions(customer.Id, InteractionType.Support, 0.8f);
                customers.Add(customer);
            }

            //  GRUP 2 RİSKLİ  MÜŞTERİLER 
            var churnFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.Address.City())
                .RuleFor(c => c.Sector, f => f.PickRandom("Perakende", "Hizmet"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(5)); // Eski üye

            for (int i = 0; i < count * 0.3; i++)
            {
                var customer = churnFaker.Generate();
                // az sipariş
                customer.Orders = GenerateOrders(customer.Id, 1, 100, DateTime.Now.AddDays(-400));
                // şikayet
                customer.Interactions = GenerateInteractions(customer.Id, InteractionType.Complaint, -0.9f);
                customers.Add(customer);
            }

            // GRUP 3 NORMAL MÜŞTERİLER 
            var randomFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email()) 
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.Address.City())
                .RuleFor(c => c.Sector, f => f.PickRandom("Teknoloji", "Sağlık", "İnşaat"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(4));

            for (int i = 0; i < count * 0.4; i++)
            {
                var customer = randomFaker.Generate();
                customer.Orders = GenerateOrders(customer.Id, 2, 500, DateTime.Now.AddDays(-150));
                customer.Interactions = GenerateInteractions(customer.Id, InteractionType.Call, 0.1f);
                customers.Add(customer);
            }

            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();
        }

        // rastgal sipariş üret
        private List<Order> GenerateOrders(int customerId, int count, decimal minAmount, DateTime baseDate)
        {
            var faker = new Faker<Order>()
                .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(minAmount, minAmount * 2))
                .RuleFor(o => o.OrderDate, f => f.Date.Between(baseDate.AddDays(-60), baseDate))
                .RuleFor(o => o.OrderNumber, f => f.Random.AlphaNumeric(8).ToUpper())
                .RuleFor(o => o.Status, "Tamamlandı");

            return faker.Generate(count);
        }

        // rastgele etkileşim üret
        private List<Interaction> GenerateInteractions(int customerId, InteractionType type, float sentiment)
        {
            var faker = new Faker<Interaction>()
                .RuleFor(i => i.Type, type)
                .RuleFor(i => i.Date, f => f.Date.Past(1))
                .RuleFor(i => i.Notes, f => f.Lorem.Sentence())
                .RuleFor(i => i.SentimentScore, sentiment); 

            return faker.Generate(1); 
        }
    }
}