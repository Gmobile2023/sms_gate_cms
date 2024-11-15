using SmsGateCms.ServiceModel;

namespace SmsGateCms.ServiceInterface;

public interface IBaseConnector
{
   Task<string> SendSmsAsync(SendMessageToProviderRequest sendRequest);
}