using SmsGateCms.ServiceModel;

namespace SmsGateCms.ServiceInterface;

public interface IMessageService
{
    public Task<bool> CreateSendMessage(CreateSendMessageRequest request);
}