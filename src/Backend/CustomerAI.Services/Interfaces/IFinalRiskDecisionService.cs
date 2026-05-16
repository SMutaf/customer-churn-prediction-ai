using CustomerAI.Core.DTOs;
using CustomerAI.Core.Risk;

namespace CustomerAI.Services.Interfaces
{
    public interface IFinalRiskDecisionService
    {
        CustomerRiskProfile BuildFinalProfile(CustomerRiskProfile coreProfile, AiResponseDto mlResponse);
    }
}
