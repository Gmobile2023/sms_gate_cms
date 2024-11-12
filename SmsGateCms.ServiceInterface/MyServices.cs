using Microsoft.Extensions.Logging;
using ServiceStack;
using SmsGateCms.ServiceModel;

namespace SmsGateCms.ServiceInterface;

public class MyServices : Service
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MyServices> _logger;

    public MyServices(ILogger<MyServices> logger, IMessageService messageService)
    {
        _logger = logger;
        _messageService = messageService;
    }

    public async Task<object> PostAsync(CreateSendMessageRequest request)
    {
        var result = await _messageService.CreateSendMessage(request);
        return result;
    }
}