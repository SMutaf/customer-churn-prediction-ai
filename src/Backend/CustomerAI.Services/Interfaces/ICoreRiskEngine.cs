using CustomerAI.Core.Risk;

namespace CustomerAI.Services.Interfaces
{
    public interface ICoreRiskEngine
    {
        CustomerRiskProfile Evaluate(CustomerFeatureVector featureVector);
    }
}
