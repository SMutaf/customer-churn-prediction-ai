using CustomerAI.Core.Risk;

namespace CustomerAI.Services.Interfaces
{
    public interface IFeatureExtractionService
    {
        CustomerFeatureVector Extract(CustomerBehaviorProfile behaviorProfile);
    }
}
