using Bogus;
using CustomerAI.Core.Entities;
using CustomerAI.Core.Enums;
using CustomerAI.Data.Context;

namespace CustomerAI.Data.Seeds
{
    public class DataSeeder
    {
        private const int MinimumSeedCount = 500;
        private readonly CustomerAiDbContext _context;
        private readonly Faker _faker = new("tr");

        public DataSeeder(CustomerAiDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync(int count = 1000)
        {
            var targetCount = Math.Max(count, MinimumSeedCount);
            var customers = new List<Customer>();

            while (customers.Count < targetCount)
            {
                customers.Add(CreateRandomScenarioCustomer());
            }

            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync();
        }

        private Customer CreateRandomScenarioCustomer()
        {
            var scenarioRoll = _faker.Random.Double();

            return scenarioRoll switch
            {
                < 0.10 => CreateVipLoyal(),
                < 0.22 => CreateSilentChurnRisk(),
                < 0.32 => CreateAngryHighValue(),
                < 0.44 => CreateDormantCustomer(),
                < 0.56 => CreatePriceSensitive(),
                < 0.68 => CreateDiscountHunter(),
                < 0.78 => CreateHighEngagementLowRevenue(),
                _ => CreateStandardCustomer()
            };
        }

        private Customer CreateVipLoyal()
        {
            var customer = CreateBaseCustomer("Teknoloji", "Finans", "E-Ticaret");
            customer.MembershipDate = DateTime.Now.AddDays(-_faker.Random.Int(900, 1800));
            customer.Orders = GenerateOrders(_faker.Random.Int(14, 28), 1800, 6500, DateTime.Now.AddDays(-365), DateTime.Now.AddDays(-3));
            customer.Interactions = GenerateInteractions(_faker.Random.Int(3, 6), InteractionType.Support, 4.2f, 5.0f, 180);
            return customer;
        }

        private Customer CreateSilentChurnRisk()
        {
            var customer = CreateBaseCustomer("SaaS", "Finans", "Perakende");
            customer.MembershipDate = DateTime.Now.AddDays(-_faker.Random.Int(600, 1400));
            customer.Orders = GenerateOrders(_faker.Random.Int(6, 12), 900, 4200, DateTime.Now.AddDays(-500), DateTime.Now.AddDays(-120));
            customer.Interactions = GenerateInteractions(_faker.Random.Int(1, 3), InteractionType.Email, 2.8f, 3.4f, 220);
            return customer;
        }

        private Customer CreateAngryHighValue()
        {
            var customer = CreateBaseCustomer("Kurumsal", "Lojistik", "Finans");
            customer.MembershipDate = DateTime.Now.AddDays(-_faker.Random.Int(700, 1600));
            customer.Orders = GenerateOrders(_faker.Random.Int(8, 18), 2000, 8000, DateTime.Now.AddDays(-420), DateTime.Now.AddDays(-20));
            customer.Interactions = GenerateInteractions(_faker.Random.Int(3, 6), InteractionType.Complaint, 0.6f, 2.0f, 45);
            return customer;
        }

        private Customer CreateDormantCustomer()
        {
            var customer = CreateBaseCustomer("Hizmet", "Turizm", "Insaat");
            customer.MembershipDate = DateTime.Now.AddDays(-_faker.Random.Int(800, 2200));
            customer.Orders = GenerateOrders(_faker.Random.Int(0, 3), 100, 1200, DateTime.Now.AddDays(-900), DateTime.Now.AddDays(-220));
            customer.Interactions = GenerateInteractions(_faker.Random.Int(0, 1), InteractionType.Email, 2.5f, 3.2f, 500);
            return customer;
        }

        private Customer CreatePriceSensitive()
        {
            var customer = CreateBaseCustomer("Perakende", "E-Ticaret", "Hizmet");
            customer.MembershipDate = DateTime.Now.AddDays(-_faker.Random.Int(500, 1200));
            customer.Orders = GenerateOrders(_faker.Random.Int(5, 10), 250, 1700, DateTime.Now.AddDays(-360), DateTime.Now.AddDays(-35));
            customer.Interactions = GenerateInteractions(_faker.Random.Int(2, 4), InteractionType.Email, 2.6f, 3.6f, 120);
            return customer;
        }

        private Customer CreateDiscountHunter()
        {
            var customer = CreateBaseCustomer("E-Ticaret", "Perakende", "Teknoloji");
            customer.MembershipDate = DateTime.Now.AddDays(-_faker.Random.Int(300, 1000));
            customer.Orders = GenerateOrders(_faker.Random.Int(8, 16), 80, 650, DateTime.Now.AddDays(-260), DateTime.Now.AddDays(-10));
            customer.Interactions = GenerateInteractions(_faker.Random.Int(1, 3), InteractionType.Email, 3.0f, 4.0f, 90);
            return customer;
        }

        private Customer CreateHighEngagementLowRevenue()
        {
            var customer = CreateBaseCustomer("SaaS", "Egitim", "Hizmet");
            customer.MembershipDate = DateTime.Now.AddDays(-_faker.Random.Int(200, 900));
            customer.Orders = GenerateOrders(_faker.Random.Int(1, 3), 80, 400, DateTime.Now.AddDays(-180), DateTime.Now.AddDays(-20));
            customer.Interactions = GenerateMixedInteractions(_faker.Random.Int(6, 12), 2.8f, 4.2f, 120);
            return customer;
        }

        private Customer CreateStandardCustomer()
        {
            var customer = CreateBaseCustomer("Teknoloji", "Finans", "Perakende", "Hizmet", "Turizm");
            customer.MembershipDate = DateTime.Now.AddDays(-_faker.Random.Int(120, 900));
            customer.Orders = GenerateOrders(_faker.Random.Int(2, 7), 350, 2500, DateTime.Now.AddDays(-240), DateTime.Now.AddDays(-15));
            customer.Interactions = GenerateMixedInteractions(_faker.Random.Int(1, 4), 2.8f, 4.3f, 180);
            return customer;
        }

        private Customer CreateBaseCustomer(params string[] sectors)
        {
            return new Customer
            {
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Phone = _faker.Phone.PhoneNumber(),
                City = _faker.PickRandom("Istanbul", "Ankara", "Izmir", "Antalya", "Bursa", "Konya"),
                Sector = _faker.PickRandom(sectors),
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
        }

        private List<Order> GenerateOrders(int orderCount, decimal minAmount, decimal maxAmount, DateTime startDate, DateTime endDate)
        {
            var orders = new List<Order>();
            for (var i = 0; i < orderCount; i++)
            {
                orders.Add(new Order
                {
                    OrderNumber = _faker.Random.AlphaNumeric(10).ToUpperInvariant(),
                    TotalAmount = _faker.Finance.Amount(minAmount, maxAmount),
                    OrderDate = _faker.Date.Between(startDate, endDate),
                    Status = _faker.PickRandom("Tamamlandi", "Teslim Edildi", "Iptal", "Iade"),
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                });
            }

            return orders.OrderBy(o => o.OrderDate).ToList();
        }

        private List<Interaction> GenerateInteractions(int count, InteractionType type, float minSentiment, float maxSentiment, int pastDays)
        {
            var interactions = new List<Interaction>();
            for (var i = 0; i < count; i++)
            {
                interactions.Add(CreateInteraction(type, minSentiment, maxSentiment, pastDays));
            }

            return interactions;
        }

        private List<Interaction> GenerateMixedInteractions(int count, float minSentiment, float maxSentiment, int pastDays)
        {
            var interactions = new List<Interaction>();
            for (var i = 0; i < count; i++)
            {
                var type = _faker.PickRandom(InteractionType.Call, InteractionType.Email, InteractionType.Support, InteractionType.Meeting);
                interactions.Add(CreateInteraction(type, minSentiment, maxSentiment, pastDays));
            }

            return interactions;
        }

        private Interaction CreateInteraction(InteractionType type, float minSentiment, float maxSentiment, int pastDays)
        {
            return new Interaction
            {
                Type = type,
                Date = _faker.Date.Between(DateTime.Now.AddDays(-pastDays), DateTime.Now),
                Notes = _faker.Lorem.Sentence(),
                SentimentScore = _faker.Random.Float(minSentiment, maxSentiment),
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
        }
    }
}
