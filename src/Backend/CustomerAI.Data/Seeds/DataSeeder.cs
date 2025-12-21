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
            var customers = new List<Customer>();

            // vip müşteriler - 15%
            var vipFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.PickRandom("Istanbul", "Ankara", "Izmir", "Antalya"))
                .RuleFor(c => c.Sector, f => f.PickRandom("Teknoloji", "Finans", "Saglik"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(4, DateTime.Now.AddYears(-1)))
                .RuleFor(c => c.CreatedAt, f => DateTime.Now)
                .RuleFor(c => c.IsDeleted, false);

            for (int i = 0; i < count * 0.15; i++)
            {
                var customer = vipFaker.Generate();
                customer.Orders = GenerateOrders(customer.Id,
                    orderCount: new Faker().Random.Int(8, 15),
                    minAmount: 3000,
                    maxAmount: 8000,
                    baseDate: DateTime.Now.AddDays(-15));

                customer.Interactions = GenerateInteractions(customer.Id,
                    InteractionType.Support,
                    sentiment: new Faker().Random.Float(4.0f, 5.0f));

                customers.Add(customer);
            }

            // memnun müşteriler - 25%
            var happyFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.Address.City())
                .RuleFor(c => c.Sector, f => f.PickRandom("Teknoloji", "Finans", "Egitim", "E-Ticaret"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(3))
                .RuleFor(c => c.CreatedAt, f => DateTime.Now)
                .RuleFor(c => c.IsDeleted, false);

            for (int i = 0; i < count * 0.25; i++)
            {
                var customer = happyFaker.Generate();
                customer.Orders = GenerateOrders(customer.Id,
                    orderCount: new Faker().Random.Int(4, 8),
                    minAmount: 1500,
                    maxAmount: 4000,
                    baseDate: DateTime.Now.AddDays(new Faker().Random.Int(-45, -20)));

                customer.Interactions = GenerateInteractions(customer.Id,
                    InteractionType.Support,
                    sentiment: new Faker().Random.Float(3.5f, 4.5f));

                customers.Add(customer);
            }

            // normal müşteriler - 30%
            var normalFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.Address.City())
                .RuleFor(c => c.Sector, f => f.PickRandom("Perakende", "Hizmet", "Turizm", "Insaat"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(2))
                .RuleFor(c => c.CreatedAt, f => DateTime.Now)
                .RuleFor(c => c.IsDeleted, false);

            for (int i = 0; i < count * 0.30; i++)
            {
                var customer = normalFaker.Generate();
                customer.Orders = GenerateOrders(customer.Id,
                    orderCount: new Faker().Random.Int(2, 5),
                    minAmount: 800,
                    maxAmount: 2500,
                    baseDate: DateTime.Now.AddDays(new Faker().Random.Int(-90, -50)));

                customer.Interactions = GenerateInteractions(customer.Id,
                    new Faker().PickRandom(InteractionType.Call, InteractionType.Email, InteractionType.Support),
                    sentiment: new Faker().Random.Float(2.5f, 3.8f));

                customers.Add(customer);
            }

            // risk taşıyan müşteriler - 15%
            var coolingFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.Address.City())
                .RuleFor(c => c.Sector, f => f.PickRandom("Perakende", "Hizmet"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(3, DateTime.Now.AddYears(-1)))
                .RuleFor(c => c.CreatedAt, f => DateTime.Now)
                .RuleFor(c => c.IsDeleted, false);

            for (int i = 0; i < count * 0.15; i++)
            {
                var customer = coolingFaker.Generate();
                customer.Orders = GenerateOrders(customer.Id,
                    orderCount: new Faker().Random.Int(1, 3),
                    minAmount: 300,
                    maxAmount: 1200,
                    baseDate: DateTime.Now.AddDays(new Faker().Random.Int(-200, -120)));

                customer.Interactions = GenerateInteractions(customer.Id,
                    new Faker().PickRandom(InteractionType.Call, InteractionType.Support),
                    sentiment: new Faker().Random.Float(1.8f, 3.0f));

                customers.Add(customer);
            }

            // kritik müşteri - 10%
            var churnFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.Address.City())
                .RuleFor(c => c.Sector, f => f.PickRandom("Perakende", "Hizmet", "Lojistik"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(5, DateTime.Now.AddYears(-2)))
                .RuleFor(c => c.CreatedAt, f => DateTime.Now)
                .RuleFor(c => c.IsDeleted, false);

            for (int i = 0; i < count * 0.10; i++)
            {
                var customer = churnFaker.Generate();
                customer.Orders = GenerateOrders(customer.Id,
                    orderCount: new Faker().Random.Int(0, 2),
                    minAmount: 50,
                    maxAmount: 600,
                    baseDate: DateTime.Now.AddDays(new Faker().Random.Int(-450, -250)));

                customer.Interactions = GenerateInteractions(customer.Id,
                    new Faker().PickRandom(InteractionType.Complaint, InteractionType.Support),
                    sentiment: new Faker().Random.Float(-1.0f, 1.5f));

                customers.Add(customer);
            }

            // yeni üye - 5%
            var newFaker = new Faker<Customer>("tr")
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.City, f => f.Address.City())
                .RuleFor(c => c.Sector, f => f.PickRandom("Teknoloji", "E-Ticaret", "Finans"))
                .RuleFor(c => c.MembershipDate, f => f.Date.Past(0, DateTime.Now.AddDays(-10)))
                .RuleFor(c => c.CreatedAt, f => DateTime.Now)
                .RuleFor(c => c.IsDeleted, false);

            for (int i = 0; i < count * 0.05; i++)
            {
                var customer = newFaker.Generate();
                customer.Orders = GenerateOrders(customer.Id,
                    orderCount: new Faker().Random.Int(1, 2),
                    minAmount: 200,
                    maxAmount: 1500,
                    baseDate: DateTime.Now.AddDays(new Faker().Random.Int(-30, -5)));

                customer.Interactions = GenerateInteractions(customer.Id,
                    InteractionType.Email,
                    sentiment: new Faker().Random.Float(2.0f, 4.0f));

                customers.Add(customer);
            }

            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();
        }

        private List<Order> GenerateOrders(int customerId, int orderCount, decimal minAmount, decimal maxAmount, DateTime baseDate)
        {
            if (orderCount == 0) return new List<Order>();

            var faker = new Faker<Order>()
                .RuleFor(o => o.TotalAmount, f => f.Finance.Amount(minAmount, maxAmount))
                .RuleFor(o => o.OrderDate, f => f.Date.Between(baseDate.AddDays(-90), baseDate))
                .RuleFor(o => o.OrderNumber, f => f.Random.AlphaNumeric(8).ToUpper())
                .RuleFor(o => o.Status, f => f.PickRandom("Tamamlandi", "Teslim Edildi", "Iptal", "Iade"))
                .RuleFor(o => o.CreatedAt, f => DateTime.Now)
                .RuleFor(o => o.IsDeleted, false);

            return faker.Generate(orderCount);
        }

        private List<Interaction> GenerateInteractions(int customerId, InteractionType type, float sentiment)
        {
            var faker = new Faker<Interaction>()
                .RuleFor(i => i.Type, type)
                .RuleFor(i => i.Date, f => f.Date.Past(1))
                .RuleFor(i => i.Notes, f => f.Lorem.Sentence())
                .RuleFor(i => i.SentimentScore, sentiment)
                .RuleFor(i => i.CreatedAt, f => DateTime.Now)
                .RuleFor(i => i.IsDeleted, false);

            return faker.Generate(new Faker().Random.Int(1, 3));
        }
    }
}