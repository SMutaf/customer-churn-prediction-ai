using CustomerAI.Core.Risk;

namespace CustomerAI.Services.Interfaces
{
    public interface ICustomerBehaviorService
    {
        Task<CustomerBehaviorProfile> BuildBehaviorProfileAsync(int customerId);
    }
}
