using SmsGate.Shared.Common;
using SmsGateCms.ServiceModel;

namespace SmsGateCms.ServiceInterface.BusinessService;

public interface IMessageService
{
    public Task<ResponseMessageBase<object>> SendMessageRequest(SendMessageRequest request);
}