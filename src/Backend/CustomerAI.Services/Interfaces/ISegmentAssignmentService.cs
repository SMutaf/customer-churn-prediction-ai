using CustomerAI.Core.Enums;
using CustomerAI.Core.Risk;

namespace CustomerAI.Services.Interfaces
{
    public interface ISegmentAssignmentService
    {
        SegmentType Assign(CustomerRiskProfile riskProfile, CustomerFeatureVector featureVector);
    }
}
